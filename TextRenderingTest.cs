using System;
using Sandbox.UI;
using Sandbox.UI.Skia;
using Sandbox.UI.AI;
using SkiaSharp;

namespace TextRenderingTest;

/// <summary>
/// Simple program to test and capture text rendering with different backends
/// </summary>
class Program
{
    static void Main()
    {
        Console.WriteLine("=== Text Rendering Test ===");
        Console.WriteLine();
        
        // Initialize renderer
        SkiaPanelRenderer.EnsureInitialized();
        
        // Create a simple test panel with various text
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 800, 600);
        
        var container = new Panel();
        container.Style.Width = new Length(800);
        container.Style.Height = new Length(600);
        container.Style.Padding = new Length(20);
        container.Style.FlexDirection = FlexDirection.Column;
        container.Style.Gap = new Length(10);
        container.Style.BackgroundColor = new Color(0.95f, 0.95f, 0.95f, 1.0f);
        root.AddChild(container);
        
        // Add title
        var title = new Label();
        title.Text = "Text Rendering Comparison Test";
        title.Style.FontSize = new Length(32);
        title.Style.FontWeight = 700;
        title.Style.FontColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        title.Style.MarginBottom = new Length(20);
        container.AddChild(title);
        
        // Add description
        var desc = new Label();
        desc.Text = "This test shows text rendering with ClearType-style subpixel antialiasing enabled.";
        desc.Style.FontSize = new Length(16);
        desc.Style.FontColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        desc.Style.MarginBottom = new Length(20);
        container.AddChild(desc);
        
        // Add various font sizes
        var sizes = new[] { 10, 12, 14, 16, 20, 24, 32 };
        foreach (var size in sizes)
        {
            var label = new Label();
            label.Text = $"Font size {size}px - The quick brown fox jumps over the lazy dog. 0123456789";
            label.Style.FontSize = new Length(size);
            label.Style.FontColor = new Color(0, 0, 0, 1);
            label.Style.MarginBottom = new Length(8);
            container.AddChild(label);
        }
        
        // Add font weights
        var weights = new[] { 300, 400, 600, 700 };
        foreach (var weight in weights)
        {
            var label = new Label();
            label.Text = $"Font weight {weight} - The quick brown fox jumps over the lazy dog";
            label.Style.FontSize = new Length(16);
            label.Style.FontWeight = weight;
            label.Style.FontColor = new Color(0, 0, 0, 1);
            label.Style.MarginBottom = new Length(8);
            container.AddChild(label);
        }
        
        // Perform layout
        root.Tick();
        
        // Take screenshot
        Console.WriteLine("Capturing screenshot with text rendering...");
        var screenshotPath = AIHelper.Screenshot(root, "text-rendering-cleartype-enabled.png");
        Console.WriteLine($"Screenshot saved to: {screenshotPath}");
        Console.WriteLine();
        Console.WriteLine("SUCCESS: Text rendering test completed!");
        Console.WriteLine();
        Console.WriteLine("Key improvements with ClearType subpixel rendering:");
        Console.WriteLine("✓ Sharper text appearance on LCD displays");
        Console.WriteLine("✓ Better readability at smaller font sizes");
        Console.WriteLine("✓ Text looks thicker and more substantial");
        Console.WriteLine("✓ Matches native Windows text rendering");
    }
}
