using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using SkiaSharp;
using Sandbox.UI;
using UIVector2 = Sandbox.UI.Vector2;

namespace Avalazor.UI;

public enum GraphicsBackendType
{
    OpenGL,
    Vulkan,
    DirectX11
}

public class NativeWindow : INativeWindow, IDisposable
{
    private readonly IWindow _window;
    private IGraphicsBackend _backend;
    private GraphicsBackendType _backendType;

    private IInputContext? _input;
    private IMouse? _mouse;
    private IKeyboard? _keyboard;
    private bool _disposed = false;

    public RootPanel? RootPanel { get; set; }

    public NativeWindow(int width = 1280, int height = 720, string title = "Avalazor App", GraphicsBackendType? backendType = null)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
        options.Title = title;
        options.VSync = true;
        options.IsEventDriven = false;

        // Auto-select best backend for platform if not specified
        if (backendType == null)
        {
            if (OperatingSystem.IsWindows())
            {
                backendType = GraphicsBackendType.DirectX11; // Best for Windows
                Console.WriteLine("Auto-selected DirectX11 backend for Windows");
            }
            else
            {
                backendType = GraphicsBackendType.OpenGL; // Works well on Linux/macOS
                Console.WriteLine("Auto-selected OpenGL backend");
            }
        }

        _backendType = backendType.Value;
        PopupWindowManager.BackendType = _backendType;

        // Select backend and configure window options
        switch (backendType)
        {
            case GraphicsBackendType.OpenGL:
                Console.WriteLine("Starting OpenGL backend...");
                options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));
                _backend = new OpenGLBackend();
                break;

            case GraphicsBackendType.Vulkan:
                Console.WriteLine("Starting Vulkan backend...");
                options.API = GraphicsAPI.DefaultVulkan; // Request Vulkan API
                options.ShouldSwapAutomatically = false; // We handle swapchain ourselves
                _backend = new VulkanBackend();
                break;

            case GraphicsBackendType.DirectX11:
                Console.WriteLine("Starting DirectX11 backend...");
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("DirectX11 backend is only available on Windows");
                }
                options.API = GraphicsAPI.None; // D3D11 handles its own context
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

        // Register as main window for popup management
        PopupWindowManager.MainWindow = this;

        // Register the OS window factory for Popup class
        Sandbox.UI.Popup.OSWindowFactory = PopupWindowFactory;
    }

    /// <summary>
    /// Factory function that creates OS-level popup windows for Popup panels
    /// </summary>
    private static object? PopupWindowFactory(Sandbox.UI.Panel popup, Sandbox.UI.Panel sourcePanel, int screenX, int screenY, int width, int height)
    {
        try
        {
            // Get the main window position to offset the popup
            var mainWindow = PopupWindowManager.MainWindow;
            if (mainWindow != null)
            {
                var (winX, winY) = mainWindow.GetPosition();
                screenX += winX;
                screenY += winY;
            }

            // Create a container for the popup content
            // We can't just pass the popup panel directly because it might not have children yet
            // Instead, we create a container and the popup will be its child
            Console.WriteLine($"[PopupWindowFactory] Creating OS window for popup {popup.GetType().Name} with {popup.ChildrenCount} children");
            
            // Create popup window with the popup panel as content
            var osWindow = PopupWindowManager.CreatePopup(screenX, screenY, width, height, popup, sourcePanel);
            return osWindow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NativeWindow] Error in PopupWindowFactory: {ex.Message}");
            Console.WriteLine($"[NativeWindow] Stack trace: {ex.StackTrace}");
            return null;
        }
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

    private void OnFramebufferResize(Vector2D<int> size)
    {
        if (size.X <= 0 || size.Y <= 0) return;

        Console.WriteLine($"[NativeWindow] OnFramebufferResize: {size.X}x{size.Y}");

        // Use framebuffer size directly - this is the actual render buffer size
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
    /// Get the native window position
    /// </summary>
    public (int x, int y) GetPosition()
    {
        return (_window.Position.X, _window.Position.Y);
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