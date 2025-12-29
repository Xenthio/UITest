namespace Sandbox.UI;

/// <summary>
/// A title bar for windows with draggable area and control buttons.
/// Based on XGUI-3's TitleBar.
/// </summary>
[Library("titlebar")]
public class TitleBar : Panel
{
    /// <summary>
    /// Reference to the parent window
    /// </summary>
    public Window? ParentWindow { get; set; }

    /// <summary>
    /// The title icon panel
    /// </summary>
    public Panel? TitleIcon { get; protected set; }

    /// <summary>
    /// The title label
    /// </summary>
    public Label? TitleLabel { get; protected set; }

    /// <summary>
    /// The spacer that fills space and acts as drag handle
    /// </summary>
    public Panel? TitleSpacer { get; protected set; }

    /// <summary>
    /// Container for title elements (icon, label, spacer, controls)
    /// </summary>
    public Panel? TitleElements { get; protected set; }

    /// <summary>
    /// Background panel for the title bar
    /// </summary>
    public Panel? TitleBackground { get; protected set; }

    public TitleBar()
    {
        AddClass("titlebar");
        ElementName = "titlebar";
        
        // Create the background first (will be behind other elements)
        TitleBackground = AddChild(new Panel(this, "titlebackground"));
        
        // Create container for title elements
        TitleElements = AddChild(new Panel(this, "titleelements"));
        
        // Add title components
        TitleIcon = TitleElements.AddChild(new Panel(this, "titleicon"));
        TitleLabel = TitleElements.AddChild(new Label("", "titlelabel"));
        TitleSpacer = TitleElements.AddChild(new Panel(this, "titlespacer"));
    }

    public void SetTitle(string title)
    {
        if (TitleLabel != null)
            TitleLabel.Text = title;
    }
}
