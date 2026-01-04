using Sandbox.UI;
using Silk.NET.Windowing;

namespace Avalazor.UI;

/// <summary>
/// Manages popup windows in the application.
/// Tracks active popup windows and provides utilities for creating and closing them.
/// Uses shared event loop approach for proper window management.
/// </summary>
public static class PopupWindowManager
{
    private static readonly List<PopupWindow> _activePopups = new();
    private static readonly object _lock = new();

    /// <summary>
    /// The main application window. Popups use the same graphics backend.
    /// </summary>
    public static NativeWindow? MainWindow { get; set; }

    /// <summary>
    /// Graphics backend type to use for popups
    /// </summary>
    public static GraphicsBackendType BackendType { get; set; } = GraphicsBackendType.OpenGL;

    /// <summary>
    /// Whether popup windows are enabled. When false, falls back to in-window popups.
    /// </summary>
    public static bool EnablePopupWindows { get; set; } = true;

    /// <summary>
    /// Create a popup window at the specified screen position.
    /// The popup window is created immediately but doesn't block - it shares the main event loop.
    /// </summary>
    public static PopupWindow? CreatePopup(int screenX, int screenY, int width, int height, Panel content, Panel? sourcePanel = null)
    {
        if (!EnablePopupWindows)
        {
            Console.WriteLine("[PopupWindowManager] Popup windows disabled, returning null");
            return null;
        }

        lock (_lock)
        {
            try
            {
                // Create root panel for the popup
                var rootPanel = new RootPanel();
                rootPanel.PanelBounds = new Rect(0, 0, width, height);
                
                // Add content to root panel
                content.Parent = rootPanel;

                // Create popup window - it will handle its own event loop integration
                var popup = new PopupWindow(screenX, screenY, width, height, BackendType);
                popup.RootPanel = rootPanel;
                popup.SourcePanel = sourcePanel;
                popup.OwnerWindow = MainWindow;
                
                // Handle cleanup when popup closes
                popup.OnClosed = () =>
                {
                    lock (_lock)
                    {
                        _activePopups.Remove(popup);
                    }
                };

                _activePopups.Add(popup);

                Console.WriteLine($"[PopupWindowManager] Created popup window at ({screenX}, {screenY}) size {width}x{height}");
                return popup;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PopupWindowManager] Error creating popup: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Close all active popup windows
    /// </summary>
    public static void CloseAll()
    {
        lock (_lock)
        {
            Console.WriteLine($"[PopupWindowManager] Closing {_activePopups.Count} popup windows");
            foreach (var popup in _activePopups.ToArray())
            {
                try
                {
                    popup.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PopupWindowManager] Error closing popup: {ex.Message}");
                }
            }
            _activePopups.Clear();
        }
    }

    /// <summary>
    /// Update all popup windows (called from main window render loop)
    /// </summary>
    internal static void UpdateAll()
    {
        lock (_lock)
        {
            foreach (var popup in _activePopups.ToArray())
            {
                if (!popup.IsValid())
                {
                    _activePopups.Remove(popup);
                }
            }
        }
    }

    /// <summary>
    /// Get the number of active popup windows
    /// </summary>
    public static int ActivePopupCount
    {
        get
        {
            lock (_lock)
            {
                return _activePopups.Count;
            }
        }
    }
}
