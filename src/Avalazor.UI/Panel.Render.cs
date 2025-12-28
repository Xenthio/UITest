using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Panel partial class: Rendering
/// Based on s&box's Panel.Render.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Indicates if this panel has custom content to render
    /// Override in derived classes (e.g., Label returns true)
    /// </summary>
    public virtual bool HasContent => false;

    /// <summary>
    /// Called to draw the panel's background (s&box pattern)
    /// </summary>
    protected virtual void DrawBackground(SKCanvas canvas)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Called to draw the panel's content (s&box pattern)
    /// </summary>
    protected virtual void DrawContent(SKCanvas canvas)
    {
        // Override in derived classes (e.g., Label draws text here)
    }

    /// <summary>
    /// Called when panel needs to paint itself
    /// Override to provide custom rendering
    /// </summary>
    protected virtual void OnPaint(SKCanvas canvas)
    {
        Console.WriteLine($"OnPaint: {Tag}, Background={ComputedStyle?.BackgroundColor}, Box=({Box.Rect.Width}x{Box.Rect.Height})");
        
        // Base implementation paints background, border, etc.
        PaintBackground(canvas);
        PaintBorder(canvas);
        
        // Call custom content drawing if panel has content
        if (HasContent)
        {
            DrawContent(canvas);
        }
    }

    /// <summary>
    /// Paint the panel and its children.
    /// Uses Box.Rect for positioning (s&box pattern)
    /// </summary>
    internal void Paint(SKCanvas canvas)
    {
        if (!IsVisible) return;

        // Save canvas state
        canvas.Save();

        // OnPaint draws at (0,0) using Box.Rect.Width/Height
        OnPaint(canvas);

        // Paint children relative to this panel
        foreach (var child in _children)
        {
            canvas.Save();
            // Translate to child's position relative to parent
            canvas.Translate(child.Box.Rect.Left - Box.Rect.Left, child.Box.Rect.Top - Box.Rect.Top);
            child.Paint(canvas);
            canvas.Restore();
        }
        
        canvas.Restore();
    }

    private void PaintBackground(SKCanvas canvas)
    {
        if (_computedStyle?.BackgroundColor != null)
        {
            Console.WriteLine($"PaintBackground: {Tag} rendering {_computedStyle.BackgroundColor} at (0, 0, {Box.Rect.Width}, {Box.Rect.Height})");
            
            using var paint = new SKPaint
            {
                Color = _computedStyle.BackgroundColor.Value,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var rect = new SKRect(0, 0, Box.Rect.Width, Box.Rect.Height);
            var borderRadius = _computedStyle.BorderRadius;

            if (borderRadius > 0)
            {
                canvas.DrawRoundRect(rect, borderRadius, borderRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
    }

    private void PaintBorder(SKCanvas canvas)
    {
        if (_computedStyle?.BorderWidth > 0 && _computedStyle?.BorderColor != null)
        {
            using var paint = new SKPaint
            {
                Color = _computedStyle.BorderColor.Value,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = _computedStyle.BorderWidth,
                IsAntialias = true
            };

            var rect = new SKRect(0, 0, Box.Rect.Width, Box.Rect.Height);
            var borderRadius = _computedStyle.BorderRadius;

            if (borderRadius > 0)
            {
                canvas.DrawRoundRect(rect, borderRadius, borderRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
    }

    /// <summary>
    /// Called when mouse is pressed down on this panel
    /// </summary>
    public virtual void OnMouseDown(MouseEventArgs e)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when mouse is released on this panel
    /// </summary>
    public virtual void OnMouseUp(MouseEventArgs e)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when mouse enters this panel
    /// </summary>
    public virtual void OnMouseEnter(MouseEventArgs e)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Called when mouse leaves this panel
    /// </summary>
    public virtual void OnMouseLeave(MouseEventArgs e)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Check if point is inside this panel
    /// </summary>
    public bool ContainsPoint(float x, float y)
    {
        return Box.Rect.Contains(x, y);
    }
}
