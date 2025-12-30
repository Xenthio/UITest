namespace Sandbox.UI;

/// <summary>
/// A panel that the user can enter text into.
/// Based on S&box's TextEntry control.
/// </summary>
[Library("textentry")]
public class TextEntry : Panel
{
    /// <summary>
    /// The label that contains the text
    /// </summary>
    protected Label Label { get; set; }

    private bool _disabled = false;
    private bool _numeric = false;
    private string _placeholder = "";

    /// <summary>
    /// Is the text entry disabled?
    /// </summary>
    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            // Note: AcceptsFocus not implemented in base Panel yet
            SetClass("disabled", value);
        }
    }

    /// <summary>
    /// Access to the raw text in the text entry
    /// </summary>
    public string Text
    {
        get => Label?.Text ?? "";
        set
        {
            if (Label != null)
                Label.Text = value ?? "";
        }
    }

    /// <summary>
    /// The value of the text entry
    /// </summary>
    public string Value
    {
        get => Text;
        set => Text = value;
    }

    /// <summary>
    /// Makes it possible to enter new lines
    /// </summary>
    public bool Multiline { get; set; } = false;

    /// <summary>
    /// If true, only numeric input is allowed
    /// </summary>
    public bool Numeric
    {
        get => _numeric;
        set
        {
            _numeric = value;
            SetClass("numeric", value);
        }
    }

    /// <summary>
    /// Format for numeric values (e.g., "0.###")
    /// </summary>
    public string NumberFormat { get; set; } = "0.###";

    /// <summary>
    /// Minimum value for numeric input
    /// </summary>
    public float? MinValue { get; set; }

    /// <summary>
    /// Maximum value for numeric input
    /// </summary>
    public float? MaxValue { get; set; }

    /// <summary>
    /// Placeholder text when empty
    /// </summary>
    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value;
            SetClass("has-placeholder", !string.IsNullOrEmpty(value));
        }
    }

    /// <summary>
    /// Called when text is changed
    /// </summary>
    public event Action<string>? OnTextEdited;

    public TextEntry()
    {
        AddClass("textentry");
        ElementName = "textentry";

        // TODO: AcceptsFocus = true; (not available yet in base Panel)
        Label = AddChild(new Label("", "content-label"));
    }

    public override void SetProperty(string name, string value)
    {
        switch (name)
        {
            case "text":
            case "value":
                Text = value;
                return;

            case "placeholder":
                Placeholder = value;
                return;

            case "numeric":
                Numeric = value == "true" || value == "1";
                return;

            case "disabled":
                Disabled = value == "true" || value == "1";
                return;

            case "multiline":
                Multiline = value == "true" || value == "1";
                return;

            case "format":
            case "numberformat":
                NumberFormat = value;
                return;

            case "min":
            case "minvalue":
                if (float.TryParse(value, out float min))
                    MinValue = min;
                return;

            case "max":
            case "maxvalue":
                if (float.TryParse(value, out float max))
                    MaxValue = max;
                return;
        }

        base.SetProperty(name, value);
    }

    public override void SetContent(string? value)
    {
        Text = value ?? "";
    }

    /// <summary>
    /// Called when value changes
    /// </summary>
    protected virtual void OnValueChanged()
    {
        CreateValueEvent("value", Text);
        OnTextEdited?.Invoke(Text);
    }

    /// <summary>
    /// Format numeric text
    /// </summary>
    protected string FixNumeric()
    {
        if (!Numeric) return Text;

        if (float.TryParse(Text, out float val))
        {
            // Clamp to min/max
            if (MinValue.HasValue && val < MinValue.Value)
                val = MinValue.Value;
            if (MaxValue.HasValue && val > MaxValue.Value)
                val = MaxValue.Value;

            return val.ToString(NumberFormat);
        }

        return Text;
    }

    /// <summary>
    /// Handle character input
    /// </summary>
    public override void OnKeyTyped(char k)
    {
        if (Disabled)
            return;

        // Handle backspace
        if (k == '\b')
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Text = Text.Substring(0, Text.Length - 1);
                OnValueChanged();
            }
            return;
        }

        // Handle enter/return
        if (k == '\n' || k == '\r')
        {
            if (!Multiline)
            {
                // Blur/unfocus would go here
                return;
            }
        }

        // Don't allow control characters (except newline for multiline)
        if (char.IsControl(k) && k != '\n')
            return;

        // Check for numeric only
        if (Numeric && !char.IsDigit(k) && k != '.' && k != '-')
            return;

        // Add character to text
        Text += k;
        OnValueChanged();
    }
}
