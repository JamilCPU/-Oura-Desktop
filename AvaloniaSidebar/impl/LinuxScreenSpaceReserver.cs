using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaSidebar.intr;

namespace AvaloniaSidebar.impl;

public class LinuxScreenSpaceReserver : IScreenSpaceReserver
{
    private readonly Window _window;
    private IntPtr _handle;
    private IntPtr _display;
    private IntPtr _x11Window;
    private bool _isRegistered;
    
    private const string AtomNameStrutPartial = "_NET_WM_STRUT_PARTIAL";
    private const string AtomNameStrut = "_NET_WM_STRUT";
    
    private const int PropModeReplace = 0;
    private const int AnyPropertyType = 0;
    
    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);
    
    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);
    
    [DllImport("libX11.so.6")]
    private static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);
    
    [DllImport("libX11.so.6")]
    private static extern int XChangeProperty(
        IntPtr display,
        IntPtr window,
        IntPtr property,
        IntPtr type,
        int format,
        int mode,
        byte[] data,
        int nelements);
    
    [DllImport("libX11.so.6")]
    private static extern int XDeleteProperty(IntPtr display, IntPtr window, IntPtr property);
    
    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);
    
    public LinuxScreenSpaceReserver(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void Register()
    {
        if (_isRegistered)
            return;

        var platformHandle = _window.TryGetPlatformHandle();
        if (platformHandle == null)
        {
            throw new InvalidOperationException("Window platform handle is not available");
        }

        _handle = platformHandle.Handle;
        _x11Window = _handle;

        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to open X11 display");
        }

        _isRegistered = true;
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (!_isRegistered || _display == IntPtr.Zero || _x11Window == IntPtr.Zero)
            return;

        var primaryScreen = _window.Screens.Primary ?? _window.Screens.All[0];
        var screenBounds = primaryScreen.Bounds;
        
        const double sidebarWidth = 250;
        var rightStrut = (int)sidebarWidth;
        var startY = (int)screenBounds.Y;
        var endY = (int)(screenBounds.Y + screenBounds.Height);

        var atomStrutPartial = XInternAtom(_display, AtomNameStrutPartial, false);
        if (atomStrutPartial == IntPtr.Zero)
        {
            var atomStrut = XInternAtom(_display, AtomNameStrut, false);
            if (atomStrut != IntPtr.Zero)
            {
                var strutData = new uint[4];
                strutData[1] = (uint)rightStrut;
                
                var dataBytes = new byte[sizeof(uint) * 4];
                Buffer.BlockCopy(strutData, 0, dataBytes, 0, dataBytes.Length);
                
                XChangeProperty(_display, _x11Window, atomStrut,
                    XInternAtom(_display, "CARDINAL", false),
                    32, PropModeReplace, dataBytes, 4);
            }
        }
        else
        {
            var strutPartialData = new uint[12];
            strutPartialData[1] = (uint)rightStrut;
            strutPartialData[6] = (uint)startY;
            strutPartialData[7] = (uint)endY;
            
            var dataBytes = new byte[sizeof(uint) * 12];
            Buffer.BlockCopy(strutPartialData, 0, dataBytes, 0, dataBytes.Length);
            
            XChangeProperty(_display, _x11Window, atomStrutPartial,
                XInternAtom(_display, "CARDINAL", false),
                32, PropModeReplace, dataBytes, 12);
        }

        XFlush(_display);
    }

    public void Unregister()
    {
        if (!_isRegistered || _display == IntPtr.Zero || _x11Window == IntPtr.Zero)
            return;

        var atomStrutPartial = XInternAtom(_display, AtomNameStrutPartial, false);
        if (atomStrutPartial != IntPtr.Zero)
        {
            XDeleteProperty(_display, _x11Window, atomStrutPartial);
        }
        
        var atomStrut = XInternAtom(_display, AtomNameStrut, false);
        if (atomStrut != IntPtr.Zero)
        {
            XDeleteProperty(_display, _x11Window, atomStrut);
        }

        XFlush(_display);
        _isRegistered = false;
    }

    public void Dispose()
    {
        Unregister();
        
        if (_display != IntPtr.Zero)
        {
            XCloseDisplay(_display);
            _display = IntPtr.Zero;
        }
    }
}
