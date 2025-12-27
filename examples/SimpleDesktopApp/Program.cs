using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Run the application with MainApp as the root component
        // Now using custom SkiaSharp+Yoga renderer!
        Avalazor.UI.AvalazorApplication.RunComponent<MainApp>(title: "Avalazor - Desktop Razor with XGUI Themes");
    }
}
