using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using AngleSharp.Html.Parser;
using Sandbox.UI.Reflection;
using System.Reflection;

namespace Sandbox.UI.Razor;

/// <summary>
/// Processes the Blazor RenderTree from Panel components and creates child panels.
/// This is the bridge between Blazor's Razor rendering and Sandbox.UI's Panel tree.
/// Adapted from S&box's PanelRenderTreeBuilder approach but using standard Blazor runtime.
/// </summary>
public class RazorTreeProcessor
{
    /// <summary>
    /// Build the render tree for a Panel and populate its children from the Razor markup.
    /// </summary>
    public static void BuildPanelRenderTree(Panel panel)
    {
        // Create a RenderTreeBuilder to capture the output
        using var builder = new RenderTreeBuilder();
        
        // Call the panel's BuildRenderTree method via reflection (it's protected)
        var method = panel.GetType().GetMethod("BuildRenderTree", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic | 
            BindingFlags.Public);
        
        if (method != null)
        {
            try
            {
                method.Invoke(panel, new object[] { builder });
                Console.WriteLine($"BuildRenderTree invoked for {panel.GetType().Name}");
                
                // Get the frames from the builder
                var getFramesMethod = typeof(RenderTreeBuilder).GetMethod("GetFrames", 
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                
                if (getFramesMethod != null)
                {
                    var frames = (ArrayRange<RenderTreeFrame>)getFramesMethod.Invoke(builder, null)!;
                    Console.WriteLine($"Captured {frames.Count} frames");
                    
                    // Process the frames to build child panels
                    ProcessFrames(panel, frames);
                }
                else
                {
                    Console.WriteLine("GetFrames method not found, trying alternative approach");
                    // Alternative: use reflection to access the internal buffer
                    TryProcessBuilderFrames(panel, builder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking BuildRenderTree: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        else
        {
            Console.WriteLine($"BuildRenderTree method not found on {panel.GetType().Name}");
        }
        
        // Call OnAfterTreeRender for the panel
        try
        {
            var afterRenderMethod = panel.GetType().GetMethod("OnAfterTreeRender",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (afterRenderMethod != null)
            {
                afterRenderMethod.Invoke(panel, new object[] { true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling OnAfterTreeRender: {ex.Message}");
        }
    }

    private static void TryProcessBuilderFrames(Panel panel, RenderTreeBuilder builder)
    {
        try
        {
            // Try to access the internal _entries field
            var entriesField = typeof(RenderTreeBuilder).GetField("_entries", 
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (entriesField != null)
            {
                var entries = entriesField.GetValue(builder);
                Console.WriteLine($"Found entries field, type: {entries?.GetType().Name}");
                
                // Process entries if we got them
                if (entries != null)
                {
                    // The entries should be an ArrayBuilder<RenderTreeFrame>
                    var toArrayMethod = entries.GetType().GetMethod("ToArray");
                    if (toArrayMethod != null)
                    {
                        var frameArray = (RenderTreeFrame[]?)toArrayMethod.Invoke(entries, null);
                        if (frameArray != null && frameArray.Length > 0)
                        {
                            ProcessFrameArray(panel, frameArray);
                            return;
                        }
                    }
                }
            }
            
            Console.WriteLine("Could not access RenderTreeBuilder frames - using markup parsing fallback");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing builder frames: {ex.Message}");
        }
    }

    private static void ProcessFrames(Panel panel, ArrayRange<RenderTreeFrame> frames)
    {
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            Console.WriteLine($"Frame {i}: Type={frame.FrameType}");
            
            if (frame.FrameType == RenderTreeFrameType.Markup)
            {
                var markup = frame.MarkupContent;
                
                if (string.IsNullOrWhiteSpace(markup))
                {
                    Console.WriteLine("Found empty or null markup content.");
                    continue;
                }

                var previewLength = Math.Min(100, markup.Length);
                Console.WriteLine($"Found markup content: {markup.Substring(0, previewLength)}...");

                var childPanel = ParseMarkupToPanel(markup);
                if (childPanel != null)
                {
                    panel.AddChild(childPanel);
                    Console.WriteLine($"Added panel with {childPanel.ChildrenCount} children from markup");
                }
            }
            else if (frame.FrameType == RenderTreeFrameType.Text)
            {
                var text = frame.TextContent;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var label = new Label(text.Trim());
                    panel.AddChild(label);
                    Console.WriteLine($"Added text label: {text.Trim()}");
                }
            }
        }
    }

    private static void ProcessFrameArray(Panel panel, RenderTreeFrame[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            Console.WriteLine($"Frame {i}: Type={frame.FrameType}");
            
            if (frame.FrameType == RenderTreeFrameType.Markup)
            {
                var markup = frame.MarkupContent;
                
                if (string.IsNullOrWhiteSpace(markup))
                {
                    Console.WriteLine("Found empty or null markup content.");
                    continue;
                }

                var previewLength = Math.Min(100, markup.Length);
                Console.WriteLine($"Found markup content: {markup.Substring(0, previewLength)}...");

                var childPanel = ParseMarkupToPanel(markup);
                if (childPanel != null)
                {
                    panel.AddChild(childPanel);
                    Console.WriteLine($"Added panel with {childPanel.ChildrenCount} children from markup");
                }
            }
            else if (frame.FrameType == RenderTreeFrameType.Text)
            {
                var text = frame.TextContent;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var label = new Label(text.Trim());
                    panel.AddChild(label);
                    Console.WriteLine($"Added text label: {text.Trim()}");
                }
            }
        }
    }

    private static Panel? ParseMarkupToPanel(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        try
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);
            
            // Convert the first element in the body
            var firstElement = document.Body?.FirstElementChild;
            if (firstElement == null)
                return null;
            
            return ConvertNodeToPanel(firstElement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing markup: {ex.Message}");
            return null;
        }
    }

    private static Panel? ConvertNodeToPanel(AngleSharp.Dom.IElement? element)
    {
        if (element == null)
            return null;

        // Create panel for this element
        var panel = CreatePanelForElement(element.TagName);
        
        // Set attributes
        foreach (var attr in element.Attributes)
        {
            panel.SetProperty(attr.Name, attr.Value);
        }
        
        if (element.HasAttribute("class"))
        {
            var className = element.GetAttribute("class");
            if (!string.IsNullOrEmpty(className))
            {
                foreach (var cls in className.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    panel.AddClass(cls);
                }
            }
        }
        
        if (element.HasAttribute("style"))
        {
            var style = element.GetAttribute("style");
            if (!string.IsNullOrEmpty(style))
            {
                ParseInlineStyle(panel, style);
            }
        }

        // Process child nodes
        foreach (var child in element.ChildNodes)
        {
            if (child is AngleSharp.Dom.IElement childElement)
            {
                var childPanel = ConvertNodeToPanel(childElement);
                if (childPanel != null)
                {
                    panel.AddChild(childPanel);
                }
            }
            else if (child is AngleSharp.Dom.IText textNode)
            {
                var text = textNode.TextContent?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Add text directly to label elements
                    if (panel is Label label)
                    {
                        label.Text = text;
                    }
                    else
                    {
                        var textLabel = new Label(text);
                        panel.AddChild(textLabel);
                    }
                }
            }
        }

        return panel;
    }

    private static Panel CreatePanelForElement(string elementName)
    {
        var lowerName = elementName.ToLower();
        
        // Try to create using PanelFactory (for controls with [Library] or [Alias] attributes)
        var panel = PanelFactory.Create(lowerName);
        if (panel != null)
        {
            panel.ElementName = lowerName;
            Console.WriteLine($"Created {panel.GetType().Name} for element '{lowerName}' via PanelFactory");
            return panel;
        }
        
        // Fallback to built-in HTML elements
        panel = lowerName switch
        {
            "div" => new Panel { ElementName = "div" },
            "span" => new Panel { ElementName = "span" },
            "header" => new Panel { ElementName = "header" },
            "main" => new Panel { ElementName = "main" },
            "footer" => new Panel { ElementName = "footer" },
            "section" => new Panel { ElementName = "section" },
            "h1" => CreateStyledLabel("h1", 24, 700),
            "h2" => CreateStyledLabel("h2", 20, 700),
            "h3" => CreateStyledLabel("h3", 18, 600),
            "p" => CreateStyledLabel("p", 14, 400),
            "ul" => new Panel { ElementName = "ul" },
            "li" => new Panel { ElementName = "li" },
            "strong" => CreateStyledLabel("strong", 14, 700),
            "label" => new Label(),
            _ => new Panel { ElementName = lowerName }
        };
        
        return panel;
    }

    private static Label CreateStyledLabel(string elementName, float fontSize, int fontWeight)
    {
        var label = new Label { ElementName = elementName };
        label.Style.FontSize = Length.Pixels(fontSize);
        label.Style.FontWeight = fontWeight;
        return label;
    }

    private static void ParseInlineStyle(Panel panel, string styleString)
    {
        var styles = styleString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var style in styles)
        {
            var parts = style.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            var property = parts[0].ToLower().Trim();
            var value = parts[1].Trim();

            try
            {
                switch (property)
                {
                    case "background-color":
                        panel.Style.BackgroundColor = ParseColor(value);
                        break;
                    case "color":
                        panel.Style.FontColor = ParseColor(value);
                        break;
                    case "width":
                        panel.Style.Width = ParseLength(value);
                        break;
                    case "height":
                        panel.Style.Height = ParseLength(value);
                        break;
                    case "padding":
                        var padding = ParseLength(value);
                        panel.Style.PaddingLeft = padding;
                        panel.Style.PaddingTop = padding;
                        panel.Style.PaddingRight = padding;
                        panel.Style.PaddingBottom = padding;
                        break;
                    case "padding-left":
                        panel.Style.PaddingLeft = ParseLength(value);
                        break;
                    case "padding-top":
                        panel.Style.PaddingTop = ParseLength(value);
                        break;
                    case "padding-right":
                        panel.Style.PaddingRight = ParseLength(value);
                        break;
                    case "padding-bottom":
                        panel.Style.PaddingBottom = ParseLength(value);
                        break;
                    case "margin":
                        var margin = ParseLength(value);
                        panel.Style.MarginLeft = margin;
                        panel.Style.MarginTop = margin;
                        panel.Style.MarginRight = margin;
                        panel.Style.MarginBottom = margin;
                        break;
                    case "margin-left":
                        panel.Style.MarginLeft = ParseLength(value);
                        break;
                    case "margin-top":
                        panel.Style.MarginTop = ParseLength(value);
                        break;
                    case "margin-right":
                        panel.Style.MarginRight = ParseLength(value);
                        break;
                    case "margin-bottom":
                        panel.Style.MarginBottom = ParseLength(value);
                        break;
                    case "font-size":
                        panel.Style.FontSize = ParseLength(value);
                        break;
                    case "display":
                        if (value == "flex")
                            panel.Style.Display = DisplayMode.Flex;
                        else if (value == "none")
                            panel.Style.Display = DisplayMode.None;
                        break;
                    case "flex-direction":
                        if (value == "column")
                            panel.Style.FlexDirection = FlexDirection.Column;
                        else if (value == "row")
                            panel.Style.FlexDirection = FlexDirection.Row;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing style '{property}: {value}': {ex.Message}");
            }
        }
    }

    private static Color ParseColor(string value)
    {
        value = value.Trim().ToLower();
        
        // Handle hex colors
        if (value.StartsWith("#"))
        {
            value = value.Substring(1);
            if (value.Length == 3)
            {
                // Expand shorthand (#RGB -> #RRGGBB)
                value = $"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}";
            }
            
            if (value.Length == 6)
            {
                int r = Convert.ToInt32(value.Substring(0, 2), 16);
                int g = Convert.ToInt32(value.Substring(2, 2), 16);
                int b = Convert.ToInt32(value.Substring(4, 2), 16);
                return new Color(r / 255f, g / 255f, b / 255f, 1f);
            }
        }
        
        // Handle named colors (basic set)
        return value switch
        {
            "white" => new Color(1, 1, 1, 1),
            "black" => new Color(0, 0, 0, 1),
            "red" => new Color(1, 0, 0, 1),
            "green" => new Color(0, 0.5f, 0, 1),
            "blue" => new Color(0, 0, 1, 1),
            "yellow" => new Color(1, 1, 0, 1),
            "transparent" => new Color(0, 0, 0, 0),
            _ => new Color(0.5f, 0.5f, 0.5f, 1f) // Default gray
        };
    }

    private static Length ParseLength(string value)
    {
        value = value.Trim().ToLower();
        
        if (value.EndsWith("px"))
        {
            if (float.TryParse(value.Replace("px", ""), out float px))
                return Length.Pixels(px);
        }
        else if (value.EndsWith("%"))
        {
            if (float.TryParse(value.Replace("%", ""), out float percent))
                return Length.Percent(percent);
        }
        else if (value == "auto")
        {
            return Length.Auto;
        }
        else if (float.TryParse(value, out float num))
        {
            return Length.Pixels(num);
        }
        
        return Length.Auto;
    }
}
