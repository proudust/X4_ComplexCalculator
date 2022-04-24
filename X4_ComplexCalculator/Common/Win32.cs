using System;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace X4_ComplexCalculator.Common;

/// <summary>
/// Win32API用ラッパクラス
/// </summary>
public class Win32
{
    #region 定数
    public const int HCBT_ACTIVATE = 5;
    #endregion

    #region 処理簡略化のためのメソッド
    /// <summary>
    /// 指定したハンドルのクラス名を取得
    /// </summary>
    /// <param name="hWnd">対象ハンドル</param>
    /// <returns>クラス名</returns>
    internal static unsafe ReadOnlySpan<char> GetClassName(HWND hWnd)
    {
        const int nMaxCount = 128;
        fixed (char* buffer = stackalloc char[nMaxCount])
        {
            PWSTR className = buffer;
            _ = PInvoke.GetClassName(hWnd, className, nMaxCount);
            return className.AsSpan();
        }
    }


    /// <summary>
    /// 指定したウィンドウハンドルのタイトル文字列を取得s
    /// </summary>
    /// <param name="hWnd">対象ウィンドウハンドル</param>
    /// <returns>タイトル文字列</returns>
    internal static unsafe ReadOnlySpan<char> GetWindowText(HWND hWnd)
    {
        int nMaxCount = PInvoke.GetWindowTextLength(hWnd) + 1;
        fixed (char* buffer = stackalloc char[nMaxCount])
        {
            PWSTR windowText = buffer;
            _ = PInvoke.GetWindowText(hWnd, windowText, nMaxCount);
            return windowText.AsSpan();
        }
    }


    /// <summary>
    /// 指定したダイアログの指定したコントロールIDの文字列を取得する
    /// </summary>
    /// <param name="hDlg">対象ダイアログハンドル</param>
    /// <param name="nIDDlgItem">対象コントロールID</param>
    /// <returns>コントロールの文字列</returns>
    internal static unsafe ReadOnlySpan<char> GetDlgItemText(HWND hDlg, int nIDDlgItem)
    {
        var hItem = PInvoke.GetDlgItem(hDlg, nIDDlgItem);
        if (hItem == IntPtr.Zero)
        {
            return "";
        }

        int nMaxCount = PInvoke.GetWindowTextLength(hItem) + 1;
        fixed (char* buffer = stackalloc char[nMaxCount])
        {
            PWSTR dlgItemText = buffer;
            _ = PInvoke.GetWindowText(hItem, dlgItemText, nMaxCount);
            return dlgItemText.AsSpan();
        }
    }
    #endregion
}
