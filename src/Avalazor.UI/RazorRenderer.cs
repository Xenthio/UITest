using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace Avalazor.UI;

/// <summary>
/// Simple dispatcher that doesn't enforce thread affinity
/// </summary>
internal class SimpleDispatcher : Dispatcher
{
    public override bool CheckAccess() => true;

    public override Task InvokeAsync(Action workItem)
    {
        workItem();
        return Task.CompletedTask;
    }

    public override Task InvokeAsync(Func<Task> workItem) => workItem();

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem) =>
        Task.FromResult(workItem());

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem) =>
        workItem();
}

/// <summary>
/// Connects Razor component output to Panel tree
/// Processes RenderTree and creates corresponding Panel instances
/// </summary>
public class RazorRenderer : Renderer
{
    private Panel? _rootPanel;
    private readonly Dictionary<int, Panel> _componentToPanelMap = new();
    private readonly SimpleDispatcher _dispatcher = new();

    public RazorRenderer(IServiceProvider serviceProvider) : base(serviceProvider, new NullLoggerFactory())
    {
    }

    public override Dispatcher Dispatcher => _dispatcher;

    public async Task<Panel> RenderComponent<T>() where T : IComponent
    {
        var component = (IComponent)Activator.CreateInstance(typeof(T))!;
        var componentId = AssignRootComponentId(component);
        await RenderRootComponentAsync(componentId);
        return _rootPanel ?? new Panel();
    }

    protected override void HandleException(Exception exception)
    {
        Console.WriteLine($"Renderer exception: {exception}");
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        // Process the render batch and update Panel tree
        for (int i = 0; i < renderBatch.UpdatedComponents.Count; i++)
        {
            var update = renderBatch.UpdatedComponents.Array[i];
            ProcessComponentUpdate(update);
        }

        return Task.CompletedTask;
    }

    private void ProcessComponentUpdate(RenderTreeDiff diff)
    {
        var frames = GetCurrentRenderTreeFrames(diff.ComponentId);
        if (frames.Count == 0) return;

        Console.WriteLine($"Processing component {diff.ComponentId} with {frames.Count} frames");
        
        // Skip the component wrapper frame if present and process the actual content
        int startIndex = 0;
        if (frames.Count > 0 && frames.Array[0].FrameType == RenderTreeFrameType.Component)
        {
            startIndex = 1;
        }

        var panel = ProcessFrames(frames, startIndex, frames.Count - startIndex);
        
        if (_rootPanel == null)
        {
            _rootPanel = panel;
        }
        
        _componentToPanelMap[diff.ComponentId] = panel;
    }

    private Panel ProcessFrames(ArrayRange<RenderTreeFrame> frames, int start, int count)
    {
        var rootPanel = new Panel();
        
        // Apply default styling to make panels visible
        rootPanel.Style = "background-color: white; padding: 10px;";

        int endIndex = start + count;
        for (int i = start; i < endIndex && i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            Console.WriteLine($"Frame {i}: Type={frame.FrameType}, Element={frame.ElementName}, Text={frame.TextContent?.Substring(0, Math.Min(50, frame.TextContent?.Length ?? 0))}");

            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                    var elementPanel = ProcessElement(frames, ref i);
                    rootPanel.AddChild(elementPanel);
                    break;

                case RenderTreeFrameType.Text:
                    if (!string.IsNullOrWhiteSpace(frame.TextContent))
                    {
                        var textPanel = new Label 
                        { 
                            Text = frame.TextContent.Trim()
                        };
                        Console.WriteLine($"Adding text: '{textPanel.Text}'");
                        rootPanel.AddChild(textPanel);
                    }
                    break;

                case RenderTreeFrameType.Markup:
                    Console.WriteLine($"Processing Markup frame with HTML: {frame.MarkupContent?.Substring(0, Math.Min(100, frame.MarkupContent?.Length ?? 0))}...");
                    var markupPanel = ParseMarkupToPanel(frame.MarkupContent);
                    if (markupPanel != null)
                    {
                        rootPanel.AddChild(markupPanel);
                        Console.WriteLine($"Added parsed markup with {markupPanel.Children.Count} children");
                    }
                    break;

                case RenderTreeFrameType.Component:
                    // Handle child components (future enhancement)
                    Console.WriteLine($"Skipping component frame at index {i}");
                    break;
                    
                case RenderTreeFrameType.Region:
                    // Process region content
                    Console.WriteLine($"Processing region at index {i}");
                    break;
            }
        }

        return rootPanel.Children.Count == 1 ? rootPanel.Children[0] : rootPanel;
    }

    private Panel ProcessElement(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        var frame = frames.Array[index];
        var elementName = frame.ElementName;
        var panel = CreatePanelForElement(elementName);
        
        Console.WriteLine($"Processing element: {elementName}");

        // Process attributes
        index++;
        while (index < frames.Count)
        {
            var current = frames.Array[index];
            
            if (current.FrameType == RenderTreeFrameType.Attribute)
            {
                ProcessAttribute(panel, current);
                index++;
            }
            else
            {
                break;
            }
        }

        // Process children - look for text content and nested elements
        var childStart = index;
        var childCount = 0;
        
        // Scan ahead to find the subtree size
        while (index < frames.Count)
        {
            var current = frames.Array[index];
            
            // Check if we're at the end of this element
            if (index > childStart && current.FrameType == RenderTreeFrameType.Element && current.ElementSubtreeLength == 0)
            {
                // This might be a sibling element
                break;
            }
            
            if (current.FrameType == RenderTreeFrameType.Text)
            {
                // Add text directly to label elements
                if (panel is Label label && !string.IsNullOrWhiteSpace(current.TextContent))
                {
                    label.Text = current.TextContent.Trim();
                    Console.WriteLine($"Set label text: '{label.Text}'");
                }
                else if (!string.IsNullOrWhiteSpace(current.TextContent))
                {
                    var textLabel = new Label { Text = current.TextContent.Trim() };
                    panel.AddChild(textLabel);
                    Console.WriteLine($"Added text child: '{textLabel.Text}'");
                }
                index++;
                childCount++;
            }
            else if (current.FrameType == RenderTreeFrameType.Element)
            {
                var childPanel = ProcessElement(frames, ref index);
                panel.AddChild(childPanel);
                childCount++;
            }
            else
            {
                index++;
            }
            
            // Check if we've processed all children based on subtree length
            if (childCount > 0 && index >= childStart + frame.ElementSubtreeLength - 1)
            {
                break;
            }
        }

        return panel;
    }

    private Panel CreatePanelForElement(string elementName)
    {
        Panel panel = elementName.ToLower() switch
        {
            "div" => new Panel { Tag = "div" },
            "span" => new Panel { Tag = "span" },
            "header" => new Panel { Tag = "header", Style = "background-color: #4CAF50; padding: 20px; color: white;" },
            "main" => new Panel { Tag = "main", Style = "padding: 20px;" },
            "footer" => new Panel { Tag = "footer", Style = "background-color: #333; color: white; padding: 10px;" },
            "section" => new Panel { Tag = "section", Style = "margin: 10px 0; padding: 10px; background-color: #f5f5f5;" },
            "h1" => new Label { Tag = "h1", Style = "font-size: 24px; font-weight: bold; margin: 10px 0;" },
            "h2" => new Label { Tag = "h2", Style = "font-size: 20px; font-weight: bold; margin: 8px 0;" },
            "p" => new Label { Tag = "p", Style = "margin: 5px 0;" },
            "ul" => new Panel { Tag = "ul", Style = "margin: 5px 0; padding-left: 20px;" },
            "li" => new Label { Tag = "li", Style = "margin: 3px 0;" },
            "strong" => new Label { Tag = "strong", Style = "font-weight: bold;" },
            "label" => new Label(),
            "button" => new Panel { Tag = "button" }, // Future: Button control
            _ => new Panel()
        };
        
        return panel;
    }

    private void ProcessAttribute(Panel panel, RenderTreeFrame frame)
    {
        var name = frame.AttributeName;
        var value = frame.AttributeValue?.ToString() ?? "";

        switch (name)
        {
            case "class":
                foreach (var className in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    panel.AddClass(className);
                }
                break;

            case "style":
                // Future: Parse inline styles
                break;
        }
    }

    private Panel? ParseMarkupToPanel(string? html)
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

    private Panel? ConvertNodeToPanel(AngleSharp.Dom.IElement? element)
    {
        if (element == null)
            return null;

        // Create panel for this element
        var panel = CreatePanelForElement(element.TagName);
        
        // Set attributes
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
            panel.Style = element.GetAttribute("style");
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
                        var textLabel = new Label { Text = text };
                        panel.AddChild(textLabel);
                    }
                }
            }
        }

        return panel;
    }
}

// Null logger factory for Renderer
internal class NullLoggerFactory : Microsoft.Extensions.Logging.ILoggerFactory
{
    public void AddProvider(Microsoft.Extensions.Logging.ILoggerProvider provider) { }
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new NullLogger();
    public void Dispose() { }
}

internal class NullLogger : Microsoft.Extensions.Logging.ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
