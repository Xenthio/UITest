using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace Avalazor.Runtime;

/// <summary>
/// Application builder for Avalazor applications
/// Hides all Avalonia complexity from the user
/// </summary>
public static class AvalazorApplication
{
    /// <summary>
    /// Run an Avalazor application with the specified main component
    /// </summary>
    /// <typeparam name="TMainComponent">The main Razor component type</typeparam>
    /// <param name="args">Command line arguments</param>
    public static void Run<TMainComponent>(string[] args) where TMainComponent : class
    {
        BuildAvaloniaApp<TMainComponent>()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Build the Avalonia application (internal implementation)
    /// </summary>
    private static AppBuilder BuildAvaloniaApp<TMainComponent>() where TMainComponent : class
    {
        return AppBuilder.Configure(() => new AvalazorApp(typeof(TMainComponent)))
            .UsePlatformDetect()
            .LogToTrace();
    }
}
