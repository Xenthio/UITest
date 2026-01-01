namespace Sandbox.UI;

/// <summary>
/// Logging utilities (stub for S&box compatibility)
/// </summary>
public static class Log
{
    public static void Warning(Exception ex, string message)
    {
        Console.WriteLine($"[WARNING] {message}: {ex}");
    }
    
    public static void Warning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }
    
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
    
    public static void Error(Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
}
