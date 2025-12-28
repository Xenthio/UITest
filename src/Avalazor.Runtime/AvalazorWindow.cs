using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Avalazor.Runtime;

/// <summary>
/// Main window that hosts Razor components - hidden from user view
/// </summary>
internal class AvalazorWindow : Window
{
    public AvalazorWindow(Type componentType)
    {
        InitializeComponent(componentType);
    }

    private void InitializeComponent(Type componentType)
    {
        // Set basic window properties
        Title = "Avalazor Application";
        Width = 800;
        Height = 600;
        
        // Create a simple panel with information
        var stackPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 10
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Avalazor - Desktop Razor Framework",
            FontSize = 24,
            FontWeight = FontWeight.Bold
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = $"Component Type: {componentType.Name}",
            FontSize = 14
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Status: Razor transpilation working ✓",
            FontSize = 14
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Status: SCSS compilation working ✓",
            FontSize = 14
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "\nNote: Full Blazor rendering integration is in progress.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.Gray),
            TextWrapping = TextWrapping.Wrap
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "The Razor files are being transpiled to C# successfully, but the runtime renderer",
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.Gray),
            TextWrapping = TextWrapping.Wrap
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "to convert Blazor render trees into Avalonia controls is not yet implemented.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.Gray),
            TextWrapping = TextWrapping.Wrap
        });

        var scrollViewer = new ScrollViewer
        {
            Content = stackPanel
        };

        Content = scrollViewer;
    }
}
