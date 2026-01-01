namespace Sandbox.UI;

/// <summary>
/// Handles the storage, progression and application of CSS transitions for a single <see cref="Panel"/>.
/// </summary>
public sealed class Transitions
{
    public delegate void TransitionFunction(Styles style, float delta);

    public struct Entry
    {
        public string Property { get; init; }
        public double StartTime { get; init; }
        public double Length { get; init; }
        public int Target { get; init; }
        public Func<float, float> EasingFunction { get; init; }
        public bool IsKilled { get; private set; }

        public TransitionFunction Action { get; init; }

        public Entry(string property, double startTime, double length, int target, TransitionFunction action, Func<float, float> easingFunction) : this()
        {
            Property = property;
            StartTime = startTime;
            Length = length;
            Target = target;
            Action = action;
            EasingFunction = easingFunction;

            IsKilled = false;
        }

        internal void Kill()
        {
            IsKilled = true;
        }

        internal void Restore()
        {
            IsKilled = false;
        }

        public float Ease(float delta) => EasingFunction?.Invoke(delta) ?? delta;
        public void Invoke(Styles style, float delta) => Action?.Invoke(style, delta);

        public override string ToString()
        {
            return $"Entry( '{Property}', {StartTime}s, {Length}s, '{Target}', {Action}, {EasingFunction} )";
        }
    }

    /// <summary>
    /// Active CSS transitions.
    /// </summary>
    public List<Entry>? Entries = null;

    /// <summary>
    /// Whether there are any active CSS transitions.
    /// </summary>
    public bool HasAny => Entries?.Count > 0;

    private Panel panel;

    internal Transitions(Panel panel)
    {
        this.panel = panel;
    }

    internal void Kill(Styles from)
    {
        if (!from.HasTransitions) return;
        if (Entries == null) return;

        for (int i = 0; i < Entries.Count; i++)
        {
            if (!from.Transitions!.List.Any(x => x.Property == Entries[i].Property))
                continue;

            var transition = Entries[i];
            transition.Kill();
            Entries[i] = transition;
        }
    }

    /// <summary>
    /// Clear all transitions. This will immediately remove transitions, leaving styles wherever they are.
    /// </summary>
    internal void Clear()
    {
        Entries?.Clear();
    }

    /// <summary>
    /// Immediately snaps all transitions to the end point, at which point they're removed.
    /// </summary>
    internal void Kill()
    {
        if (Entries == null) return;

        for (int i = 0; i < Entries.Count; i++)
        {
            var transition = Entries[i];
            transition.Kill();
            Entries[i] = transition;
        }
    }

    // TODO: Add more transition methods when needed
}
