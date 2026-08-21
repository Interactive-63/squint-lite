using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SquintLite.Models;
using SquintLite.Services;

namespace SquintLite.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public event EventHandler? ForegroundEyedropperRequested;
    public event EventHandler? BackgroundEyedropperRequested;

    private static readonly SolidColorBrush PassBrush = new(Color.Parse("#2E7D32"));
    private static readonly SolidColorBrush FailBrush = new(Color.Parse("#C62828"));
    private static readonly SolidColorBrush WarningBrush = new(Color.Parse("#E65100"));
    private static readonly SolidColorBrush PendingBrush = new(Color.Parse("#4A4A4A"));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ForegroundBrush))]
    [NotifyPropertyChangedFor(nameof(ForegroundColor))]
    [NotifyPropertyChangedFor(nameof(ContrastRatio))]
    [NotifyPropertyChangedFor(nameof(ContrastRatioDisplay))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(GraphicalAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAABadge))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(GraphicalAABadge))]
    [NotifyPropertyChangedFor(nameof(RatioDisplayBrush))]
    public partial string ForegroundHex { get; set; } = "#000000";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ForegroundBrush))]
    [NotifyPropertyChangedFor(nameof(ForegroundColor))]
    [NotifyPropertyChangedFor(nameof(ContrastRatio))]
    [NotifyPropertyChangedFor(nameof(ContrastRatioDisplay))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(GraphicalAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAABadge))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(GraphicalAABadge))]
    [NotifyPropertyChangedFor(nameof(RatioDisplayBrush))]
    public partial double ForegroundAlpha { get; set; } = 1.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    [NotifyPropertyChangedFor(nameof(ContrastRatio))]
    [NotifyPropertyChangedFor(nameof(ContrastRatioDisplay))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAResult))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAAResult))]
    [NotifyPropertyChangedFor(nameof(GraphicalAAResult))]
    [NotifyPropertyChangedFor(nameof(NormalTextAABadge))]
    [NotifyPropertyChangedFor(nameof(NormalTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAABadge))]
    [NotifyPropertyChangedFor(nameof(LargeTextAAABadge))]
    [NotifyPropertyChangedFor(nameof(GraphicalAABadge))]
    [NotifyPropertyChangedFor(nameof(RatioDisplayBrush))]
    public partial string BackgroundHex { get; set; } = "#FFFFFF";

    public Color ForegroundColor
    {
        get
        {
            try
            {
                Color c = Color.Parse(ForegroundHex);
                return new Color((byte)Math.Round(ForegroundAlpha * 255), c.R, c.G, c.B);
            }
            catch { return Colors.Black; }
        }
        set
        {
            string hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            double alpha = value.A / 255.0;
            if (ForegroundHex != hex) ForegroundHex = hex;
            if (Math.Abs(ForegroundAlpha - alpha) > 0.001) ForegroundAlpha = alpha;
        }
    }

    public Color BackgroundColor
    {
        get
        {
            try { return Color.Parse(BackgroundHex); }
            catch { return Colors.White; }
        }
        set
        {
            string hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            if (BackgroundHex != hex) BackgroundHex = hex;
        }
    }

    public SolidColorBrush ForegroundBrush => new SolidColorBrush(ForegroundColor);
    public SolidColorBrush BackgroundBrush => ParseOpaqueBrush(BackgroundHex, Colors.White);

    public double ContrastRatio => ComputeContrastRatio();

    public string ContrastRatioDisplay =>
        ContrastRatio >= 0 ? $"{ContrastRatio:F2}:1" : "-";

    // Text result strings
    public string NormalTextAAResult => GetResult(ContrastRatio, 4.5);
    public string NormalTextAAAResult => GetResult(ContrastRatio, 7.0);
    public string LargeTextAAResult => GetResult(ContrastRatio, 3.0);
    public string LargeTextAAAResult => GetResult(ContrastRatio, 4.5);
    public string GraphicalAAResult => GetResult(ContrastRatio, 3.0);

    // Badge background brushes
    public SolidColorBrush NormalTextAABadge => GetBadgeBrush(ContrastRatio, 4.5);
    public SolidColorBrush NormalTextAAABadge => GetBadgeBrush(ContrastRatio, 7.0);
    public SolidColorBrush LargeTextAABadge => GetBadgeBrush(ContrastRatio, 3.0);
    public SolidColorBrush LargeTextAAABadge => GetBadgeBrush(ContrastRatio, 4.5);
    public SolidColorBrush GraphicalAABadge => GetBadgeBrush(ContrastRatio, 3.0);

    // Ratio box border: green for AAA-level pass, amber for AA-only, red for fail.
    public SolidColorBrush RatioDisplayBrush =>
        ContrastRatio < 0 ? PendingBrush :
        ContrastRatio >= 7.0 ? PassBrush :
        ContrastRatio >= 4.5 ? WarningBrush :
                               FailBrush;

    [RelayCommand]
    private void RequestForegroundEyedropper()
    {
        if (EyedropperService.IsSupported)
            ForegroundEyedropperRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void RequestBackgroundEyedropper()
    {
        if (EyedropperService.IsSupported)
            BackgroundEyedropperRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyPickedColor(Color color, bool isForeground)
    {
        if (isForeground) ForegroundColor = color;
        else BackgroundColor = color;
    }

    private static string GetResult(double ratio, double threshold) =>
        ratio < 0 ? "-" : ratio >= threshold ? "Pass" : "Fail";

    private static SolidColorBrush GetBadgeBrush(double ratio, double threshold) =>
        ratio < 0 ? PendingBrush : ratio >= threshold ? PassBrush : FailBrush;

    private double ComputeContrastRatio()
    {
        try
        {
            Color fg = ForegroundColor;
            Color bg = Color.Parse(BackgroundHex);
            Color eff = BlendForegroundOnBackground(fg, bg);
            return ContrastCalculator.GetContrastRatio(eff, bg);
        }
        catch { return -1; }
    }

    private static Color BlendForegroundOnBackground(Color foreground, Color background)
    {
        if (foreground.A == 255) return foreground;
        double a = foreground.A / 255.0;
        return new Color(
            255,
            (byte)Math.Round(a * foreground.R + (1 - a) * background.R),
            (byte)Math.Round(a * foreground.G + (1 - a) * background.G),
            (byte)Math.Round(a * foreground.B + (1 - a) * background.B));
    }

    private static SolidColorBrush ParseOpaqueBrush(string hex, Color fallback)
    {
        try
        {
            Color c = Color.Parse(hex);
            return new SolidColorBrush(new Color(255, c.R, c.G, c.B));
        }
        catch { return new SolidColorBrush(fallback); }
    }
}