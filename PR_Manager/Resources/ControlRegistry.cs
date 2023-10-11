using Microsoft.Win32;
using PR_Manager.Resources;
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
        //private RegistryKey key;

        /// <summary>
        /// レジストリキーを開きます
        /// </summary>
        /// <param name="CheckOnly">レジストリキーのチェックのみを行う場合(true)を指定します。</param>
        /// <returns>正常に開けた場合(true)、開けなかった場合(false)を返します</returns>
        public static bool CheckReg(out RegistryKey key, bool CheckOnly = false)
        {
            key = Registry.CurrentUser.OpenSubKey(InternalSettings.RegKey, true);

            if (key == null)
            {
                _ = MessageBox.Show("レジストリキーにアクセスできませんでした。ゲームがインストールされていない可能性があります。", InternalSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (CheckOnly)
            {
                key.Close();
            }

            return true;
        }

        /// <summary>
        /// レジストリの値を書き換えます
        /// 引数で指定されなかった値の書き換えは行いません
        /// </summary>
        public static void WriteReg(int? Width = null, int? Height = null, int? fullScreen = null, int? allowNative = null, int? chooseMonitor = null)
        {
            if (!CheckReg(out RegistryKey key)) { return; }

            if (Width != null)              // 横解像度
            {
                key.SetValue(InternalSettings.WidthKey, Width);
            }
            if (Height != null)             // 縦解像度
            {
                key.SetValue(InternalSettings.HeightKey, Height);
            }
            if (fullScreen != null)         // ウインドウまたはフルスクリーンモード
            {
                key.SetValue(InternalSettings.fullScreenKey, fullScreen);
            }
            if (allowNative != null)        // ネイティブ
            {
                key.SetValue(InternalSettings.allowNativeKey, allowNative);
            }
            if (chooseMonitor != null)      // モニタ選択
            {
                key.SetValue(InternalSettings.chooseMonitorKey, chooseMonitor);
            }
            key.Close();

            if (ConfigurationManager.AppSettings["ShowApplyMessage"] != "False")
            {
                _ = MessageBox.Show("レジストリを書き換えました。", InternalSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// レジストリの値を読み込みます
        /// 参照渡しで指定された引数にレジストリから読み込んだ値を反映します
        /// </summary>
        /// <returns>
        /// 正常に読み込めた場合もしくは別のエラーを関数内で表示した場合(true)、NullReferenceExceptionをキャッチした場合(false)
        /// </returns>
        public static bool ReadReg(ref int Width, ref int Height, ref int fullScreen, ref int allowNative, ref int chooseMonitor)
        {
            if (!CheckReg(out RegistryKey key)) { return true; }

            try
            {
                Width           = (int)key.GetValue(InternalSettings.WidthKey);              // 横解像度
                Height          = (int)key.GetValue(InternalSettings.HeightKey);             // 縦解像度
                fullScreen      = (int)key.GetValue(InternalSettings.fullScreenKey);         // ウインドウまたはフルスクリーンモード
                allowNative     = (int)key.GetValue(InternalSettings.allowNativeKey);        // ネイティブ
                chooseMonitor   = (int)key.GetValue(InternalSettings.chooseMonitorKey);      // モニタ選択
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
