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
        private const string LatestConfigFileVersion = "1.1.0";

        /// <summary>
        /// ツール起動時のイベント
        /// </summary>
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            // カレントディレクトリの設定
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            // 各引数の設定
            int Mode = 1;                   // ツールの実行モード
            bool AllowOtherArgs = true;     // 他の引数の実行を許可するか
            bool InvalidArgs = false;       // 無効な引数が入力されたか

            bool BypassRegistryCheck = false;

            // 引数の読み込み
            for (int i = 0; i < e.Args.Length; i++)
            {
                // 引数を受け取る
                Debug.WriteLine("取得した引数：" + e.Args[i]);
                switch (e.Args[i])
                {
                    // プリコネRを起動
                    case "--run-priconner":
                        Mode = 2;
                        break;
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
                            Mode = 0;
                            AllowOtherArgs = false;
                        }
                        else
                        {
                            InvalidArgs = true;
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
                            Mode = 0;
                            AllowOtherArgs = false;
                        }
                        else
                        {
                            InvalidArgs = true;
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
                            Mode = 0;
                            AllowOtherArgs = false;
                        }
                        else
                        {
                            InvalidArgs = true;
                        }
                        break;
                    // その他の引数はすべて無視
                    default:
                        InvalidArgs = true;
                        break;
                }

                if (!AllowOtherArgs)
                {
                    break;
                }
            }

            // 無効な引数が入力された場合メッセージを表示する
            if (InvalidArgs)
            {
                ShowMessage("無効な引数が入力されたか、引数の使い方が誤っています。\n\"--help\"もしくは\"-h\"でヘルプを表示できます。", MessageBoxImage.Error);
            }

            // モード別の挙動
            if (Mode > 0)
            {
                CheckExeConfigFile();
                if (!BypassRegistryCheck)
                {
                    CheckRegistry();
                }
                switch (Mode)
                {
                    // 通常起動
                    case 1:
                        // メインウインドウを呼び出す
                        MainWindow window = new();
                        window.Show();
                        break;
                    // ゲームの起動
                    case 2:
                        string GameStartupUri = ConfigurationManager.AppSettings["GameStartupUri"] ?? "dmmgameplayer://play/GCL/priconner/cl/win";
                        string GameStartupUriArgs = ConfigurationManager.AppSettings["GameStartupUriArgs"] ?? string.Empty;

                        ProcessStartInfo StartGame = new(GameStartupUri)
                        {
                            Arguments = GameStartupUriArgs
                        };

                        _ = Process.Start(StartGame);
                        Shutdown();
                        break;
                }
            }
            else
            {
                // 終了
                Shutdown();
            }
        }

        /// <summary>
        /// メッセージを表示します。
        /// コンソールが利用可能な場合はコンソールに出力、そうでなければメッセージボックスで表示します。
        /// </summary>
        public static void ShowMessage(string message = null, MessageBoxImage messageboximage = MessageBoxImage.None)
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
        public static string GetUserSettingsPath()
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
        /// PR_Manager.exe.configファイルの存在とバージョンを確認します
        /// 存在しない場合は作成し、バージョンが古い場合は警告メッセージを表示します。
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
            // "PR_Manager.exe.config"ファイルのバージョンを確認する
            if (PR_Manager.Properties.Settings.Default.ConfigFileVersion != LatestConfigFileVersion)
            {
                _ = MessageBox.Show("\"" + ConfigFileName + "\"ファイルのバージョンが古いようです。新しいバージョンに更新してください。\n古いファイルを削除することで次回実行時に新しいバージョンのファイルが生成されます。", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            Load.Close();
        }
    }
}
