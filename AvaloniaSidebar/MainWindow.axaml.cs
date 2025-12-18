using Avalonia.Controls;
using Avalonia;
using AvaloniaSidebar.intr;
using AvaloniaSidebar.fact;

namespace AvaloniaSidebar;

public partial class MainWindow : Window
{
    // Configurable sidebar width - adjust this value to change sidebar width
    private const double SidebarWidth = 250;
    private IScreenSpaceReserver? _spaceReserver;

    public MainWindow()
    {
        InitializeComponent();
        
        // Configure window properties
        WindowStartupLocation = WindowStartupLocation.Manual;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        
        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Opened(object? sender, System.EventArgs e)
    {
        PositionWindow();
        
        // Create platform-specific screen space reserver and register
        _spaceReserver = ScreenSpaceReserverFactory.Create(this);
        _spaceReserver.Register();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        // Unregister screen space reservation when window closes
        _spaceReserver?.Dispose();
    }

    private void PositionWindow()
    {
        // Get the primary screen
        var primaryScreen = Screens.Primary;
        if (primaryScreen == null)
        {
            // Fallback: use first available screen if primary is not available
            var screens = Screens.All;
            if (screens.Count == 0)
                return;
            primaryScreen = screens[0];
        }

        // Get screen bounds
        var screenBounds = primaryScreen.Bounds;
        
        // Calculate position: right side of screen
        var x = screenBounds.X + screenBounds.Width - SidebarWidth;
        var y = screenBounds.Y;
        
        // Set window position and size
        Position = new PixelPoint((int)x, (int)y);
        Width = SidebarWidth;
        Height = screenBounds.Height;
    }
}