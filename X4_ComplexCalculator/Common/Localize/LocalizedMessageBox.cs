using System;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace X4_ComplexCalculator.Common.Localize;

/// <summary>
/// ローカライズ対応メッセージボックス
/// </summary>
public class LocalizedMessageBox
{
    /// <summary>
    /// フック処理
    /// </summary>
    private static HOOKPROC? _HookProcDelegate = null;

    /// <summary>
    /// フックプロシージャのハンドル
    /// </summary>
    private static HHOOK _hHook = default;

    /// <summary>
    /// メッセージボックスのタイトル文字列
    /// </summary>
    private static string? _Title = null;

    /// <summary>
    /// メッセージボックスの表示文字列
    /// </summary>
    private static string? _Msg = null;



    /// <summary>
    /// ローカライズされたメッセージボックスを表示
    /// </summary>
    /// <param name="messageBoxTextKey">表示文字列用キー</param>
    /// <param name="captionKey">タイトル部分用キー</param>
    /// <param name="button">ボタンのスタイル</param>
    /// <param name="icon">アイコンのスタイル</param>
    /// <param name="defaultResult">フォーカスするボタン</param>
    /// <param name="param">表示文字列用パラメータ</param>
    /// <returns>MessageBox.Showの戻り値</returns>
    public static MessageBoxResult Show(
        string              messageBoxTextKey, 
        string              captionKey      = "",
        MessageBoxButton    button          = MessageBoxButton.OK,
        MessageBoxImage     icon            = MessageBoxImage.None,
        MessageBoxResult    defaultResult   = MessageBoxResult.OK,
        params object[] param)
    {
        var format = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(messageBoxTextKey, null, null);

        _Msg = string.Format(format, param);
        _Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(captionKey, null, null); ;

        _HookProcDelegate = new HOOKPROC(HookCallback);

        _hHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_CBT, _HookProcDelegate, default(HINSTANCE), PInvoke.GetCurrentThreadId());

        var result = MessageBox.Show(_Msg, _Title, button, icon, defaultResult);

        UnHook();

        return result;
    }


    /// <summary>
    /// フック時のコールバック処理
    /// </summary>
    /// <param name="code"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private static LRESULT HookCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        var hHook = _hHook;

        if (code == Win32.HCBT_ACTIVATE)
        {
            var hWnd = new HWND((nint)wParam.Value);

            // ハンドルのクラス名がダイアログボックスの場合のみ処理
            if (Win32.GetClassName(hWnd) == "#32770")
            {
                // ダイアログボックスのタイトルとメッセージを取得
                var title = Win32.GetWindowText(hWnd);
                var msg = Win32.GetDlgItemText(hWnd, 0xFFFF);      // -1 = IDC_STATIC

                // タイトルとメッセージが一致した場合、ダイアログの位置を親ウィンドウの中央に移動する
                if (title.SequenceEqual(_Title) && msg.SequenceEqual(_Msg))
                {
                    MoveToCenterOnParent(hWnd);
                    UnHook();
                }
            }
        }

        return PInvoke.CallNextHookEx(hHook, code, wParam, lParam);
    }


    /// <summary>
    /// フック解除
    /// </summary>
    private static void UnHook()
    {
        PInvoke.UnhookWindowsHookEx(_hHook);
        _hHook = default;
        _Msg   = null;
        _Title = null;
        _HookProcDelegate = null;
    }


    /// <summary>
    /// 子ウィンドウを親ウィンドウの中央に移動する
    /// </summary>
    /// <param name="hChildWnd"></param>
    private static void MoveToCenterOnParent(HWND hChildWnd)
    {
        // 子ウィンドウの領域取得
        PInvoke.GetWindowRect(hChildWnd, out var childRect);
        int cxChild = childRect.right - childRect.left;
        int cyChild = childRect.bottom - childRect.top;

        // 親ウィンドウの領域取得
        PInvoke.GetWindowRect(PInvoke.GetParent(hChildWnd), out var parentRect);
        int cxParent = parentRect.right - parentRect.left;
        int cyParent = parentRect.bottom - parentRect.top;

        // 子ウィンドウを親ウィンドウの中央に移動
        int x = parentRect.left + (cxParent - cxChild) / 2;
        int y = parentRect.top + (cyParent - cyChild) / 2;
        PInvoke.SetWindowPos(
            hChildWnd,
            default,
            x,
            y,
            0,
            0,
            SET_WINDOW_POS_FLAGS.SWP_NOSIZE
                | SET_WINDOW_POS_FLAGS.SWP_NOZORDER
                | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
        );
    }
}
