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
    }

    public Label(string text) : this()
    {
        Text = text;
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
