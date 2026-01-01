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
    
    // Directories to search for font files
    private static readonly List<string> _fontDirectories = new();
    
    // Cache for font file paths by family name (lowercase)
    private static readonly ConcurrentDictionary<string, string?> _fontFileCache = new();
    
    /// <summary>
    /// Add a directory to search for font files (.ttf, .otf)
    /// </summary>
    public static void AddFontDirectory(string directory)
    {
        if (!_fontDirectories.Contains(directory))
        {
            _fontDirectories.Add(directory);
            // Clear cache when directories change
            _fontFileCache.Clear();
        }
    }

    /// <summary>
    /// Static constructor to set up text measurement for Labels (backward compatibility)
    /// </summary>
    static SkiaPanelRenderer()
    {
        // Register text measurement function for Label layout calculations
        // This ensures measurement works even if RegisterAsActiveRenderer isn't called
        Label.TextMeasureFunc = (text, fontFamily, fontSize, fontWeight, maxWidth, allowWrapping) => 
            MeasureTextStatic(text, fontFamily, fontSize, fontWeight, maxWidth, allowWrapping);
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
        Label.TextMeasureFunc = (text, fontFamily, fontSize, fontWeight, maxWidth, allowWrapping) => 
            MeasureTextStatic(text, fontFamily, fontSize, fontWeight, maxWidth, allowWrapping);
    }

    /// <summary>
    /// Measure text using SkiaSharp for accurate layout calculations (IPanelRenderer implementation)
    /// </summary>
    public Vector2 MeasureText(string text, string? fontFamily, float fontSize, int fontWeight)
    {
        return MeasureTextStatic(text, fontFamily, fontSize, fontWeight, float.NaN, false);
    }

    /// <summary>
    /// Static text measurement for use before renderer instance is created.
    /// Now uses RichTextKit for proper text layout matching s&box.
    /// </summary>
    private static Vector2 MeasureTextStatic(string text, string? fontFamily, float fontSize, int fontWeight, float maxWidth, bool allowWrapping)
    {
        // Create a TextBlock wrapper for measurement
        var textBlock = new TextBlockWrapper();
        textBlock.Update(text, fontFamily, fontSize, fontWeight);
        
        // If wrapping is disabled or no width constraint, measure without width limit
        if (!allowWrapping || float.IsNaN(maxWidth) || maxWidth <= 0)
        {
            return textBlock.Measure(float.NaN, float.NaN);
        }
        
        // Measure with width constraint for wrapping
        return textBlock.Measure(maxWidth, float.NaN);
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
            // First try to load from font file
            var fontFile = FindFontFile(key.family);
            if (fontFile != null)
            {
                var fileTypeface = SKTypeface.FromFile(fontFile);
                if (fileTypeface != null)
                    return fileTypeface;
            }
            
            // Fall back to system font
            var typeface = SKTypeface.FromFamilyName(key.family, key.style);
            return typeface ?? SKTypeface.Default;
        });
    }
    
    /// <summary>
    /// Find a font file by family name in registered directories
    /// </summary>
    private static string? FindFontFile(string fontFamily)
    {
        var lowerFamily = fontFamily.ToLowerInvariant();
        
        return _fontFileCache.GetOrAdd(lowerFamily, family =>
        {
            // Common font file name patterns
            var patterns = new[]
            {
                $"{family}.ttf",
                $"{family}.otf",
                $"{fontFamily}.ttf",
                $"{fontFamily}.otf",
            };
            
            foreach (var dir in _fontDirectories)
            {
                if (!Directory.Exists(dir)) continue;
                
                foreach (var pattern in patterns)
                {
                    var path = Path.Combine(dir, pattern);
                    if (File.Exists(path))
                        return path;
                }
                
                // Also try searching for files containing the font name
                try
                {
                    foreach (var file in Directory.GetFiles(dir, "*.ttf"))
                    {
                        if (Path.GetFileNameWithoutExtension(file).ToLowerInvariant().Contains(family))
                            return file;
                    }
                    foreach (var file in Directory.GetFiles(dir, "*.otf"))
                    {
                        if (Path.GetFileNameWithoutExtension(file).ToLowerInvariant().Contains(family))
                            return file;
                    }
                }
                catch
                {
                    // Ignore directory access errors
                }
            }
            
            return null;
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
        
        // Convert Matrix4x4 to SKMatrix (use 2D portion)
        // The TransformMatrix already contains all the transform operations (translate, rotate, scale)
        // built from PanelTransform.BuildTransform() which handles transform-origin for perspective
        var m = panel.TransformMatrix;
        var skMatrix = new SKMatrix(
            m.M11, m.M21, m.M41,  // First row (scaleX, skewY, translateX)
            m.M12, m.M22, m.M42,  // Second row (skewX, scaleY, translateY)
            m.M14, m.M24, m.M44   // Perspective row
        );
        
        canvas.Concat(ref skMatrix);
        
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

        // Border radius - each corner can have a different radius (matches s&box)
        var radiusTL = style.BorderTopLeftRadius?.GetPixels(1f) ?? 0;
        var radiusTR = style.BorderTopRightRadius?.GetPixels(1f) ?? 0;
        var radiusBL = style.BorderBottomLeftRadius?.GetPixels(1f) ?? 0;
        var radiusBR = style.BorderBottomRightRadius?.GetPixels(1f) ?? 0;
        var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBL > 0 || radiusBR > 0;

        // Background gradient takes priority over solid color
        if (style.BackgroundGradient.HasValue && style.BackgroundGradient.Value.IsValid)
        {
            DrawGradientBackground(canvas, skRect, style.BackgroundGradient.Value, opacity, radiusTL, radiusTR, radiusBR, radiusBL);
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
                using var path = CreateRoundedRectPath(skRect, radiusTL, radiusTR, radiusBR, radiusBL);
                canvas.DrawPath(path, paint);
            }
            else
            {
                canvas.DrawRect(skRect, paint);
            }
        }
        
        // Background image
        DrawBackgroundImage(canvas, panel, skRect, opacity, radiusTL, radiusTR, radiusBR, radiusBL);

        // Border
        DrawBorder(canvas, panel, ref state);
    }
    
    /// <summary>
    /// Create a rounded rectangle path with per-corner radii (matches s&box behavior)
    /// Uses SKRoundRect for proper rounded corners
    /// </summary>
    private SKPath CreateRoundedRectPath(SKRect rect, float radiusTL, float radiusTR, float radiusBR, float radiusBL)
    {
        var path = new SKPath();
        
        // Create an SKRoundRect with individual corner radii
        // SKRoundRect expects radii in a specific order
        var roundRect = new SKRoundRect();
        
        // SetRectRadii takes an array of 4 SKPoint values, one for each corner
        // Order: top-left, top-right, bottom-right, bottom-left
        // Each SKPoint has X (horizontal radius) and Y (vertical radius)
        roundRect.SetRectRadii(rect, new SKPoint[]
        {
            new SKPoint(radiusTL, radiusTL),  // Top-left
            new SKPoint(radiusTR, radiusTR),  // Top-right
            new SKPoint(radiusBR, radiusBR),  // Bottom-right
            new SKPoint(radiusBL, radiusBL)   // Bottom-left
        });
        
        path.AddRoundRect(roundRect);
        return path;
    }
    
    private void DrawBackgroundImage(SKCanvas canvas, Panel panel, SKRect skRect, float opacity, float radiusTL, float radiusTR, float radiusBR, float radiusBL)
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
        
        // Apply clipping if has border radius (per-corner)
        var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBR > 0 || radiusBL > 0;
        if (hasRadius)
        {
            canvas.Save();
            using var clipPath = CreateRoundedRectPath(skRect, radiusTL, radiusTR, radiusBR, radiusBL);
            canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
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

    private void DrawGradientBackground(SKCanvas canvas, SKRect rect, GradientInfo gradient, float opacity, float radiusTL, float radiusTR, float radiusBR, float radiusBL)
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
            var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBR > 0 || radiusBL > 0;
            if (hasRadius)
            {
                using var path = CreateRoundedRectPath(rect, radiusTL, radiusTR, radiusBR, radiusBL);
                canvas.DrawPath(path, paint);
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

        // Get border radius - each corner can be different (matches s&box)
        var radiusTL = style.BorderTopLeftRadius?.GetPixels(1f) ?? 0;
        var radiusTR = style.BorderTopRightRadius?.GetPixels(1f) ?? 0;
        var radiusBL = style.BorderBottomLeftRadius?.GetPixels(1f) ?? 0;
        var radiusBR = style.BorderBottomRightRadius?.GetPixels(1f) ?? 0;
        var hasRadius = radiusTL > 0 || radiusTR > 0 || radiusBL > 0 || radiusBR > 0;

        // Check if all borders are uniform (same color and width)
        var uniformColor = style.BorderLeftColor == style.BorderTopColor &&
                          style.BorderTopColor == style.BorderRightColor &&
                          style.BorderRightColor == style.BorderBottomColor;
        var uniformWidth = Math.Abs(leftWidth - topWidth) < BorderWidthTolerance &&
                          Math.Abs(topWidth - rightWidth) < BorderWidthTolerance &&
                          Math.Abs(rightWidth - bottomWidth) < BorderWidthTolerance;
        var borderWidth = (leftWidth + topWidth + rightWidth + bottomWidth) / 4f;

        // If borders are uniform, use path-based stroke rendering (supports per-corner radii)
        if (uniformColor && uniformWidth && style.BorderLeftColor.HasValue)
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

            if (hasRadius)
            {
                // Use per-corner radii with path-based rendering
                using var path = CreateRoundedRectPath(adjustedRect, radiusTL, radiusTR, radiusBR, radiusBL);
                canvas.DrawPath(path, paint);
            }
            else
            {
                // Simple rectangle border
                canvas.DrawRect(adjustedRect, paint);
            }
        }
        else
        {
            // Non-uniform borders: draw each edge separately
            // Like s&box, we support border-radius even with non-uniform borders
            
            if (hasRadius)
            {
                // Draw non-uniform borders with rounded corners using path clipping
                DrawNonUniformBorderWithRadius(canvas, rect, style, opacity, 
                    leftWidth, topWidth, rightWidth, bottomWidth,
                    radiusTL, radiusTR, radiusBR, radiusBL);
            }
            else
            {
                // Simple rectangular borders without radius
                DrawNonUniformBorderSimple(canvas, rect, style, opacity,
                    leftWidth, topWidth, rightWidth, bottomWidth);
            }
        }
    }

    /// <summary>
    /// Draw non-uniform borders without radius (simple rectangles)
    /// </summary>
    private void DrawNonUniformBorderSimple(SKCanvas canvas, Rect rect, Styles style, float opacity,
        float leftWidth, float topWidth, float rightWidth, float bottomWidth)
    {
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

    /// <summary>
    /// Draw non-uniform borders with rounded corners
    /// Creates individual border paths for each edge that properly include their adjacent corners
    /// </summary>
    private void DrawNonUniformBorderWithRadius(SKCanvas canvas, Rect rect, Styles style, float opacity,
        float leftWidth, float topWidth, float rightWidth, float bottomWidth,
        float radiusTL, float radiusTR, float radiusBR, float radiusBL)
    {
        var skRect = ToSKRect(rect);
        
        // Create outer rounded rect (full panel boundary)
        using var outerPath = CreateRoundedRectPath(skRect, radiusTL, radiusTR, radiusBR, radiusBL);
        
        // Create inner rounded rect (content area, inside the border)
        var innerRect = new SKRect(
            skRect.Left + leftWidth,
            skRect.Top + topWidth,
            skRect.Right - rightWidth,
            skRect.Bottom - bottomWidth
        );
        
        // Adjust inner radii to account for border width (can't be negative)
        // Use average of adjacent borders for corner radius reduction
        var innerRadiusTL = Math.Max(0, radiusTL - (leftWidth + topWidth) / 2);
        var innerRadiusTR = Math.Max(0, radiusTR - (rightWidth + topWidth) / 2);
        var innerRadiusBR = Math.Max(0, radiusBR - (rightWidth + bottomWidth) / 2);
        var innerRadiusBL = Math.Max(0, radiusBL - (leftWidth + bottomWidth) / 2);
        
        using var innerPath = CreateRoundedRectPath(innerRect, innerRadiusTL, innerRadiusTR, innerRadiusBR, innerRadiusBL);
        
        // Create the full border region path
        using var fullBorderPath = new SKPath();
        outerPath.Op(innerPath, SKPathOp.Difference, fullBorderPath);
        
        // For non-uniform borders with radius, we need to handle corners specially
        // Corners are split between adjacent edges based on a 45-degree diagonal
        // This matches typical CSS border rendering behavior
        
        // Calculate the center points for corner splitting
        var centerX = skRect.MidX;
        var centerY = skRect.MidY;
        
        // Top border (includes top-left and top-right corners)
        if (topWidth > 0 && style.BorderTopColor.HasValue)
        {
            canvas.Save();
            
            // Create a clipping path that includes the top edge and portions of corners
            // Split corners diagonally from center to outer corners
            using var topClipPath = new SKPath();
            topClipPath.MoveTo(skRect.Left, centerY);
            topClipPath.LineTo(skRect.Left, skRect.Top);
            topClipPath.LineTo(skRect.Right, skRect.Top);
            topClipPath.LineTo(skRect.Right, centerY);
            topClipPath.LineTo(centerX, centerY);
            topClipPath.Close();
            
            canvas.ClipPath(topClipPath);
            
            var color = ToSKColor(style.BorderTopColor.Value, opacity);
            using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawPath(fullBorderPath, paint);
            canvas.Restore();
        }
        
        // Right border (includes top-right and bottom-right corners)
        if (rightWidth > 0 && style.BorderRightColor.HasValue)
        {
            canvas.Save();
            
            using var rightClipPath = new SKPath();
            rightClipPath.MoveTo(centerX, skRect.Top);
            rightClipPath.LineTo(skRect.Right, skRect.Top);
            rightClipPath.LineTo(skRect.Right, skRect.Bottom);
            rightClipPath.LineTo(centerX, skRect.Bottom);
            rightClipPath.LineTo(centerX, centerY);
            rightClipPath.Close();
            
            canvas.ClipPath(rightClipPath);
            
            var color = ToSKColor(style.BorderRightColor.Value, opacity);
            using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawPath(fullBorderPath, paint);
            canvas.Restore();
        }
        
        // Bottom border (includes bottom-left and bottom-right corners)
        if (bottomWidth > 0 && style.BorderBottomColor.HasValue)
        {
            canvas.Save();
            
            using var bottomClipPath = new SKPath();
            bottomClipPath.MoveTo(skRect.Left, centerY);
            bottomClipPath.LineTo(centerX, centerY);
            bottomClipPath.LineTo(skRect.Right, centerY);
            bottomClipPath.LineTo(skRect.Right, skRect.Bottom);
            bottomClipPath.LineTo(skRect.Left, skRect.Bottom);
            bottomClipPath.Close();
            
            canvas.ClipPath(bottomClipPath);
            
            var color = ToSKColor(style.BorderBottomColor.Value, opacity);
            using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawPath(fullBorderPath, paint);
            canvas.Restore();
        }
        
        // Left border (includes top-left and bottom-left corners)
        if (leftWidth > 0 && style.BorderLeftColor.HasValue)
        {
            canvas.Save();
            
            using var leftClipPath = new SKPath();
            leftClipPath.MoveTo(skRect.Left, skRect.Top);
            leftClipPath.LineTo(centerX, skRect.Top);
            leftClipPath.LineTo(centerX, centerY);
            leftClipPath.LineTo(centerX, skRect.Bottom);
            leftClipPath.LineTo(skRect.Left, skRect.Bottom);
            leftClipPath.Close();
            
            canvas.ClipPath(leftClipPath);
            
            var color = ToSKColor(style.BorderLeftColor.Value, opacity);
            using var paint = new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawPath(fullBorderPath, paint);
            canvas.Restore();
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
        var fontWeight = style.FontWeight ?? 400;
        var fontSmooth = style.FontSmooth ?? FontSmooth.Auto;
        
        // Get text rect
        var rect = label.Box.RectInner;

        // Create and configure TextBlock using RichTextKit (matches s&box approach)
        var textBlock = new TextBlockWrapper();
        textBlock.Update(processedText, fontFamily, fontSize, fontWeight, fontSmooth);
        
        // Measure with width constraint if wrapping is enabled
        var shouldWrap = style.WhiteSpace != WhiteSpace.NoWrap;
        if (shouldWrap && rect.Width > 0)
        {
            textBlock.Measure(rect.Width, rect.Height);
        }
        else
        {
            textBlock.Measure(float.NaN, float.NaN);
        }

        // Calculate starting position based on text alignment
        var x = rect.Left;
        var y = rect.Top;

        // Horizontal alignment
        if (style.TextAlign == TextAlign.Center)
        {
            x = rect.Left + (rect.Width - textBlock.MeasuredWidth) / 2;
        }
        else if (style.TextAlign == TextAlign.Right)
        {
            x = rect.Right - textBlock.MeasuredWidth;
        }

        // Vertical alignment (if needed, based on AlignItems)
        if (style.AlignItems == Align.Center)
        {
            y = rect.Top + (rect.Height - textBlock.MeasuredHeight) / 2;
        }
        else if (style.AlignItems == Align.FlexEnd)
        {
            y = rect.Bottom - textBlock.MeasuredHeight;
        }

        // Paint the text using RichTextKit with font-smooth settings
        var skColor = ToSKColor(textColor, opacity);
        textBlock.Paint(canvas, x, y, skColor);
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
