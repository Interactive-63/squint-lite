using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;

namespace SquintLite.Services;

public sealed class EyedropperService : IDisposable
{
    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern uint GetPixel(IntPtr hdc, int x, int y);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData, flags, time;
        public IntPtr dwExtraInfo;
    }

    private IntPtr _hookHandle;
    private LowLevelMouseProc? _hookProc;
    private TaskCompletionSource<Color>? _tcs;
    private DispatcherTimer? _previewTimer;
    private Action<Color, int, int>? _hoverCallback;

    public static bool IsSupported => OperatingSystem.IsWindows();

    // Starts eyedropper pick mode. hoverCallback fires on the UI thread
    // approximately every 50ms with the colour and position under the cursor.
    public Task<Color> PickAsync(Action<Color, int, int>? hoverCallback = null)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Eyedropper is only supported on Windows.");

        _hoverCallback = hoverCallback;
        _tcs = new TaskCompletionSource<Color>();
        _hookProc = HookCallback;
        _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle(null), 0);

        if (hoverCallback != null)
        {
            _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _previewTimer.Tick += OnPreviewTick;
            _previewTimer.Start();
        }

        return _tcs.Task;
    }

    private void OnPreviewTick(object? sender, EventArgs e)
    {
        GetCursorPos(out POINT point);
        _hoverCallback?.Invoke(SamplePixel(point.X, point.Y), point.X, point.Y);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            var s = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            Color picked = SamplePixel(s.pt.X, s.pt.Y);

            Dispatcher.UIThread.Post(() =>
            {
                Cleanup();
                _tcs?.TrySetResult(picked);
            });
        }
        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    private static Color SamplePixel(int x, int y)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        try
        {
            uint pixel = GetPixel(hdc, x, y);
            return new Color(
                255,
                (byte)(pixel & 0xFF),
                (byte)((pixel >> 8) & 0xFF),
                (byte)((pixel >> 16) & 0xFF));
        }
        finally
        {
            ReleaseDC(IntPtr.Zero, hdc);
        }
    }

    private void Cleanup()
    {
        _previewTimer?.Stop();
        _previewTimer = null;
        _hoverCallback = null;

        if (_hookHandle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
        _hookProc = null;
    }

    public void Dispose() => Cleanup();
}