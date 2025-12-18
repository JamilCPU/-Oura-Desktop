using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaSidebar.intr;

namespace AvaloniaSidebar.impl;

public class WindowsScreenSpaceReserver : IScreenSpaceReserver
{
    private readonly Window _window;
    private IntPtr _handle;
    private bool _isRegistered;
    private const int ABM_NEW = 0x00000000;
    private const int ABM_REMOVE = 0x00000001;
    private const int ABM_QUERYPOS = 0x00000002;
    private const int ABM_SETPOS = 0x00000003;
    private const int ABM_GETSTATE = 0x00000004;
    private const int ABM_GETTASKBARPOS = 0x00000005;
    private const int ABM_ACTIVATE = 0x00000006;
    private const int ABM_GETAUTOHIDEBAR = 0x00000007;
    private const int ABM_SETAUTOHIDEBAR = 0x00000008;
    private const int ABM_WINDOWPOSCHANGED = 0x00000009;
    
    private const int ABE_LEFT = 0;
    private const int ABE_TOP = 1;
    private const int ABE_RIGHT = 2;
    private const int ABE_BOTTOM = 3;
    
    private const int WM_ACTIVATE = 0x0006;
    private const int WM_WINDOWPOSCHANGED = 0x0047;
    private const int WM_APPBAR_CALLBACK = 0x8000;

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public RECT rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("shell32.dll")]
    private static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public WindowsScreenSpaceReserver(Window window)
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

        var abd = new APPBARDATA
        {
            cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
            hWnd = _handle,
            uCallbackMessage = WM_APPBAR_CALLBACK,
            uEdge = ABE_RIGHT
        };

        uint result = SHAppBarMessage(ABM_NEW, ref abd);
        if (result != 0)
        {
            _isRegistered = true;
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (!_isRegistered || _handle == IntPtr.Zero)
            return;

        var primaryScreen = _window.Screens.Primary ?? _window.Screens.All[0];
        var screenBounds = primaryScreen.Bounds;
        
        const double sidebarWidth = 250;
        var desiredLeft = (int)(screenBounds.X + screenBounds.Width - sidebarWidth);
        var desiredTop = (int)screenBounds.Y;
        var desiredRight = (int)(screenBounds.X + screenBounds.Width);
        var desiredBottom = (int)(screenBounds.Y + screenBounds.Height);

        var abd = new APPBARDATA
        {
            cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
            hWnd = _handle,
            uEdge = ABE_RIGHT,
            rc = new RECT
            {
                left = desiredLeft,
                top = desiredTop,
                right = desiredRight,
                bottom = desiredBottom
            }
        };

        SHAppBarMessage(ABM_QUERYPOS, ref abd);

        if (abd.rc.right < desiredRight)
        {
            abd.rc.left = desiredLeft;
            abd.rc.right = desiredRight;
        }

        SHAppBarMessage(ABM_SETPOS, ref abd);

        SetWindowPos(_handle, IntPtr.Zero, abd.rc.left, abd.rc.top,
            abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top,
            SWP_SHOWWINDOW);
    }

    public void Unregister()
    {
        if (!_isRegistered || _handle == IntPtr.Zero)
            return;

        var abd = new APPBARDATA
        {
            cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
            hWnd = _handle
        };

        SHAppBarMessage(ABM_REMOVE, ref abd);
        _isRegistered = false;
    }

    public void Dispose()
    {
        Unregister();
    }
}
