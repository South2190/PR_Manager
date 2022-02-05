using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace PR_Manager
{
    /// <summary>
    /// Version_Info.xaml の相互作用ロジック
    /// </summary>
    public partial class Version_Info : Window
    {
        /// <summary>
        /// バージョン情報を表示します
        /// </summary>
        public Version_Info()
        {
            InitializeComponent();
            ThisName.Text = MainWindow.ThisName;
            _ = OKButton.Focus();
        }

        /// <summary>
        /// OKボタンがクリックされた場合にウインドウを閉じます
        /// </summary>
        private void Close_Popup(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// ハイパーリンクがクリックされた時の挙動を指定します
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            _ = Process.Start(e.Uri.ToString());
        }
    }
}
