using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Run the Panel Inspector Test to demonstrate the new inspector feature
        Avalazor.UI.AvalazorApplication.RunPanel<PanelInspectorTest>(title: "Avalazor - Panel Inspector Demo");
        
        // Other examples:
        // Avalazor.UI.AvalazorApplication.RunPanel<FlexboxTest>(title: "Avalazor - Flexbox Layout Test");
        // Avalazor.UI.AvalazorApplication.RunPanel<XGUIPortTest>(title: "Avalazor - Desktop Razor with XGUI Themes");
    }
}
