namespace Sandbox.UI;

/// <summary>
/// Handles input focus for Panels.
/// Based on S&box's InputFocus from engine/Sandbox.Engine/Systems/UI/Input/InputFocus.cs
/// </summary>
public class InputFocus
{
    /// <summary>
    /// The panel that currently has input focus.
    /// </summary>
    public static Panel? Current { get; internal set; }

    /// <summary>
    /// The panel that will have the input focus next.
    /// </summary>
    internal static Panel? Next { get; set; }

    /// <summary>
    /// Whether we have a pending focus change
    /// </summary>
    internal static bool PendingChange { get; set; }

    /// <summary>
    /// Set the focus to this panel (or its nearest ancestor with AcceptsFocus).
    /// Note that <see cref="Current"/> won't change until the next frame.
    /// </summary>
    public static bool Set(Panel? panel)
    {
        return TrySetOrParent(panel);
    }

    /// <summary>
    /// Clear focus away from this panel.
    /// </summary>
    public static bool Clear(Panel? panel)
    {
        Next = null;
        PendingChange = true;

        Set(panel?.Parent);

        return true;
    }

    /// <summary>
    /// Clear keyboard focus
    /// </summary>
    public static bool Clear()
    {
        if (Current == null)
            return false;

        Next = null;
        PendingChange = true;
        return true;
    }

    static bool TrySetOrParent(Panel? panel)
    {
        if (panel == null) return false;
        if (Next == panel) return true;

        if (panel.AcceptsFocus)
        {
            Next = panel;
            PendingChange = true;
            return true;
        }

        return TrySetOrParent(panel.Parent);
    }

    internal static void Tick()
    {
        // If our focus became ineligible then defocus
        if (Current != null)
        {
            if (!IsPanelEligibleForFocus(Current))
            {
                if (!PendingChange || Next == Current)
                {
                    Next = null;
                    PendingChange = true;
                }
            }
        }

        // Don't swap to an ineligible panel
        if (PendingChange && Next != null)
        {
            if (!Next.AcceptsFocus)
            {
                Next = null;
                PendingChange = false;
            }
        }

        if (PendingChange)
        {
            PendingChange = false;

            if (Current != Next)
            {
                if (Current != null)
                {
                    Current.Switch(PseudoClass.Focus, false);
                }

                Current = Next;

                if (Current != null)
                {
                    Current.Switch(PseudoClass.Focus, true);
                }
            }
        }

        Next = null;
    }

    static bool IsPanelEligibleForFocus(Panel panel)
    {
        if (!panel.IsVisible) return false;
        if (!panel.AcceptsFocus) return false;

        return true;
    }
}
