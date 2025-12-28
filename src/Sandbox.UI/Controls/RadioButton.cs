namespace Sandbox.UI;

/// <summary>
/// A radio button control that works as part of a RadioButtons group.
/// Based on XGUI-3's RadioButton.
/// </summary>
public class RadioButton : Panel
{
    /// <summary>
    /// The radio button icon panel.
    /// </summary>
    public Panel? CheckMark { get; protected set; }

    /// <summary>
    /// Optional radio segments for themes that use characters to make up the radio button.
    /// </summary>
    internal Label? OptionalRadioSegment1 { get; set; }
    internal Label? OptionalRadioSegment2 { get; set; }
    internal Label? OptionalRadioSegment3 { get; set; }

    protected bool _selected = false;

    /// <summary>
    /// Returns true if this radio button is selected
    /// </summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value)
                return;

            _selected = value;
            OnValueChanged();
        }
    }

    /// <summary>
    /// The value associated with this radio button
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The label associated with the radio button
    /// </summary>
    public Label? Label { get; protected set; }

    /// <summary>
    /// The text displayed on the label
    /// </summary>
    public string? LabelText
    {
        get => Label?.Text;
        set
        {
            if (Label == null)
            {
                Label = AddChild(new Label());
            }

            Label.Text = value ?? "";
        }
    }

    public RadioButton()
    {
        AddClass("radiobutton");
        ElementName = "radiobutton";

        CheckMark = AddChild(new Panel(this, "checkpanel"));
        var checkLabel = CheckMark.AddChild(new Label("a", "checklabel"));
        
        OptionalRadioSegment1 = AddChild(new Label("", "radio-seg1"));
        OptionalRadioSegment2 = AddChild(new Label("", "radio-seg2"));
        OptionalRadioSegment3 = AddChild(new Label("", "radio-seg3"));
        
        if (CheckMark != null)
        {
            CheckMark.AddChild(OptionalRadioSegment1);
            CheckMark.AddChild(OptionalRadioSegment2);
            CheckMark.AddChild(OptionalRadioSegment3);
        }
    }

    public virtual void SetProperty(string name, string value)
    {
        if (name == "selected" || name == "checked")
        {
            Selected = value == "true" || value == "1";
        }

        if (name == "value")
        {
            Value = value;
        }

        if (name == "text")
        {
            LabelText = value;
        }
    }

    public virtual void SetContent(string? value)
    {
        LabelText = value?.Trim() ?? "";
    }

    /// <summary>
    /// Called when the selection state changes
    /// </summary>
    public event Action<bool>? ValueChanged;

    public virtual void OnValueChanged()
    {
        UpdateState();
        ValueChanged?.Invoke(Selected);

        if (Selected)
        {
            OnSelected?.Invoke();
        }
        else
        {
            OnDeselected?.Invoke();
        }
    }

    /// <summary>
    /// Called when radio button is selected
    /// </summary>
    public event Action? OnSelected;

    /// <summary>
    /// Called when radio button is deselected
    /// </summary>
    public event Action? OnDeselected;

    protected virtual void UpdateState()
    {
        SetClass("checked", Selected);
    }

    /// <summary>
    /// Select this radio button (and deselect others in the group if parent is RadioButtons)
    /// </summary>
    public void Select()
    {
        if (Parent is RadioButtons radioButtons)
        {
            radioButtons.SelectOption(this);
        }
        else
        {
            Selected = true;
        }
    }
}
