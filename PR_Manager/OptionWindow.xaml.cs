using System.Windows;

namespace PR_Manager
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        public OptionWindow()
        {
            InitializeComponent();
            GameEndButton.SelectedIndex = Properties.Settings.Default.GameEndButton switch
            {
                "SendSignal" => 1,
                "TaskKill" => 2,
                _ => 0,
            };
        }

        /// <summary>
        /// 設定を保存し、ウインドウを閉じます
        /// </summary>
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
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
