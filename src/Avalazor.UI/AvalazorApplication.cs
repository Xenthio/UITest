using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

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

    /// <summary>
    /// Run a Razor component as the root of the application
    /// </summary>
    public static async void RunComponent<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : IComponent
    {
        // Set up DI for Blazor components
        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        // Create renderer and render the component
        var renderer = new RazorRenderer(serviceProvider);
        var rootPanel = await renderer.RenderComponent<T>();

        Run(rootPanel, width, height, title);
    }
}
