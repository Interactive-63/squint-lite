using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SquintLite.Services;

public sealed class ScreenCaptureData
{
    public WriteableBitmap Bitmap { get; }

    private readonly byte[] _pixels;
    private readonly int _width;
    private readonly int _height;

    internal ScreenCaptureData(WriteableBitmap bitmap, byte[] pixels, int width, int height)
    {
        Bitmap = bitmap;
        _pixels = pixels;
        _width = width;
        _height = height;
    }

    // Returns the colour of the pixel at physical screen coordinates.
    // Coordinates are clamped to bitmap bounds.
    public Color GetPixelAt(int x, int y)
    {
        x = Math.Clamp(x, 0, _width - 1);
        y = Math.Clamp(y, 0, _height - 1);
        int i = (y * _width + x) * 4;
        return new Color(255, _pixels[i + 2], _pixels[i + 1], _pixels[i]);
    }
}