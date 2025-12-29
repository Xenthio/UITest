namespace Sandbox.UI;

/// <summary>
/// Per-panel style handler. Manages inline styles and computed styles.
/// Based on s&box's PanelStyle from engine/Sandbox.Engine/Systems/UI/PanelStyle.cs
/// </summary>
public sealed class PanelStyle : Styles
{
    Panel panel;

    internal Styles Cached = new Styles();
    internal Styles Final = new Styles();

    /// <summary>
    /// This could be a local variable if we wanted to create a new class every time
    /// </summary>
    List<StyleSelector>? activeRules;

    /// <summary>
    /// Store the last active rules so we can compare them when they change
    /// </summary>
    internal List<StyleSelector>? LastActiveRules;

    /// <summary>
    /// Cache of the active rules that are applied
    /// </summary>
    int ActiveRulesGuid;

    private bool isDirty = true;
    bool rulesChanged = true;

    public override void Dirty() => isDirty = true;
    internal bool IsDirty => isDirty;

    internal PanelStyle(Panel panel)
    {
        this.panel = panel;
    }

    /// <summary>
    /// Should be called when a stylesheet in our bundle has changed.
    /// </summary>
    internal void UnderlyingStyleHasChanged()
    {
        ActiveRulesGuid = -1;
    }

    /// <summary>
    /// All these styles could possibly apply to us.
    /// </summary>
    StyleBlock[]? StyleBlocks;

    /// <summary>
    /// A hash of the things that are checked in the broadphase.
    /// </summary>
    int broadPhaseHash = 0;

    /// <summary>
    /// Called when a stylesheet has been added or removed from ourselves or one of
    /// our ancestor panels - because under that condition we need to rebuild our
    /// broadphase.
    /// </summary>
    internal void InvalidateBroadphase()
    {
        if (StyleBlocks == null)
            return;

        StyleBlocks = null;

        foreach (var child in panel.Children)
        {
            child.Style.InvalidateBroadphase();
        }
    }

    void BuildApplicableBlocks()
    {
        StyleBlocks = panel.AllStyleSheets
                                    .SelectMany(x => x.Nodes)
                                    .Where(x => x.TestBroadphase(panel))
                                    .ToArray();
    }

    /// <summary>
    /// Called from the root panel in a thread. We replace activeRules with all of the rules that
    /// we want applied and return true if the rules changed.
    /// </summary>
    internal bool BuildRulesInThread()
    {
        activeRules?.Clear();

        var hash = HashCode.Combine(panel.Id, panel.ElementName, panel.Classes);
        if (StyleBlocks == null || hash != broadPhaseHash)
        {
            BuildApplicableBlocks();
        }
        broadPhaseHash = hash;

        if (StyleBlocks != null)
        {
            foreach (var c in StyleBlocks)
            {
                var winningSelector = c.Test(panel);
                if (winningSelector == null) continue;

                activeRules ??= new();
                activeRules.Add(winningSelector);
            }
        }

        int ruleguid = 0;
        if (activeRules != null)
        {
            activeRules.Sort(StyleOrderer.Instance);

            foreach (var entry in activeRules)
            {
                ruleguid = HashCode.Combine(ruleguid, entry);
            }
        }

        rulesChanged = rulesChanged || ruleguid != ActiveRulesGuid;
        ActiveRulesGuid = ruleguid;

        return rulesChanged;
    }

    internal bool BuildCached(ref LayoutCascade cascade)
    {
        if (!isDirty && !cascade.SelectorChanged && !rulesChanged)
            return false;

        isDirty = false;

        Cached.From(Styles.Default);

        if (activeRules != null)
        {
            foreach (var entry in activeRules)
            {
                Cached.Add(entry.Block.Styles);
            }
        }

        //
        // Rules changed
        //
        if (rulesChanged)
        {
            rulesChanged = false;
            cascade.SelectorChanged = true;

            LastActiveRules ??= new();
            activeRules ??= new();

            LastActiveRules.Clear();
            LastActiveRules.AddRange(activeRules);
        }

        Cached.Add(this);

        // ApplyScale is simplified - removed for now
        return true;
    }

    internal Styles BuildFinal(ref LayoutCascade cascade, out bool changed)
    {
        changed = BuildCached(ref cascade);

        Final.From(Cached);
        cascade.ApplyCascading(Final);
        Final.FillDefaults();  // THIS IS THE KEY FIX!

        return Final;
    }

    public override bool Set(string property, string value)
    {
        isDirty = true;

        return base.Set(property, value);
    }
}

internal class StyleOrderer : IComparer<StyleSelector>
{
    internal static StyleOrderer Instance = new StyleOrderer();

    public int Compare(StyleSelector? x, StyleSelector? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        return x.Score - y.Score;
    }
}
