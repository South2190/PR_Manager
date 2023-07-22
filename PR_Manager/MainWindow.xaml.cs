using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
//using System.Linq;
//using System.Management;
using System.Media;
using System.Runtime.InteropServices;
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
        /// <summary>
        /// Debug時とRelease時のXAMLの表示を操作します
        /// </summary>
        public static Visibility IsDebugVisible
        {
#if DEBUG
            get { return Visibility.Visible; }
#elif TRACE
            get { return Visibility.Collapsed; }
#endif
        }

        // ---------------ツールの設定(読み取り専用)---------------
        // ウインドウタイトル
#if DEBUG
        public static readonly string ThisName = "PR_Manager(DEBUG)";
#elif TRACE
        public static readonly string ThisName = "PR_Manager";
#endif
        // バージョン
        public static readonly string Version = "1.1.0.230722";
        //public static readonly string AssemblyVersion = "1.0.0";
        // --------------------------------------------------------

        // グローバル変数の宣言
        // ゲーム起動URI
        public static readonly string GameStartupUri        = ConfigurationManager.AppSettings["GameStartupUri"]        ?? "dmmgameplayer://play/GCL/priconner/cl/win";
        public static readonly string GameStartupUriArgs    = ConfigurationManager.AppSettings["GameStartupUriArgs"]    ?? string.Empty;
        // ゲーム実行ファイルの名前
        public static readonly string TargetAppName         = ConfigurationManager.AppSettings["TargetAppName"]         ?? "PrincessConnectReDive";

        public int intWidth;
        public int intHeight;
        public int fullScreen;
        public int chooseMonitor;
        public int allowNative;
        public bool WidthFocus = false;
        public bool HeightFocus = false;

        // タイマーの宣言
        public readonly Timer timer = new();

        private static readonly ControlRegistry ctrlreg = new()
        {
            RegKey              = ConfigurationManager.AppSettings["RegKey"]                ?? @"Software\Cygames\PrincessConnectReDive",
            WidthKey            = ConfigurationManager.AppSettings["WidthKey"]              ?? "Screenmanager Resolution Width_h182942802",
            HeightKey           = ConfigurationManager.AppSettings["HeightKey"]             ?? "Screenmanager Resolution Height_h2627697771",
            fullScreenKey       = ConfigurationManager.AppSettings["fullScreenKey"]         ?? "Screenmanager Fullscreen mode_h3630240806",
            allowNativeKey      = ConfigurationManager.AppSettings["allowNativeKey"]        ?? "Screenmanager Resolution Use Native_h1405027254",
            chooseMonitorKey    = ConfigurationManager.AppSettings["chooseMonitorKey"]      ?? "UnitySelectMonitor_h17969598",
        };

        // user32.dll関数の定義
        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hwnd, ref System.Drawing.Point lpPoint);
        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);
        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

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
            Title = ThisName;

            if (!Properties.Settings.Default.IsUpgraded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.IsUpgraded = true;
                Properties.Settings.Default.Save();
            }

            // タイマーの設定
            timer.Tick += new EventHandler(TickHandler);
            timer.Interval = 1000/* ms */;

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

            //if (!File.Exists())

            // 接続されているディスプレイ数だけコンボボックスに選択肢を追加
            for (i = 1; i <= Screen.AllScreens.Length; i++)
            {
                _ = selMonitor.Items.Add("ディスプレイ" + i);
            }

            // 仮の初期値
            selMonitor.SelectedIndex = -1;

            // インポート設定に応じた内容をフォームに読み込む
            switch (Properties.Settings.Default.ImportInStarting)
            {
                case "LasttimeEnded":
                    LoadLasttimeEnded();
                    break;
                case "Registry":
                    LoadRegistry();
                    break;
                case "DefaultValue":
                    LoadDefault();
                    break;
                default:
                    break;
            }

            ChangeButton();
            timer.Start();
        }

        /*
        /// <summary>
        /// ディスプレイ型番の取得の際に利用する関数です
        /// </summary>
        /// <param name="ushortArray"></param>
        /// <returns></returns>
        private static string UInt16ArrayToString(ushort[] ushortArray)
        {
            return new string(ushortArray.Select(u => (char)u).Where(c => c != 0).ToArray());
        }
        */

        /// <summary>
        /// 一定時間毎にこの関数内のプログラムを実行します。
        /// </summary>
        private void TickHandler(object sender, EventArgs e)
        {
            ChangeButton();
        }

        /// <summary>
        /// プリコネRのプロセスの存在有無別でボタンの表示と動作を切り替えます
        /// </summary>
        public void ChangeButton()
        {
            // プリコネRが起動している場合
            if (Process.GetProcessesByName(TargetAppName).Length <= 0)
            {
                if (!startButton.IsEnabled)
                {
                    startButton.IsEnabled = true;
                }
                startButton.Content = "ゲームを起動";
                startButton.ToolTip = "ゲームを起動します";
            }
            // プリコネRが起動していない場合、設定に応じて表示と動作を変える
            else
            {
                switch (Properties.Settings.Default.GameEndButton)
                {
                    case "SendSignal":
                        if (!startButton.IsEnabled)
                        {
                            startButton.IsEnabled = true;
                        }
                        startButton.Content = "ゲームを終了";
                        startButton.ToolTip = "ゲームを終了します";
                        break;
                    case "TaskKill":
                        if (!startButton.IsEnabled)
                        {
                            startButton.IsEnabled = true;
                        }
                        startButton.Content = "ゲームを強制終了";
                        startButton.ToolTip = "ゲームを強制終了します";
                        break;
                    case "Disabled":
                    default:
                        if (startButton.IsEnabled)
                        {
                            startButton.IsEnabled = false;
                        }
                        startButton.Content = "ゲームを起動";
                        startButton.ToolTip = "ゲームを起動します";
                        break;
                }
            }
        }

        /// <summary>
        /// 前回終了時の入力内容をフォームに反映します
        /// </summary>
        private void LoadLasttimeEnded()
        {
            WidthBox.Text               = Properties.Settings.Default.Width;
            HeightBox.Text              = Properties.Settings.Default.Height;
            UseNative.IsChecked         = Properties.Settings.Default.AllowNative;
            selMonitor.SelectedIndex    = Properties.Settings.Default.ChooseMonitor;
            AllowFixedass.IsChecked     = Properties.Settings.Default.AllowFix;
            AssW.Text                   = Properties.Settings.Default.AspectW;
            AssH.Text                   = Properties.Settings.Default.AspectH;
            // モード設定の反映
            switch (Properties.Settings.Default.WindowMode)
            {
                // フルスクリーンもしくは仮想フルスクリーン
                case 0:
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
                    //loadResult = false;
                    break;
            }
        }

        /// <summary>
        /// 終了時に現在の設定内容をconfigに保存します
        /// </summary>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Width           = WidthBox.Text;
            Properties.Settings.Default.Height          = HeightBox.Text;
            Properties.Settings.Default.AllowNative     = (bool)UseNative.IsChecked;
            Properties.Settings.Default.ChooseMonitor   = selMonitor.SelectedIndex;
            Properties.Settings.Default.AllowFix        = (bool)AllowFixedass.IsChecked;
            Properties.Settings.Default.AspectW         = AssW.Text;
            Properties.Settings.Default.AspectH         = AssH.Text;
            // モード設定の反映
            // 仮想フルスクリーン
            if (ModeW.IsChecked == false && ModeF.IsChecked == true)
            {
                Properties.Settings.Default.WindowMode = 1;
            }
            // ウインドウ
            else if (ModeW.IsChecked == true && ModeF.IsChecked == false)
            {
                Properties.Settings.Default.WindowMode = 3;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// レジストリから現在の設定値を取得し、フォームに内容を反映します
        /// </summary>
        private void LoadRegistry()
        {
            bool loadResult;

            loadResult = ctrlreg.ReadReg(ref intWidth, ref intHeight, ref fullScreen, ref allowNative, ref chooseMonitor);

            WidthBox.Text = intWidth.ToString();
            HeightBox.Text = intHeight.ToString();

            // モード設定の反映
            switch (fullScreen)
            {
                // フルスクリーンもしくは仮想フルスクリーン
                case 0:
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
                _ = System.Windows.MessageBox.Show("一部設定が正常に読み込まれませんでした", ThisName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// 公式のデフォルト設定をフォームに反映します。
        /// </summary>
        private void LoadDefault()
        {
            ModeW.IsChecked             = true;
            ModeF.IsChecked             = false;
            WidthBox.Text               = "1280";
            HeightBox.Text              = "720";
            UseNative.IsEnabled         = false;
            AssW.Text                   = "16";
            AssH.Text                   = "9";
            selMonitor.SelectedIndex    = 0;
        }

        /// <summary>
        /// フォームに入力されている内容を変数に代入します
        /// </summary>
        /// <returns>
        /// フォームの入力内容が正しく変数に代入された場合(true)、それ以外の場合(false)を返します
        /// </returns>
        private bool LoadForm()
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
                _ = System.Windows.MessageBox.Show("解像度の入力値が不正です。正しく入力したか確認してください。\n\n・半角数字のみが入力されているか\n・正の整数が入力されているか", ThisName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // 仮想フルスクリーン時、チェックが入っていた場合ネイティブにする
            allowNative = ((bool)UseNative.IsChecked && fullScreen == 1) ? 1 : 0;
            return true;
        }

        /// <summary>
        /// フォームの入力内容をレジストリに反映します。
        /// </summary>
        private void RewriteReg()
        {
            //int cX = 0, cY = 0;
            // 変数の値を更新し、エラーが返された場合中断
            if (!LoadForm())
            {
                return;
            }

            // リアルタイムでの解像度変更が有効になっている場合
            if (ConfigurationManager.AppSettings["ApplyInRunning"] == "True")
            {
                // プリコネRが起動している場合
                foreach (Process p in Process.GetProcessesByName(TargetAppName))
                {
                    // 現在のプリコネRのウインドウ位置を取得
                    System.Drawing.Point point = default;
                    ClientToScreen(p.MainWindowHandle, ref point);
                    //cX = point.X;
                    //cY = point.Y;
                    // ウインドウサイズを変更
                    MoveWindow(p.MainWindowHandle, point.X - 8, point.Y - 31, intWidth + 16, intHeight + 39, 1);
                    //point = default;
                    //ClientToScreen(p.MainWindowHandle, ref point);
                    //cX = point.X - cX;
                    //cY = point.Y - cY;
                }
            }

            ctrlreg.WriteReg(intWidth, intHeight, fullScreen, allowNative, selMonitor.SelectedIndex);
        }

        /// <summary>
        /// プリコネRが起動している場合タスクキル、起動していない場合起動します
        /// </summary>
        private void LaunchPriconner(object sender, RoutedEventArgs e)
        {
            const int WM_CLOSE = 0x0010;
            Process[] ps = Process.GetProcessesByName(TargetAppName);

            // プリコネRが起動していない場合起動する
            if (ps.Length <= 0)
            {
                try
                {
                    ProcessStartInfo StartGame = new(GameStartupUri)
                    {
                        Arguments = GameStartupUriArgs
                    };

                    _ = Process.Start(StartGame);
                }
                catch
                {
                    return;
                }
            }
            // プリコネRが起動している場合
            else
            {
                switch (Properties.Settings.Default.GameEndButton)
                {
                    // タスクキルを行う場合
                    case "TaskKill":
                        foreach (Process p in ps)
                        {
                            p.Kill();
                        }
                        break;
                    // 終了シグナルを送信する場合
                    case "SendSignal":
                        foreach (Process p in ps)
                        {
                            _ = PostMessage(p.MainWindowHandle, WM_CLOSE, (IntPtr)0, (IntPtr)0);
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// 一方の解像度が入力された時、指定されたアスペクト比に応じてもう一方の解像度を計算し自動入力します
        /// </summary>
        /// <param name="Side">横の場合は(W)、縦の場合は(H)</param>
        private void FixedAspectRatio(char Side, int Resolution)
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

            switch (Side)
            {
                // 横から計算する場合
                case 'W':
                    Ans = AspectH * Resolution / AspectW;
                    HeightBox.Text = Ans.ToString();
                    break;
                // 縦から計算する場合
                case 'H':
                    Ans = AspectW * Resolution / AspectH;
                    WidthBox.Text = Ans.ToString();
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// 各関数へのトリガー
        /// </summary>
        private void LoadRegistry_(object sender, RoutedEventArgs e)
        {
            LoadRegistry();
        }
        private void LoadLasttimeEnded_(object sender, RoutedEventArgs e)
        {
            LoadLasttimeEnded();
        }
        private void LoadDefault_(object sender, RoutedEventArgs e)
        {
            LoadDefault();
        }
        private void RewriteReg_(object sender, RoutedEventArgs e)
        {
            RewriteReg();
        }

        /// <summary>
        /// オプションウインドウを表示します
        /// </summary>
        private void ShowOptionWindow(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new()
            {
                Owner = this
            };
            _ = optionWindow.ShowDialog();
        }

        /// <summary>
        /// バージョン情報を表示します
        /// </summary>
        private void VersionInfo(object sender, RoutedEventArgs e)
        {
            VersionInfo infoWindow = new()
            {
                Owner = this
            };
            _ = infoWindow.ShowDialog();
        }

        /// <summary>
        /// ツールを終了します
        /// </summary>
        private void ThisExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 「ウインドウ」ラジオボタンが選択された場合の処理
        /// </summary>
        private void WindowRadioChecked(object sender, RoutedEventArgs e)
        {
            fullScreen = 3;
            // Nativeチェックボックスを無効化
            UseNative.IsEnabled = false;
            // 解像度テキストボックスを有効化
            WidthBox.IsEnabled = true;
            HeightBox.IsEnabled = true;
        }

        /// <summary>
        /// 「フルスクリーン」ラジオボタンが選択された場合の処理
        /// </summary>
        private void FullscreenRadioChecked(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["UseVirtualFullscreenMode"] == "False")
            {
                // フルスクリーンモード
                fullScreen = 0;
            }
            else
            {
                // 仮想フルスクリーンモード
                fullScreen = 1;
            }
            // Nativeチェックボックスを有効化
            UseNative.IsEnabled = true;
            // Nativeオプションが有効の場合解像度テキストボックスを無効化
            if (UseNative.IsChecked == true)
            {
                WidthBox.IsEnabled = false;
                HeightBox.IsEnabled = false;
            }
        }

        /// <summary>
        /// Nativeチェックボックスのチェック状況に応じた解像度テキストボックスの有効・無効の切替
        /// </summary>
        private void UseNativeChecked(object sender, RoutedEventArgs e)
        {
            if (ModeW.IsChecked == false)
            {
                WidthBox.IsEnabled = false;
                HeightBox.IsEnabled = false;
            }
        }
        private void UseNativeUnchecked(object sender, RoutedEventArgs e)
        {
            WidthBox.IsEnabled = true;
            HeightBox.IsEnabled = true;
        }

        /// <summary>
        /// 「横」テキストボックスの内容が変更された場合の処理
        /// </summary>
        private void FixassW(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(WidthBox.Text, out int getVal) && WidthFocus)
            {
                FixedAspectRatio('W', getVal);
            }
        }

        /// <summary>
        /// 「縦」テキストボックスの内容が変更された場合の処理
        /// </summary>
        private void FixassH(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(HeightBox.Text, out int getVal) && HeightFocus)
            {
                FixedAspectRatio('H', getVal);
            }
        }

        /// <summary>
        /// テストしたいプログラムや機能がある場合この関数内に記述します
        /// </summary>
        private void TestProgram(object sender, RoutedEventArgs e)
        {
            // テストしたいプログラムをここに入力
            /*
            ダークモード実装の試みの残骸
            SolidColorBrush Dark = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            Background = Dark;
            MenuBar.Background = new SolidColorBrush(Color.FromArgb(255, 90, 90, 90));
            */
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
        /// 任意のキーが押下された際に呼び出される関数です
        /// </summary>
        private void AnyKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ctrl + Sの判定
            if (Control.ModifierKeys == Keys.Control && e.Key == Key.S && ConfigurationManager.AppSettings["EnableApplyShortcutKey"] == "True")
            {
                e.Handled = true;
                RewriteReg();
            }
        }

        /// <summary>
        /// テキストボックスの一部のキー入力を無効化し、警告音を鳴らします
        /// </summary>
        private void TextBoxKeyJudge(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // テンキー含めた数字キー、Tab、Ctrl以外のキーが入力された場合
            if ((e.Key < Key.D0 || e.Key > Key.D9) && (e.Key < Key.NumPad0 || e.Key > Key.NumPad9) && e.Key != Key.Tab && !(Control.ModifierKeys == Keys.Control))
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }
    }
}
