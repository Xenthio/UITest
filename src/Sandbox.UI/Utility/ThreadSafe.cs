namespace Sandbox.UI;

/// <summary>
/// Thread safety utilities (stub for S&box compatibility)
/// </summary>
public static class ThreadSafe
{
    public static void AssertIsMainThread()
    {
        // In a real implementation, this would check if we're on the main thread
        // For now, we'll just skip the check
    }
}
