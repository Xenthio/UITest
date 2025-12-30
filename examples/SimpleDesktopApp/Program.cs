using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Enable Panel Inspector in separate windows mode
        // Comment this line to use overlay mode instead
        Avalazor.UI.PanelInspectorHelper.EnableSeparateWindows();
        
        // Or use overlay mode:
        // Avalazor.UI.PanelInspectorHelper.EnableOverlayMode();
        
        // Run the Panel Inspector Test to demonstrate the new inspector feature
        Avalazor.UI.AvalazorApplication.RunPanel<PanelInspectorTest>(title: "Avalazor - Panel Inspector Demo");
        
        // Other examples:
        // Avalazor.UI.AvalazorApplication.RunPanel<FlexboxTest>(title: "Avalazor - Flexbox Layout Test");
        // Avalazor.UI.AvalazorApplication.RunPanel<XGUIPortTest>(title: "Avalazor - Desktop Razor with XGUI Themes");
    }
}
