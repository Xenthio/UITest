using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.UI;

namespace Avalazor.UI;

/// <summary>
/// Main application entry point for Avalazor apps
/// Simplified API similar to s&box's application model
/// </summary>
public static class AvalazorApplication
{
    public static void Run(RootPanel rootPanel, int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        using var window = new AvalazorWindow(width, height, title);
        window.RootPanel = rootPanel;
        window.Run();
    }

    /// <summary>
    /// Run a Panel-derived Razor component as the root of the application.
    /// This properly processes the Razor render tree to create child panels.
    /// </summary>
    public static void RunPanel<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : Panel, new()
    {
        try
        {
            // Set up DI for Blazor components
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            // Create the panel and use RazorRenderer to build its render tree
            var panel = new T();
            var renderer = new PanelRazorRenderer(serviceProvider);
            renderer.BuildPanelRenderTree(panel);
            
            // Wrap in RootPanel
            var rootPanel = new RootPanel();
            rootPanel.AddChild(panel);

            Console.WriteLine($"Root panel created: {rootPanel != null}");
            Console.WriteLine($"Component type: {panel.GetType().Name}");
            Console.WriteLine($"Root panel children: {rootPanel.ChildrenCount}");
            
            PrintPanelTree(rootPanel, 0);

            Run(rootPanel, width, height, title);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RunPanel: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Run a Blazor IComponent as the root of the application (legacy support).
    /// For new code, prefer RunPanel with Panel-derived Razor components.
    /// </summary>
    public static void RunComponent<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : IComponent
    {
        try
        {
            // Set up DI for Blazor components
            var services = new ServiceCollection();
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            // Create renderer and render the component directly
            var renderer = new RazorRenderer(serviceProvider);
            var rootPanel = renderer.RenderComponent<T>().GetAwaiter().GetResult();

            Console.WriteLine($"Root panel created: {rootPanel != null}");
            Console.WriteLine($"Root panel type: {rootPanel?.GetType().Name}");
            Console.WriteLine($"Root panel children: {rootPanel?.ChildrenCount ?? 0}");
            
            if (rootPanel != null)
            {
                PrintPanelTree(rootPanel, 0);
            }

            if (rootPanel == null)
            {
                throw new InvalidOperationException("Failed to create root panel from component");
            }

            Run(rootPanel, width, height, title);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RunComponent: {ex}");
            throw;
        }
    }
    
    private static void PrintPanelTree(Panel panel, int depth)
    {
        var indent = new string(' ', depth * 2);
        var text = panel is Label label ? $" Text='{label.Text}'" : "";
        Console.WriteLine($"{indent}{panel.GetType().Name} (ElementName={panel.ElementName}, Children={panel.ChildrenCount}){text}");
        foreach (var child in panel.Children)
        {
            PrintPanelTree(child, depth + 1);
        }
    }
}
