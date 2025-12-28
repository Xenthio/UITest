namespace Sandbox.UI;

/// <summary>
/// A generic box that displays a given texture within itself.
/// Based on s&box's Image from engine/Sandbox.Engine/Systems/UI/Controls/Image.cs
/// </summary>
public partial class Image : Panel
{
    /// <summary>
    /// The texture/image path being displayed by this panel
    /// </summary>
    public string? TexturePath { get; set; }

    public override bool HasContent => TexturePath != null;

    public Image()
    {
        YogaNode?.SetMeasureFunction(MeasureTexture);
    }

    /// <summary>
    /// Set the texture from a file path
    /// </summary>
    public virtual void SetTexture(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        TexturePath = name;
        YogaNode?.MarkDirty();
    }

    private float oldScaleToScreen = 1.0f;

    internal override void PreLayout(LayoutCascade cascade)
    {
        base.PreLayout(cascade);

        if (ScaleToScreen != oldScaleToScreen)
        {
            YogaNode?.MarkDirty();
        }
    }

    Vector2 MeasureTexture(YGNodeRef node, float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        // Default measurement - renderers should override this based on actual texture size
        // For now return a placeholder size
        if (string.IsNullOrEmpty(TexturePath))
            return new Vector2(0, 0);

        oldScaleToScreen = ScaleToScreen;

        // Return a reasonable default size - actual measurement depends on loaded texture
        var defaultSize = new Vector2(100 * ScaleToScreen, 100 * ScaleToScreen);

        var exact = YGMeasureMode.Exactly;
        var atMost = YGMeasureMode.AtMost;

        if (widthMode == exact) return new Vector2(width, width);
        if (heightMode == exact) return new Vector2(height, height);

        if (widthMode == atMost && width < defaultSize.x) return new Vector2(width, width);
        if (heightMode == atMost && height < defaultSize.y) return new Vector2(height, height);

        return defaultSize;
    }

    public override void DrawContent(ref RenderState state)
    {
        // Actual image drawing is handled by renderer implementation
    }

    public virtual void SetProperty(string name, string value)
    {
        if (name == "src")
            SetTexture(value);
    }
}
