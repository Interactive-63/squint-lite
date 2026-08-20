using System;
using Avalonia.Media;

namespace SquintLite.Models;

public static class ContrastCalculator
{
    // Returns the WCAG 2.1 contrast ratio between two colours.
    // The ratio is always expressed as a value >= 1, with the lighter
    // colour as the numerator.
    public static double GetContrastRatio(Color foreground, Color background)
    {
        double l1 = GetRelativeLuminance(foreground);
        double l2 = GetRelativeLuminance(background);

        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);

        return (lighter + 0.05) / (darker + 0.05);
    }

    // Relative luminance per WCAG 2.1, section 1.4.3.
    // Coefficients reflect human eye sensitivity to each channel.
    private static double GetRelativeLuminance(Color color)
    {
        double r = ToLinear(color.R / 255.0);
        double g = ToLinear(color.G / 255.0);
        double b = ToLinear(color.B / 255.0);

        return (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
    }

    // Converts gamma-compressed sRGB component to a linear light value.
    private static double ToLinear(double sRgb)
    {
        return sRgb <= 0.04045
            ? sRgb / 12.92
            : Math.Pow((sRgb + 0.055) / 1.055, 2.4);
    }
}