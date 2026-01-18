using System;

namespace AvaloniaSidebar.Utils;

public class Logger
{
    private readonly string _category;

    public Logger(string category)
    {
        _category = category ?? throw new ArgumentNullException(nameof(category));
    }

    public void Log(string message)
    {
        Console.WriteLine($"[{_category}] {message}");
    }

    public void LogError(string message, Exception? ex = null)
    {
        Console.WriteLine($"[{_category}] Error: {message}");
        if (ex != null)
        {
            Console.WriteLine($"[{_category}] Exception: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[{_category}] Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    public void LogProgress(string message)
    {
        Console.Write(message);
    }
}
