using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using System;

namespace Avalazor.Runtime;

/// <summary>
/// Base Avalonia application class - handles all Avalonia bootstrapping internally
/// </summary>
internal class AvalazorApp : Application
{
    private readonly Type _mainComponentType;

    public AvalazorApp(Type mainComponentType)
    {
        _mainComponentType = mainComponentType;
    }

    public override void Initialize()
    {
        // Set up a basic theme so controls render properly
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create the main window with the user's Razor component
            desktop.MainWindow = new AvalazorWindow(_mainComponentType);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
