using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace PR_Manager
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /*
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        static App()
        {
            AttachConsole(-1);
        }
        */

        private readonly MainWindow mainWindow = new();

        private const string ResourceConfigFile = "PR_Manager.App.config";
        private const string ConfigFileName = "PR_Manager.exe.config";

        /// <summary>
        /// ツール起動時のイベント
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // configファイルの存在確認
            if (!File.Exists(ConfigFileName))
            {
                // 現在実行しているアセンブリを取得
                Assembly assembly = Assembly.GetExecutingAssembly();

                // リソース"App.config"を取得
                StreamReader reader = new(assembly.GetManifestResourceStream(ResourceConfigFile));

                // 外部configファイルに出力する
                File.WriteAllText(ConfigFileName, reader.ReadToEnd());
            }

            Configuration appSettings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Debug.WriteLine("設定ファイルの保存パス：" + appSettings.FilePath); // 設定ファイルの保存パス

            // configファイルのバージョンが古い場合は更新する
            if (!PR_Manager.Properties.Settings.Default.IsUpgraded)
            {
                PR_Manager.Properties.Settings.Default.Upgrade();
                PR_Manager.Properties.Settings.Default.IsUpgraded = true;
                PR_Manager.Properties.Settings.Default.Save();
                Debug.WriteLine("Upgraded");
            }

            // 管理者権限での起動が指定されている場合は管理者権限で再起動
            if (ConfigurationManager.AppSettings["RequireAdmin"] == "True" && !IsAdministrator())
            {
                _ = RunSelfAsAdmin();
                Shutdown();
                return;
            }

            // メインウインドウを呼び出す
            mainWindow.Show();
        }

        /// <summary>
        /// ツール終了時のイベント
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            mainWindow.SaveCurrentSettings();
            base.OnExit(e);
        }

        /// <summary>
        /// ツールを管理者権限で再起動します
        /// </summary>
        public bool RunSelfAsAdmin()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            ProcessStartInfo startInfo = new(assembly.Location)
            {
                UseShellExecute = true,
                Verb = "runas",
            };

            try
            {
                _ = Process.Start(startInfo);
            }
            // ユーザーが「いいえ」を選択した場合処理を中断
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ツールに管理者権限があるかどうかを調べます
        /// </summary>
        /// <returns>
        /// 管理者権限がある場合は(True)、そうでない場合は(False)を返します
        /// </returns>
        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
