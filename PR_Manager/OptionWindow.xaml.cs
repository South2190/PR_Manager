using System.Windows;

namespace PR_Manager
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        private int GameEndButtonCache;
        public OptionWindow()
        {
            InitializeComponent();
            GameEndButtonCache = Properties.Settings.Default.GameEndButton switch
            {
                "SendSignal" => 1,
                "TaskKill" => 2,
                _ => 0,
            };
            GameEndButton.SelectedIndex = GameEndButtonCache;
        }

        /// <summary>
        /// 選択された値によってOKボタンの有効・無効を切り替えます
        /// </summary>
        private void ControlOKButton(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GameEndButton.SelectedIndex == GameEndButtonCache)
            {
                OKButton.IsEnabled = false;
            }
            else
            {
                OKButton.IsEnabled = true;
            }
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
