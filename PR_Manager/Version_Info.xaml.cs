using System;
using System.Collections.Generic;
using System.Windows;

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
            _ = OKButton.Focus();
        }

        private void Close_Popup(object sender, RoutedEventArgs e)      // ウインドウを閉じる
        {
            Close();
        }
    }
}
