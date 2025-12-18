using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using AvaloniaSidebar.intr;
using AvaloniaSidebar.impl;

namespace AvaloniaSidebar;

/// <summary>
/// Factory class that creates the appropriate IScreenSpaceReserver implementation
/// based on the current operating system.
/// </summary>
public static class ScreenSpaceReserverFactory
{
    /// <summary>
    /// Creates a platform-specific implementation of IScreenSpaceReserver.
    /// </summary>
    /// <param name="window">The window that needs screen space reserved</param>
    /// <returns>An implementation of IScreenSpaceReserver for the current platform</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown if the current platform is not supported</exception>
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
