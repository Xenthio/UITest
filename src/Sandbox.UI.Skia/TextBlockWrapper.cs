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
    private FontSmooth _fontSmooth;
    private int _fontHash;

    public Vector2 BlockSize { get; private set; }

    /// <summary>
    /// Update the text and font properties. Recreates the block if properties changed.
    /// </summary>
    public void Update(string text, string? fontFamily, float fontSize, int fontWeight, FontSmooth fontSmooth = FontSmooth.Auto)
    {
        var newHash = HashCode.Combine(text, fontFamily, fontSize, fontWeight, fontSmooth);
        
        if (newHash == _fontHash && _block != null)
            return; // No changes
        
        _fontHash = newHash;
        _text = text;
        _fontFamily = fontFamily;
        _fontSize = fontSize;
        _fontWeight = fontWeight;
        _fontSmooth = fontSmooth;
        
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
    public void Paint(SKCanvas canvas, float x, float y, SKColor color, int selectionStart = 0, int selectionEnd = 0, Color? selectionColor = null)
    {
        if (_block == null)
            return;
        
        // Update text color for this render
        if (_style != null)
        {
            _style.TextColor = color;
        }
        
        // Configure paint options based on font-smooth setting
        // Using Edging property (IsAntialias and LcdRenderText are obsolete in RichTextKit)
        // Default (Auto) now uses SubpixelAntialias for RGB subpixel rendering which provides
        // sharper text on LCD displays by leveraging the RGB subpixel structure
        var edging = _fontSmooth switch
        {
            FontSmooth.None => SKFontEdging.Alias,  // No antialiasing
            FontSmooth.Antialiased => SKFontEdging.SubpixelAntialias,  // LCD subpixel rendering (explicit)
            FontSmooth.GrayscaleAntialiased => SKFontEdging.Antialias,  // Standard grayscale antialiasing 
            FontSmooth.Auto => SKFontEdging.SubpixelAntialias,  // Auto defaults to LCD subpixel rendering
            _ => SKFontEdging.SubpixelAntialias  // Fallback to subpixel for unknown values
        };
        
        var paintOptions = new TextPaintOptions
        {
            Edging = edging,
            // Disable subpixel positioning when aliased rendering is requested for consistency
            SubpixelPositioning = edging != SKFontEdging.Alias,
        };
        
        // Add selection if provided (matches S&box implementation)
        // Only render selection when there's an actual range (start != end)
        if ((selectionStart > 0 || selectionEnd > 0) && selectionStart != selectionEnd)
        {
            paintOptions.Selection = new TextRange(selectionStart, selectionEnd);
            paintOptions.SelectionColor = selectionColor.HasValue 
                ? new SKColor((byte)(selectionColor.Value.r * 255), 
                             (byte)(selectionColor.Value.g * 255), 
                             (byte)(selectionColor.Value.b * 255), 
                             (byte)(selectionColor.Value.a * 255))
                : new SKColor(0, 255, 255, 100); // Default cyan with alpha
        }
        
        // Paint the text block with configured options
        _block.Paint(canvas, new SKPoint(x, y), paintOptions);
    }

    /// <summary>
    /// Get the measured width of the text block.
    /// </summary>
    public float MeasuredWidth => _block?.MeasuredWidth ?? 0;

    /// <summary>
    /// Get the measured height of the text block.
    /// </summary>
    public float MeasuredHeight => _block?.MeasuredHeight ?? 0;

    /// <summary>
    /// Get the caret rect at a given character index.
    /// Returns the position and size of the caret for rendering.
    /// </summary>
    public Rect GetCaretRect(int charIndex)
    {
        if (_block == null)
            return new Rect(0, 0, 2, 10);

        try
        {
            var caretInfo = _block.GetCaretInfo(new CaretPosition { CodePointIndex = charIndex });
            
            return new Rect(
                (float)caretInfo.CaretRectangle.Left,
                (float)caretInfo.CaretRectangle.Top,
                2f, // Standard caret width
                (float)caretInfo.CaretRectangle.Height
            );
        }
        catch (Exception ex)
        {
            // Log and fallback if index is out of range
            Console.WriteLine($"TextBlockWrapper.GetCaretRect: Failed for charIndex {charIndex}: {ex.Message}");
            return new Rect(0, 0, 2, _fontSize * 1.2f);
        }
    }

    /// <summary>
    /// Get the character index at a given position.
    /// Returns -1 if no text or position is invalid.
    /// </summary>
    public int HitTest(float x, float y)
    {
        if (_block == null)
            return -1;

        try
        {
            var hit = _block.HitTest(x, y);
            return hit.ClosestCodePointIndex;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TextBlockWrapper.HitTest: Failed at ({x}, {y}): {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Get the rectangles for a text selection range.
    /// Returns a list of rectangles that cover the selected text.
    /// </summary>
    public List<Rect> GetSelectionRects(int selectionStart, int selectionEnd)
    {
        var rects = new List<Rect>();
        
        if (_block == null || selectionStart >= selectionEnd)
            return rects;

        try
        {
            // Get rectangles for the selection range
            var lines = _block.Lines;
            if (lines == null || lines.Count == 0)
                return rects;

            // For each line, check if it contains part of the selection
            foreach (var line in lines)
            {
                var lineStart = line.Start;
                var lineEnd = line.Start + line.Length;

                // Check if this line overlaps with the selection
                if (lineEnd <= selectionStart || lineStart >= selectionEnd)
                    continue;

                // Calculate the intersection
                var rangeStart = Math.Max(lineStart, selectionStart);
                var rangeEnd = Math.Min(lineEnd, selectionEnd);

                // Get caret positions for start and end of selection in this line
                var startCaret = _block.GetCaretInfo(new CaretPosition { CodePointIndex = rangeStart });
                var endCaret = _block.GetCaretInfo(new CaretPosition { CodePointIndex = rangeEnd });

                var rect = new Rect(
                    (float)startCaret.CaretRectangle.Left,
                    (float)line.YCoord,
                    (float)(endCaret.CaretRectangle.Left - startCaret.CaretRectangle.Left),
                    (float)line.Height
                );

                rects.Add(rect);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TextBlockWrapper.GetSelectionRects: Failed for range ({selectionStart}, {selectionEnd}): {ex.Message}");
            // Return empty list on failure
        }

        return rects;
    }
}
