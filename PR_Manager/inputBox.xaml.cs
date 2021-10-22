using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace PR_Manager
{
    /// <summary>
    /// inputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class inputBox : Window
    {
        public string gettingValue;
        /// <summary>
        /// ユーザーに任意の文字列を入力するよう促すウインドウを表示します。
        /// </summary>
        /// <param name="Message">メッセージ</param>
        /// <param name="Name">ウインドウタイトル</param>
        /// <param name="Value">テキストボックスに格納しておく値</param>
        public inputBox(string Message, string Name = "", string Value = "")
        {
            InitializeComponent();
            Title = Name;
            Mess.Text = Message;
            if (Value == null || Value == "")
            {
                OKButton.IsEnabled = false;
            }
            else
            {
                TextBox.Text = Value;
            }
            _ = TextBox.Focus();
        }

        private void Return_Value(object sender, RoutedEventArgs e)
        {
            gettingValue = TextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Window_Close(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Char_Judge(object sender, TextChangedEventArgs e)
        {
            if (TextBox.Text == null || TextBox.Text == "")
            {
                OKButton.IsEnabled = false;
            }
            else
            {
                OKButton.IsEnabled = true;
            }
        }

        private void Window_Closeing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DialogResult == null)
            {
                DialogResult = false;
            }
        }
    }
}
