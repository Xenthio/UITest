using SkiaSharp;
using Topten.RichTextKit;

namespace Sandbox.UI.Skia;

/// <summary>
/// Wrapper around RichTextKit's TextBlock for text measurement and rendering.
/// Based on s&box's TextBlock implementation in engine/Sandbox.Engine/Systems/UI/Engine/TextBlock.cs
/// </summary>
internal class TextBlockWrapper
{
    private Topten.RichTextKit.TextBlock? _block;
    private Topten.RichTextKit.Style? _style;
    private readonly Dictionary<int, Vector2> _sizeCache = new();
    
    private string _text = "";
    private string? _fontFamily;
    private float _fontSize;
    private int _fontWeight;
    private int _fontHash;

    public Vector2 BlockSize { get; private set; }

    /// <summary>
    /// Update the text and font properties. Recreates the block if properties changed.
    /// </summary>
    public void Update(string text, string? fontFamily, float fontSize, int fontWeight)
    {
        var newHash = HashCode.Combine(text, fontFamily, fontSize, fontWeight);
        
        if (newHash == _fontHash && _block != null)
            return; // No changes
        
        _fontHash = newHash;
        _text = text;
        _fontFamily = fontFamily;
        _fontSize = fontSize;
        _fontWeight = fontWeight;
        
        // Clear cache when text/font changes
        _sizeCache.Clear();
        
        // Create RichTextKit style
        _style = new Topten.RichTextKit.Style
        {
            FontFamily = fontFamily ?? "Arial",
            FontSize = fontSize,
            FontWeight = fontWeight,
            TextColor = SKColors.White // Will be overridden during rendering
        };
        
        // Create and configure the text block
        _block = new Topten.RichTextKit.TextBlock();
        _block.AddText(text, _style);
    }

    /// <summary>
    /// Measure the text with optional width constraint.
    /// Matches s&box's Measure method.
    /// </summary>
    public Vector2 Measure(float width, float height)
    {
        if (_block == null)
            return new Vector2(0, 0);
        
        // Round width like s&box does
        if (!float.IsNaN(width))
            width = (float)Math.Ceiling(width);
        if (!float.IsNaN(height))
            height = (float)Math.Ceiling(height);
        
        var hash = (int)width;
        if (_sizeCache.TryGetValue(hash, out var size))
            return size;
        
        // Set MaxWidth to enable wrapping (matches s&box)
        _block.MaxWidth = float.IsNaN(width) ? null : (width + 1);
        
        // Measure the block
        var measuredSize = new Vector2(
            (float)Math.Ceiling(_block.MeasuredWidth),
            (float)Math.Ceiling(_block.MeasuredHeight)
        );
        
        BlockSize = measuredSize;
        _sizeCache[hash] = measuredSize;
        
        return measuredSize;
    }

    /// <summary>
    /// Paint the text block to the canvas at the specified position.
    /// </summary>
    public void Paint(SKCanvas canvas, float x, float y, SKColor color)
    {
        if (_block == null)
            return;
        
        // Update text color for this render
        if (_style != null)
        {
            _style.TextColor = color;
        }
        
        // Paint the text block
        _block.Paint(canvas, new SKPoint(x, y));
    }

    /// <summary>
    /// Get the measured width of the text block.
    /// </summary>
    public float MeasuredWidth => _block?.MeasuredWidth ?? 0;

    /// <summary>
    /// Get the measured height of the text block.
    /// </summary>
    public float MeasuredHeight => _block?.MeasuredHeight ?? 0;
}
