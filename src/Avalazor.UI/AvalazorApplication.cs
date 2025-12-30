using Microsoft.AspNetCore.Components;
using Sandbox.UI;
using Sandbox.UI.Reflection;

namespace Avalazor.UI;

/// <summary>
/// Main application entry point for Avalazor apps
/// Simplified API similar to s&box's application model
/// </summary>
public static class AvalazorApplication
{
    public static void Run(RootPanel rootPanel, int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        using var window = new NativeWindow(width, height, title);
        window.RootPanel = rootPanel;
        window.Run();
    }

    /// <summary>
    /// Run a Panel-derived Razor component as the root of the application.
    /// This properly processes the Razor render tree to create child panels.
    /// If the panel is a Window, extracts window properties for native window creation.
    /// </summary>
    public static void RunPanel<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : Panel, new()
    {
        try
        {
            // Initialize PanelFactory to register all [Library] and [Alias] types
            PanelFactory.Initialize();

            // Create the panel and build its render tree
            var panel = new T();
            Sandbox.UI.Razor.RazorTreeProcessor.BuildPanelRenderTree(panel);
            
            // If the panel is a Window, extract window properties
            if (panel is Sandbox.UI.Window window)
            {
                // Use window properties for native window configuration
                width = window.WindowWidth > 0 ? window.WindowWidth : width;
                height = window.WindowHeight > 0 ? window.WindowHeight : height;
                title = !string.IsNullOrEmpty(window.Title) ? window.Title : title;
                
                Console.WriteLine($"Creating native window from Window properties: {width}x{height}, Title: '{title}'");
            }
            
            // Wrap in RootPanel
            var rootPanel = new RootPanel();
            rootPanel.AddChild(panel);

            Console.WriteLine($"Root panel created: {rootPanel != null}");
            Console.WriteLine($"Component type: {panel.GetType().Name}");
            Console.WriteLine($"Root panel children: {rootPanel.ChildrenCount}");
            
            PrintPanelTree(rootPanel, 0);

            // Create and configure native window
            var nativeWindow = new NativeWindow(width, height, title);
            
            // If panel is a Window, give it a reference to the native window
            if (panel is Sandbox.UI.Window win)
            {
                win.SetNativeWindow(nativeWindow);
            }
            
            nativeWindow.RootPanel = rootPanel;
            nativeWindow.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RunPanel: {ex}");
            throw;
        }
    }

    /// <summary>
    /// [DEPRECATED] Run a Blazor IComponent as the root of the application.
    /// This method is no longer supported. Use RunPanel with Panel-derived Razor components instead.
    /// </summary>
    [Obsolete("Use RunPanel<T> instead with Panel-derived components")]
    public static void RunComponent<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : IComponent
    {
        throw new NotSupportedException("RunComponent is no longer supported. Use RunPanel<T> with Panel-derived components instead.");
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
