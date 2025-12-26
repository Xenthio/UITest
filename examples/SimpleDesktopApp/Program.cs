using Avalazor.Runtime;
using System;

namespace SimpleDesktopApp;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Run the application with MainApp as the root component
        // No AXAML files needed - everything is Razor!
        AvalazorApplication.Run<MainApp>(args);
    }
}
