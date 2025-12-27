namespace Avalazor.UI;

/// <summary>
/// Main application entry point for Avalazor apps
/// Simplified API similar to s&box's application model
/// </summary>
public static class AvalazorApplication
{
    public static void Run(Panel rootPanel, int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        using var window = new AvalazorWindow(width, height, title);
        window.RootPanel = rootPanel;
        window.Run();
    }

    public static void Run<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : Panel, new()
    {
        Run(new T(), width, height, title);
    }
}
