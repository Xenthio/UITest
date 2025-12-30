namespace Sandbox.UI;

/// <summary>
/// Assertion utilities (stub for S&box compatibility)
/// </summary>
public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Assertion failed");
        }
    }
    
    public static void NotNull<T>(T? value, string? message = null) where T : class
    {
        if (value == null)
        {
            throw new InvalidOperationException(message ?? "Value must not be null");
        }
    }
}
