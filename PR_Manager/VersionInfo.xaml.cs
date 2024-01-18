using PR_Manager.Classes;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace PR_Manager
{
    /// <summary>
    /// VersionInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionInfo : Window
    {
        /// <summary>
        /// バージョン情報を表示します
        /// </summary>
        public VersionInfo()
        {
            InitializeComponent();
            ThisName.Text = InternalSettings.AppName;
            Version.Text = InternalSettings.AppVersion;
            Date.Text = InternalSettings.ReleaseDate.ToString("yyyy/MM/dd");
            _ = OKButton.Focus();
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
