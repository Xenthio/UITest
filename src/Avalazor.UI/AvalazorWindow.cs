using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Main application window using Silk.NET for cross-platform windowing
/// Phase 3: Proper GPU-accelerated rendering with OpenGL backend
/// </summary>
public class AvalazorWindow : IDisposable
{
    private readonly IWindow _window;
    private GL? _gl;
    private SKSurface? _surface;
    private GRContext? _grContext;
    private GRGlInterface? _grGlInterface;
    private Panel? _rootPanel;
    private readonly StyleEngine _styleEngine = new();
    private readonly YogaLayoutEngine _yogaLayout = new();
    private uint _framebuffer;
    private uint _texture;
    private uint _renderbuffer;

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

    public void LoadStylesheet(string name, string css)
    {
        _styleEngine.AddStylesheet(name, css);
        Invalidate();
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
        if (_gl == null || _surface == null || _rootPanel == null || _grContext == null) return;

        // Clear screen
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Render to Skia surface
        var canvas = _surface.Canvas;
        canvas.Clear(new SKColor(240, 240, 240)); // Light gray background

        // Compute styles if needed
        ComputeStyles(_rootPanel);

        // Perform layout (Yoga integration will be added later)
        PerformLayout(_rootPanel, _window.Size.X, _window.Size.Y);

        // Paint the UI
        _rootPanel.Paint(canvas);

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

    private void PerformLayout(Panel panel, float width, float height)
    {
        // Use Yoga flexbox layout engine
        _yogaLayout.CalculateLayout(panel, width, height);
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

    private void OnMouseDown(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        if (_rootPanel == null) return;

        var pos = mouse.Position;
        var e = new MouseEventArgs
        {
            X = pos.X,
            Y = pos.Y,
            Button = (int)button
        };

        // Find panel at position and fire event
        HitTest(_rootPanel, e.X, e.Y)?.OnMouseDown(e);
    }

    private void OnMouseUp(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        if (_rootPanel == null) return;

        var pos = mouse.Position;
        var e = new MouseEventArgs
        {
            X = pos.X,
            Y = pos.Y,
            Button = (int)button
        };

        // Find panel at position and fire event
        HitTest(_rootPanel, e.X, e.Y)?.OnMouseUp(e);
    }

    private Panel? HitTest(Panel panel, float x, float y)
    {
        // Check children first (front to back)
        for (int i = panel.Children.Count - 1; i >= 0; i--)
        {
            var child = panel.Children[i];
            var hit = HitTest(child, x, y);
            if (hit != null) return hit;
        }

        // Check this panel
        if (panel.ContainsPoint(x, y))
        {
            return panel;
        }

        return null;
    }

    public void Dispose()
    {
        _yogaLayout?.Dispose();
        
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
