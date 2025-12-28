using Sandbox.UI;
using Sandbox.UI.Skia;
using SkiaSharp;
using Xunit;

namespace Sandbox.UI.Tests;

/// <summary>
/// Visual rendering tests - test SkiaSharp conversion and basic rendering
/// </summary>
public class VisualRenderingTests
{
    [Fact]
    public void SkiaExtensions_ColorConversion_Works()
    {
        var color = new Color(1.0f, 0.5f, 0.25f, 0.75f);

        var skColor = color.ToSKColor();

        Assert.Equal(255, skColor.Red);
        Assert.True(skColor.Green is >= 126 and <= 128); // Allow small rounding difference
        Assert.True(skColor.Blue is >= 63 and <= 65);
        Assert.True(skColor.Alpha is >= 190 and <= 192);
    }

    [Fact]
    public void SkiaExtensions_RectConversion_Works()
    {
        var rect = new Rect(10, 20, 100, 200);

        var skRect = rect.ToSKRect();

        Assert.Equal(10, skRect.Left);
        Assert.Equal(20, skRect.Top);
        Assert.Equal(110, skRect.Right);
        Assert.Equal(220, skRect.Bottom);
    }

    [Fact]
    public void SkiaExtensions_Vector2Conversion_Works()
    {
        var vector = new Vector2(15.5f, 25.5f);

        var skPoint = vector.ToSKPoint();

        Assert.Equal(15.5f, skPoint.X);
        Assert.Equal(25.5f, skPoint.Y);
    }

    [Fact]
    public void SkiaPanelRenderer_CreatesSuccessfully()
    {
        var renderer = new SkiaPanelRenderer();

        Assert.NotNull(renderer);
    }

    [Fact]
    public void SkiaPanelRenderer_ToSKColor_ConvertsCorrectly()
    {
        var color = new Color(1.0f, 0.0f, 0.0f, 1.0f);

        var skColor = SkiaPanelRenderer.ToSKColor(color);

        Assert.Equal(255, skColor.Red);
        Assert.Equal(0, skColor.Green);
        Assert.Equal(0, skColor.Blue);
        Assert.Equal(255, skColor.Alpha);
    }

    [Fact]
    public void SkiaPanelRenderer_ToSKColor_AppliesOpacity()
    {
        var color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        var skColor = SkiaPanelRenderer.ToSKColor(color, 0.5f);

        Assert.Equal(255, skColor.Red);
        Assert.Equal(255, skColor.Green);
        Assert.Equal(255, skColor.Blue);
        Assert.True(skColor.Alpha is >= 126 and <= 128); // ~0.5 * 255
    }

    [Fact]
    public void SkiaPanelRenderer_ToSKRect_ConvertsCorrectly()
    {
        var rect = new Rect(10, 20, 100, 200);

        var skRect = SkiaPanelRenderer.ToSKRect(rect);

        Assert.Equal(10, skRect.Left);
        Assert.Equal(20, skRect.Top);
        Assert.Equal(110, skRect.Right);
        Assert.Equal(220, skRect.Bottom);
    }

    [Fact]
    public void SkiaPanelRenderer_ToSKFontStyle_ReturnsBoldForWeight700()
    {
        var fontStyle = SkiaPanelRenderer.ToSKFontStyle(700);

        Assert.Equal((int)SKFontStyleWeight.Bold, fontStyle.Weight);
    }

    [Fact]
    public void SkiaPanelRenderer_ToSKFontStyle_ReturnsNormalForWeight400()
    {
        var fontStyle = SkiaPanelRenderer.ToSKFontStyle(400);

        Assert.Equal((int)SKFontStyleWeight.Normal, fontStyle.Weight);
    }

    [Fact]
    public void SKBitmap_CanDrawRect()
    {
        // Test that basic SkiaSharp drawing works
        using var bitmap = new SKBitmap(100, 100);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(10, 10, 50, 50, paint);

        // Verify red pixel inside the drawn rect
        var insidePixel = bitmap.GetPixel(25, 25);
        Assert.Equal(255, insidePixel.Red);
        Assert.Equal(0, insidePixel.Green);
        Assert.Equal(0, insidePixel.Blue);

        // Verify white pixel outside the drawn rect
        var outsidePixel = bitmap.GetPixel(75, 75);
        Assert.Equal(255, outsidePixel.Red);
        Assert.Equal(255, outsidePixel.Green);
        Assert.Equal(255, outsidePixel.Blue);
    }

    [Fact]
    public void SKBitmap_CanDrawText()
    {
        // Test that text drawing works
        using var bitmap = new SKBitmap(200, 50);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 20,
            IsAntialias = true
        };
        canvas.DrawText("Hello", 10, 30, paint);

        // The bitmap should have some non-white pixels where text is drawn
        bool hasNonWhitePixels = false;
        for (int x = 0; x < bitmap.Width && !hasNonWhitePixels; x++)
        {
            for (int y = 0; y < bitmap.Height && !hasNonWhitePixels; y++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel != SKColors.White)
                {
                    hasNonWhitePixels = true;
                }
            }
        }

        Assert.True(hasNonWhitePixels, "Text should render non-white pixels");
    }
}
