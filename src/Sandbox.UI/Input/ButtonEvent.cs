namespace Sandbox.UI;

/// <summary>
/// Keyboard (and mouse) key press event.
/// Based on S&box's ButtonEvent from engine/Sandbox.Engine/Systems/UI/Input/ButtonEvent.cs
/// </summary>
public record ButtonEvent
{
    /// <summary>
    /// The button that triggered the event.
    /// </summary>
    public string Button { get; }

    /// <summary>
    /// Whether the button was pressed in, or released.
    /// </summary>
    public bool Pressed { get; }

    /// <summary>
    /// The keyboard modifier keys that were held down at the moment the event triggered.
    /// </summary>
    public KeyboardModifiers KeyboardModifiers { get; }

    /// <summary>
    /// Whether <c>Shift</c> key was being held down at the time of the event.
    /// </summary>
    public bool HasShift => KeyboardModifiers.HasFlag(KeyboardModifiers.Shift);

    /// <summary>
    /// Whether <c>Control</c> key was being held down at the time of the event.
    /// </summary>
    public bool HasCtrl => KeyboardModifiers.HasFlag(KeyboardModifiers.Ctrl);

    /// <summary>
    /// Whether <c>Alt</c> key was being held down at the time of the event.
    /// </summary>
    public bool HasAlt => KeyboardModifiers.HasFlag(KeyboardModifiers.Alt);

    /// <summary>
    /// Set to <see langword="true"/> to prevent the event from propagating to the parent panel.
    /// </summary>
    public bool StopPropagation { get; set; }

    public ButtonEvent(string button, bool pressed, KeyboardModifiers modifiers = KeyboardModifiers.None)
    {
        Button = button;
        Pressed = pressed;
        KeyboardModifiers = modifiers;
    }

    public override string ToString() => $"{Button} {(Pressed ? "pressed" : "released")}";
}

/// <summary>
/// Keyboard modifier keys
/// </summary>
[Flags]
public enum KeyboardModifiers
{
    None = 0,
    Shift = 1,
    Ctrl = 2,
    Alt = 4
}
