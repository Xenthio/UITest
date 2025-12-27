using ExCSS;
using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// CSS style engine - parses and computes styles for panels
/// Based on s&box's style computation system
/// </summary>
public class StyleEngine
{
    private readonly Dictionary<string, Stylesheet> _stylesheets = new();
    private readonly StylesheetParser _cssParser = new();

    public void AddStylesheet(string name, string css)
    {
        try
        {
            var stylesheet = _cssParser.Parse(css);
            _stylesheets[name] = stylesheet;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse stylesheet '{name}': {ex.Message}");
        }
    }

    public ComputedStyle ComputeStyle(Panel panel)
    {
        var style = new ComputedStyle();

        // Apply matched CSS rules
        foreach (var stylesheet in _stylesheets.Values)
        {
            foreach (var rule in stylesheet.StyleRules)
            {
                if (SelectorMatches(rule.Selector, panel))
                {
                    ApplyDeclarations(style, rule.Style);
                }
            }
        }

        // Apply inline styles
        if (!string.IsNullOrWhiteSpace(panel.Style))
        {
            var inlineStyle = _cssParser.Parse($"* {{{panel.Style}}}");
            var styleRules = inlineStyle.StyleRules.ToList();
            if (styleRules.Count > 0)
            {
                ApplyDeclarations(style, styleRules[0].Style);
            }
        }

        return style;
    }

    private bool SelectorMatches(ISelector selector, Panel panel)
    {
        var selectorText = selector.Text;

        // Simple tag matching
        if (selectorText == panel.Tag || selectorText == "*")
            return true;

        // Class matching
        if (selectorText.StartsWith("."))
        {
            var className = selectorText.Substring(1);
            return panel.HasClass(className);
        }

        // TODO: Implement full CSS selector matching
        return false;
    }

    private void ApplyDeclarations(ComputedStyle style, StyleDeclaration declarations)
    {
        foreach (var prop in declarations)
        {
            var name = prop.Name.ToLowerInvariant();
            var value = prop.Value;

            switch (name)
            {
                case "width":
                    style.Width = ParseLength(value);
                    break;
                case "height":
                    style.Height = ParseLength(value);
                    break;
                case "min-width":
                    style.MinWidth = ParseLength(value);
                    break;
                case "min-height":
                    style.MinHeight = ParseLength(value);
                    break;
                case "max-width":
                    style.MaxWidth = ParseLength(value);
                    break;
                case "max-height":
                    style.MaxHeight = ParseLength(value);
                    break;
                case "background-color":
                    style.BackgroundColor = ParseColor(value);
                    break;
                case "color":
                    style.Color = ParseColor(value);
                    break;
                case "border-width":
                    style.BorderWidth = ParseLength(value) ?? 0;
                    break;
                case "border-color":
                    style.BorderColor = ParseColor(value);
                    break;
                case "border-radius":
                    style.BorderRadius = ParseLength(value) ?? 0;
                    break;
                case "flex-direction":
                    style.FlexDirection = ParseFlexDirection(value);
                    break;
                case "flex-grow":
                    style.FlexGrow = float.TryParse(value, out var fg) ? fg : 0;
                    break;
                case "flex-shrink":
                    style.FlexShrink = float.TryParse(value, out var fs) ? fs : 1;
                    break;
                case "margin-top":
                    style.MarginTop = ParseLength(value) ?? 0;
                    break;
                case "margin-right":
                    style.MarginRight = ParseLength(value) ?? 0;
                    break;
                case "margin-bottom":
                    style.MarginBottom = ParseLength(value) ?? 0;
                    break;
                case "margin-left":
                    style.MarginLeft = ParseLength(value) ?? 0;
                    break;
                case "padding-top":
                    style.PaddingTop = ParseLength(value) ?? 0;
                    break;
                case "padding-right":
                    style.PaddingRight = ParseLength(value) ?? 0;
                    break;
                case "padding-bottom":
                    style.PaddingBottom = ParseLength(value) ?? 0;
                    break;
                case "padding-left":
                    style.PaddingLeft = ParseLength(value) ?? 0;
                    break;
                case "font-size":
                    style.FontSize = ParseLength(value) ?? 14;
                    break;
            }
        }
    }

    private float? ParseLength(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Trim().ToLowerInvariant();

        // Remove "px" suffix
        if (value.EndsWith("px"))
            value = value.Substring(0, value.Length - 2);

        return float.TryParse(value, out var result) ? result : null;
    }

    private SKColor? ParseColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Trim();

        // Hex colors
        if (value.StartsWith("#"))
        {
            return SKColor.TryParse(value, out var color) ? color : null;
        }

        // rgb() / rgba()
        if (value.StartsWith("rgb"))
        {
            // TODO: Implement RGB parsing
            return null;
        }

        // Named colors
        return value.ToLowerInvariant() switch
        {
            "transparent" => SKColors.Transparent,
            "white" => SKColors.White,
            "black" => SKColors.Black,
            "red" => SKColors.Red,
            "green" => SKColors.Green,
            "blue" => SKColors.Blue,
            _ => null
        };
    }

    private FlexDirection ParseFlexDirection(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "row" => FlexDirection.Row,
            "column" => FlexDirection.Column,
            "row-reverse" => FlexDirection.RowReverse,
            "column-reverse" => FlexDirection.ColumnReverse,
            _ => FlexDirection.Column
        };
    }
}
