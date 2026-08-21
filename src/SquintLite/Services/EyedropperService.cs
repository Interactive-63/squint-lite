using System;

namespace SquintLite.Services;

public static class EyedropperService
{
    public static bool IsSupported => OperatingSystem.IsWindows();
}