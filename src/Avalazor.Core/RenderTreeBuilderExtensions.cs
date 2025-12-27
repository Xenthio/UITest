using Microsoft.AspNetCore.Components.Rendering;

namespace Microsoft.AspNetCore.Components.Rendering;

/// <summary>
/// Extension methods for RenderTreeBuilder to support s&box-compatible Razor code generation
/// Based on s&box's Sandbox.UI extensions (MIT licensed)
/// </summary>
public static class RenderTreeBuilderExtensions
{
    /// <summary>
    /// Add location information for debugging (s&box compatibility - noop in Avalazor)
    /// </summary>
    public static void AddLocation(this RenderTreeBuilder builder, string filename, int line, int column)
    {
        // In s&box, this adds source location information for debugging
        // In Avalazor/Blazor, we don't need this - it's a no-op
    }

    /// <summary>
    /// Open an element with optional sourceLineIndex parameter (s&box compatibility)
    /// </summary>
    public static void OpenElement(this RenderTreeBuilder builder, int sequence, string elementName, int? sourceLineIndex)
    {
        // The third parameter is for s&box's source tracking
        // Standard Blazor's OpenElement only takes 2 parameters
        builder.OpenElement(sequence, elementName);
    }

    /// <summary>
    /// Open a component with type parameter (s&box compatibility)
    /// </summary>
    public static void OpenElement<T>(this RenderTreeBuilder builder, int sequence, int? sourceLineIndex) where T : IComponent
    {
        // This is s&box's way of opening components
        // In standard Blazor, we use OpenComponent
        builder.OpenComponent<T>(sequence);
    }
}
