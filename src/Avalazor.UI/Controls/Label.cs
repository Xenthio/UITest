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
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14,
                IsAntialias = true
            };

            canvas.DrawText(Text, 10, 20, paint);
        }
    }
}
