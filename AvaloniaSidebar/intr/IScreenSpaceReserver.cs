using System;

namespace AvaloniaSidebar.intr;

/// <summary>
/// Interface for reserving screen space to prevent other windows from overlapping the sidebar.
/// Different platforms implement this using their native APIs (Windows AppBar, Linux EWMH, etc.)
/// </summary>
public interface IScreenSpaceReserver : IDisposable
{
    void Register();
    void Unregister();
    void UpdatePosition();
}
