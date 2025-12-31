using Microsoft.VisualBasic.FileIO;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Sandbox.UI;


/// <summary>
/// A UI control which provides multiple options via a dropdown box.
/// </summary>
[Library("combobox"), Alias("selector")]
public class ComboBox : Button
{

    //XGUIPopup Pane;
    ///// <summary>
    ///// The icon of an arrow pointing down on the right of the element.
    ///// </summary>
    //protected IconPanel DropdownIndicator;

    /// <summary>
    /// Called when the value has been changed,
    /// </summary>
    public System.Action<string> ValueChanged { get; set; }

    ///// <summary>
    ///// Called just before opening, allows options to be dynamic
    ///// </summary>
    //public System.Func<List<Option>> BuildOptions { get; set; }

    ///// <summary>
    ///// The options to show on click. You can edit these directly via this property.
    ///// </summary>
    //public List<Option> Options { get; set; } = new();

    //Option selected;

    //object _value;
    //int _valueHash;

    ///// <summary>
    ///// The current string value. This is useful to have if Selected is null.
    ///// </summary>
    //public object Value
    //{
    //    get => _value;
    //    set
    //    {
    //        if (_valueHash == HashCode.Combine(value))
    //            return;

    //        if ($"{_value}" == $"{value}")
    //            return;

    //        _valueHash = HashCode.Combine(value);
    //        _value = value;

    //        if (BuildOptions != null)
    //        {
    //            Options = BuildOptions.Invoke();
    //        }

    //        if (_value != null && Options.Count == 0)
    //        {
    //            PopulateOptionsFromType(_value.GetType());
    //        }

    //        Select(_value?.ToString(), false);
    //    }
    //}

    ///// <summary>
    ///// The currently selected option.
    ///// </summary>
    //public Option Selected
    //{
    //    get => selected;
    //    set
    //    {
    //        if (selected == value) return;

    //        selected = value;

    //        if (selected != null)
    //        {
    //            Value = $"{selected.Value}";
    //            Icon = selected.Icon;
    //            Text = selected.Title;

    //            ValueChanged?.Invoke(Value.ToString());
    //            CreateEvent("onchange");
    //            CreateValueEvent("value", selected?.Value);
    //        }
    //    }
    //}

    //public ComboBox()
    //{
    //    AddClass("selector");
    //    DropdownIndicator = Add.Icon("u", "selector_indicator");
    //}

    //public ComboBox(Panel parent) : this()
    //{
    //    Parent = parent;
    //}

    //public override void SetPropertyObject(string name, object value)
    //{
    //    base.SetPropertyObject(name, value);
    //}

    ///// <summary>
    ///// Given the type, populate options. This is useful if you're an enum type.
    ///// </summary>
    //private void PopulateOptionsFromType(Type type)
    //{
    //    if (type == typeof(bool))
    //    {
    //        Options.Add(new Option("True", true));
    //        Options.Add(new Option("False", false));
    //        return;
    //    }

    //    if (type.IsEnum)
    //    {
    //        var names = type.GetEnumNames();
    //        var values = type.GetEnumValues();

    //        for (int i = 0; i < names.Length; i++)
    //        {
    //            Options.Add(new Option(names[i], values.GetValue(i)));
    //        }

    //        return;
    //    }

    //    //Log.Info( $"Dropdown Type: {type}" );
    //}


    //public bool IsOpen = false;
    //protected override void OnMouseDown(MousePanelEvent e)
    //{
    //    base.OnMouseDown(e);
    //    if (!IsOpen)
    //    {
    //        Open();
    //    }
    //    else
    //    {
    //        Close();
    //    }
    //}
    ///// <summary>
    ///// Open the dropdown.
    ///// </summary>
    //public void Open()
    //{
    //    IsOpen = true;
    //    Pane = new XGUIPopup(this, XGUIPopup.PositionMode.BelowStretch, 0.0f);
    //    Pane.Focus();

    //    Pane.AddClass("flat-top");
    //    if (BuildOptions != null)
    //    {
    //        Options = BuildOptions.Invoke();
    //    }

    //    foreach (var option in Options)
    //    {
    //        var o = Pane.AddOption(option.Title, option.Icon, () => Select(option));
    //        if (Selected != null && option.Value == Selected.Value)
    //        {
    //            o.AddClass("active");
    //        }
    //    }
    //}

    //public void Close()
    //{
    //    if (Pane != null)
    //    {
    //        Pane.Delete();
    //        Pane = null;
    //    }
    //    Focus();
    //    IsOpen = false;
    //}

    ///// <summary>
    ///// Select an option.
    ///// </summary>
    //protected virtual void Select(Option option, bool triggerChange = true)
    //{
    //    if (!triggerChange)
    //    {
    //        selected = option;

    //        if (option != null)
    //        {
    //            Value = option.Value;
    //            Icon = option.Icon;
    //            Text = option.Title;
    //        }
    //    }
    //    else
    //    {
    //        Selected = option;
    //    }
    //    Close();
    //}

    ///// <summary>
    ///// Select an option by value string.
    ///// </summary>
    //protected virtual void Select(string value, bool triggerChange = true)
    //{
    //    Select(Options.FirstOrDefault(x => string.Equals(x.Value.ToString(), value, StringComparison.OrdinalIgnoreCase)), triggerChange);
    //}
    //string Override = null;
    //public override void Tick()
    //{
    //    base.Tick();

    //    SetClass("open", Pane != null && !Pane.IsDeleting);
    //    SetClass("active", Pane != null && !Pane.IsDeleting);

    //    if (Pane != null)
    //    {
    //        Pane.Style.Width = Box.Rect.Width * ScaleFromScreen;
    //    }
    //}
    //public override void SetProperty(string name, string value)
    //{
    //    if (name == "default")
    //    {
    //        Override = value;
    //        return;
    //    }
    //    base.SetProperty(name, value);
    //}
    //protected override void OnParametersSet()
    //{
    //    // Only clear if we have some options to populate
    //    if (Children.Any(x => x.ElementName.Equals("option", StringComparison.OrdinalIgnoreCase))) Options.Clear();

    //    foreach (var child in Children.ToList())
    //    {
    //        if (child.ElementName.Equals("option", StringComparison.OrdinalIgnoreCase))
    //        {
    //            var o = new Option();
    //            o.Title = string.Join("", child.Descendants.OfType<Label>().Select(x => x.Text));
    //            o.Value = child.GetAttribute("value", o.Title);
    //            o.Icon = child.GetAttribute("icon", null);

    //            Options.Add(o);
    //            if (Override == child.GetAttribute("value", o.Title))
    //            {
    //                Select(o, true);
    //            }
    //        }
    //    }
    //}
}