using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using AvaloniaSidebar.intr;
using AvaloniaSidebar.impl;

namespace AvaloniaSidebar.fact;

public static class ScreenSpaceReserverFactory
{
    public static IScreenSpaceReserver Create(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsScreenSpaceReserver(window);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxScreenSpaceReserver(window);
        }
        else
        {
            throw new PlatformNotSupportedException(
                $"Screen space reservation is not supported on {RuntimeInformation.OSDescription}. " +
                "Currently supported platforms: Windows, Linux");
        }
    }
}
