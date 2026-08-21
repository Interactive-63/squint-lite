using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SquintLite.Services;
using SquintLite.ViewModels;

namespace SquintLite.Views;

public partial class MainWindow : Window
{
    private MainViewModel? _vm;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is not MainViewModel vm) return;
        _vm = vm;

        vm.ForegroundEyedropperRequested += (_, _) => RunEyedropper(isForeground: true);
        vm.BackgroundEyedropperRequested += (_, _) => RunEyedropper(isForeground: false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (_vm is null) return;
        _vm.ForegroundEyedropperRequested -= (_, _) => RunEyedropper(isForeground: true);
        _vm.BackgroundEyedropperRequested -= (_, _) => RunEyedropper(isForeground: false);
    }

    private async void RunEyedropper(bool isForeground)
    {
        try
        {
            WindowState = WindowState.Minimized;

            // Allow the minimise animation to complete before capturing the screen.
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
    }
}