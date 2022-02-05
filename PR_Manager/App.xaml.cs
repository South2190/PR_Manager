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

            if (PR_Manager.Properties.Settings.Default.RequireAdmin && !IsAdministrator())
            {
                _ = RunSelfAsAdmin();
                Shutdown();
                return;
            }

            // メインウインドウを呼び出す
            MainWindow window = new();
            window.Show();
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
        /// <returns></returns>
        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
