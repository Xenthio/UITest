using Microsoft.AspNetCore.Components;

namespace UITest.UI;

/// <summary>
/// Base class for UI components
/// </summary>
public abstract class UIComponent : ComponentBase
{
    /// <summary>
    /// Get the render tree checksum for this component
    /// Override this to provide a unique identifier for the component's render tree
    /// </summary>
    protected virtual string GetRenderTreeChecksum()
    {
        return GetType().FullName ?? "unknown";
    }

    /// <summary>
    /// Build a hash for the component state
    /// Override this to control when the component should re-render
    /// </summary>
    protected virtual int BuildHash()
    {
        return 0;
    }
}
