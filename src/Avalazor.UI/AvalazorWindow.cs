using Silk.NET.Windowing;
using Silk.NET.Maths;
using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Main application window using Silk.NET for cross-platform windowing
/// </summary>
public class AvalazorWindow : IDisposable
{
    private readonly IWindow _window;
    private SKSurface? _surface;
    private GRContext? _grContext;
    private Panel? _rootPanel;
    private readonly StyleEngine _styleEngine = new();

    public Panel? RootPanel
    {
        get => _rootPanel;
        set
        {
            _rootPanel = value;
            Invalidate();
        }
    }

    public AvalazorWindow(int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.API = GraphicsAPI.None; // We'll handle rendering ourselves with Skia

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Resize += OnResize;
        _window.Closing += OnClosing;
    }

    public void Run()
    {
        _window.Run();
    }

    public void LoadStylesheet(string name, string css)
    {
        _styleEngine.AddStylesheet(name, css);
        Invalidate();
    }

    private void OnLoad()
    {
        // Initialize Skia rendering context
        var info = new SKImageInfo(_window.Size.X, _window.Size.Y, SKColorType.Rgba8888, SKAlphaType.Premul);
        _surface = SKSurface.Create(info);
    }

    private void OnRender(double deltaTime)
    {
        if (_surface == null || _rootPanel == null) return;

        var canvas = _surface.Canvas;
        canvas.Clear(SKColors.White);

        // Compute styles if needed
        ComputeStyles(_rootPanel);

        // Perform layout using Yoga
        YogaLayoutEngine.Layout(_rootPanel, _window.Size.X, _window.Size.Y);

        // Paint the UI
        _rootPanel.Paint(canvas);

        canvas.Flush();

        // TODO: Present to screen (needs platform-specific implementation)
    }

    private void ComputeStyles(Panel panel)
    {
        // Recursively compute styles for panel tree
        panel.GetType().GetField("_computedStyle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(panel, _styleEngine.ComputeStyle(panel));

        panel.GetType().GetField("_needsStyleCompute", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .SetValue(panel, false);

        foreach (var child in panel.Children)
        {
            ComputeStyles(child);
        }
    }



    private void OnResize(Vector2D<int> size)
    {
        if (_surface != null)
        {
            _surface.Dispose();
            var info = new SKImageInfo(size.X, size.Y, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info);
        }

        Invalidate();
    }

    private void OnClosing()
    {
        Dispose();
    }

    private void Invalidate()
    {
        // Mark for redraw
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _grContext?.Dispose();
        _window?.Dispose();
    }
}
