using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SquintLite.Services;

public static class ScreenCapture
{
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;
    private const uint SRCCOPY = 0x00CC0020;

    [DllImport("user32.dll")] static extern int GetSystemMetrics(int n);
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int w, int h);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr h);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern bool BitBlt(
        IntPtr dest, int dx, int dy, int w, int h,
        IntPtr src, int sx, int sy, uint rop);

    [DllImport("gdi32.dll")]
    static extern int GetDIBits(
        IntPtr hdc, IntPtr hbmp, uint start, uint lines,
        byte[]? bits, ref BITMAPINFOHEADER bmi, uint usage);

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize, biWidth, biHeight;
        public short biPlanes, biBitCount;
        public int biCompression, biSizeImage;
        public int biXPelsPerMeter, biYPelsPerMeter;
        public int biClrUsed, biClrImportant;
    }

    // Captures the primary display as a WriteableBitmap.
    // Returns null on non-Windows platforms.
    public static ScreenCaptureData? CaptureFullScreen()
    {
        if (!OperatingSystem.IsWindows()) return null;

        int w = GetSystemMetrics(SM_CXSCREEN);
        int h = GetSystemMetrics(SM_CYSCREEN);

        IntPtr hdcScreen = GetDC(IntPtr.Zero);
        IntPtr hdcMem = CreateCompatibleDC(hdcScreen);
        IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, w, h);
        IntPtr hOld = SelectObject(hdcMem, hBitmap);

        BitBlt(hdcMem, 0, 0, w, h, hdcScreen, 0, 0, SRCCOPY);

        var header = new BITMAPINFOHEADER
        {
            biSize = Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth = w,
            biHeight = -h,
            biPlanes = 1,
            biBitCount = 32,
            biCompression = 0
        };

        var pixels = new byte[w * h * 4];
        GetDIBits(hdcMem, hBitmap, 0, (uint)h, pixels, ref header, 0);

        // GDI leaves the alpha byte as 0; Avalonia requires 255 for opaque pixels.
        for (int i = 3; i < pixels.Length; i += 4)
            pixels[i] = 255;

        var bitmap = new WriteableBitmap(
            new PixelSize(w, h),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        using (var fb = bitmap.Lock())
            Marshal.Copy(pixels, 0, fb.Address, pixels.Length);

        SelectObject(hdcMem, hOld);
        DeleteObject(hBitmap);
        DeleteDC(hdcMem);
        ReleaseDC(IntPtr.Zero, hdcScreen);

        return new ScreenCaptureData(bitmap, pixels, w, h);
    }
}