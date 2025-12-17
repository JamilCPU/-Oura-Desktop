using Avalonia.Controls;
using Avalonia;

namespace AvaloniaSidebar;

public partial class MainWindow : Window
{
    // Configurable sidebar width - adjust this value to change sidebar width
    private const double SidebarWidth = 350;

    public MainWindow()
    {
        InitializeComponent();
        
        // Configure window properties
        WindowStartupLocation = WindowStartupLocation.Manual;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        
        // Position window when it opens
        Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object? sender, System.EventArgs e)
    {
        PositionWindow();
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