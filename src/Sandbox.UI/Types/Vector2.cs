namespace Sandbox;

/// <summary>
/// Renderer-agnostic 2D vector. Matches s&box's Vector2.
/// </summary>
public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 Zero => new(0, 0);
    public static Vector2 One => new(1, 1);

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x + b.x, a.y + b.y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x - b.x, a.y - b.y);
    public static Vector2 operator *(Vector2 a, float b) => new(a.x * b, a.y * b);
    public static Vector2 operator /(Vector2 a, float b) => new(a.x / b, a.y / b);
    public static Vector2 operator *(float a, Vector2 b) => new(a * b.x, a * b.y);

    public static implicit operator Vector2(float value) => new(value, value);

    public bool IsNearZeroLength => Math.Abs(x) < 0.0001f && Math.Abs(y) < 0.0001f;

    public float Length => MathF.Sqrt(x * x + y * y);

    public override string ToString() => $"({x}, {y})";
}
