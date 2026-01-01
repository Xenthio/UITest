using Microsoft.AspNetCore.Components;
using Sandbox.UI;
using Sandbox.UI.AI;
using Sandbox.UI.Reflection;
using Sandbox.UI.Skia;

namespace Avalazor.UI;

/// <summary>
/// Main application entry point for Avalazor apps
/// Simplified API similar to s&box's application model
/// </summary>
public static class AvalazorApplication
{
    /// <summary>
    /// Force AI renderer mode even if a display is available
    /// </summary>
    public static bool ForceAIMode { get; set; } = false;
    
    /// <summary>
    /// Callback invoked when running in AI mode, allowing custom handling of the root panel.
    /// If not set, default AI output is printed to console.
    /// </summary>
    public static Action<RootPanel>? AIModeCon { get; set; }

    /// <summary>
    /// Static constructor to ensure text measurement is available before any panels are created
    /// </summary>
    static AvalazorApplication()
    {
        // Touch SkiaPanelRenderer to trigger its static constructor,
        // which registers the text measurement function for Label layout
        _ = SkiaPanelRenderer.EnsureInitialized();
    }
    
    /// <summary>
    /// Check if a display environment is available
    /// </summary>
    public static bool HasDisplayEnvironment()
    {
        if (ForceAIMode)
            return false;
            
        // Check environment variable override
        var aiMode = Environment.GetEnvironmentVariable("AVALAZOR_AI_MODE");
        if (aiMode == "1" || aiMode?.ToLower() == "true")
            return false;
            
        // Check for common display environment indicators
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            // Windows typically always has a display
            return true;
        }
        
        // On Linux/macOS, check for DISPLAY or WAYLAND_DISPLAY
        var display = Environment.GetEnvironmentVariable("DISPLAY");
        var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        
        return !string.IsNullOrEmpty(display) || !string.IsNullOrEmpty(waylandDisplay);
    }

    public static void Run(RootPanel rootPanel, int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        if (!HasDisplayEnvironment())
        {
            RunWithAIRenderer(rootPanel, width, height, title);
            return;
        }
        
        using var window = new NativeWindow(width, height, title);
        window.RootPanel = rootPanel;
        window.Run();
    }

    /// <summary>
    /// Run a Panel-derived Razor component as the root of the application.
    /// This properly processes the Razor render tree to create child panels.
    /// If the panel is a Window, extracts window properties for native window creation.
    /// Automatically detects if a display is available and falls back to AI renderer if not.
    /// </summary>
    public static void RunPanel<T>(int width = 1280, int height = 720, string title = "Avalazor Application") where T : Panel, new()
    {
        try
        {
            // Initialize PanelFactory to register all [Library] and [Alias] types
            PanelFactory.Initialize();

            // Create the panel
            var panel = new T();
            
            // Wrap in RootPanel
            var rootPanel = new RootPanel();
            rootPanel.PanelBounds = new Rect(0, 0, width, height);
            rootPanel.AddChild(panel);
            
            // Perform initial layout which will trigger Razor tree processing
            rootPanel.Layout();

            // Re-read window properties AFTER layout (Razor attributes are now processed)
            if (panel is Sandbox.UI.Window windowPanel)
            {
                width = windowPanel.WindowWidth > 0 ? windowPanel.WindowWidth : width;
                height = windowPanel.WindowHeight > 0 ? windowPanel.WindowHeight : height;
                title = !string.IsNullOrEmpty(windowPanel.Title) ? windowPanel.Title : title;
            }
            
            // Check if we have a display environment
            if (!HasDisplayEnvironment())
            {
                Console.WriteLine($"[Avalazor] No display environment detected - using AI renderer");
                RunWithAIRenderer(rootPanel, width, height, title);
                return;
            }

            Console.WriteLine($"Creating native window from Window properties (after layout): {width}x{height}, Title: '{title}'");
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
    /// Run the application with the AI renderer (for headless/CI environments or AI agent debugging)
    /// </summary>
    private static void RunWithAIRenderer(RootPanel rootPanel, int width, int height, string title)
    {
        // Update bounds
        rootPanel.PanelBounds = new Rect(0, 0, width, height);
        rootPanel.Layout();
        
        // If custom handler is set, use it
        if (AIModeCon != null)
        {
            AIModeCon(rootPanel);
            return;
        }
        
        // Default AI mode output
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AVALAZOR AI RENDERER MODE                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Application: {title}");
        Console.WriteLine($"Viewport: {width}x{height}");
        Console.WriteLine($"No display detected - outputting UI state for AI analysis.\n");
        
        // Output the UI state
        Console.WriteLine(AIHelper.Snapshot(rootPanel));
        
        Console.WriteLine("\n" + new string('─', 60));
        Console.WriteLine("INTERACTIVE ELEMENTS");
        Console.WriteLine(new string('─', 60) + "\n");
        Console.WriteLine(AIHelper.GetInteractiveElements(rootPanel));
        
        Console.WriteLine("\n" + new string('─', 60));
        Console.WriteLine("QUICK SUMMARY");
        Console.WriteLine(new string('─', 60) + "\n");
        Console.WriteLine(AIHelper.QuickSummary(rootPanel));
        
        // Try to save a screenshot if SkiaSharp is available
        try
        {
            var screenshotPath = AIHelper.Screenshot(rootPanel);
            Console.WriteLine($"\n[AI] Screenshot saved to: {screenshotPath}");
            Console.WriteLine("[AI] You can view this image to see the visual state of the UI.");
        }
        catch (Exception)
        {
            // SkiaSharp native library might not be available in headless environments
            Console.WriteLine("\n[AI] Note: Screenshot not available (SkiaSharp native library not loaded)");
        }
        
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("AI MODE TIPS:");
        Console.WriteLine("- Use AIHelper.Snapshot(rootPanel) to get current UI state");
        Console.WriteLine("- Use AIHelper.WhatIsAt(rootPanel, x, y) to inspect coordinates");
        Console.WriteLine("- Use AIHelper.FindByClass(rootPanel, \"classname\") to find elements");
        Console.WriteLine("- Set AVALAZOR_AI_MODE=0 or provide a DISPLAY to use GUI mode");
        Console.WriteLine(new string('═', 60));
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
        Console.WriteLine($"{indent}{panel.GetType().Name} (ElementName={panel.ElementName}, Children={panel.ChildrenCount}){text}, classes=[{string.Join(", ", panel.Classes)}]");
        foreach (var child in panel.Children)
        {
            PrintPanelTree(child, depth + 1);
        }
    }
}
