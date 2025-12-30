using System;
using System.Numerics;

namespace Sandbox.UI;

public partial class RootPanel
{
    /// <summary>
    /// Panel we're currently hovered over
    /// </summary>
    internal Panel? Hovered { get; private set; }

    /// <summary>
    /// Panel we're currently pressing down
    /// </summary>
    internal Panel? Active { get; private set; }

    /// <summary>
    /// Update input for this root panel
    /// </summary>
    public void UpdateInput(Vector2 mousePosition, bool mouseIsActive)
    {
        MousePos = mousePosition;

        // Update focus system
        InputFocus.Tick();

        if (mouseIsActive)
        {
            UpdateMouse(mousePosition);
        }
        else
        {
            SetHovered(null);
        }
    }

    /// <summary>
    /// Process a button event (mouse or keyboard)
    /// </summary>
    public void ProcessButtonEvent(string button, bool pressed, KeyboardModifiers modifiers = KeyboardModifiers.None)
    {
        var e = new ButtonEvent(button, pressed, modifiers);

        // Handle mouse button clicks
        if (button.StartsWith("mouse") && Hovered != null)
        {
            Console.WriteLine($"Mouse {button} {(pressed ? "pressed" : "released")} on {Hovered.GetType().Name}");
            
            if (pressed)
            {
                // Set active panel on mouse down
                SetActive(Hovered);
                
                // Try to focus on click
                if (Hovered.AcceptsFocus)
                {
                    Hovered.Focus();
                }
            }
            else
            {
                // Trigger click event on mouse up if still over the same panel
                if (Active == Hovered && Active is Button btn)
                {
                    Console.WriteLine($"Triggering Click() on button: {btn.ElementName}");
                    btn.Click();
                }
                
                SetActive(null);
            }
        }

        // Send button event to hovered or focused panel
        var target = Hovered ?? InputFocus.Current;
        target?.OnButtonEvent(e);

        // If focused, also send typed event
        if (pressed && InputFocus.Current != null)
        {
            InputFocus.Current.OnButtonTyped(e);
        }
    }

    /// <summary>
    /// Process a character typed event
    /// </summary>
    public void ProcessCharTyped(char character)
    {
        InputFocus.Current?.OnKeyTyped(character);
    }

    /// <summary>
    /// Process mouse wheel event
    /// </summary>
    public void ProcessMouseWheel(Vector2 delta, KeyboardModifiers modifiers = KeyboardModifiers.None)
    {
        // Windows apps translate vertical scroll to horizontal if shift is held
        if (modifiers.HasFlag(KeyboardModifiers.Shift))
        {
            delta = new Vector2(-delta.y, 0);
        }

        Hovered?.OnMouseWheel(delta);
    }

    private void UpdateMouse(Vector2 mousePos)
    {
        Panel? newHovered = null;
        
        // Find topmost panel under mouse
        CheckHover(this, mousePos, ref newHovered);

        SetHovered(newHovered);
    }

    private void CheckHover(Panel panel, Vector2 mousePos, ref Panel? hoveredPanel)
    {
        if (panel == null || !panel.IsVisible) return;
        if (panel.ComputedStyle == null) return;

        // Check if mouse is within this panel
        if (!panel.IsInside(mousePos)) return;

        // This panel is under the mouse
        hoveredPanel = panel;

        // Check children (in reverse order for proper z-ordering)
        if (panel.ChildrenCount > 0)
        {
            var children = panel.Children.ToList();
            for (int i = children.Count - 1; i >= 0; i--)
            {
                CheckHover(children[i], mousePos, ref hoveredPanel);
            }
        }
    }

    private void SetHovered(Panel? panel)
    {
        if (Hovered == panel) return;

        // Remove hover from old panel
        if (Hovered != null)
        {
            Hovered.Switch(PseudoClass.Hover, false);
        }

        Hovered = panel;
        
        if (panel != null)
        {
            Console.WriteLine($"Hovering over: {panel.GetType().Name} at {panel.Box.Rect}");
        }

        // Add hover to new panel
        if (Hovered != null)
        {
            Hovered.Switch(PseudoClass.Hover, true);
        }
    }

    private void SetActive(Panel? panel)
    {
        if (Active == panel) return;

        // Remove active from old panel
        if (Active != null)
        {
            Active.Switch(PseudoClass.Active, false);
        }

        Active = panel;

        // Add active to new panel
        if (Active != null)
        {
            Active.Switch(PseudoClass.Active, true);
        }
    }
}
