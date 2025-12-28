using SkiaSharp;

namespace Sandbox.UI.Skia;

/// <summary>
/// SkiaSharp-based panel renderer.
/// Implements IPanelRenderer to render Sandbox.UI panels using SkiaSharp.
/// </summary>
public class SkiaPanelRenderer : IPanelRenderer
{
    private SKCanvas? _canvas;
    
    // Cache typefaces to avoid recreation each frame (which can cause font rendering issues on resize).
    // This cache is bounded by the finite number of font family/style combinations used by the application.
    // Typefaces are long-lived resources and should not be frequently created/destroyed.
    private static readonly Dictionary<(string family, SKFontStyle style), SKTypeface> _typefaceCache = new();

    public Rect Screen { get; private set; }

    /// <summary>
    /// The current SkiaSharp canvas being rendered to
    /// </summary>
    public SKCanvas? Canvas => _canvas;

    /// <summary>
    /// Render a root panel to the given canvas
    /// </summary>
    public void Render(SKCanvas canvas, RootPanel panel, float opacity = 1.0f)
    {
        _canvas = canvas;
        Screen = panel.PanelBounds;

        var state = new RenderState
        {
            X = Screen.Left,
            Y = Screen.Top,
            Width = Screen.Width,
            Height = Screen.Height,
            RenderOpacity = opacity
        };

        Render(panel, state);
    }

    public void Render(RootPanel panel, float opacity = 1.0f)
    {
        if (_canvas == null)
            throw new InvalidOperationException("Canvas not set. Call Render(SKCanvas, RootPanel) instead.");

        Screen = panel.PanelBounds;

        var state = new RenderState
        {
            X = Screen.Left,
            Y = Screen.Top,
            Width = Screen.Width,
            Height = Screen.Height,
            RenderOpacity = opacity
        };

        Render(panel, state);
    }

    public void Render(Panel panel, RenderState state)
    {
        if (_canvas == null) return;
        if (panel.ComputedStyle == null) return;
        if (!panel.IsVisible) return;

        _canvas.Save();

        // Draw background
        if (panel.HasBackground)
        {
            DrawBackground(_canvas, panel, ref state);
        }

        // Draw content (text, images, etc.)
        if (panel.HasContent)
        {
            DrawContent(_canvas, panel, ref state);
        }

        // Draw children
        if (panel.HasChildren)
        {
            panel.RenderChildren(this, ref state);
        }

        _canvas.Restore();
    }

    private void DrawBackground(SKCanvas canvas, Panel panel, ref RenderState state)
    {
        var style = panel.ComputedStyle;
        if (style == null) return;

        var rect = panel.Box.Rect;
        var skRect = ToSKRect(rect);

        // Background color
        if (style.BackgroundColor.HasValue && style.BackgroundColor.Value.a > 0)
        {
            var opacity = panel.Opacity * state.RenderOpacity;
            var color = ToSKColor(style.BackgroundColor.Value, opacity);

            using var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Border radius
            var radiusTL = style.BorderTopLeftRadius?.GetPixels(1f) ?? 0;
            var radiusTR = style.BorderTopRightRadius?.GetPixels(1f) ?? 0;
            var radiusBL = style.BorderBottomLeftRadius?.GetPixels(1f) ?? 0;
            var radiusBR = style.BorderBottomRightRadius?.GetPixels(1f) ?? 0;

            if (radiusTL > 0 || radiusTR > 0 || radiusBL > 0 || radiusBR > 0)
            {
                // Use average radius for now (SKRoundRect supports per-corner but more complex)
                var avgRadius = (radiusTL + radiusTR + radiusBL + radiusBR) / 4f;
                canvas.DrawRoundRect(skRect, avgRadius, avgRadius, paint);
            }
            else
            {
                canvas.DrawRect(skRect, paint);
            }
        }

        // Border
        DrawBorder(canvas, panel, ref state);
    }

    private void DrawBorder(SKCanvas canvas, Panel panel, ref RenderState state)
    {
        var style = panel.ComputedStyle;
        if (style == null) return;

        var rect = panel.Box.Rect;
        var opacity = panel.Opacity * state.RenderOpacity;

        // Left border
        if (style.BorderLeftColor.HasValue && style.BorderLeftWidth.HasValue)
        {
            var width = style.BorderLeftWidth.Value.GetPixels(1f);
            if (width > 0)
            {
                var color = ToSKColor(style.BorderLeftColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Top, width, rect.Height, paint);
            }
        }

        // Top border
        if (style.BorderTopColor.HasValue && style.BorderTopWidth.HasValue)
        {
            var width = style.BorderTopWidth.Value.GetPixels(1f);
            if (width > 0)
            {
                var color = ToSKColor(style.BorderTopColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Top, rect.Width, width, paint);
            }
        }

        // Right border
        if (style.BorderRightColor.HasValue && style.BorderRightWidth.HasValue)
        {
            var width = style.BorderRightWidth.Value.GetPixels(1f);
            if (width > 0)
            {
                var color = ToSKColor(style.BorderRightColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Right - width, rect.Top, width, rect.Height, paint);
            }
        }

        // Bottom border
        if (style.BorderBottomColor.HasValue && style.BorderBottomWidth.HasValue)
        {
            var width = style.BorderBottomWidth.Value.GetPixels(1f);
            if (width > 0)
            {
                var color = ToSKColor(style.BorderBottomColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Bottom - width, rect.Width, width, paint);
            }
        }
    }

    private void DrawContent(SKCanvas canvas, Panel panel, ref RenderState state)
    {
        // Draw based on panel type
        if (panel is Label label)
        {
            DrawLabel(canvas, label, ref state);
        }
        else if (panel is Image image)
        {
            DrawImage(canvas, image, ref state);
        }
        else
        {
            // Allow custom content drawing
            panel.DrawContent(ref state);
        }
    }

    private void DrawLabel(SKCanvas canvas, Label label, ref RenderState state)
    {
        if (string.IsNullOrEmpty(label.Text)) return;

        var style = label.ComputedStyle;
        if (style == null) return;

        var opacity = label.Opacity * state.RenderOpacity;
        var textColor = style.Color ?? new Color(0, 0, 0, 1);
        var fontSize = style.FontSize?.GetPixels(16f) ?? 16f;
        var fontFamily = style.FontFamily ?? "Arial";
        var fontStyle = ToSKFontStyle(style.FontWeight ?? 400);
        
        // Get or create cached typeface
        var typeface = GetCachedTypeface(fontFamily, fontStyle);

        using var paint = new SKPaint
        {
            Color = ToSKColor(textColor, opacity),
            TextSize = fontSize,
            IsAntialias = true,
            SubpixelText = true,
            // Note: LcdRenderText is not used as it can cause rendering issues when
            // the background changes or when rendering to offscreen textures
            Typeface = typeface
        };

        // Get text rect
        var rect = label.Box.RectInner;

        // Calculate text position
        var metrics = paint.FontMetrics;
        var x = rect.Left;
        var y = rect.Top - metrics.Ascent;

        // Apply text alignment
        if (style.TextAlign == TextAlign.Center)
        {
            var textWidth = paint.MeasureText(label.Text);
            x = rect.Left + (rect.Width - textWidth) / 2;
        }
        else if (style.TextAlign == TextAlign.Right)
        {
            var textWidth = paint.MeasureText(label.Text);
            x = rect.Right - textWidth;
        }

        // Check if text needs wrapping (only if width is constrained)
        var textWidth2 = paint.MeasureText(label.Text);
        var shouldWrap = rect.Width > 0 && textWidth2 > rect.Width && style.WordWrap != WordWrap.NoWrap;

        if (shouldWrap)
        {
            DrawWrappedText(canvas, label.Text, paint, rect, x, y, metrics, style.TextAlign);
        }
        else
        {
            canvas.DrawText(label.Text, x, y, paint);
        }
    }

    private void DrawWrappedText(SKCanvas canvas, string text, SKPaint paint, Rect rect, float startX, float startY, SKFontMetrics metrics, TextAlign? textAlign)
    {
        var lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;
        var y = startY;
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            var testWidth = paint.MeasureText(testLine);

            if (testWidth > rect.Width && !string.IsNullOrEmpty(currentLine))
            {
                // Draw the current line
                var x = rect.Left;
                if (textAlign == TextAlign.Center)
                {
                    var lineWidth = paint.MeasureText(currentLine);
                    x = rect.Left + (rect.Width - lineWidth) / 2;
                }
                else if (textAlign == TextAlign.Right)
                {
                    var lineWidth = paint.MeasureText(currentLine);
                    x = rect.Right - lineWidth;
                }

                canvas.DrawText(currentLine, x, y, paint);
                currentLine = word;
                y += lineHeight;

                // Stop if we've exceeded the rect height
                if (y - metrics.Ascent > rect.Bottom)
                    break;
            }
            else
            {
                currentLine = testLine;
            }
        }

        // Draw the last line if we haven't exceeded bounds
        if (!string.IsNullOrEmpty(currentLine) && y - metrics.Ascent <= rect.Bottom)
        {
            var x = rect.Left;
            if (textAlign == TextAlign.Center)
            {
                var lineWidth = paint.MeasureText(currentLine);
                x = rect.Left + (rect.Width - lineWidth) / 2;
            }
            else if (textAlign == TextAlign.Right)
            {
                var lineWidth = paint.MeasureText(currentLine);
                x = rect.Right - lineWidth;
            }

            canvas.DrawText(currentLine, x, y, paint);
        }
    }

    private static SKTypeface GetCachedTypeface(string fontFamily, SKFontStyle fontStyle)
    {
        var key = (fontFamily, fontStyle);
        if (!_typefaceCache.TryGetValue(key, out var typeface))
        {
            typeface = SKTypeface.FromFamilyName(fontFamily, fontStyle);
            _typefaceCache[key] = typeface;
        }
        return typeface;
    }

    private void DrawImage(SKCanvas canvas, Image image, ref RenderState state)
    {
        if (string.IsNullOrEmpty(image.TexturePath)) return;

        // Image loading would be handled by a texture system
        // For now, draw a placeholder or skip
        var rect = image.Box.Rect;
        var skRect = ToSKRect(rect);

        // Draw placeholder border
        using var paint = new SKPaint
        {
            Color = SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };
        canvas.DrawRect(skRect, paint);
    }

    #region Conversion Helpers

    public static SKRect ToSKRect(Rect rect)
    {
        return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    public static SKColor ToSKColor(Color color, float opacity = 1.0f)
    {
        return new SKColor(
            (byte)(color.r * 255),
            (byte)(color.g * 255),
            (byte)(color.b * 255),
            (byte)(color.a * opacity * 255)
        );
    }

    public static SKFontStyle ToSKFontStyle(int fontWeight)
    {
        var weight = fontWeight switch
        {
            100 => SKFontStyleWeight.Thin,
            200 => SKFontStyleWeight.ExtraLight,
            300 => SKFontStyleWeight.Light,
            400 => SKFontStyleWeight.Normal,
            500 => SKFontStyleWeight.Medium,
            600 => SKFontStyleWeight.SemiBold,
            700 => SKFontStyleWeight.Bold,
            800 => SKFontStyleWeight.ExtraBold,
            900 => SKFontStyleWeight.Black,
            _ => SKFontStyleWeight.Normal
        };

        return new SKFontStyle(weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }

    #endregion
}
