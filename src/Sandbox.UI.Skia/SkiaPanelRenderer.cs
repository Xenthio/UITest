using SkiaSharp;

namespace Sandbox.UI.Skia;

/// <summary>
/// SkiaSharp-based panel renderer.
/// Implements IPanelRenderer to render Sandbox.UI panels using SkiaSharp.
/// </summary>
public class SkiaPanelRenderer : IPanelRenderer
{
    private SKCanvas? _canvas;

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

        using var paint = new SKPaint
        {
            Color = ToSKColor(textColor, opacity),
            TextSize = fontSize,
            IsAntialias = true,
            SubpixelText = true,
            LcdRenderText = true,
            Typeface = SKTypeface.FromFamilyName(fontFamily, ToSKFontStyle(style.FontWeight ?? 400))
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

        canvas.DrawText(label.Text, x, y, paint);
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
