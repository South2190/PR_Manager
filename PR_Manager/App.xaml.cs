using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Resources;

namespace PR_Manager
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern int AttachConsole(int processId);

        private const string ConfigFileName = "PR_Manager.exe.config";

        /// <summary>
        /// ツール起動時のイベント
        /// </summary>
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            // カレントディレクトリの設定
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            // 各引数の設定
            bool AllowStart = true;         // ツールの通常起動を許可するか
            bool AllowOtherArgs = true;     // 他の引数の実行を許可するか

            bool BypassRegistryCheck = false;

            // 引数の読み込み
            for (int i = 0; i < e.Args.Length; i++)
            {
                // 引数を受け取る
                Debug.WriteLine("取得した引数：" + e.Args[i]);
                switch (e.Args[i])
                {
                    // レジストリチェックを行わずに起動
                    case "--registrycheck-bypass":
                        BypassRegistryCheck = true;
                        break;
                    // user.configの削除
                    case "--userconfig-delete":
                    case "--userconfig-delete-current":
                        if (i == 0) {
                            string UserConfigPath = e.Args[i] switch
                            {
                                "--userconfig-delete" => Environment.GetEnvironmentVariable("LOCALAPPDATA") + @"\PR_Manager",
                                "--userconfig-delete-current" => GetUserSettingsPath(),
                                _ => null
                            };
                            if (Directory.Exists(UserConfigPath))
                            {
                                Directory.Delete(UserConfigPath, true);
                                ShowMessage("設定ファイルを削除しました。", MessageBoxImage.Asterisk);
                            }
                            else
                            {
                                ShowMessage("削除できる設定ファイルが見つかりませんでした。", MessageBoxImage.Error);
                            }
                            AllowStart = false;
                            AllowOtherArgs = false;
                        }
                        break;
                    // 現在のuser.configのみ削除
                    // バージョン情報を表示
                    case "-v":
                    case "--version":
                        if (i == 0)
                        {
                            VersionInfo versionInfo = new();
                            _ = versionInfo.ShowDialog();
                            AllowStart = false;
                            AllowOtherArgs = false;
                        }
                        break;
                    // ヘルプを表示
                    case "-h":
                    case "--help":
                        if (i == 0) {
                            string HelpArgsTxt;
                            StreamResourceInfo info = GetResourceStream(new Uri("/HelpArgs.txt", UriKind.Relative));
                            using (StreamReader sr = new(info.Stream))
                            {
                                HelpArgsTxt = sr.ReadToEnd();
                            }

                            ShowMessage(HelpArgsTxt, MessageBoxImage.Asterisk);
                            AllowStart = false;
                            AllowOtherArgs = false;
                        }
                        break;
                    // その他の引数はすべて無視
                    default:
                        break;
                }

                if (!AllowOtherArgs)
                {
                    break;
                }
            }

            // 引数が指定されなかった場合通常起動する
            if (AllowStart)
            {
                CheckExeConfigFile();
                if (!BypassRegistryCheck)
                {
                    CheckRegistry();
                }
                ReadytoStart();
            }
            else
            {
                Shutdown();
            }
        }

        /// <summary>
        /// ツールを起動します。
        /// ツールの起動準備をしてからメインウインドウを呼び出します。
        /// </summary>
        public void ReadytoStart()
        {
#if DEBUG
            if (Environment.Is64BitProcess)
            {
                Debug.WriteLine("64ビットで動作しています");
            }
            Configuration appSettings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Debug.WriteLine("設定ファイルの存在：" + appSettings.HasFile); // 設定ファイルの存在確認
            Debug.WriteLine(appSettings.FilePath); // 設定ファイルの保存パス
            Debug.WriteLine(GetUserSettingsPath());
            Debug.WriteLine(Environment.GetEnvironmentVariable("LOCALAPPDATA") + @"\PR_Manager");
            Debug.WriteLine(Assembly.GetEntryAssembly().Location);
            Debug.WriteLine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
#endif

            // メインウインドウを呼び出す
            // グローバル"mainWindow"からは絶対に呼び出さないこと
            MainWindow window = new();
            window.Show();
        }

        /// <summary>
        /// メッセージを表示します。
        /// コンソールが利用可能な場合はコンソールに出力、そうでなければメッセージボックスで表示します。
        /// </summary>
        public void ShowMessage(string message = null, MessageBoxImage messageboximage = MessageBoxImage.None)
        {
            if (AttachConsole(-1) == 0)
            {
                _ = MessageBox.Show(message, "PR_Manager", MessageBoxButton.OK, messageboximage);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// 現在の実行ファイルが使用しているUserConfigフォルダの場所を取得します。
        /// </summary>
        /// <returns>UserConfigフォルダの場所を絶対パスで返します。</returns>
        public string GetUserSettingsPath()
        {
            string filepath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string[] split = filepath.Split('\\');
            string result = null;

            foreach (string s in split)
            {
                result += s;
                if (Regex.IsMatch(s, @"^PR_Manager\.exe_Url_"))
                {
                    break;
                }
                else
                {
                    result += '\\';
                }
            }

            return result;
        }

        /// <summary>
        /// PR_Manager.exe.configファイルの存在を確認します
        /// 存在しない場合は作成します
        /// </summary>
        public void CheckExeConfigFile()
        {
            if (!File.Exists(ConfigFileName))
            {
                string ReadConfigFile;
                StreamResourceInfo info = GetResourceStream(new Uri("/App.config", UriKind.Relative));
                using (StreamReader sr = new(info.Stream))
                {
                    ReadConfigFile = sr.ReadToEnd();
                }
                File.WriteAllText(ConfigFileName, ReadConfigFile);

                // configファイルを新たに作成した際、user.configが存在するとフリーズしてしまうので再起動する
                if (Directory.Exists(GetUserSettingsPath()))
                {
                    _ = Process.Start(Assembly.GetEntryAssembly().Location);
                    Shutdown();
                }
            }
        }

        /// <summary>
        /// レジストリが存在するかどうかを確認します。
        /// 存在しない場合はエラーメッセージを表示した上で終了コード(-1)で終了します。
        /// </summary>
        public void CheckRegistry()
        {
            string RegKey = ConfigurationManager.AppSettings["RegKey"] ?? @"Software\Cygames\PrincessConnectReDive";

            RegistryKey Load = Registry.CurrentUser.OpenSubKey(RegKey);

            // レジストリが存在しない場合起動を中止する
            if (Load == null)
            {
                _ = MessageBox.Show("レジストリキーが見つかりませんでした。ゲームがインストールされていない可能性があります。", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }
    }
}
