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

    public RootPanel? RootPanel
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
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
        options.VSync = true;

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

        // Create render target
        CreateRenderTarget(_window.Size.X, _window.Size.Y);
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

        // Clear screen
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render to Skia surface
        var canvas = _surface.Canvas;
        canvas.Clear(new SKColor(240, 240, 240)); // Light gray background

        // Set panel bounds to window size
        _rootPanel.PanelBounds = new Rect(0, 0, _window.Size.X, _window.Size.Y);

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
            0, 0, _window.Size.X, _window.Size.Y,
            0, 0, _window.Size.X, _window.Size.Y,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void OnResize(Vector2D<int> size)
    {
        if (_gl != null)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }

        // Recreate render target with new size
        if (_surface != null)
        {
            _surface.Dispose();
            
            if (_gl != null)
            {
                if (_framebuffer != 0) _gl.DeleteFramebuffer(_framebuffer);
                if (_texture != 0) _gl.DeleteTexture(_texture);
                if (_renderbuffer != 0) _gl.DeleteRenderbuffer(_renderbuffer);
            }

            CreateRenderTarget(size.X, size.Y);
        }

        Invalidate();
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
