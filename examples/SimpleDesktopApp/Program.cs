using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Run the application with FlexboxTest to verify layout (colored divs)
        // Switch to MainApp to test text rendering
        // Avalazor.UI.AvalazorApplication.RunPanel<FlexboxTest>(title: "Avalazor - Flexbox Layout Test");
        
        // Or use MainApp with text:
        Avalazor.UI.AvalazorApplication.RunPanel<MainApp>(title: "Avalazor - Desktop Razor with XGUI Themes");
    }
}
