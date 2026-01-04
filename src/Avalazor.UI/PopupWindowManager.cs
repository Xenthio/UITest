using Sandbox.UI;

namespace Avalazor.UI;

/// <summary>
/// Manages popup windows in the application.
/// Tracks active popup windows and provides utilities for creating and closing them.
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
    /// Create a popup window at the specified screen position
    /// </summary>
    public static PopupWindow CreatePopup(int screenX, int screenY, int width, int height, Panel content, Panel? sourcePanel = null)
    {
        lock (_lock)
        {
            // Create root panel for the popup
            var rootPanel = new RootPanel();
            rootPanel.PanelBounds = new Rect(0, 0, width, height);
            
            // Add content to root panel
            content.Parent = rootPanel;

            // Create popup window
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

            // Run the popup window in a new thread so it doesn't block the main window
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    popup.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PopupWindowManager] Error running popup: {ex.Message}");
                }
                finally
                {
                    popup.Dispose();
                }
            });
            thread.IsBackground = true;
            thread.Start();

            return popup;
        }
    }

    /// <summary>
    /// Close all active popup windows
    /// </summary>
    public static void CloseAll()
    {
        lock (_lock)
        {
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
