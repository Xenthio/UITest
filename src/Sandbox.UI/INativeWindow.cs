namespace Sandbox.UI;

/// <summary>
/// Interface for native window implementations to allow Window controls to interact with them.
/// Avoids reflection and provides type-safe access to native window properties.
/// </summary>
public interface INativeWindow
{
    /// <summary>
    /// Set the native window title
    /// </summary>
    void SetTitle(string title);

    /// <summary>
    /// Set the native window position
    /// </summary>
    void SetPosition(int x, int y);

    /// <summary>
    /// Set the native window size
    /// </summary>
    void SetSize(int width, int height);
}
