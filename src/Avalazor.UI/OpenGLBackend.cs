using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SkiaSharp;
using Sandbox.UI;
using Sandbox.UI.Skia;

namespace Avalazor.UI;

/// <summary>
/// OpenGL graphics backend for Avalazor.
/// Provides hardware-accelerated rendering using OpenGL and SkiaSharp.
/// </summary>
/// <remarks>
/// Known Limitation: On Windows with GLFW, window resizing may cause brief visual artifacts
/// due to GLFW's modal event loop blocking render callbacks during drag operations.
/// This is a platform limitation, not a bug. For best Windows experience, use DirectX11 backend.
/// 
/// This backend works well on Linux and macOS where the windowing systems handle resize differently.
/// </remarks>
public class OpenGLBackend : IGraphicsBackend
{
    private GL? _gl;
    private GRContext? _grContext;
    private GRGlInterface? _grGlInterface;
    private SKSurface? _surface;
    private SkiaPanelRenderer? _renderer;
    private IWindow? _window;
    private int _width;
    private int _height;
    private bool _needsResize;
    
    // Offscreen FBO for rendering
    private uint _fbo;
    private uint _colorTexture;
    private uint _depthStencilRenderbuffer;

    public void Initialize(IWindow window)
    {
        _window = window;
        _gl = window.CreateOpenGL();

        _grGlInterface = GRGlInterface.Create((name) =>
            _gl.Context.TryGetProcAddress(name, out var addr) ? addr : IntPtr.Zero);

        _grContext = GRContext.CreateGl(_grGlInterface);
        _renderer = new SkiaPanelRenderer();

        _width = window.FramebufferSize.X;
        _height = window.FramebufferSize.Y;
        _needsResize = false;
        
        CreateOffscreenRenderTarget(window.FramebufferSize);
    }

    public void Resize(Vector2D<int> size)
    {
        if (_gl == null || size.X <= 0 || size.Y <= 0) return;

        // Don't resize if dimensions haven't changed
        if (size.X == _width && size.Y == _height)
        {
            return;
        }

        _width = size.X;
        _height = size.Y;
        
        // Mark that we need to resize - defer actual work to render call
        _needsResize = true;
    }

    public void Render(RootPanel panel)
    {
        if (_gl == null || _renderer == null || _grContext == null || _window == null) return;

        // Handle pending resize
        if (_needsResize || _surface == null)
        {
            // Recreate the offscreen render target with new dimensions
            DestroyOffscreenRenderTarget();
            CreateOffscreenRenderTarget(new Vector2D<int>(_width, _height));
            _needsResize = false;
        }

        if (_surface == null) return;

        // Render to offscreen FBO
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);

        // Reset Skia's GL state tracking
        _grContext.ResetContext();

        // Render the UI to the offscreen surface
        _surface.Canvas.Clear(new SKColor(240, 240, 240));
        _renderer.Render(_surface.Canvas, panel);
        _surface.Canvas.Flush();
        _grContext.Flush();

        // Blit from offscreen FBO to the default framebuffer (screen)
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _fbo);
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        
        // Use the same size for source and destination to avoid any scaling/stretching
        _gl.BlitFramebuffer(
            0, 0, _width, _height,  // Source rectangle (our offscreen FBO)
            0, 0, _width, _height,  // Destination rectangle (same size - no scaling)
            ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Nearest  // Use nearest for 1:1 pixel copy
        );
        
        // Restore default framebuffer binding
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void CreateOffscreenRenderTarget(Vector2D<int> size)
    {
        if (_grContext == null || _gl == null || size.X <= 0 || size.Y <= 0) return;

        // Create framebuffer
        _fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        // Create color texture
        _colorTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
        
        unsafe
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)size.X, (uint)size.Y, 
                           0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        }
        
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, 
                                 TextureTarget.Texture2D, _colorTexture, 0);

        // Create depth/stencil renderbuffer
        _depthStencilRenderbuffer = _gl.GenRenderbuffer();
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthStencilRenderbuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, 
                                (uint)size.X, (uint)size.Y);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, 
                                    RenderbufferTarget.Renderbuffer, _depthStencilRenderbuffer);

        // Check framebuffer completeness
        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
        {
            throw new Exception($"Framebuffer is not complete: {status}");
        }

        // Create Skia surface wrapping this FBO
        var glInfo = new GRGlFramebufferInfo(
            fboId: _fbo,
            format: 0x8058  // GL_RGBA8
        );
        
        var target = new GRBackendRenderTarget(size.X, size.Y, 0, 8, glInfo);
        var surfProps = new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal);
        
        _surface = SKSurface.Create(_grContext, target, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888, surfProps);
        
        if (_surface == null)
        {
            throw new Exception($"Failed to create SKSurface with dimensions {size.X}x{size.Y}");
        }

        // Restore default framebuffer
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void DestroyOffscreenRenderTarget()
    {
        if (_gl == null) return;

        _surface?.Dispose();
        _surface = null;

        if (_colorTexture != 0)
        {
            _gl.DeleteTexture(_colorTexture);
            _colorTexture = 0;
        }

        if (_depthStencilRenderbuffer != 0)
        {
            _gl.DeleteRenderbuffer(_depthStencilRenderbuffer);
            _depthStencilRenderbuffer = 0;
        }

        if (_fbo != 0)
        {
            _gl.DeleteFramebuffer(_fbo);
            _fbo = 0;
        }
    }

    public void Dispose()
    {
        DestroyOffscreenRenderTarget();
        _grContext?.Dispose();
        _grGlInterface?.Dispose();
        _gl?.Dispose();
    }
}