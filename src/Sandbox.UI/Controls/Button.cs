namespace Sandbox.UI;

/// <summary>
/// A simple button panel.
/// Based on s&box's Button.
/// </summary>
[Library("button")]
public class Button : Panel
{
    /// <summary>
    /// The Label that displays Text.
    /// </summary>
    protected Label? TextLabel;

    /// <summary>
    /// The IconPanel that displays Icon.
    /// </summary>
    protected IconPanel? IconPanel;

    /// <summary>
    /// The Label that displays Help.
    /// </summary>
    protected Label? HelpLabel;

    /// <summary>
    /// The column on the right, holding the label and help
    /// </summary>
    protected Panel? RightColumn;

    /// <summary>
    /// Used for selection status in things like ButtonGroup
    /// </summary>
    public virtual object? Value { get; set; }

    public Button()
    {
        AddClass("button");
        ElementName = "button";

        IconPanel = AddChild(new IconPanel("people", "icon"));
        IconPanel.Style.Display = DisplayMode.None;

        RightColumn = AddChild(new Panel(this, "button-right-column"));
        RightColumn.Style.Display = DisplayMode.None;

        TextLabel = RightColumn.AddChild(new Label("Empty Label", "button-label button-text"));
        TextLabel.Style.Display = DisplayMode.None;

        HelpLabel = RightColumn.AddChild(new Label("", "button-help"));
        HelpLabel.Style.Display = DisplayMode.None;
    }

    public Button(string? text, Action? action = default) : this()
    {
        if (text != null)
            Text = text;

        if (action != null)
            OnClick += action;
    }

    public Button(string? text, string? icon) : this()
    {
        if (icon != null)
            Icon = icon;

        if (text != null)
            Text = text;
    }

    public Button(string? text, string? icon, Action? onClick) : this(text, icon)
    {
        if (onClick != null)
            OnClick += onClick;
    }

    public Button(string? text, string? icon, string? className, Action? onClick) : this(text, icon, onClick)
    {
        if (className != null)
            AddClass(className);
    }

    /// <summary>
    /// The button is disabled for some reason
    /// </summary>
    public bool Disabled
    {
        get => HasClass("disabled");
        set => SetClass("disabled", value);
    }

    /// <summary>
    /// Allow external factors to force the active state
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// Text for the button.
    /// </summary>
    public string? Text
    {
        get => TextLabel?.Text;
        set
        {
            if (TextLabel == null) return;

            if (string.IsNullOrEmpty(value))
            {
                TextLabel.Style.Display = DisplayMode.None;
                TextLabel.Text = "";
                return;
            }

            TextLabel.Style.Display = DisplayMode.Flex;
            if (RightColumn != null)
                RightColumn.Style.Display = DisplayMode.Flex;
            TextLabel.Text = value;
        }
    }

    /// <summary>
    /// Help for the button.
    /// </summary>
    public string? Help
    {
        get => HelpLabel?.Text;
        set
        {
            if (HelpLabel == null) return;

            if (string.IsNullOrEmpty(value))
            {
                HelpLabel.Style.Display = DisplayMode.None;
                HelpLabel.Text = "";
                return;
            }

            HelpLabel.Style.Display = DisplayMode.Flex;
            if (RightColumn != null)
                RightColumn.Style.Display = DisplayMode.Flex;
            HelpLabel.Text = value;
        }
    }

    /// <summary>
    /// Icon for the button.
    /// </summary>
    public string? Icon
    {
        get => IconPanel?.Text;
        set
        {
            if (IconPanel == null) return;

            if (string.IsNullOrEmpty(value))
            {
                IconPanel.Style.Display = DisplayMode.None;
                return;
            }

            IconPanel.Style.Display = DisplayMode.Flex;
            IconPanel.Text = value;
            SetClass("has-icon", IconPanel.IsValid());
        }
    }

    /// <summary>
    /// Set the text for the button.
    /// </summary>
    public virtual void SetText(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Click event handler
    /// </summary>
    public event Action? OnClick;

    /// <summary>
    /// Imitate the button being clicked.
    /// </summary>
    public void Click()
    {
        OnClick?.Invoke();
    }

    public override void SetProperty(string name, string value)
    {
        switch (name)
        {
            case "text":
            case "html":
                SetText(value);
                return;

            case "icon":
                Icon = value;
                return;

            case "active":
                SetClass("active", value == "true" || value == "1");
                return;

            case "disabled":
                Disabled = value == "true" || value == "1";
                return;
        }

        base.SetProperty(name, value);
    }

    public override void SetContent(string? value)
    {
        SetText(value?.Trim() ?? "");
    }

    public override void Tick()
    {
        base.Tick();
        UpdateActiveState();
    }

    protected void UpdateActiveState()
    {
        SetClass("active", Active);
    }
}
