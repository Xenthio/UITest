using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using SkiaSharp;
using Sandbox.UI;
using UIVector2 = Sandbox.UI.Vector2;

namespace Avalazor.UI;

/// <summary>
/// A popup window that appears as a separate OS window.
/// Used for dropdown menus, context menus, and other popups.
/// </summary>
public class PopupWindow : IDisposable
{
    private readonly IWindow _window;
    private IGraphicsBackend _backend;
    private IInputContext? _input;
    private IMouse? _mouse;
    private IKeyboard? _keyboard;
    private bool _disposed = false;

    /// <summary>
    /// The root panel for this popup window
    /// </summary>
    public RootPanel? RootPanel { get; set; }

    /// <summary>
    /// The panel that triggered this popup
    /// </summary>
    public Panel? SourcePanel { get; set; }

    /// <summary>
    /// The main application window that owns this popup
    /// </summary>
    public NativeWindow? OwnerWindow { get; set; }

    /// <summary>
    /// Callback invoked when the popup is closed
    /// </summary>
    public Action? OnClosed { get; set; }

    public PopupWindow(int x, int y, int width, int height, GraphicsBackendType? backendType = null)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Position = new Vector2D<int>(x, y);
        options.Title = "";
        options.VSync = true;
        options.IsEventDriven = false;
        
        // Popup window configuration
        options.WindowBorder = WindowBorder.Hidden; // No title bar
        options.IsVisible = true;
        options.TopMost = true; // Stay on top

        // Auto-select best backend for platform if not specified
        if (backendType == null)
        {
            if (OperatingSystem.IsWindows())
            {
                backendType = GraphicsBackendType.DirectX11;
            }
            else
            {
                backendType = GraphicsBackendType.OpenGL;
            }
        }

        // Select backend and configure window options
        switch (backendType)
        {
            case GraphicsBackendType.OpenGL:
                options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
                _backend = new OpenGLBackend();
                break;

            case GraphicsBackendType.Vulkan:
                options.API = GraphicsAPI.DefaultVulkan;
                options.ShouldSwapAutomatically = false;
                _backend = new VulkanBackend();
                break;

            case GraphicsBackendType.DirectX11:
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("DirectX11 backend is only available on Windows");
                }
                options.API = GraphicsAPI.None;
                _backend = new D3D11Backend();
                break;

            default:
                throw new ArgumentException($"Unsupported backend type: {backendType}");
        }

        _window = Silk.NET.Windowing.Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClosing;
        _window.FramebufferResize += OnFramebufferResize;
        _window.FocusChanged += OnFocusChanged;
    }

    /// <summary>
    /// Start the popup window's event loop
    /// </summary>
    public void Run() => _window.Run();

    /// <summary>
    /// Close the popup window
    /// </summary>
    public void Close()
    {
        _window.Close();
    }

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

        UpdateDpiScale();
    }

    private void UpdateDpiScale()
    {
        var size = _window.Size;
        var fbSize = _window.FramebufferSize;

        if (size.X > 0 && fbSize.X > 0)
        {
            var dpiScaleX = (float)fbSize.X / size.X;
            var dpiScaleY = (float)fbSize.Y / size.Y;

            RootPanel.SystemDpiScale = Math.Max(dpiScaleX, dpiScaleY);
        }
    }

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (size.X <= 0 || size.Y <= 0) return;

        _backend.Resize(size);

        if (RootPanel != null)
        {
            RootPanel.PanelBounds = new Rect(0, 0, size.X, size.Y);
            RootPanel.InvalidateLayout();
            RootPanel.Layout();
        }
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

    private void OnFocusChanged(bool focused)
    {
        // Close popup when it loses focus (clicked outside)
        if (!focused)
        {
            Close();
        }
    }

    private void OnClosing()
    {
        OnClosed?.Invoke();
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
