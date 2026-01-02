using System.Diagnostics;

namespace Sandbox.UI;

/// <summary>
/// Access to time. Simplified port from s&box.
/// </summary>
public static class RealTime
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    /// <summary>
    /// The time since startup, in seconds.
    /// </summary>
    public static float Now => (float)_stopwatch.Elapsed.TotalSeconds;

    /// <summary>
    /// The time delta (in seconds) between the last frame and the current
    /// </summary>
    public static float Delta { get; internal set; }

    /// <summary>
    /// Like Delta but smoothed to avoid large disparities between deltas
    /// </summary>
    public static float SmoothDelta { get; internal set; }

    private static double _lastTick;

    public static void Update(double now)
    {
        if (_lastTick > 0)
        {
            Delta = (float)Math.Clamp(now - _lastTick, 0.0, 2.0);
            SmoothDelta = MathX.Lerp(SmoothDelta, Delta, 0.1f);
        }

        _lastTick = now;
    }
}
