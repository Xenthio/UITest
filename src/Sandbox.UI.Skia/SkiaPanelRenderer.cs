using SkiaSharp;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics;

namespace Sandbox.UI.Skia;

/// <summary>
/// SkiaSharp-based panel renderer.
/// Implements IPanelRenderer to render Sandbox.UI panels using SkiaSharp.
/// </summary>
public class SkiaPanelRenderer : IPanelRenderer
{
    private SKCanvas? _canvas;
    
    // Tolerance for comparing border widths to determine if they're uniform
    private const float BorderWidthTolerance = 0.1f;
    
    // Cache typefaces to avoid recreation each frame (which can cause font rendering issues on resize).
    // This cache is bounded by the finite number of font family/style combinations used by the application.
    // Typefaces are long-lived resources and should not be frequently created/destroyed.
    // Use ConcurrentDictionary for thread safety when multiple windows are rendering simultaneously.
    private static readonly ConcurrentDictionary<(string family, SKFontStyle style), SKTypeface> _typefaceCache = new();

    /// <summary>
    /// Static constructor to set up text measurement for Labels (backward compatibility)
    /// </summary>
    static SkiaPanelRenderer()
    {
        // Register text measurement function for Label layout calculations
        // This ensures measurement works even if RegisterAsActiveRenderer isn't called
        Label.TextMeasureFunc = MeasureTextStatic;
    }

    /// <summary>
    /// Ensures the static constructor has run and text measurement is available.
    /// Call this before creating any panels to ensure accurate text layout.
    /// </summary>
    /// <returns>True when initialization is complete</returns>
    public static bool EnsureInitialized()
    {
        // Simply accessing this triggers the static constructor if not already run
        return Label.TextMeasureFunc != null;
    }

    /// <summary>
    /// Register this renderer as the active renderer for text measurement.
    /// Call this once when the renderer is created to enable accurate Label layout.
    /// </summary>
    public void RegisterAsActiveRenderer()
    {
        Label.TextMeasureFunc = MeasureText;
    }

    /// <summary>
    /// Measure text using SkiaSharp for accurate layout calculations (IPanelRenderer implementation)
    /// </summary>
    public Vector2 MeasureText(string text, string? fontFamily, float fontSize, int fontWeight)
    {
        return MeasureTextStatic(text, fontFamily, fontSize, fontWeight);
    }

    /// <summary>
    /// Static text measurement for use before renderer instance is created
    /// </summary>
    private static Vector2 MeasureTextStatic(string text, string? fontFamily, float fontSize, int fontWeight)
    {
        var fontStyle = ToSKFontStyleStatic(fontWeight);
        var typeface = GetCachedTypefaceStatic(fontFamily ?? "Arial", fontStyle);
        
        using var paint = new SKPaint
        {
            TextSize = fontSize,
            Typeface = typeface,
            IsAntialias = true
        };
        
        var width = paint.MeasureText(text);
        var metrics = paint.FontMetrics;
        var height = metrics.Descent - metrics.Ascent;
        
        // Add 1 pixel buffer to prevent truncation (matches s&box's CeilToInt + 1 pattern)
        return new Vector2((float)Math.Ceiling(width) + 1f, (float)Math.Ceiling(height));
    }

    private static SKFontStyle ToSKFontStyleStatic(int weight)
    {
        var skWeight = weight switch
        {
            <= 100 => SKFontStyleWeight.Thin,
            <= 200 => SKFontStyleWeight.ExtraLight,
            <= 300 => SKFontStyleWeight.Light,
            <= 400 => SKFontStyleWeight.Normal,
            <= 500 => SKFontStyleWeight.Medium,
            <= 600 => SKFontStyleWeight.SemiBold,
            <= 700 => SKFontStyleWeight.Bold,
            <= 800 => SKFontStyleWeight.ExtraBold,
            _ => SKFontStyleWeight.Black
        };

        return new SKFontStyle(skWeight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
    }

    private static SKTypeface GetCachedTypefaceStatic(string fontFamily, SKFontStyle fontStyle)
    {
        return _typefaceCache.GetOrAdd((fontFamily, fontStyle), key =>
        {
            var typeface = SKTypeface.FromFamilyName(key.family, key.style);
            return typeface ?? SKTypeface.Default;
        });
    }

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

        // Apply transform matrix if present
        var hasTransform = ApplyPanelTransform(_canvas, panel);

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

    /// <summary>
    /// Apply the panel's transform matrix to the canvas.
    /// </summary>
    /// <param name="canvas">The canvas to transform</param>
    /// <param name="panel">The panel whose transform to apply</param>
    /// <returns>True if a transform was applied</returns>
    private bool ApplyPanelTransform(SKCanvas canvas, Panel panel)
    {
        var style = panel.ComputedStyle;
        if (style == null) return false;
        
        // Check if transform is empty
        if (style.Transform?.IsEmpty() ?? true) return false;
        if (panel.TransformMatrix == System.Numerics.Matrix4x4.Identity) return false;
        
        var rect = panel.Box.Rect;
        
        // Get transform origin (defaults to center)
        var originX = style.TransformOriginX?.GetPixels(rect.Width) ?? rect.Width * 0.5f;
        var originY = style.TransformOriginY?.GetPixels(rect.Height) ?? rect.Height * 0.5f;
        
        // Calculate origin in absolute coordinates
        var absoluteOriginX = rect.Left + originX;
        var absoluteOriginY = rect.Top + originY;
        
        // Translate to origin, apply transform, translate back
        canvas.Translate(absoluteOriginX, absoluteOriginY);
        
        // Convert Matrix4x4 to SKMatrix (use 2D portion)
        var m = panel.TransformMatrix;
        var skMatrix = new SKMatrix(
            m.M11, m.M21, m.M41,  // First row
            m.M12, m.M22, m.M42,  // Second row
            m.M14, m.M24, m.M44   // Perspective row
        );
        
        canvas.Concat(ref skMatrix);
        canvas.Translate(-absoluteOriginX, -absoluteOriginY);
        
        // Update GlobalMatrix for child panels
        // Note: We don't have full matrix chain support yet, but this provides the local transform
        panel.LocalMatrix = panel.TransformMatrix;
        panel.GlobalMatrix = panel.TransformMatrix;
        
        return true;
    }

    private void DrawBackground(SKCanvas canvas, Panel panel, ref RenderState state)
    {
        var style = panel.ComputedStyle;
        if (style == null) return;

        var rect = panel.Box.Rect;
        var skRect = ToSKRect(rect);
        var opacity = panel.Opacity * state.RenderOpacity;

        // Border radius
        var radiusTL = style.BorderTopLeftRadius?.GetPixels(1f) ?? 0;
        var radiusTR = style.BorderTopRightRadius?.GetPixels(1f) ?? 0;
        var radiusBL = style.BorderBottomLeftRadius?.GetPixels(1f) ?? 0;
        var radiusBR = style.BorderBottomRightRadius?.GetPixels(1f) ?? 0;
        var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBL > 0 || radiusBR > 0;
        var avgRadius = hasRadius ? (radiusTL + radiusTR + radiusBL + radiusBR) / 4f : 0f;

        // Background gradient takes priority over solid color
        if (style.BackgroundGradient.HasValue && style.BackgroundGradient.Value.IsValid)
        {
            DrawGradientBackground(canvas, skRect, style.BackgroundGradient.Value, opacity, hasRadius, avgRadius);
        }
        // Background color
        else if (style.BackgroundColor.HasValue && style.BackgroundColor.Value.a > 0)
        {
            var color = ToSKColor(style.BackgroundColor.Value, opacity);

            using var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            if (hasRadius)
            {
                canvas.DrawRoundRect(skRect, avgRadius, avgRadius, paint);
            }
            else
            {
                canvas.DrawRect(skRect, paint);
            }
        }
        
        // Background image
        DrawBackgroundImage(canvas, panel, skRect, opacity, hasRadius, avgRadius);

        // Border
        DrawBorder(canvas, panel, ref state);
    }
    
    private void DrawBackgroundImage(SKCanvas canvas, Panel panel, SKRect skRect, float opacity, bool hasRadius, float avgRadius)
    {
        var style = panel.ComputedStyle;
        if (style?.BackgroundImage == null || string.IsNullOrEmpty(style.BackgroundImage.Path))
            return;
            
        var texture = style.BackgroundImage;
        
        // Load texture if not already loaded
        if (texture.NativeHandle == null)
        {
            texture.LoadData();
        }
        
        // Try to get SKImage from native handle
        SKImage? image = texture.NativeHandle as SKImage;
        if (image == null)
        {
            // Try to load from path using file system
            image = LoadTextureFromPath(texture.Path);
            if (image != null)
            {
                texture.NativeHandle = image;
                texture.Width = image.Width;
                texture.Height = image.Height;
            }
        }
        
        if (image == null) return;
        
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, (byte)(255 * opacity)),
            FilterQuality = SKFilterQuality.High,
            IsAntialias = true
        };
        
        // Apply clipping if has border radius
        if (hasRadius)
        {
            canvas.Save();
            var rrect = new SKRoundRect(skRect, avgRadius, avgRadius);
            canvas.ClipRoundRect(rrect, SKClipOperation.Intersect, true);
        }
        
        // Calculate destination rect based on background-size (default is cover-like behavior)
        var srcRect = new SKRect(0, 0, image.Width, image.Height);
        var dstRect = CalculateBackgroundImageRect(skRect, image.Width, image.Height, style);
        
        canvas.DrawImage(image, srcRect, dstRect, paint);
        
        if (hasRadius)
        {
            canvas.Restore();
        }
    }
    
    private SKRect CalculateBackgroundImageRect(SKRect container, int imageWidth, int imageHeight, Styles style)
    {
        // Default: scale to fit container while maintaining aspect ratio (cover)
        float containerAspect = container.Width / container.Height;
        float imageAspect = (float)imageWidth / imageHeight;
        
        float destWidth, destHeight;
        
        if (imageAspect > containerAspect)
        {
            // Image is wider - fit height
            destHeight = container.Height;
            destWidth = destHeight * imageAspect;
        }
        else
        {
            // Image is taller - fit width
            destWidth = container.Width;
            destHeight = destWidth / imageAspect;
        }
        
        // Center the image
        float x = container.Left + (container.Width - destWidth) / 2;
        float y = container.Top + (container.Height - destHeight) / 2;
        
        return new SKRect(x, y, x + destWidth, y + destHeight);
    }
    
    // Cache for loaded texture images
    private static readonly ConcurrentDictionary<string, SKImage?> _textureCache = new();
    
    private SKImage? LoadTextureFromPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        // Check cache first
        if (_textureCache.TryGetValue(path, out var cached))
            return cached;
            
        SKImage? image = null;
        
        try
        {
            // Try to load as file path
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                image = SKImage.FromEncodedData(stream);
            }
            // Try common asset paths
            else
            {
                var possiblePaths = new[]
                {
                    path,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.TrimStart('/')),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", path.TrimStart('/')),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", path.TrimStart('/'))
                };
                
                foreach (var possiblePath in possiblePaths)
                {
                    if (File.Exists(possiblePath))
                    {
                        using var stream = File.OpenRead(possiblePath);
                        image = SKImage.FromEncodedData(stream);
                        if (image != null) break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load texture '{path}': {ex.Message}");
        }
        
        // Cache result (even if null to avoid repeated load attempts)
        _textureCache[path] = image;
        return image;
    }

    private void DrawGradientBackground(SKCanvas canvas, SKRect rect, GradientInfo gradient, float opacity, bool hasRadius, float avgRadius)
    {
        SKShader? shader = null;

        if (gradient.GradientType == GradientInfo.GradientTypes.Linear)
        {
            shader = CreateLinearGradientShader(rect, gradient, opacity);
        }
        else if (gradient.GradientType == GradientInfo.GradientTypes.Radial)
        {
            shader = CreateRadialGradientShader(rect, gradient, opacity);
        }

        if (shader == null) return;

        using (shader)
        using (var paint = new SKPaint
        {
            Shader = shader,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        })
        {
            if (hasRadius)
            {
                canvas.DrawRoundRect(rect, avgRadius, avgRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
    }

    private SKShader? CreateLinearGradientShader(SKRect rect, GradientInfo gradient, float opacity)
    {
        var colorOffsets = gradient.ColorOffsets;
        if (colorOffsets.IsDefaultOrEmpty || colorOffsets.Length < 2)
            return null;

        // Calculate start and end points based on angle
        // CSS angles: 0deg = to top, 90deg = to right, 180deg = to bottom (default)
        var angle = gradient.Angle;
        var centerX = rect.MidX;
        var centerY = rect.MidY;
        var halfWidth = rect.Width / 2f;
        var halfHeight = rect.Height / 2f;

        // Calculate the length needed to cover the entire rect at this angle
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        var length = MathF.Abs(halfWidth * sin) + MathF.Abs(halfHeight * cos);

        var startX = centerX - sin * length;
        var startY = centerY - cos * length;
        var endX = centerX + sin * length;
        var endY = centerY + cos * length;

        var colors = new SKColor[colorOffsets.Length];
        var positions = new float[colorOffsets.Length];

        for (int i = 0; i < colorOffsets.Length; i++)
        {
            colors[i] = ToSKColor(colorOffsets[i].color, opacity);
            positions[i] = colorOffsets[i].offset ?? (float)i / (colorOffsets.Length - 1);
        }

        return SKShader.CreateLinearGradient(
            new SKPoint(startX, startY),
            new SKPoint(endX, endY),
            colors,
            positions,
            SKShaderTileMode.Clamp
        );
    }

    private SKShader? CreateRadialGradientShader(SKRect rect, GradientInfo gradient, float opacity)
    {
        var colorOffsets = gradient.ColorOffsets;
        if (colorOffsets.IsDefaultOrEmpty || colorOffsets.Length < 2)
            return null;

        // GetPixels handles the percentage conversion correctly
        var centerX = rect.Left + gradient.OffsetX.GetPixels(rect.Width);
        var centerY = rect.Top + gradient.OffsetY.GetPixels(rect.Height);
        var radius = MathF.Max(rect.Width, rect.Height) / 2f;

        var colors = new SKColor[colorOffsets.Length];
        var positions = new float[colorOffsets.Length];

        for (int i = 0; i < colorOffsets.Length; i++)
        {
            colors[i] = ToSKColor(colorOffsets[i].color, opacity);
            positions[i] = colorOffsets[i].offset ?? (float)i / (colorOffsets.Length - 1);
        }

        return SKShader.CreateRadialGradient(
            new SKPoint(centerX, centerY),
            radius,
            colors,
            positions,
            SKShaderTileMode.Clamp
        );
    }

    private void DrawBorder(SKCanvas canvas, Panel panel, ref RenderState state)
    {
        var style = panel.ComputedStyle;
        if (style == null) return;

        var rect = panel.Box.Rect;
        var skRect = ToSKRect(rect);
        var opacity = panel.Opacity * state.RenderOpacity;

        // Get border widths
        var leftWidth = style.BorderLeftWidth?.GetPixels(1f) ?? 0;
        var topWidth = style.BorderTopWidth?.GetPixels(1f) ?? 0;
        var rightWidth = style.BorderRightWidth?.GetPixels(1f) ?? 0;
        var bottomWidth = style.BorderBottomWidth?.GetPixels(1f) ?? 0;
        
        // Check if any border exists
        var hasBorder = leftWidth > 0 || topWidth > 0 || rightWidth > 0 || bottomWidth > 0;
        if (!hasBorder) return;

        // Get border radius
        var radiusTL = style.BorderTopLeftRadius?.GetPixels(1f) ?? 0;
        var radiusTR = style.BorderTopRightRadius?.GetPixels(1f) ?? 0;
        var radiusBL = style.BorderBottomLeftRadius?.GetPixels(1f) ?? 0;
        var radiusBR = style.BorderBottomRightRadius?.GetPixels(1f) ?? 0;
        var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBL > 0 || radiusBR > 0;
        var avgRadius = hasRadius ? (radiusTL + radiusTR + radiusBL + radiusBR) / 4f : 0f;

        // Check if all borders are uniform (same color and width)
        var uniformColor = style.BorderLeftColor == style.BorderTopColor &&
                          style.BorderTopColor == style.BorderRightColor &&
                          style.BorderRightColor == style.BorderBottomColor;
        var uniformWidth = Math.Abs(leftWidth - topWidth) < BorderWidthTolerance &&
                          Math.Abs(topWidth - rightWidth) < BorderWidthTolerance &&
                          Math.Abs(rightWidth - bottomWidth) < BorderWidthTolerance;
        var borderWidth = (leftWidth + topWidth + rightWidth + bottomWidth) / 4f;

        // If borders are uniform and we have radius, draw a rounded border
        if (hasRadius && uniformColor && uniformWidth && style.BorderLeftColor.HasValue)
        {
            var color = ToSKColor(style.BorderLeftColor.Value, opacity);
            using var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = borderWidth,
                IsAntialias = true
            };

            // Adjust rect to account for stroke being drawn on center of path
            var adjustedRect = new SKRect(
                skRect.Left + borderWidth / 2,
                skRect.Top + borderWidth / 2,
                skRect.Right - borderWidth / 2,
                skRect.Bottom - borderWidth / 2
            );

            canvas.DrawRoundRect(adjustedRect, avgRadius, avgRadius, paint);
        }
        else
        {
            // Fall back to simple rectangle borders (non-rounded)
            // Left border
            if (leftWidth > 0 && style.BorderLeftColor.HasValue)
            {
                var color = ToSKColor(style.BorderLeftColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Top, leftWidth, rect.Height, paint);
            }

            // Top border
            if (topWidth > 0 && style.BorderTopColor.HasValue)
            {
                var color = ToSKColor(style.BorderTopColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Top, rect.Width, topWidth, paint);
            }

            // Right border
            if (rightWidth > 0 && style.BorderRightColor.HasValue)
            {
                var color = ToSKColor(style.BorderRightColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Right - rightWidth, rect.Top, rightWidth, rect.Height, paint);
            }

            // Bottom border
            if (bottomWidth > 0 && style.BorderBottomColor.HasValue)
            {
                var color = ToSKColor(style.BorderBottomColor.Value, opacity);
                using var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawRect(rect.Left, rect.Bottom - bottomWidth, rect.Width, bottomWidth, paint);
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

        // Process text according to white-space style
        var processedText = label.ProcessWhiteSpace(label.Text);
        if (string.IsNullOrEmpty(processedText)) return;

        var opacity = label.Opacity * state.RenderOpacity;
        var textColor = style.FontColor ?? new Color(0, 0, 0, 1);
        var fontSize = style.FontSize?.GetPixels(16f) ?? 16f;
        var fontFamily = style.FontFamily ?? "Arial";
        var fontStyle = ToSKFontStyle(style.FontWeight ?? 400);
        var fontSmooth = style.FontSmooth ?? FontSmooth.Auto;
        
        // Get or create cached typeface
        var typeface = GetCachedTypeface(fontFamily, fontStyle);

        using var paint = new SKPaint
        {
            Color = ToSKColor(textColor, opacity),
            TextSize = fontSize,
            IsAntialias = fontSmooth != FontSmooth.None,
            SubpixelText = fontSmooth != FontSmooth.None,
            // Note: LcdRenderText can cause rendering issues when the background changes 
            // or when rendering to offscreen textures, so only enable for subpixel antialiasing
            LcdRenderText = fontSmooth == FontSmooth.SubpixelAntialiased,
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
            var textWidth = paint.MeasureText(processedText);
            x = rect.Left + (rect.Width - textWidth) / 2;
        }
        else if (style.TextAlign == TextAlign.Right)
        {
            var textWidth = paint.MeasureText(processedText);
            x = rect.Right - textWidth;
        }

        // Check if text needs wrapping (only if width is constrained)
        var textWidth2 = paint.MeasureText(processedText);
        var shouldWrap = rect.Width > 0 && textWidth2 > rect.Width && style.WhiteSpace != WhiteSpace.NoWrap;

        if (shouldWrap)
        {
            DrawWrappedText(canvas, processedText, paint, rect, x, y, metrics, style.TextAlign);
        }
        else
        {
            canvas.DrawText(processedText, x, y, paint);
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
        return _typefaceCache.GetOrAdd(key, k => SKTypeface.FromFamilyName(k.family, k.style));
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
