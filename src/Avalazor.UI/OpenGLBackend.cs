using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SkiaSharp;
using Sandbox.UI;
using Sandbox.UI.Skia;

namespace Avalazor.UI;

public class OpenGLBackend : IGraphicsBackend
{
    private GL? _gl;
    private GRContext? _grContext;
    private GRGlInterface? _grGlInterface;
    private SKSurface? _surface;
    private SkiaPanelRenderer? _renderer;
    private IWindow? _window;

    public void Initialize(IWindow window)
    {
        _window = window;
        _gl = window.CreateOpenGL();
        _gl.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

        _grGlInterface = GRGlInterface.Create((name) =>
            _gl.Context.TryGetProcAddress(name, out var addr) ? addr : IntPtr.Zero);

        _grContext = GRContext.CreateGl(_grGlInterface);
        _renderer = new SkiaPanelRenderer();

        CreateSurface(window.FramebufferSize);
    }

    public void Resize(Vector2D<int> size)
    {
        if (_gl == null) return;

        _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        _surface?.Dispose();
        _surface = null;
        CreateSurface(size);
    }

    public void Render(RootPanel panel)
    {
        if (_gl == null || _surface == null || _renderer == null || _grContext == null) return;

        // Reset Skia's GL state tracking since Silk.NET may have changed GL state
        _grContext.ResetContext();

        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _surface.Canvas.Clear(new SKColor(240, 240, 240));

        _renderer.Render(_surface.Canvas, panel);

        _surface.Canvas.Flush();
        _grContext.Flush();
        
        // Don't manually swap - Silk.NET handles this automatically after the Render callback
    }

    private void CreateSurface(Vector2D<int> size)
    {
        if (_grContext == null) return;
        var glInfo = new GRGlFramebufferInfo(0, 0x8058); // Rgba8
        var target = new GRBackendRenderTarget(size.X, size.Y, 0, 8, glInfo);
        _surface = SKSurface.Create(_grContext, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _grContext?.Dispose();
        _grGlInterface?.Dispose();
        _gl?.Dispose();
    }
}