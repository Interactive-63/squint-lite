using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SquintLite.Services;

namespace SquintLite.Views;

public partial class EyedropperScreenWindow : Window
{
    private readonly ScreenCaptureData _capture;
    private readonly TaskCompletionSource<Color?> _tcs = new();
    private double _scaling = 1.0;

    public EyedropperScreenWindow(ScreenCaptureData capture)
    {
        _capture = capture;
        InitializeComponent();
        ScreenImage.Source = capture.Bitmap;
    }

    public Task<Color?> PickAsync()
    {
        Show();
        Focus();
        return _tcs.Task;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _scaling = RenderScaling;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var pos = e.GetPosition(ScreenImage);
        var color = SampleAt(pos);

        PreviewSwatch.Background = new SolidColorBrush(color);
        PreviewHex.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        // Offset the preview label so it does not sit directly under the cursor.
        CursorPreview.Margin = new Thickness(pos.X + 20, pos.Y + 20, 0, 0);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _tcs.TrySetResult(SampleAt(e.GetPosition(ScreenImage)));
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key != Key.Escape) return;
        _tcs.TrySetResult(null);
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _tcs.TrySetResult(null);
    }

    // Converts a logical pointer position to physical bitmap coordinates
    // using the current DPI scale, then samples the captured pixel.
    private Color SampleAt(Point logical) =>
        _capture.GetPixelAt((int)(logical.X * _scaling), (int)(logical.Y * _scaling));
}