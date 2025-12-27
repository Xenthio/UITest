using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Collections.Generic;

namespace Avalazor.UI;

/// <summary>
/// Connects Razor component output to Panel tree
/// Processes RenderTree and creates corresponding Panel instances
/// </summary>
public class RazorRenderer : Renderer
{
    private Panel? _rootPanel;
    private readonly Dictionary<int, Panel> _componentToPanelMap = new();

    public RazorRenderer(IServiceProvider serviceProvider) : base(serviceProvider, new NullLoggerFactory())
    {
    }

    public async Task<Panel> RenderComponent<T>() where T : IComponent
    {
        var componentId = AssignRootComponentId(typeof(T));
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
        foreach (var update in renderBatch.UpdatedComponents)
        {
            ProcessComponentUpdate(update);
        }

        return Task.CompletedTask;
    }

    private void ProcessComponentUpdate(RenderTreeDiff diff)
    {
        var frames = GetCurrentRenderTreeFrames(diff.ComponentId);
        if (frames.Count == 0) return;

        var panel = ProcessFrames(frames, 0, frames.Count);
        
        if (_rootPanel == null)
        {
            _rootPanel = panel;
        }
        
        _componentToPanelMap[diff.ComponentId] = panel;
    }

    private Panel ProcessFrames(ArrayRange<RenderTreeFrame> frames, int start, int count)
    {
        var rootPanel = new Panel();

        for (int i = start; i < start + count; i++)
        {
            var frame = frames.Array[i];

            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                    var elementPanel = ProcessElement(frames, ref i);
                    rootPanel.AddChild(elementPanel);
                    break;

                case RenderTreeFrameType.Text:
                    var textPanel = new Controls.Label 
                    { 
                        Text = frame.TextContent 
                    };
                    rootPanel.AddChild(textPanel);
                    break;

                case RenderTreeFrameType.Component:
                    // Handle child components (future enhancement)
                    break;
            }
        }

        return rootPanel.Children.Count == 1 ? rootPanel.Children[0] : rootPanel;
    }

    private Panel ProcessElement(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        var frame = frames.Array[index];
        var panel = CreatePanelForElement(frame.ElementName);

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

        // Process children (content until we reach the matching close tag)
        var depth = 1;
        var childStart = index;
        
        while (index < frames.Count && depth > 0)
        {
            var current = frames.Array[index];
            
            if (current.FrameType == RenderTreeFrameType.Element)
            {
                depth++;
            }
            else if (current.FrameType == RenderTreeFrameType.ElementReferenceCapture)
            {
                depth--;
                if (depth == 0) break;
            }
            
            index++;
        }

        // Process child content
        if (index > childStart)
        {
            var childPanel = ProcessFrames(frames, childStart, index - childStart);
            if (childPanel.Children.Count > 0)
            {
                foreach (var child in childPanel.Children)
                {
                    panel.AddChild(child);
                }
            }
        }

        return panel;
    }

    private Panel CreatePanelForElement(string elementName)
    {
        return elementName.ToLower() switch
        {
            "div" => new Panel(),
            "span" => new Panel(),
            "label" => new Controls.Label(),
            "button" => new Panel(), // Future: Button control
            _ => new Panel()
        };
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
