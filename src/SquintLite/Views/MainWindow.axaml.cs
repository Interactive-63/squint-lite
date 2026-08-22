using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using SquintLite.Services;
using SquintLite.ViewModels;

namespace SquintLite.Views;

public partial class MainWindow : Window
{
    private MainViewModel? _vm;
    private EventHandler? _fgEyedropperHandler;
    private EventHandler? _bgEyedropperHandler;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not MainViewModel vm) return;
        _vm = vm;

        _fgEyedropperHandler = (_, _) => RunEyedropper(isForeground: true);
        _bgEyedropperHandler = (_, _) => RunEyedropper(isForeground: false);

        vm.ForegroundEyedropperRequested += _fgEyedropperHandler;
        vm.BackgroundEyedropperRequested += _bgEyedropperHandler;

        ForegroundHexInput.TextChanged += OnHexTextChanged;
        BackgroundHexInput.TextChanged += OnHexTextChanged;
        ForegroundHexInput.LostFocus += OnHexLostFocus;
        BackgroundHexInput.LostFocus += OnHexLostFocus;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (_vm is not null)
        {
            _vm.ForegroundEyedropperRequested -= _fgEyedropperHandler;
            _vm.BackgroundEyedropperRequested -= _bgEyedropperHandler;
        }

        ForegroundHexInput.TextChanged -= OnHexTextChanged;
        BackgroundHexInput.TextChanged -= OnHexTextChanged;
        ForegroundHexInput.LostFocus -= OnHexLostFocus;
        BackgroundHexInput.LostFocus -= OnHexLostFocus;
    }

    // Handles the paste/type case where a full 6-char hex arrives without '#'.
    private static void OnHexTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox box) return;
        var text = box.Text ?? string.Empty;

        if (text.Length == 6 && !text.StartsWith('#'))
        {
            box.Text = "#" + text;
            box.CaretIndex = box.Text?.Length ?? 0;
        }
    }

    // Catches any remaining un-prefixed input when the field loses focus.
    private static void OnHexLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox box) return;
        var text = box.Text ?? string.Empty;

        if (text.Length > 0 && !text.StartsWith('#'))
            box.Text = "#" + text;
    }

    private async void RunEyedropper(bool isForeground)
    {
    #if WINDOWS
        try
        {
            WindowState = WindowState.Minimized;
            await System.Threading.Tasks.Task.Delay(300);

            var capture = ScreenCapture.CaptureFullScreen();
            if (capture is null) return;

            var picker = new EyedropperScreenWindow(capture);
            Color? picked = await picker.PickAsync();

            if (picked.HasValue && DataContext is MainViewModel vm)
                vm.ApplyPickedColor(picked.Value, isForeground);
        }
        finally
        {
            WindowState = WindowState.Normal;
            Activate();
        }
    #else
        await System.Threading.Tasks.Task.CompletedTask;
    #endif
    }
}