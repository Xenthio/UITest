namespace Sandbox.UI;

/// <summary>
/// Renderer-agnostic rectangle. Matches s&box's Rect.
/// </summary>
public struct Rect
{
    /// <summary>
    /// A zero-sized rect at origin.
    /// </summary>
    public static readonly Rect Zero = new(0, 0, 0, 0);

    public float Left;
    public float Top;
    public float Width;
    public float Height;

    public float Right
    {
        get => Left + Width;
        set => Width = value - Left;
    }

    public float Bottom
    {
        get => Top + Height;
        set => Height = value - Top;
    }

    public Vector2 Position
    {
        get => new(Left, Top);
        set
        {
            Left = value.x;
            Top = value.y;
        }
    }

    public Vector2 Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.x;
            Height = value.y;
        }
    }

    public Vector2 Center => new(Left + Width / 2, Top + Height / 2);

    public Rect(float left, float top, float width, float height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }

    public Rect(Vector2 position, Vector2 size)
    {
        Left = position.x;
        Top = position.y;
        Width = size.x;
        Height = size.y;
    }

    public bool Contains(Vector2 point)
    {
        return point.x >= Left && point.x <= Right &&
               point.y >= Top && point.y <= Bottom;
    }

    public bool Contains(float x, float y) => Contains(new Vector2(x, y));

    /// <summary>
    /// Grow the rect by the given margins
    /// </summary>
    public Rect Grow(float left, float top, float right, float bottom)
    {
        return new Rect(Left - left, Top - top, Width + left + right, Height + top + bottom);
    }

    /// <summary>
    /// Shrink the rect by the given margins
    /// </summary>
    public Rect Shrink(float left, float top, float right, float bottom)
    {
        return new Rect(Left + left, Top + top, Width - left - right, Height - top - bottom);
    }

    /// <summary>
    /// Round coordinates to integer values
    /// </summary>
    public Rect Floor()
    {
        return new Rect(
            MathF.Floor(Left),
            MathF.Floor(Top),
            MathF.Floor(Width),
            MathF.Floor(Height)
        );
    }

    /// <summary>
    /// Add another rect to this one (union)
    /// </summary>
    public void Add(Rect other)
    {
        float newLeft = Math.Min(Left, other.Left);
        float newTop = Math.Min(Top, other.Top);
        float newRight = Math.Max(Right, other.Right);
        float newBottom = Math.Max(Bottom, other.Bottom);

        Left = newLeft;
        Top = newTop;
        Width = newRight - newLeft;
        Height = newBottom - newTop;
    }

    public static Rect Empty => new(0, 0, 0, 0);

    public override string ToString() => $"Rect({Left}, {Top}, {Width}, {Height})";
}
