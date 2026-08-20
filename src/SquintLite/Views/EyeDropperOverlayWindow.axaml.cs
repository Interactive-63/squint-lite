using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SquintLite.Views;

public partial class EyedropperOverlayWindow : Window
{
    // Makes the window transparent to mouse input so clicks pass through
    // to the content beneath it on screen.
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

    public EyedropperOverlayWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);

        if (!OperatingSystem.IsWindows()) return;

        var handle = TryGetPlatformHandle()?.Handle ?? System.IntPtr.Zero;
        if (handle == System.IntPtr.Zero) return;

        int style = GetWindowLong(handle, GWL_EXSTYLE);
        SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
    }

    // Called by the view on each hover tick to update the displayed colour
    // and reposition the overlay near the cursor.
    public void UpdateDisplay(Color color, int cursorX, int cursorY)
    {
        ColorSwatch.Background = new SolidColorBrush(color);
        HexLabel.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        Position = new PixelPoint(cursorX + 24, cursorY + 24);
    }
}