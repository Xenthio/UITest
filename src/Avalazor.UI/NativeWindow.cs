using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using SkiaSharp;
using Sandbox.UI;
using Sandbox.UI.Skia;
using SysVector2 = System.Numerics.Vector2;
using UIVector2 = Sandbox.UI.Vector2;

namespace Avalazor.UI;

/// <summary>
/// Native window implementation using Silk.NET for cross-platform windowing.
/// Uses Sandbox.UI for panel system and Sandbox.UI.Skia for rendering.
/// This is the actual native OS window that hosts the UI.
/// </summary>
public class NativeWindow : IDisposable
{
    private readonly IWindow _window;
    private GL? _gl;
    private SKSurface? _surface;
    private GRContext? _grContext;
    private GRGlInterface? _grGlInterface;
    private RootPanel? _rootPanel;
    private SkiaPanelRenderer? _renderer;
    private bool _needsLayout = true;
    private Vector2D<int> _lastSize;
    private bool _disposed = false;
    private IInputContext? _input;
    private IMouse? _mouse;
    private IKeyboard? _keyboard;

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

    public NativeWindow(int width = 1280, int height = 720, string title = "Avalazor Application")
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
        options.VSync = true;

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Resize += OnResize;
        _window.Closing += OnClosing;
    }

    public void Run()
    {
        _window.Run();
    }

    /// <summary>
    /// Set the native window title
    /// </summary>
    public void SetTitle(string title)
    {
        if (_window != null)
        {
            _window.Title = title;
        }
    }

    /// <summary>
    /// Set the native window size
    /// </summary>
    public void SetSize(int width, int height)
    {
        if (_window != null)
        {
            _window.Size = new Vector2D<int>(width, height);
        }
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

        // Initialize input handling
        _input = _window.CreateInput();
        
        // Wire up mouse events
        foreach (var mouse in _input.Mice)
        {
            _mouse = mouse;
            _mouse.MouseDown += OnMouseDown;
            _mouse.MouseUp += OnMouseUp;
            _mouse.Scroll += OnMouseScroll;
            break; // Use first mouse
        }

        // Wire up keyboard events
        foreach (var keyboard in _input.Keyboards)
        {
            _keyboard = keyboard;
            _keyboard.KeyDown += OnKeyDown;
            _keyboard.KeyUp += OnKeyUp;
            _keyboard.KeyChar += OnKeyChar;
            break; // Use first keyboard
        }
    }

    private unsafe void CreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;

        // Create Skia render target directly on the default framebuffer (0)
        var glInfo = new GRGlFramebufferInfo(0, (uint)InternalFormat.Rgba8);
        var renderTarget = new GRBackendRenderTarget(width, height, 0, 8, glInfo);
        _surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    }

    private void OnRender(double deltaTime)
    {
        if (_gl == null || _surface == null || _rootPanel == null || _grContext == null || _renderer == null) return;

        var currentSize = _window.Size;
        
        // Check if size changed (handles resize mid-frame)
        bool sizeChanged = _lastSize.X != currentSize.X || _lastSize.Y != currentSize.Y;
        if (sizeChanged)
        {
            _lastSize = currentSize;
            _needsLayout = true;
            
            // Update viewport
            _gl.Viewport(0, 0, (uint)currentSize.X, (uint)currentSize.Y);

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

        // Update input for root panel
        var mousePos = _mouse != null ? new UIVector2(_mouse.Position.X, _mouse.Position.Y) : UIVector2.Zero;
        _rootPanel.UpdateInput(mousePos, _mouse != null);

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
    }

    private void OnResize(Vector2D<int> size)
    {
        _lastSize = size;
        if (_gl != null)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        }

        // Mark as needing layout and recreate render target
        _needsLayout = true;
        RecreateRenderTarget(size.X, size.Y);

        Invalidate();
    }

    private void RecreateRenderTarget(int width, int height)
    {
        if (_gl == null || _grContext == null) return;
        if (width <= 0 || height <= 0) return;

        // Flush the GRContext before disposing resources to ensure all pending GPU commands are completed
        _grContext.Flush();
        
        // Clean up old resources
        _surface?.Dispose();
        _surface = null;

        // Reset the GRContext state to clear any cached GPU resource references
        _grContext.ResetContext();

        // Create new render target
        CreateRenderTarget(width, height);
    }

    private void OnClosing()
    {
        // Clean up only our internal resources here, not the window itself.
        // The window will be disposed naturally outside the render loop
        // by the using statement in AvalazorApplication.Run().
        CleanupResources();
    }

    private void Invalidate()
    {
        // Request redraw - Silk.NET handles this automatically
    }

    private void CleanupResources()
    {
        if (_disposed) return;
        
        _surface?.Dispose();
        _surface = null;
        
        _grContext?.Dispose();
        _grContext = null;
        _grGlInterface?.Dispose();
        _grGlInterface = null;
        _gl?.Dispose();
        _gl = null;
        _input?.Dispose();
        _input = null;
        
        _disposed = true;
    }

    private void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (_rootPanel == null) return;
        
        var buttonName = button switch
        {
            MouseButton.Left => "mouseleft",
            MouseButton.Right => "mouseright",
            MouseButton.Middle => "mousemiddle",
            _ => $"mouse{(int)button}"
        };

        var modifiers = GetKeyboardModifiers();
        _rootPanel.ProcessButtonEvent(buttonName, true, modifiers);
    }

    private void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if (_rootPanel == null) return;
        
        var buttonName = button switch
        {
            MouseButton.Left => "mouseleft",
            MouseButton.Right => "mouseright",
            MouseButton.Middle => "mousemiddle",
            _ => $"mouse{(int)button}"
        };

        var modifiers = GetKeyboardModifiers();
        _rootPanel.ProcessButtonEvent(buttonName, false, modifiers);
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        if (_rootPanel == null) return;
        
        var delta = new UIVector2(scroll.X, scroll.Y);
        var modifiers = GetKeyboardModifiers();
        _rootPanel.ProcessMouseWheel(delta, modifiers);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        if (_rootPanel == null) return;
        
        var buttonName = key.ToString().ToLower();
        var modifiers = GetKeyboardModifiers();
        _rootPanel.ProcessButtonEvent(buttonName, true, modifiers);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode)
    {
        if (_rootPanel == null) return;
        
        var buttonName = key.ToString().ToLower();
        var modifiers = GetKeyboardModifiers();
        _rootPanel.ProcessButtonEvent(buttonName, false, modifiers);
    }

    private void OnKeyChar(IKeyboard keyboard, char character)
    {
        if (_rootPanel == null) return;
        _rootPanel.ProcessCharTyped(character);
    }

    private KeyboardModifiers GetKeyboardModifiers()
    {
        if (_keyboard == null) return KeyboardModifiers.None;

        var modifiers = KeyboardModifiers.None;
        
        if (_keyboard.IsKeyPressed(Key.ShiftLeft) || _keyboard.IsKeyPressed(Key.ShiftRight))
            modifiers |= KeyboardModifiers.Shift;
            
        if (_keyboard.IsKeyPressed(Key.ControlLeft) || _keyboard.IsKeyPressed(Key.ControlRight))
            modifiers |= KeyboardModifiers.Ctrl;
            
        if (_keyboard.IsKeyPressed(Key.AltLeft) || _keyboard.IsKeyPressed(Key.AltRight))
            modifiers |= KeyboardModifiers.Alt;

        return modifiers;
    }

    public void Dispose()
    {
        // Clean up our resources if not already done
        CleanupResources();
        
        // Dispose the window - this should only be called outside the render loop
        // (e.g., from the using statement in AvalazorApplication.Run())
        _window?.Dispose();
    }
}
