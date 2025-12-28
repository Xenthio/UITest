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
    private uint _texture;
    private uint _renderbuffer;
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
        CreateRenderTarget(_window.FramebufferSize.X, _window.FramebufferSize.Y);
    }

    private unsafe void CreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;

        // Create FBO for rendering
        _framebuffer = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);

        // Create backing texture
        _texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texture, 0);

        // Create Skia render target
        var info = new GRBackendRenderTarget(width, height, 0, 8, new GRGlFramebufferInfo(_framebuffer, (uint)InternalFormat.Rgba8));
        _surface = SKSurface.Create(_grContext, info, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

        // Unbind
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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

        // Render to Skia surface
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

        // Blit to screen
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebuffer);
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        _gl.BlitFramebuffer(
            0, 0, currentSize.X, currentSize.Y,
            0, 0, currentSize.X, currentSize.Y,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (_gl != null)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            
            // Clear the default framebuffer immediately to prevent stretched appearance
            // Without this, the old content gets stretched by the OS until new content is rendered
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        // Mark as needing layout
        _needsLayout = true;

        // Force immediate render to prevent stretched frame appearance
        // This ensures the new size is rendered immediately instead of
        // stretching the old frame until the next render cycle
        // OnRender will handle recreating the render target
        OnRender(0);
    }

    private void RecreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;
        if (width <= 0 || height <= 0) return;

        // Clean up old resources
        _surface?.Dispose();
        _surface = null;

        if (_framebuffer != 0)
        {
            _gl.DeleteFramebuffer(_framebuffer);
            _framebuffer = 0;
        }
        if (_texture != 0)
        {
            _gl.DeleteTexture(_texture);
            _texture = 0;
        }
        if (_renderbuffer != 0)
        {
            _gl.DeleteRenderbuffer(_renderbuffer);
            _renderbuffer = 0;
        }

        // Create new render target
        CreateRenderTarget(width, height);
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
        
        if (_gl != null)
        {
            if (_framebuffer != 0) _gl.DeleteFramebuffer(_framebuffer);
            if (_texture != 0) _gl.DeleteTexture(_texture);
            if (_renderbuffer != 0) _gl.DeleteRenderbuffer(_renderbuffer);
        }

        _grContext?.Dispose();
        _grGlInterface?.Dispose();
        _gl?.Dispose();
        _window?.Dispose();
    }
}
