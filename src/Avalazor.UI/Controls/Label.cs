using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Text label panel
/// </summary>
public class Label : Panel
{
    private string _text = "";

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                MarkNeedsLayout();
            }
        }
    }

    public Label()
    {
        Tag = "label";
        
        // Set Yoga measure function for text content
        if (YogaNode != null)
        {
            YogaNode.SetMeasureFunction(MeasureText);
        }
    }

    public Label(string text) : this()
    {
        Text = text;
    }
    
    /// <summary>
    /// Measure function for Yoga - tells Yoga how big the text is
    /// </summary>
    private YGSize MeasureText(YGNodeRef node, float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        if (string.IsNullOrEmpty(Text))
            return new YGSize { Width = 0, Height = 0 };
        
        var fontSize = ComputedStyle?.FontSize ?? 14;
        
        using var paint = new SKPaint
        {
            TextSize = fontSize,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
        };
        
        // Measure text bounds
        var bounds = new SKRect();
        paint.MeasureText(Text, ref bounds);
        
        // Get font metrics for proper height including ascent/descent
        var metrics = paint.FontMetrics;
        var textHeight = metrics.Descent - metrics.Ascent;
        
        // Add padding
        var paddingLeft = ComputedStyle?.PaddingLeft ?? 0;
        var paddingRight = ComputedStyle?.PaddingRight ?? 0;
        var paddingTop = ComputedStyle?.PaddingTop ?? 0;
        var paddingBottom = ComputedStyle?.PaddingBottom ?? 0;
        
        var measuredWidth = bounds.Width + paddingLeft + paddingRight;
        var measuredHeight = textHeight + paddingTop + paddingBottom;
        
        return new YGSize 
        { 
            Width = measuredWidth, 
            Height = measuredHeight 
        };
    }

    protected override void OnPaint(SKCanvas canvas)
    {
        base.OnPaint(canvas);

        if (!string.IsNullOrEmpty(Text))
        {
            // Use computed style color if available, otherwise black
            var textColor = ComputedStyle?.Color ?? SKColors.Black;
            var fontSize = ComputedStyle?.FontSize ?? 14;
            
            using var paint = new SKPaint
            {
                Color = textColor,
                TextSize = fontSize,
                IsAntialias = true,
                SubpixelText = true,
                LcdRenderText = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
            };

            // Draw text positioned within this panel's bounds (0,0 to Width,Height)
            // Use padding from computed style if available
            var paddingLeft = ComputedStyle?.PaddingLeft ?? 0;
            var paddingTop = ComputedStyle?.PaddingTop ?? 0;
            
            var metrics = paint.FontMetrics;
            var x = paddingLeft;
            var y = paddingTop - metrics.Ascent; // Position baseline
            
            canvas.DrawText(Text, x, y, paint);
        }
    }
}
