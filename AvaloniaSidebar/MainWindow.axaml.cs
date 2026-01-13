using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using AvaloniaSidebar.intr;
using AvaloniaSidebar.fact;
using AvaloniaSidebar.ViewModels;
using AvaloniaSidebar.Services;
using backend.api.auth.intr;
using backend.api.auth.impl;
using backend.api.oura.intr;
using backend.api.oura.impl;

namespace AvaloniaSidebar;

public partial class MainWindow : Window
{
    private const double SidebarWidth = 250;
    private IScreenSpaceReserver? _spaceReserver;
    private IOAuthService? _oauthService;
    private MainWindowViewModel? _viewModel;
    private AdvisorService? _advisorService;

    public MainWindow()
    {
        InitializeComponent();
        
        WindowStartupLocation = WindowStartupLocation.Manual;
        CanResize = false;
        SystemDecorations = SystemDecorations.None;
        
        Opened += MainWindow_Opened;
        Closing += MainWindow_Closing;
        PropertyChanged += MainWindow_PropertyChanged;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        PositionWindow();
        
        _spaceReserver = ScreenSpaceReserverFactory.Create(this);
        _spaceReserver.Register();
        
        await InitializeOAuthAsync();
    }
    
    private async Task InitializeOAuthAsync()
    {
        try
        {
            var configProvider = new OAuthConfigProvider();
            var tokenStore = new TokenStore();
            _oauthService = new OAuthService(configProvider, tokenStore);
            
            if (!await _oauthService.HasValidTokensAsync())
            {
                await ShowAuthorizationDialogAsync();
            }
            
            InitializeViewModel();
        }
        catch (InvalidOperationException ex)
        {
            await ShowErrorMessageAsync($"OAuth Configuration Error", ex.Message);
        }
        catch (Exception ex)
        {
            await ShowErrorMessageAsync("OAuth Error", $"An error occurred during OAuth initialization: {ex.Message}");
        }
    }
    
    private async Task ShowAuthorizationDialogAsync()
    {
        var dialog = new Window
        {
            Title = "OAuth Authorization Required",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var cancelButton = new Button 
        { 
            Content = "Cancel", 
            Margin = new Thickness(0, 0, 10, 0) 
        };
        cancelButton.Click += (s, e) => dialog.Close(false);
        
        var authorizeButton = new Button { Content = "Authorize" };
        authorizeButton.Click += async (s, e) =>
        {
            dialog.Close(true);
            if (_oauthService != null)
            {
                try
                {
                    await _oauthService.StartAuthorizationFlowAsync();
                    await ShowSuccessMessageAsync("Authorization successful! Your Oura data is now accessible.");
                }
                catch (Exception ex)
                {
                    await ShowErrorMessageAsync("Authorization Failed", $"Failed to complete authorization: {ex.Message}");
                }
            }
        };
        
        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Children =
            {
                new TextBlock
                {
                    Text = "You need to authorize this application to access your Oura data. A browser window will open for authorization.",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 20)
                },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 10,
                    Children = { cancelButton, authorizeButton }
                }
            }
        };
        
        await dialog.ShowDialog<bool>(this);
    }
    
    private async Task ShowSuccessMessageAsync(string message)
    {
        var dialog = new Window
        {
            Title = "Success",
            Width = 350,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Width = 80
        };
        okButton.Click += (s, e) => dialog.Close();
        
        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 20)
                },
                okButton
            }
        };
        
        await dialog.ShowDialog(this);
    }
    
    private async Task ShowErrorMessageAsync(string title, string message)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Width = 80
        };
        okButton.Click += (s, e) => dialog.Close();
        
        dialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 20)
                },
                okButton
            }
        };
        
        await dialog.ShowDialog(this);
    }
    
    private async void InitializeViewModel()
    {
        if (_oauthService == null)
            return;
            
        var ouraClient = new OuraClient(_oauthService);
        var ouraService = new OuraService(ouraClient);
        
        _viewModel = new MainWindowViewModel(ouraService);
        DataContext = _viewModel;
        
        // Initialize advisor service
        try
        {
            var llmConfigProvider = new backend.api.llm.impl.LlmConfigProvider();
            var llmService = new LocalLlmService(llmConfigProvider);
            var mcpClientService = new McpClientService();
            _advisorService = new AdvisorService(llmService, mcpClientService);
            
            // Initialize asynchronously in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _advisorService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing advisor service: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating advisor service: {ex.Message}");
        }
        
        _ = _viewModel.LoadAllDataAsync();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        _spaceReserver?.Dispose();
        _viewModel?.Dispose();
    }
    
    private void MainWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty && _spaceReserver != null)
        {
            var newState = (WindowState)e.NewValue!;
            
            if (newState == WindowState.Minimized)
            {
                // Unreserve screen space when minimized
                _spaceReserver.Unregister();
            }
            else if (newState == WindowState.Normal)
            {
                // Ensure window width is maintained
                Width = SidebarWidth;
                // Reserve screen space when restored
                _spaceReserver.Register();
                // Update position in case screen configuration changed
                PositionWindow();
            }
        }
    }
    
    private void MinimizeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
    
    private void SendButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var textBox = this.FindControl<TextBox>("AdvisorInput");
        ProcessAdvisorInput(textBox);
    }
    
    private void AdvisorInput_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            var textBox = sender as TextBox;
            ProcessAdvisorInput(textBox);
            e.Handled = true;
        }
    }
    
    private async void ProcessAdvisorInput(TextBox? textBox)
    {
        if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            var inputText = textBox.Text;
            textBox.Text = string.Empty; // Clear the input after sending
            
            if (_viewModel != null && _advisorService != null)
            {
                await _viewModel.ProcessAdvisorQueryAsync(inputText, _advisorService);
            }
            else
            {
                Console.WriteLine("Advisor service not initialized yet. Please wait...");
            }
        }
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
        MinWidth = SidebarWidth;
        MaxWidth = SidebarWidth;
        Height = screenBounds.Height;
    }
}