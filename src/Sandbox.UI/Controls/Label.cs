using System.Globalization;

namespace Sandbox.UI;

/// <summary>
/// A generic text label. Can be made editable.
/// Based on s&box's Label from engine/Sandbox.Engine/Systems/UI/Controls/Label.cs
/// </summary>
public partial class Label : Panel
{
    /// <summary>
    /// Information about the Text on a per-element scale.
    /// </summary>
    protected StringInfo StringInfo = new();

    internal string? _textToken;
    internal string _text = "";
    internal Rect _textRect;

    public override bool HasContent => true;

    /// <summary>
    /// Can be selected
    /// </summary>
    public bool Selectable { get; set; } = true;

    /// <summary>
    /// If true and the text starts with #, it will be treated as a language token.
    /// </summary>
    public bool Tokenize { get; set; } = true;

    public Label()
    {
        AddClass("label");
        YogaNode?.SetMeasureFunction(MeasureText);
    }

    public Label(string? text, string? classname = null) : this()
    {
        Text = text;
        AddClass(classname);
    }

    Vector2 MeasureText(YGNodeRef node, float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        if (string.IsNullOrEmpty(_text))
            return new Vector2(2, 10);

        // Default measurement - renderers should override this
        var fontSize = ComputedStyle?.FontSize?.GetPixels(16f) ?? 16f;
        var estimated = new Vector2(_text.Length * fontSize * 0.6f, fontSize * 1.2f);

        return estimated;
    }

    /// <summary>
    /// Text to display on the label.
    /// </summary>
    public virtual string? Text
    {
        get => _text;
        set
        {
            value ??= "";

            if (_text == value)
                return;

            _text = value;
            StringInfo.String = value;
            SetNeedsPreLayout();
            YogaNode?.MarkDirty();
        }
    }

    /// <summary>
    /// Set to true if this is rich text (can support some inline html elements)
    /// </summary>
    public bool IsRich { get; set; }

    public virtual void SetProperty(string name, string value)
    {
        if (name == "text")
        {
            Text = value;
            return;
        }
    }

    public virtual void SetContent(string? value)
    {
        Text = value ?? "";
    }

    /// <summary>
    /// Position of the text cursor/caret within the text
    /// </summary>
    public int CaretPosition { get; set; }

    /// <summary>
    /// Amount of characters in the text
    /// </summary>
    public int TextLength => StringInfo.LengthInTextElements;

    public override void FinalLayout(Vector2 offset)
    {
        base.FinalLayout(offset);

        if (!IsVisible) return;
        if (ComputedStyle == null) return;

        _textRect = Box.RectInner;

        // Apply text alignment
        if (ComputedStyle.TextAlign == TextAlign.Center)
        {
            // Center text - renderer will need to handle this
        }
        else if (ComputedStyle.TextAlign == TextAlign.Right)
        {
            // Right align - renderer will need to handle this
        }
    }

    public override void DrawContent(ref RenderState state)
    {
        // Actual text drawing is handled by renderer implementation
        // This method provides hook for derived classes
    }
}
