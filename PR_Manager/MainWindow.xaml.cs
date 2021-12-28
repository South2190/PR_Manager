using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
//using System.Management;
using System.Media;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

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
        public bool WidthFocus;
        public bool HeightFocus;
        public string pName = "PR_Manager";
        public string appName = "PrincessConnectReDive";
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
            int i;

            // ↓↓↓   これはディスプレイの型番をコンボボックスに表示しようとしたんだけど、
            //          どうも(プライマリディスプレイを除き)選択したディスプレイの型番と実際に起動してくるディスプレイが噛み合わず断念
            //          (識別番号との規則性は皆無、プライマリディスプレイからの座標で決まってるのかこれ???)

            /*
            // 接続されているディスプレイの型番を取得し、コンボボックスに追加
            string[] MonitorName = new string[Screen.AllScreens.Length];
            try
            {
                i = 0;
                ManagementObjectSearcher sercher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorID");
                foreach (ManagementObject queryObj in sercher.Get())
                {
                    MonitorName[i] = UInt16ArrayToString((ushort[])queryObj["UserFriendlyName"]);
                    i++;
                }

                i = 0;
                foreach (var s in Screen.AllScreens)
                {
                    if (s.Primary)
                    {
                        _ = selMonitor.Items.Add("1." + MonitorName[i]);
                        MonitorName[i] = null;
                    }
                    i++;
                }

                int j = 2;
                for (i = 0; i < Screen.AllScreens.Length; i++)
                {
                    if (MonitorName[i] != null)
                    {
                        _ = selMonitor.Items.Add(j + "." + MonitorName[i]);
                        j++;
                    }
                }
            }
            // ディスプレイの型番が取得できなかった場合、ディスプレイ番号のみコンボボックスに追加
            catch (Exception)
            {
                for (i = 1; i <= Screen.AllScreens.Length; i++)
                {
                    _ = selMonitor.Items.Add("ディスプレイ" + i);
                }
            }
            */

            // 接続されているディスプレイ数だけコンボボックスに選択肢を追加
            for (i = 1; i <= Screen.AllScreens.Length; i++)
            {
                _ = selMonitor.Items.Add("ディスプレイ" + i);
            }

            // 仮の初期値
            selMonitor.SelectedIndex = 0;

            // シングルモニタの場合コンボボックスを無効化
            if (Screen.AllScreens.Length <= 1)
            {
                selMonitor.IsEnabled = false;
            }

            // アスペクト比テキストボックスを無効にする(仮)
            AssW.IsEnabled = false;
            AssH.IsEnabled = false;

            // タイマーの宣言
            Timer timer = new Timer();
            timer.Tick += new EventHandler(TickHandler);
            timer.Interval = 1000/* ms */;
            timer.Start();

            LoadKey();
            ChangeButton();
        }

        /// <summary>
        /// ディスプレイ型番の取得の際に利用する関数です
        /// </summary>
        /// <param name="ushortArray"></param>
        /// <returns></returns>
        private static string UInt16ArrayToString(ushort[] ushortArray)
        {
            return new string(ushortArray.Select(u => (char)u).Where(c => c != 0).ToArray());
        }

        /// <summary>
        /// 一定時間毎にこの関数内のプログラムを実行します。
        /// </summary>
        private void TickHandler(object sender, EventArgs e)
        {
            ChangeButton();
        }

        /// <summary>
        /// レジストリから現在の設定値を取得し、フォームに内容を反映します。
        /// </summary>
        private void LoadKey()
        {
            RegistryKey Load = Registry.CurrentUser.OpenSubKey(@"Software\Cygames\PrincessConnectReDive");

            // キーが見つからなかった場合は中断
            if (Load == null)
            {
                // ボタンを無効化
                startButton.IsEnabled = false;
                applyButton.IsEnabled = false;
                _ = System.Windows.MessageBox.Show("レジストリキーが見つかりませんでした。プリンセスコネクト！Re:Diveがインストールされていない可能性があります。", pName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ボタンを有効化
            startButton.IsEnabled = true;
            applyButton.IsEnabled = true;

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
            catch (NullReferenceException)
            {
                loadResult = false;
            }
            Load.Close();
            WidthBox.Text = intWidth.ToString();
            HeightBox.Text = intHeight.ToString();

            // モード設定の反映
            switch (fullScreen)
            {
                // フルスクリーン
                case 1:
                    ModeW.IsChecked = false;
                    ModeF.IsChecked = true;

                    // Nativeチェックボックスを有効化
                    UseNative.IsEnabled = true;
                    break;
                // ウインドウ
                case 3:
                    ModeW.IsChecked = true;
                    ModeF.IsChecked = false;

                    // Nativeチェックボックスを無効化
                    UseNative.IsEnabled = false;
                    break;
                // 例外
                default:
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

                // 例外
                default:
                    loadResult = false;
                    break;
            }

            // モニタ設定の反映
            if (chooseMonitor < Screen.AllScreens.Length)
            {
                selMonitor.SelectedIndex = chooseMonitor;
            }
            // 例外
            else
            {
                loadResult = false;
            }

            // 例外が発生した場合警告を表示
            if (!loadResult)
            {
                _ = System.Windows.MessageBox.Show("一部設定が正常に読み込まれませんでした", pName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// プリコネRのプロセスの存在有無別でボタンの表示と動作を切り替えます。
        /// </summary>
        private void ChangeButton()
        {
            // プリコネRが起動しているかどうかの判定
            if (Process.GetProcessesByName(appName).Length > 0)
            {
                startButton.Content = "プリコネRを強制終了";
                startButton.ToolTip = "ゲームを強制終了します";
            }
            else
            {
                startButton.Content = "プリコネRを起動";
                startButton.ToolTip = "ゲームを起動します";
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

            // 数値が負数の場合falseを返す
            if (intWidth < 0 || intHeight < 0)
            {
                convertCheck = false;
            }

            // 変数がfalseの場合中断
            if (!convertCheck)
            {
                _ = System.Windows.MessageBox.Show("解像度の入力値が不正です。正しく入力したか確認してください。\n\n・半角数字のみが入力されているか\n・正の整数が入力されているか", pName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // フルスクリーン時、チェックが入っていた場合ネイティブにする
            allowNative = ((bool)UseNative.IsChecked && fullScreen == 1) ? 1 : 0;
            return true;
        }

        /// <summary>
        /// 一方の解像度が入力された時、指定されたアスペクト比に応じてもう一方の解像度を計算し自動入力します
        /// </summary>
        /// <param name="Side">横の場合は(W)、縦の場合は(H)</param>
        private void Fixed_AspectRatio(char Side, int Resolution)
        {
            int Ans;

            // アスペクト比の固定が有効かどうかを確認する
            if (AllowFixedass.IsChecked != true)
            {
                return;
            }

            // アスペクト比が正しく入力されているか確認する
            bool convertCheck;
            convertCheck = int.TryParse(AssW.Text, out int AspectW);
            convertCheck &= int.TryParse(AssH.Text, out int AspectH);

            // 数値が負数の場合falseを返す
            if (AspectW < 0 || AspectH < 0)
            {
                convertCheck = false;
            }

            // 正しい数値でない場合は解像度の固定を行わない
            if (!convertCheck)
            {
                return;
            }

            // 横から計算した場合
            if (Side == 'W')
            {
                Ans = AspectH * Resolution / AspectW;
                HeightBox.Text = Ans.ToString();
            }
            // 縦から計算する場合
            else if (Side == 'H')
            {
                Ans = AspectW * Resolution / AspectH;
                WidthBox.Text = Ans.ToString();
            }
        }

        /// <summary>
        /// フォームの入力内容をレジストリに反映します。
        /// </summary>
        private void Rewrite_Reg(object sender, RoutedEventArgs e)
        {
            // 変数の値を更新し、エラーが返された場合中断
            if (!Load_Form())
            {
                return;
            }

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Cygames\PrincessConnectReDive", true);
            key.SetValue("Screenmanager Resolution Width_h182942802", intWidth);            // 横解像度
            key.SetValue("Screenmanager Resolution Height_h2627697771", intHeight);         // 縦解像度
            key.SetValue("Screenmanager Fullscreen mode_h3630240806", fullScreen);          // ウインドウまたはフルスクリーンモード
            key.SetValue("UnitySelectMonitor_h17969598", selMonitor.SelectedIndex);         // モニタ選択
            key.SetValue("Screenmanager Resolution Use Native_h1405027254", allowNative);   // ネイティブモード設定
            key.Close();
            _ = System.Windows.MessageBox.Show("レジストリを書き換えました。", pName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        /// <summary>
        /// プリコネRが起動している場合タスクキル、起動していない場合explorer経由で実行します
        /// </summary>
        private void Run_Priconner(object sender, RoutedEventArgs e)
        {
            Process[] ps = Process.GetProcessesByName(appName);

            // プリコネRが起動している場合タスクキル
            if (ps.Length > 0)
            {
                foreach (Process p in ps)
                {
                    p.Kill();
                }
            }
            // プリコネRが起動していない場合explorer経由で実行
            else
            {
                _ = Process.Start("explorer", "dmmgameplayer://priconner/cl/general/priconner");
            }
        }

        /// <summary>
        /// Loadkey()トリガー
        /// </summary>
        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            LoadKey();
        }

        /// <summary>
        /// バージョン情報を表示します
        /// </summary>
        private void Version_Info(object sender, RoutedEventArgs e)
        {
            Version_Info infoWindow = new Version_Info();
            _ = infoWindow.ShowDialog();
        }

        /// <summary>
        /// ツールを終了します
        /// </summary>
        private void This_Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 「ウインドウ」ラジオボタンが選択された場合の処理
        /// </summary>
        private void WindowRadio_Checked(object sender, RoutedEventArgs e)
        {
            fullScreen = 3;
            // Nativeチェックボックスを無効化
            UseNative.IsEnabled = false;
        }

        /// <summary>
        /// 「フルスクリーン」ラジオボタンが選択された場合の処理
        /// </summary>
        private void FullscreenRadio_Checked(object sender, RoutedEventArgs e)
        {
            fullScreen = 1;
            // Nativeチェックボックスを有効化
            UseNative.IsEnabled = true;
        }

        /// <summary>
        /// 「横」テキストボックスの内容が変更された場合の処理
        /// </summary>
        private void Fixass_W(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(WidthBox.Text, out int getVal) && WidthFocus)
            {
                Fixed_AspectRatio('W', getVal);
            }
        }

        /// <summary>
        /// 「縦」テキストボックスの内容が変更された場合の処理
        /// </summary>
        private void Fixass_H(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(HeightBox.Text, out int getVal) && HeightFocus)
            {
                Fixed_AspectRatio('H', getVal);
            }
        }

        /// <summary>
        /// 「縦」と「横」テキストボックスのフォーカス状況を格納します
        /// </summary>
        private void WidthGotFocus(object sender, RoutedEventArgs e)
        {
            WidthFocus = true;
        }
        private void WidthLostFocus(object sender, RoutedEventArgs e)
        {
            WidthFocus = false;
        }
        private void HeightGotFocus(object sender, RoutedEventArgs e)
        {
            HeightFocus = true;
        }
        private void HeightLostFocus(object sender, RoutedEventArgs e)
        {
            HeightFocus = false;
        }

        /// <summary>
        /// 「アスペクト比を固定する」チェックボックスのチェック状況に応じてテキストボックスの有効と無効を切り替えます
        /// </summary>
        private void Fixass_Checked(object sender, RoutedEventArgs e)
        {
            AssW.IsEnabled = true;
            AssH.IsEnabled = true;
        }
        private void Fixass_Unchecked(object sender, RoutedEventArgs e)
        {
            AssW.IsEnabled = false;
            AssH.IsEnabled = false;
        }

        /*
        /// <summary>
        /// テストしたいプログラムや機能がある場合この関数内に記述します
        /// </summary>
        private void TestProgram(object sender, RoutedEventArgs e)
        {
            // テストしたいプログラムをここに入力
        }
        */

        /// <summary>
        /// 数字以外のキー入力を無効化し、警告音を鳴らします
        /// </summary>
        private void Key_Judge(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key < Key.D0 || e.Key > Key.D9) && (e.Key < Key.NumPad0 || e.Key > Key.NumPad9) && e.Key != Key.Tab)
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }
    }
}
