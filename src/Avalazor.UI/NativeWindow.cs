using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using SkiaSharp;
using Sandbox.UI;
using UIVector2 = Sandbox.UI.Vector2;

namespace Avalazor.UI;

public class NativeWindow : INativeWindow, IDisposable
{
    private readonly IWindow _window;
    private IGraphicsBackend _backend;

    private IInputContext? _input;
    private IMouse? _mouse;
    private IKeyboard? _keyboard;
    private bool _disposed = false;

    public RootPanel? RootPanel { get; set; }

    public NativeWindow(int width = 1280, int height = 720, string title = "Avalazor App")
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.VSync = true;
        options.IsEventDriven = false;
        options.UpdatesPerSecond = 60;  // Explicitly set update rate
        options.FramesPerSecond = 60;   // Explicitly set frame rate

        // 1. SELECT BACKEND AT COMPILE TIME
//#if WINDOWS
        // On Windows, use D3D11 to fix resizing bugs
        //options.API = GraphicsAPI.None;
        //_backend = new D3D11Backend();
//#else
        // On Linux/Mac, use OpenGL
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
        _backend = new OpenGLBackend();
//#endif

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.Resize += OnResize;
    }

    public void Run() => _window.Run();

    private void OnLoad()
    {
        _backend.Initialize(_window);

        _input = _window.CreateInput();
        if (_input.Mice.Count > 0)
        {
            _mouse = _input.Mice[0];
            _mouse.MouseDown += OnMouseDown;
            _mouse.MouseUp += OnMouseUp;
            _mouse.Scroll += OnMouseScroll;
        }
        if (_input.Keyboards.Count > 0)
        {
            _keyboard = _input.Keyboards[0];
            _keyboard.KeyDown += OnKeyDown;
            _keyboard.KeyUp += OnKeyUp;
            _keyboard.KeyChar += OnKeyChar;
        }
        
        // Set system DPI scale for UI rendering
        UpdateDpiScale();
    }
    
    private void UpdateDpiScale()
    {
        // Try to get DPI from the window's monitor
        // Silk.NET uses FramebufferSize / Size to calculate content scale
        var size = _window.Size;
        var fbSize = _window.FramebufferSize;
        
        if (size.X > 0 && fbSize.X > 0)
        {
            var dpiScaleX = (float)fbSize.X / size.X;
            var dpiScaleY = (float)fbSize.Y / size.Y;
            
            // Use the larger scale (usually they're the same)
            RootPanel.SystemDpiScale = Math.Max(dpiScaleX, dpiScaleY);
        }
    }

    private void OnResize(Vector2D<int> size)
    {
        if (size.X <= 0 || size.Y <= 0) return;

        _backend.Resize(size);

        if (RootPanel != null)
        {
            RootPanel.PanelBounds = new Rect(0, 0, size.X, size.Y);
            RootPanel.InvalidateLayout();
            RootPanel.Layout();
        }
        OnRender(0);
    }

    private void OnRender(double delta)
    {
        if (RootPanel == null) return;

        // Update panel time for transitions and animations
        PanelRealTime.Update(delta);
        RealTime.Update(delta);

        var size = _window.FramebufferSize;
        RootPanel.PanelBounds = new Rect(0, 0, size.X, size.Y);

        var mousePos = _mouse != null ? new UIVector2(_mouse.Position.X, _mouse.Position.Y) : UIVector2.Zero;
        RootPanel.UpdateInput(mousePos, _mouse != null);
        RootPanel.Layout();

        _backend.Render(RootPanel);
    }

    private void OnClosing()
    {
        _backend.Dispose();
        _input?.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        OnClosing();
        _window?.Dispose();
        _disposed = true;
    }

    // --- Public API for Window control ---
    
    /// <summary>
    /// Set the native window title
    /// </summary>
    public void SetTitle(string title)
    {
        _window.Title = title;
    }

    /// <summary>
    /// Set the native window position
    /// </summary>
    public void SetPosition(int x, int y)
    {
        _window.Position = new Vector2D<int>(x, y);
    }

    /// <summary>
    /// Set the native window size
    /// </summary>
    public void SetSize(int width, int height)
    {
        _window.Size = new Vector2D<int>(width, height);
    }

    // --- Input Helpers ---
    private void OnMouseDown(IMouse mouse, MouseButton button) => RootPanel?.ProcessButtonEvent(MouseButtonToString(button), true, GetKeyboardModifiers());
    private void OnMouseUp(IMouse mouse, MouseButton button) => RootPanel?.ProcessButtonEvent(MouseButtonToString(button), false, GetKeyboardModifiers());
    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll) => RootPanel?.ProcessMouseWheel(new UIVector2(scroll.X, -scroll.Y), GetKeyboardModifiers());
    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode) => RootPanel?.ProcessButtonEvent(key.ToString().ToLower(), true, GetKeyboardModifiers());
    private void OnKeyUp(IKeyboard keyboard, Key key, int scancode) => RootPanel?.ProcessButtonEvent(key.ToString().ToLower(), false, GetKeyboardModifiers());
    private void OnKeyChar(IKeyboard keyboard, char character) => RootPanel?.ProcessCharTyped(character);
    private string MouseButtonToString(MouseButton button) => button switch { MouseButton.Left => "mouseleft", MouseButton.Right => "mouseright", MouseButton.Middle => "mousemiddle", _ => $"mouse{(int)button}" };
    private KeyboardModifiers GetKeyboardModifiers() { if (_keyboard == null) return KeyboardModifiers.None; var m = KeyboardModifiers.None; if (_keyboard.IsKeyPressed(Key.ShiftLeft) || _keyboard.IsKeyPressed(Key.ShiftRight)) m |= KeyboardModifiers.Shift; if (_keyboard.IsKeyPressed(Key.ControlLeft) || _keyboard.IsKeyPressed(Key.ControlRight)) m |= KeyboardModifiers.Ctrl; if (_keyboard.IsKeyPressed(Key.AltLeft) || _keyboard.IsKeyPressed(Key.AltRight)) m |= KeyboardModifiers.Alt; return m; }
}