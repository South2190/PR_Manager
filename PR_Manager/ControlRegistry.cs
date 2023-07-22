using Microsoft.Win32;
using System;
using System.Configuration;
using System.Windows;

namespace PR_Manager
{
    /// <summary>
    /// レジストリの読み書きを行うクラス
    /// </summary>
    internal class ControlRegistry
    {
        // レジストリキーのパス
        public string RegKey;
        // レジストリ値の名前
        public string WidthKey;              // 横解像度
        public string HeightKey;             // 縦解像度
        public string fullScreenKey;         // ウインドウまたはフルスクリーンモード
        public string allowNativeKey;        // ネイティブ
        public string chooseMonitorKey;      // モニタ選択

        private RegistryKey key;

        /// <summary>
        /// 変数に値が定義されているか確認します
        /// </summary>
        /// <exception cref="Exception">いずれかの変数に値が定義されていない場合、例外を発生させます</exception>
        private void CheckSetting()
        {
            if (RegKey == null || WidthKey == null || HeightKey == null || fullScreenKey == null || allowNativeKey == null || chooseMonitorKey == null)
            {
                throw new Exception("設定が完全に定義されていません");
            }
        }

        /// <summary>
        /// レジストリキーを開きます
        /// </summary>
        /// <returns>正常に開けた場合(true)、開けなかった場合(false)を返します</returns>
        private bool CheckReg()
        {
            key = Registry.CurrentUser.OpenSubKey(RegKey, true);

            if (key == null)
            {
                _ = MessageBox.Show("レジストリキーにアクセスできませんでした。ゲームがインストールされていない可能性があります。", MainWindow.ThisName, MessageBoxButton.OK, MessageBoxImage.Error);
                key.Close();
                return false;
            }

            return true;
        }

        /// <summary>
        /// レジストリの値を書き換えます
        /// 引数で指定されなかった値の書き換えは行いません
        /// </summary>
        public void WriteReg(int? Width = null, int? Height = null, int? fullScreen = null, int? allowNative = null, int? chooseMonitor = null)
        {
            CheckSetting();
            if (!CheckReg()) { return; }

            if (Width != null)              // 横解像度
            {
                key.SetValue(WidthKey, Width);
            }
            if (Height != null)             // 縦解像度
            {
                key.SetValue(HeightKey, Height);
            }
            if (fullScreen != null)         // ウインドウまたはフルスクリーンモード
            {
                key.SetValue(fullScreenKey, fullScreen);
            }
            if (allowNative != null)        // ネイティブ
            {
                key.SetValue(allowNativeKey, allowNative);
            }
            if (chooseMonitor != null)      // モニタ選択
            {
                key.SetValue(chooseMonitorKey, chooseMonitor);
            }
            key.Close();

            if (ConfigurationManager.AppSettings["ShowApplyMessage"] != "False")
            {
                _ = MessageBox.Show("レジストリを書き換えました。", MainWindow.ThisName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// レジストリの値を読み込みます
        /// 参照渡しで指定された引数にレジストリから読み込んだ値を反映します
        /// </summary>
        /// <returns>
        /// 正常に読み込めた場合もしくは別のエラーを関数内で表示した場合(true)、NullReferenceExceptionをキャッチした場合(false)
        /// </returns>
        public bool ReadReg(ref int Width, ref int Height, ref int fullScreen, ref int allowNative, ref int chooseMonitor)
        {
            CheckSetting();
            if (!CheckReg()) { return true; }

            try
            {
                Width           = (int)key.GetValue(WidthKey);              // 横解像度
                Height          = (int)key.GetValue(HeightKey);             // 縦解像度
                fullScreen      = (int)key.GetValue(fullScreenKey);         // ウインドウまたはフルスクリーンモード
                allowNative     = (int)key.GetValue(allowNativeKey);        // ネイティブ
                chooseMonitor   = (int)key.GetValue(chooseMonitorKey);      // モニタ選択
                key.Close();
            }
            catch (NullReferenceException)
            {
                return false;
            }

            return true;
        }
    }
}
