using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SquintLite.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ForegroundBrush))]
    public partial string ForegroundHex { get; set; } = "#000000";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundBrush))]
    public partial string BackgroundHex { get; set; } = "#FFFFFF";

    // Computed brushes derived from the hex inputs; used by the view for
    // swatch fill and preview text colour. Falls back to black or white
    // if the hex string is invalid during live input.
    public SolidColorBrush ForegroundBrush => ParseBrush(ForegroundHex, Colors.Black);
    public SolidColorBrush BackgroundBrush => ParseBrush(BackgroundHex, Colors.White);

    private static SolidColorBrush ParseBrush(string hex, Color fallback)
    {
        try
        {
            return new SolidColorBrush(Color.Parse(hex));
        }
        catch (Exception)
        {
            return new SolidColorBrush(fallback);
        }
    }
}