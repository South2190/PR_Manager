using Microsoft.Win32;
using System;
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
        public static readonly string Version = "4.2.0.220602β";

        // ツールの設定ファイル名
        //public static readonly string ConfigFileName = "PR_Manager.exe.config";
        // --------------------------------------------------------

        // グローバル変数の宣言
        // ゲーム起動URI
        public string GameStartupUri;
        // ゲーム実行ファイルの名前
        public string TargetAppName;
        // レジストリキーのパス
        public string RegKey;
        // レジストリ値の名前
        public string intWidthKey;          // 横解像度
        public string intHeightKey;         // 縦解像度
        public string fullScreenKey;        // ウインドウまたはフルスクリーンモード
        public string allowNativeKey;       // ネイティブ
        public string chooseMonitorKey;     // モニタ選択

        public int intWidth;
        public int intHeight;
        public int fullScreen;
        public int chooseMonitor;
        public int allowNative;
        public bool WidthFocus;
        public bool HeightFocus;

        // タイマーの宣言
        public readonly Timer timer = new();

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

            // グローバル変数の値を設定
            GameStartupUri      = ConfigurationManager.AppSettings["GameStartupUri"]    ?? "dmmgameplayer://play/GCL/priconner/cl/win";
            TargetAppName       = ConfigurationManager.AppSettings["TargetAppName"]     ?? "PrincessConnectReDive";
            RegKey              = ConfigurationManager.AppSettings["RegKey"]            ?? @"Software\Cygames\PrincessConnectReDive";
            intWidthKey         = ConfigurationManager.AppSettings["WidthKey"]          ?? "Screenmanager Resolution Width_h182942802";
            intHeightKey        = ConfigurationManager.AppSettings["HeightKey"]         ?? "Screenmanager Resolution Height_h2627697771";
            fullScreenKey       = ConfigurationManager.AppSettings["fullScreenKey"]     ?? "Screenmanager Fullscreen mode_h3630240806";
            allowNativeKey      = ConfigurationManager.AppSettings["allowNativeKey"]    ?? "Screenmanager Resolution Use Native_h1405027254";
            chooseMonitorKey    = ConfigurationManager.AppSettings["chooseMonitorKey"]  ?? "UnitySelectMonitor_h17969598";

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
            selMonitor.SelectedIndex = 0;

            // シングルモニタの場合コンボボックスを無効化
            if (Screen.AllScreens.Length <= 1)
            {
                selMonitor.IsEnabled = false;
            }

            //LoadLasttimeSettings();
            LoadKey();
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
        private void LoadLasttimeSettings()
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
                    //loadResult = false;
                    break;
            }
        }

        /// <summary>
        /// 現在の設定内容をconfigに保存します
        /// </summary>
        public void SaveCurrentSettings()
        {
            Properties.Settings.Default.Width           = WidthBox.Text;
            Properties.Settings.Default.Height          = HeightBox.Text;
            Properties.Settings.Default.AllowNative     = (bool)UseNative.IsChecked;
            Properties.Settings.Default.ChooseMonitor   = selMonitor.SelectedIndex;
            Properties.Settings.Default.AllowFix        = (bool)AllowFixedass.IsChecked;
            Properties.Settings.Default.AspectW         = AssW.Text;
            Properties.Settings.Default.AspectH         = AssH.Text;
            // モード設定の反映
            // フルスクリーン
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
        private void LoadKey()
        {
            RegistryKey Load = Registry.CurrentUser.OpenSubKey(RegKey);

            // キーが見つからなかった場合は中断
            if (Load == null)
            {
                // ボタンを無効化
                startButton.IsEnabled = false;
                applyButton.IsEnabled = false;
                _ = System.Windows.MessageBox.Show("レジストリキーが見つかりませんでした。プリンセスコネクト！Re:Diveがインストールされていない可能性があります。", ThisName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // レジストリから現在の設定を取得
            bool loadResult = true;
            try
            {
                intWidth        = (int)Load.GetValue(intWidthKey);          // 横解像度
                intHeight       = (int)Load.GetValue(intHeightKey);         // 縦解像度
                fullScreen      = (int)Load.GetValue(fullScreenKey);        // ウインドウまたはフルスクリーンモード
                allowNative     = (int)Load.GetValue(allowNativeKey);       // ネイティブ
                chooseMonitor   = (int)Load.GetValue(chooseMonitorKey);     // モニタ選択
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
                _ = System.Windows.MessageBox.Show("一部設定が正常に読み込まれませんでした", ThisName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// 公式のデフォルト設定をフォームに反映します。
        /// </summary>
        private void LoadDefault(object sender, RoutedEventArgs e)
        {
            WidthBox.Text               = "1280";
            HeightBox.Text              = "720";
            UseNative.IsEnabled         = false;
            ModeW.IsChecked             = true;
            ModeF.IsChecked             = false;
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

            // フルスクリーン時、チェックが入っていた場合ネイティブにする
            allowNative = ((bool)UseNative.IsChecked && fullScreen == 1) ? 1 : 0;
            return true;
        }

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hwnd, ref System.Drawing.Point lpPoint);
        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, int bRepaint);
        /// <summary>
        /// フォームの入力内容をレジストリに反映します。
        /// </summary>
        private void RewriteReg(object sender, RoutedEventArgs e)
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

            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegKey, true);
            key.SetValue(intWidthKey, intWidth);                            // 横解像度
            key.SetValue(intHeightKey, intHeight);                          // 縦解像度
            key.SetValue(fullScreenKey, fullScreen);                        // ウインドウまたはフルスクリーンモード
            key.SetValue(allowNativeKey, allowNative);                      // ネイティブ
            key.SetValue(chooseMonitorKey, selMonitor.SelectedIndex);       // モニタ選択
            key.Close();
            //_ = System.Windows.MessageBox.Show("レジストリを書き換えました。\n\n移動後の座標の差異...X:" + cX + ", Y:" + cY, ThisName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
            _ = System.Windows.MessageBox.Show("レジストリを書き換えました。", ThisName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// プリコネRが起動している場合タスクキル、起動していない場合起動します
        /// </summary>
        private void RunPriconner(object sender, RoutedEventArgs e)
        {
            const int WM_CLOSE = 0x0010;
            Process[] ps = Process.GetProcessesByName(TargetAppName);

            // プリコネRが起動していない場合起動する
            if (ps.Length <= 0)
            {
                try
                {
                    _ = Process.Start(GameStartupUri);
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
        /// Loadkey()トリガー
        /// </summary>
        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            LoadKey();
        }

        /// <summary>
        /// LoadLasttimeSettings()トリガー
        /// </summary>
        private void LoadLasttime(object sender, RoutedEventArgs e)
        {
            LoadLasttimeSettings();
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
            fullScreen = 1;
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
            WidthBox.IsEnabled = false;
            HeightBox.IsEnabled = false;
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
            // 管理者権限で再起動
            App app = (App)System.Windows.Application.Current;
            bool f = app.RunSelfAsAdmin();

            if (f)
            {
                Close();
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
        /// 数字以外のキー入力を無効化し、警告音を鳴らします
        /// </summary>
        private void KeyJudge(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key < Key.D0 || e.Key > Key.D9) && (e.Key < Key.NumPad0 || e.Key > Key.NumPad9) && e.Key != Key.Tab)
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }
    }

    /*  大量の関数をクラス分けして整理しようとした残骸
    internal class ControlRegistry
    {
        /// <summary>
        /// レジストリから現在の設定値を取得し、フォームに内容を反映します
        /// </summary>
        public void LoadKey()
        {
            MainWindow mainWindow = new();

            RegistryKey Load = Registry.CurrentUser.OpenSubKey(MainWindow.RegKey);

            // キーが見つからなかった場合は中断
            if (Load == null)
            {
                // ボタンを無効化
                mainWindow.startButton.IsEnabled = false;
                mainWindow.applyButton.IsEnabled = false;
                _ = System.Windows.MessageBox.Show("レジストリキーが見つかりませんでした。プリンセスコネクト！Re:Diveがインストールされていない可能性があります。", MainWindow.ThisName, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // ボタンを有効化
            mainWindow.startButton.IsEnabled = true;
            mainWindow.applyButton.IsEnabled = true;

            mainWindow.ChangeButton();

            // タイマーの開始
            if (!mainWindow.timer.Enabled)
            {
                mainWindow.timer.Start();
            }

            // レジストリから現在の設定を取得
            bool loadResult = true;
            try
            {
                mainWindow.intWidth = (int)Load.GetValue(MainWindow.intWidthKey);                 // 横解像度
                mainWindow.intHeight = (int)Load.GetValue(MainWindow.intHeightKey);               // 縦解像度
                mainWindow.fullScreen = (int)Load.GetValue(MainWindow.fullScreenKey);             // ウインドウまたはフルスクリーンモード
                mainWindow.allowNative = (int)Load.GetValue(MainWindow.allowNativeKey);           // ネイティブ
                mainWindow.chooseMonitor = (int)Load.GetValue(MainWindow.chooseMonitorKey);       // モニタ選択
            }
            catch (NullReferenceException)
            {
                loadResult = false;
            }
            Load.Close();
            mainWindow.WidthBox.Text = mainWindow.intWidth.ToString();
            mainWindow.HeightBox.Text = mainWindow.intHeight.ToString();

            // モード設定の反映
            switch (mainWindow.fullScreen)
            {
                // フルスクリーン
                case 1:
                    mainWindow.ModeW.IsChecked = false;
                    mainWindow.ModeF.IsChecked = true;

                    // Nativeチェックボックスを有効化
                    mainWindow.UseNative.IsEnabled = true;
                    break;
                // ウインドウ
                case 3:
                    mainWindow.ModeW.IsChecked = true;
                    mainWindow.ModeF.IsChecked = false;

                    // Nativeチェックボックスを無効化
                    mainWindow.UseNative.IsEnabled = false;
                    break;
                // 例外
                default:
                    loadResult = false;
                    break;
            }

            // ネイティブ設定の反映
            switch (mainWindow.allowNative)
            {
                case 0:
                    mainWindow.UseNative.IsChecked = false;
                    break;
                case 1:
                    mainWindow.UseNative.IsChecked = true;
                    break;

                // 例外
                default:
                    loadResult = false;
                    break;
            }

            // モニタ設定の反映
            if (mainWindow.chooseMonitor < Screen.AllScreens.Length)
            {
                mainWindow.selMonitor.SelectedIndex = mainWindow.chooseMonitor;
            }
            // 例外
            else
            {
                loadResult = false;
            }

            // 例外が発生した場合警告を表示
            if (!loadResult)
            {
                _ = System.Windows.MessageBox.Show("一部設定が正常に読み込まれませんでした", MainWindow.ThisName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
    */
}
