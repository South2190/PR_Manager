using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace PR_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ----------各変数等の宣言----------
        public int intWidth;
        public int intHeight;
        public int fullScreen;
        public int chooseMonitor;
        public int allowNative;
        private bool flg = true;

        public MainWindow()
        {
            InitializeComponent();
            Activated += (s, e) =>
            {
                if (flg)
                {
                    flg = false;
                    Setup();
                }
            };
        }

        /// <summary>
        /// アプリケーション起動時、一度だけこの関数内のプログラムを実行します
        /// </summary>
        private void Setup()
        {
            for (int i = 1; i <= Screen.AllScreens.Length; i++)
            {
                _ = selMonitor.Items.Add(i);    // 接続されているモニタ数だけコンボボックスに選択肢を追加
            }

            selMonitor.SelectedIndex = 0;   // 仮の初期値

            if (Screen.AllScreens.Length <= 1)
            {
                selMonitor.IsEnabled = false;   // シングルモニタの場合コンボボックスを無効化
            }

            LoadKey();
        }

        /// <summary>
        /// レジストリから現在の設定値を取得し、フォームに内容を反映します。
        /// </summary>
        private void LoadKey()
        {
            RegistryKey Load = Registry.CurrentUser.OpenSubKey(@"Software\Cygames\PrincessConnectReDive");

            if (Load == null)       // キーが見つからなかった場合は中断
            {
                startButton.IsEnabled = false;
                applyButton.IsEnabled = false;      // ボタンを無効化
                _ = System.Windows.MessageBox.Show("ターゲットキーが見つかりませんでした。プリンセスコネクト！Re:Diveがインストールされていない可能性があります。", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            startButton.IsEnabled = true;
            applyButton.IsEnabled = true;      // ボタンを有効化

            // レジストリから現在の設定を取得
            bool loadResult = true;
            try
            {
                fullScreen = (int)Load.GetValue("Screenmanager Fullscreen mode_h3630240806");
                intWidth = (int)Load.GetValue("Screenmanager Resolution Width_h182942802");
                intHeight = (int)Load.GetValue("Screenmanager Resolution Height_h2627697771");
                allowNative = (int)Load.GetValue("Screenmanager Resolution Use Native_h1405027254");
                chooseMonitor = (int)Load.GetValue("UnitySelectMonitor_h17969598");
            }
            catch (System.NullReferenceException)
            {
                loadResult = false;
            }
            Load.Close();
            WidthBox.Text = intWidth.ToString();
            HeightBox.Text = intHeight.ToString();

            // モード設定の反映
            switch (fullScreen)
            {
                case 1:     // フルスクリーン
                    ModeW.IsChecked = false;
                    ModeF.IsChecked = true;
                    UseNative.IsEnabled = true;     // Nativeチェックボックスを有効化
                    break;
                case 3:     // ウインドウ
                    ModeW.IsChecked = true;
                    ModeF.IsChecked = false;
                    UseNative.IsEnabled = false;    // Nativeチェックボックスを無効化
                    break;
                default:    // 例外
                    loadResult = false;
                    break;
            }

            // ネイティブ設定の反映
            switch (allowNative)
            {
                case 0:
                    UseNative.IsChecked = false;
                    break;
                case 1:
                    UseNative.IsChecked = true;
                    break;
                default:    // 例外
                    loadResult = false;
                    break;
            }

            // モニタ設定の反映
            if (chooseMonitor < Screen.AllScreens.Length)
            {
                selMonitor.SelectedIndex = chooseMonitor;
            }
            else        // 例外
            {
                loadResult = false;
            }

            // 例外が発生した場合警告を表示
            if (!loadResult)
            {
                _ = System.Windows.MessageBox.Show("一部設定が正常に読み込まれませんでした", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// フォームに入力されている内容を変数に代入します。
        /// </summary>
        /// <returns>
        /// フォームの入力内容が正しく変数に代入された場合(true)、それ以外の場合(false)を返します。
        /// </returns>
        private bool Load_Form()
        {
            // テキストボックスの値の判定と変換
            bool convertCheck;
            convertCheck = int.TryParse(WidthBox.Text, out intWidth);
            convertCheck &= int.TryParse(HeightBox.Text, out intHeight);

            if (intWidth < 0 || intHeight < 0)      // 数値が負数の場合falseを返す
            {
                convertCheck = false;
            }

            if (!convertCheck)      // 変数がfalseの場合中断
            {
                _ = System.Windows.MessageBox.Show("入力値が不正です。", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            allowNative = ((bool)UseNative.IsChecked && fullScreen == 1) ? 1 : 0;       // フルスクリーン時、チェックが入っていた場合ネイティブにする
            return true;
        }

        /// <summary>
        /// XMLファイルが存在するか、形式が正しいかどうかを検証します
        /// </summary>
        /// <param name="XmlFilePath"></param>
        /// <returns>
        /// 正常な場合はTrue、異常な場合はFalseを返します
        /// </returns>
        private bool xmlVerify(string XmlFilePath)
        {
            bool Result = false;
            if (File.Exists(XmlFilePath))       // XMLファイルが存在する場合
            {
                StreamReader xmlNullJudge = new StreamReader(XmlFilePath);      // XMLファイルの内容を取得
                string line;
                while ((line = xmlNullJudge.ReadLine()) != null)
                {
                    if (line.IndexOf(@"<") > 0 && line.IndexOf(@">") > 0)       // いずれかの行にタグが存在する場合
                    {
                        Result = true;      // 値をTrueにする
                    }
                }
                xmlNullJudge.Close();
            }

            if (Result)     // タグが存在した場合のみに実行
            {
                XmlDocument NodeSearch = new XmlDocument();
                NodeSearch.Load(XmlFilePath);
                XmlNodeList manyConfig = NodeSearch.SelectNodes(@"/config");        // "config"タグを検索
                if (manyConfig.Count != 1)      // タグが1つ以外の場合
                {
                    Result = false;     // 値をFalseにする
                }
            }

            return Result;
        }

        /// <summary>
        /// フォームの入力内容をXMLファイルに保存します
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportConfig(object sender, RoutedEventArgs e)
        {
            if (!Load_Form())   // 変数の値を更新
            {
                return;     // エラーが返された場合中断
            }

            // ユーザーにファイル名を確認する
            inputBox AskName = new inputBox("プリセット名を入力してください", "PR_Manager");
            bool? select = AskName.ShowDialog();
            if (select == false || select == null)      // キャンセルが選択された場合中断
            {
                return;
            }

            // XML出力ディレクトリ設定
            string xmlAddress = System.Environment.CurrentDirectory + @"\config.xml";
            //string xmlDebug = System.Environment.CurrentDirectory + @"\debug.xml";

            // XMLに追記する内容を変数に保存
            XElement xmlPreset = new XElement("preset",
                new XAttribute("name", AskName.gettingValue),
                new XElement("Mode", fullScreen),
                new XElement("Width", intWidth),
                new XElement("Height", intHeight),
                new XElement("Native", allowNative),
                new XElement("selMonitor", selMonitor.SelectedIndex)
            );

            if (xmlVerify(xmlAddress))
            {
                // 同名のプリセットを検索
                XmlDocument xmlSearch = new XmlDocument();
                xmlSearch.Load(xmlAddress);
                XmlNodeList searchPreset = xmlSearch.SelectNodes(@"//preset[@name='" + AskName.gettingValue + @"']");

                // 同名プリセットが存在する場合、確認のプロンプトを表示する
                if (searchPreset.Count > 0 && System.Windows.MessageBox.Show("同名のプリセットが存在します。\n上書きしますか？", "PR_Manager", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                {
                    // 上書きしない場合
                    return;
                }
                else
                {
                    // 上書きする場合
                    for (int i = searchPreset.Count - 1; i >= 0; i--)
                    {
                        _ = searchPreset[i].ParentNode.RemoveChild(searchPreset[i]);
                    }
                    //xmlSearch.Save(xmlDebug);
                    xmlSearch.Save(xmlAddress);
                }
                XElement xmlConfig = XElement.Load(xmlAddress);
                xmlConfig.Add(xmlPreset);
                xmlConfig.Save(xmlAddress);
            }
            else
            {
                XDocument xmlConfig = new XDocument
                (
                    new XDeclaration("1.0", "utf-8", "true"),
                    new XComment("このファイルは基本的に書き換えないでください"),
                    new XElement("config", xmlPreset)
                );
                xmlConfig.Save(xmlAddress);
            }
        }

        /// <summary>
        /// フォームの入力内容をレジストリに反映します。
        /// </summary>
        private void Rewrite_Reg(object sender, RoutedEventArgs e)
        {
            if (!Load_Form())   // 変数の値を更新
            {
                return;     // エラーが返された場合中断
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Cygames\PrincessConnectReDive", true);
            key.SetValue("Screenmanager Resolution Width_h182942802", intWidth);            // 横解像度
            key.SetValue("Screenmanager Resolution Height_h2627697771", intHeight);         // 縦解像度
            key.SetValue("Screenmanager Fullscreen mode_h3630240806", fullScreen);          // ウインドウまたはフルスクリーンモード
            key.SetValue("UnitySelectMonitor_h17969598", selMonitor.SelectedIndex);         // モニタ選択
            key.SetValue("Screenmanager Resolution Use Native_h1405027254", allowNative);   // ネイティブモード設定
            key.Close();
            _ = System.Windows.MessageBox.Show("レジストリを書き換えました。", "PR_Manager", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void LoadSettings(object sender, RoutedEventArgs e)     // Loadkey()トリガー
        {
            LoadKey();
        }

        private void Version_Info(object sender, RoutedEventArgs e)     // バージョン情報の表示
        {
            Version_Info infoWindow = new Version_Info();
            _ = infoWindow.ShowDialog();
        }

        private void WindowRadio_Checked(object sender, RoutedEventArgs e)      //「ウインドウ」ラジオボタンが選択された場合
        {
            fullScreen = 3;
            UseNative.IsEnabled = false;    // Nativeチェックボックスを無効化
        }

        private void FullscreenRadio_Checked(object sender, RoutedEventArgs e)  //「フルスクリーン」ラジオボタンが選択された場合
        {
            fullScreen = 1;
            UseNative.IsEnabled = true;     // Nativeチェックボックスを有効化
        }

        /// <summary>
        /// プリンセスコネクト！Re:Diveをexplorer経由で実行します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_Priconner(object sender, RoutedEventArgs e)
        {
            _ = Process.Start("explorer", "dmmgameplayer://priconner/cl/general/priconner");
        }

        private void This_Exit(object sender, RoutedEventArgs e)    // ツールの終了
        {
            Close();
        }

        /// <summary>
        /// 数字以外のキー入力を無効化し、警告音を鳴らします
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_Judge(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key < Key.D0 || e.Key > Key.D9) && (e.Key < Key.NumPad0 || e.Key > Key.NumPad9))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }
    }
}
