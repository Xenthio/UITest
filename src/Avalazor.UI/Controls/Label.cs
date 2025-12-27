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
                SubpixelText = true
            };

            // Draw text with proper positioning
            var metrics = paint.FontMetrics;
            var y = -metrics.Ascent + 10; // Offset from top with padding
            
            canvas.DrawText(Text, 10, y, paint);
        }
    }
}
