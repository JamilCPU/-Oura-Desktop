using Avalonia;
using System;
using System.Runtime.InteropServices;

namespace AvaloniaSidebar;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    [STAThread]
    public static void Main(string[] args)
    {
        // Allocate console for debugging
        AllocConsole();
        Console.WriteLine("Application starting...");
        
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .LogToTextWriter(Console.Out);
}