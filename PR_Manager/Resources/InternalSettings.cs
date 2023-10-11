using System.Configuration;

namespace PR_Manager.Resources
{
    /// <summary>
    /// 開発者が利用するツールの内部設定です。
    /// </summary>
    internal class InternalSettings
    {
        // ウインドウタイトル
#if DEBUG
        public static readonly string AppName = "PR_Manager(DEBUG)";
#elif TRACE
        public static readonly string AppName = "PR_Manager";
#endif
        // バージョン
        public static readonly string AppVersion = "1.1.1-rc0";

        // ゲーム起動URI
        public static readonly string GameStartupUri        = ConfigurationManager.AppSettings["GameStartupUri"]        ?? "dmmgameplayer://play/GCL/priconner/cl/win";
        public static readonly string GameStartupUriArgs    = ConfigurationManager.AppSettings["GameStartupUriArgs"]    ?? string.Empty;
        // ゲーム実行ファイルの名前
        public static readonly string TargetAppName         = ConfigurationManager.AppSettings["TargetAppName"]         ?? "PrincessConnectReDive";

        // レジストリ設定
        public static readonly string RegKey                = ConfigurationManager.AppSettings["RegKey"]                ?? @"Software\Cygames\PrincessConnectReDive";
        public static readonly string WidthKey              = ConfigurationManager.AppSettings["WidthKey"]              ?? "Screenmanager Resolution Width_h182942802";
        public static readonly string HeightKey             = ConfigurationManager.AppSettings["HeightKey"]             ?? "Screenmanager Resolution Height_h2627697771";
        public static readonly string fullScreenKey         = ConfigurationManager.AppSettings["fullScreenKey"]         ?? "Screenmanager Fullscreen mode_h3630240806";
        public static readonly string allowNativeKey        = ConfigurationManager.AppSettings["allowNativeKey"]        ?? "Screenmanager Resolution Use Native_h1405027254";
        public static readonly string chooseMonitorKey      = ConfigurationManager.AppSettings["chooseMonitorKey"]      ?? "UnitySelectMonitor_h17969598";
    }
}
