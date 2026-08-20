using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using SquintLite.ViewModels;

namespace SquintLite.Views;

public partial class MainWindow : Window
{
    private EyedropperOverlayWindow? _overlay;
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

        vm.EyedropperStarted += OnEyedropperStarted;
        vm.EyedropperCompleted += OnEyedropperCompleted;
        vm.HoverColorChanged += OnHoverColorChanged;
    }

    private void OnEyedropperStarted(object? sender, System.EventArgs e)
    {
        _overlay = new EyedropperOverlayWindow();
        _overlay.Show();
        WindowState = WindowState.Minimized;
    }

    private void OnHoverColorChanged(object? sender, (Color color, int x, int y) data)
    {
        _overlay?.UpdateDisplay(data.color, data.x, data.y);
    }

    private void OnEyedropperCompleted(object? sender, System.EventArgs e)
    {
        _overlay?.Close();
        _overlay = null;
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (_vm is null) return;
        _vm.EyedropperStarted -= OnEyedropperStarted;
        _vm.EyedropperCompleted -= OnEyedropperCompleted;
        _vm.HoverColorChanged -= OnHoverColorChanged;
    }
}