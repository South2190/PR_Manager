using System.Windows;

namespace PR_Manager
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        private int GameEndButtonCache, ImportInStartingCache;
        public OptionWindow()
        {
            InitializeComponent();
            ImportInStartingCache = Properties.Settings.Default.ImportInStarting switch
            {
                "LasttimeEnded" => 0,
                "Registry" => 1,
                "DefaultValue" => 2,
                _ => -1     // 想定外の値の場合は未選択状態にする
            };
            ImportInStarting.SelectedIndex = ImportInStartingCache;
            GameEndButtonCache = Properties.Settings.Default.GameEndButton switch
            {
                "Disabled" => 0,
                "SendSignal" => 1,
                "TaskKill" => 2,
                _ => -1     // 想定外の値の場合は未選択状態にする
            };
            GameEndButton.SelectedIndex = GameEndButtonCache;
        }

        /// <summary>
        /// コンボボックスの値が変更された際にフォームの表示内容を変更します
        /// </summary>
        private void ControlForm(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ImportInStartingExp.Text = "説明：" + ImportInStarting.SelectedIndex switch
            {
                0 => "前回のツール終了時にフォームへ入力されていた内容を読み込みます",
                1 => "設定されている内容をレジストリから読み込みます",
                2 => "ゲーム側の初期設定を読み込みます",
                _ => ""
            };
            GameEndButtonExp.Text = "説明：" + GameEndButton.SelectedIndex switch
            {
                0 => "ボタンをグレーアウトします",
                1 => "ゲームに対して終了シグナルを送信します(管理者権限が必要です)",
                2 => "ゲームを強制的に終了させます",
                _ => ""
            };
            OKButton.IsEnabled = ImportInStarting.SelectedIndex != ImportInStartingCache || GameEndButton.SelectedIndex != GameEndButtonCache;
        }

        /// <summary>
        /// 設定を保存し、ウインドウを閉じます
        /// </summary>
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ImportInStarting = ImportInStarting.SelectedIndex switch
            {
                1 => "Registry",
                2 => "DefaultValue",
                _ => "LasttimeEnded"
            };
            Properties.Settings.Default.GameEndButton = GameEndButton.SelectedIndex switch
            {
                1 => "SendSignal",
                2 => "TaskKill",
                _ => "Disabled"
            };
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
