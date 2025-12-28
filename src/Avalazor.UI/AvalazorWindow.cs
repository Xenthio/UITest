using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SkiaSharp;
using Sandbox.UI;
using Sandbox.UI.Skia;

namespace Avalazor.UI;

/// <summary>
/// Main application window using Silk.NET for cross-platform windowing
/// Uses Sandbox.UI for panel system and Sandbox.UI.Skia for rendering
/// </summary>
public class AvalazorWindow : IDisposable
{
    private readonly IWindow _window;
    private GL? _gl;
    private SKSurface? _surface;
    private GRContext? _grContext;
    private GRGlInterface? _grGlInterface;
    private RootPanel? _rootPanel;
    private SkiaPanelRenderer? _renderer;
    private uint _framebuffer;
    private bool _needsLayout = true;
    private Vector2D<int> _lastSize;

    public RootPanel? RootPanel
    {
        get => _rootPanel;
        set
        {
            _rootPanel = value;
            _needsLayout = true;
            Invalidate();
        }
    }

    public AvalazorWindow(int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
        options.VSync = true;

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFramebufferResize;
        _window.Closing += OnClosing;
    }

    public void Run()
    {
        _window.Run();
    }

    private void OnLoad()
    {
        // Initialize OpenGL
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

        // Initialize Skia with OpenGL backend
        unsafe
        {
            _grGlInterface = GRGlInterface.Create((name) =>
            {
                return _gl.Context.TryGetProcAddress(name, out var addr) ? addr : IntPtr.Zero;
            });
        }

        _grContext = GRContext.CreateGl(_grGlInterface);
        _renderer = new SkiaPanelRenderer();

        // Create render target - use FramebufferSize for actual pixel dimensions
        // Render directly to default framebuffer (FBO 0) to avoid stretching during resize
        CreateRenderTarget(_window.FramebufferSize.X, _window.FramebufferSize.Y);
    }

    private unsafe void CreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;

        // Render directly to the default framebuffer (FBO 0) to avoid stretching during resize
        // The default framebuffer is managed by the windowing system and automatically resizes
        _framebuffer = 0; // Use default framebuffer instead of creating custom FBO

        // Create Skia render target pointing to the default framebuffer
        var info = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(_framebuffer, (uint)InternalFormat.Rgba8));
        _surface = SKSurface.Create(_grContext, info, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    private void OnRender(double deltaTime)
    {
        if (_gl == null || _surface == null || _rootPanel == null || _grContext == null || _renderer == null) return;

        var currentSize = _window.FramebufferSize;
        
        // Check if size changed (handles resize mid-frame)
        bool sizeChanged = _lastSize.X != currentSize.X || _lastSize.Y != currentSize.Y;
        if (sizeChanged)
        {
            _lastSize = currentSize;
            _needsLayout = true;
            
            // Recreate render target if needed
            RecreateRenderTarget(currentSize.X, currentSize.Y);
        }

        // Clear screen
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render to Skia surface (which targets the default framebuffer directly)
        var canvas = _surface.Canvas;
        canvas.Clear(new SKColor(240, 240, 240)); // Light gray background

        // Set panel bounds to window size BEFORE invalidating layout
        _rootPanel.PanelBounds = new Rect(0, 0, currentSize.X, currentSize.Y);

        // Force full re-layout when size changes or panel is new
        if (_needsLayout)
        {
            _needsLayout = false;
            // Force all panels to recalculate their styles and layout
            _rootPanel.InvalidateLayout();
        }

        // Layout using Sandbox.UI's layout system
        _rootPanel.Layout();

        // Render using SkiaPanelRenderer
        _renderer.Render(canvas, _rootPanel);

        canvas.Flush();
        _grContext.Flush();
        
        // No blit needed - we rendered directly to default framebuffer
    }

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (_gl != null)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }

        // Mark as needing layout
        _needsLayout = true;

        // Double render workaround for Silk.NET resize issues:
        // First render updates/recreates framebuffers for new size
        // Second render actually draws the content at the new size
        // This prevents stretched frames during window resize
        OnRender(0);
        OnRender(0);
    }

    private void RecreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;
        if (width <= 0 || height <= 0) return;

        // Flush any pending GPU commands before disposing resources
        _grContext.Flush();

        // Clean up old surface
        _surface?.Dispose();
        _surface = null;

        // No need to delete framebuffer/texture/renderbuffer as we're using the default framebuffer (FBO 0)
        
        // Create new render target for the default framebuffer
        CreateRenderTarget(width, height);
        
        // Reset context so Skia doesn't assume cached state about old framebuffer
        _grContext.ResetContext();
    }

    private void OnClosing()
    {
        Dispose();
    }

    private void Invalidate()
    {
        // Request redraw - Silk.NET handles this automatically
    }

    public void Dispose()
    {
        _surface?.Dispose();
        
        // No custom framebuffer/texture/renderbuffer to delete - using default framebuffer

        _grContext?.Dispose();
        _grGlInterface?.Dispose();
        _gl?.Dispose();
        _window?.Dispose();
    }
}
