namespace Sandbox.UI;

/// <summary>
/// Simple logging class for compatibility with s&box code
/// </summary>
internal static class Log
{
	public static void Warning(string message)
	{
		Console.WriteLine($"[WARNING] {message}");
	}

	public static void Error(string message)
	{
		Console.Error.WriteLine($"[ERROR] {message}");
	}
}
