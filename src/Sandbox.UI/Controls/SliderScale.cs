namespace Sandbox.UI;

/// <summary>
/// A horizontal slider with scale markers.
/// Based on XGUI-3's SliderScale implementation.
/// </summary>
[Library("sliderscale")]
public class SliderScale : Panel
{
    public bool HasScales { get; set; } = true;
    public Panel? SliderArea { get; protected set; }
    public Panel? SliderControl { get; protected set; }
    public Panel? Track { get; protected set; }
    public Panel? TrackInner { get; protected set; }
    public Panel? Thumb { get; protected set; }
    public Label? ThumbIconLabel { get; protected set; }
    public Panel? ScaleSteps { get; protected set; }
    public Label? ScaleStepsMin { get; protected set; }
    public Label? ScaleStepsMax { get; protected set; }
    public Label? Label { get; protected set; }

    private float _minValue = 0;
    private float _maxValue = 100;
    private float _step = 1.0f;
    private float _value = 0;

    /// <summary>
    /// The minimum value of the slider
    /// </summary>
    public float MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (ScaleStepsMin != null)
                ScaleStepsMin.Text = value.ToString();
            UpdateSliderPositions();
        }
    }

    /// <summary>
    /// The maximum value of the slider
    /// </summary>
    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (ScaleStepsMax != null)
                ScaleStepsMax.Text = value.ToString();
            UpdateSliderPositions();
        }
    }

    /// <summary>
    /// Step size for value snapping
    /// </summary>
    public float Step
    {
        get => _step;
        set
        {
            _step = value;
            UpdateSliderPositions();
        }
    }

    /// <summary>
    /// The current value of the slider
    /// </summary>
    public float Value
    {
        get => MathX.Clamp(_value, MinValue, MaxValue);
        set
        {
            var snapped = Step > 0 ? SnapToGrid(value, Step) : value;
            snapped = MathX.Clamp(snapped, MinValue, MaxValue);

            if (_value == snapped) return;

            _value = snapped;
            OnValueChanged();
            UpdateSliderPositions();
        }
    }

    /// <summary>
    /// Called when the value changes
    /// </summary>
    public event Action<float>? ValueChanged;

    public SliderScale()
    {
        AddClass("sliderroot");
        ElementName = "sliderscale";

        Label = AddChild(new Label());
        Label.Style.Display = DisplayMode.None;
        // TODO: AcceptsFocus = true; (not available yet in base Panel)

        SliderArea = AddChild(new Panel(this, "sliderarea"));

        SliderControl = SliderArea.AddChild(new Panel(this, "slider"));
        // TODO: AcceptsFocus = true; (not available yet in base Panel)

        Track = SliderControl.AddChild(new Panel(this, "track"));
        TrackInner = Track.AddChild(new Panel(this, "inner"));

        Thumb = SliderControl.AddChild(new Panel(this, "thumb"));
        ThumbIconLabel = Thumb.AddChild(new Label("", "thumbicon"));

        if (HasScales)
        {
            AddClass("hasscales");
            ScaleSteps = SliderControl.AddChild(new Panel(this, "scalesteps"));
            
            // Add 10 scale step markers
            for (int i = 0; i < 10; i++)
            {
                ScaleSteps.AddChild(new Panel(this, "step"));
            }

            ScaleStepsMin = SliderControl.AddChild(new Label("", "scalestepmin"));
            ScaleStepsMax = SliderControl.AddChild(new Label("", "scalestepmax"));
        }
    }

    public override void SetProperty(string name, string value)
    {
        switch (name)
        {
            case "min":
                if (float.TryParse(value, out float min))
                    MinValue = min;
                return;

            case "max":
                if (float.TryParse(value, out float max))
                    MaxValue = max;
                return;

            case "step":
                if (float.TryParse(value, out float step))
                    Step = step;
                return;

            case "value":
                if (float.TryParse(value, out float val))
                    Value = val;
                return;

            case "mintext":
                if (ScaleStepsMin != null)
                    ScaleStepsMin.Text = value;
                return;

            case "maxtext":
                if (ScaleStepsMax != null)
                    ScaleStepsMax.Text = value;
                return;

            case "label":
                if (Label != null)
                {
                    Label.Style.Display = DisplayMode.Flex;
                    Label.Text = value;
                }
                return;
        }

        base.SetProperty(name, value);
    }

    protected virtual void OnValueChanged()
    {
        ValueChanged?.Invoke(Value);
    }

    /// <summary>
    /// Snap value to grid based on step size
    /// </summary>
    protected static float SnapToGrid(float value, float step)
    {
        if (step <= 0) return value;
        return MathF.Round(value / step) * step;
    }

    private int _positionHash;

    /// <summary>
    /// Update the visual position of the slider based on current value
    /// </summary>
    protected virtual void UpdateSliderPositions()
    {
        var hash = HashCode.Combine(Value, MinValue, MaxValue);
        if (hash == _positionHash) return;

        _positionHash = hash;

        var pos = MathX.LerpInverse(Value, MinValue, MaxValue, true);

        if (TrackInner != null)
        {
            TrackInner.Style.Width = Length.Fraction(pos);
            TrackInner.Style.Dirty();
        }

        if (Thumb != null)
        {
            Thumb.Style.Left = Length.Fraction(pos);
            Thumb.Style.Dirty();
        }
    }

    public override void Tick()
    {
        base.Tick();

        // TODO: Update focus class when HasFocus is available
        // Example: Label.SetClass("focus", SliderControl.HasFocus || Thumb.HasFocus || Label.HasFocus);
    }
}
