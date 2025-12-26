using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Avalazor.Runtime;

/// <summary>
/// Main window that hosts Razor components - hidden from user view
/// </summary>
internal class AvalazorWindow : Window
{
    public AvalazorWindow(Type componentType)
    {
        InitializeComponent();
        
        // TODO: Integrate Blazor component rendering here
        // For now, just set basic window properties
        Title = "Avalazor Application";
        Width = 800;
        Height = 600;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
