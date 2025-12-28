using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Run the application with FlexboxTest to verify layout (colored divs)
        // Switch to MainApp to test text rendering
        Fazor.UI.FazorApplication.RunPanel<FlexboxTest>(title: "Fazor - Flexbox Layout Test");
        
        // Or use MainApp with text:
        // Fazor.UI.FazorApplication.RunPanel<MainApp>(title: "Fazor - Desktop Razor with XGUI Themes");
    }
}
