using System;
using System.Linq;

namespace Sandbox.UI;

/// <summary>
/// A popup panel that positions itself relative to a source panel.
/// Ported from s&box's game/addons/base/code/UI/Popup.cs
/// </summary>
public partial class Popup : BasePopup
{
    /// <summary>
    /// Which panel triggered this popup. Set by <see cref="SetPositioning"/> or the constructor.
    /// </summary>
    public Panel? PopupSource { get; set; }

    /// <summary>
    /// Currently selected option in the popup. Used internally for keyboard navigation.
    /// </summary>
    public Panel? SelectedChild { get; set; }

    /// <summary>
    /// Positioning mode for this popup.
    /// </summary>
    public PositionMode Position { get; set; }

    /// <summary>
    /// Offset away from <see cref="PopupSource"/> based on <see cref="Position"/>.
    /// </summary>
    public float PopupSourceOffset { get; set; }

    /// <summary>
    /// If true, will close this popup when the <see cref="PopupSource"/> is hidden.
    /// </summary>
    public bool CloseWhenParentIsHidden { get; set; } = false;

    /// <summary>
    /// Dictates where a <see cref="Popup"/> is positioned.
    /// </summary>
    public enum PositionMode
    {
        /// <summary>
        /// To the left of the source panel, centered.
        /// </summary>
        Left,

        /// <summary>
        /// To the left of the source panel, aligned to the bottom.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// Above the source panel, aligned to the left.
        /// </summary>
        AboveLeft,

        /// <summary>
        /// Below the source panel, aligning on the left. Do not stretch to size of <see cref="Popup.PopupSource"/>.
        /// </summary>
        BelowLeft,

        /// <summary>
        /// Below the source panel, centered horizontally.
        /// </summary>
        BelowCenter,

        /// <summary>
        /// Below the source panel, stretch to the width of the <see cref="Popup.PopupSource"/>.
        /// </summary>
        BelowStretch,

        /// <summary>
        /// Above, centered
        /// </summary>
        AboveCenter,

        /// <summary>
        /// Position where the mouse cursor is currently
        /// </summary>
        UnderMouse
    }

    public Popup()
    {

    }

    /// <inheritdoc cref="SetPositioning"/>
    public Popup(Panel sourcePanel, PositionMode position, float offset)
    {
        SetPositioning(sourcePanel, position, offset);
    }

    /// <summary>
    /// Sets <see cref="PopupSource"/>, <see cref="Position"/> and <see cref="PopupSourceOffset"/>.
    /// Applies relevant CSS classes.
    /// </summary>
    /// <param name="sourcePanel">Which panel triggered this popup.</param>
    /// <param name="position">Desired positioning mode.</param>
    /// <param name="offset">Offset away from the <paramref name="sourcePanel"/>.</param>
    public void SetPositioning(Panel sourcePanel, PositionMode position, float offset)
    {
        PopupSource = sourcePanel;
        Position = position;
        PopupSourceOffset = offset;

        // Try to create OS-level popup window first via callback
        if (TryCreateOSWindow())
        {
            Console.WriteLine("[Popup] Created OS-level popup window");
            return;
        }

        // Fallback to in-window popup
        Parent = sourcePanel.FindPopupPanel();
        AddClass("popup-panel");
        PositionMe(true);

        switch (Position)
        {
            case PositionMode.Left:
                AddClass("left");
                break;

            case PositionMode.LeftBottom:
                AddClass("left-bottom");
                break;

            case PositionMode.AboveLeft:
                AddClass("above-left");
                break;

            case PositionMode.AboveCenter:
                AddClass("above-center");
                break;

            case PositionMode.BelowLeft:
                AddClass("below-left");
                break;

            case PositionMode.BelowCenter:
                AddClass("below-center");
                break;

            case PositionMode.BelowStretch:
                AddClass("below-stretch");
                break;
        }
    }

    /// <summary>
    /// Delegate for creating OS-level popup windows.
    /// Returns an object representing the OS window, or null if creation failed.
    /// </summary>
    public delegate object? PopupWindowFactory(Panel popup, Panel sourcePanel, int screenX, int screenY, int width, int height);

    /// <summary>
    /// Global factory for creating OS-level popup windows.
    /// Set this to enable OS-level popup windows instead of in-window popups.
    /// </summary>
    public static PopupWindowFactory? OSWindowFactory { get; set; }

    /// <summary>
    /// The OS-level popup window, if one was created
    /// </summary>
    private object? _osWindow;

    /// <summary>
    /// Try to create an OS-level popup window. Returns true if successful.
    /// </summary>
    private bool TryCreateOSWindow()
    {
        if (PopupSource == null || OSWindowFactory == null) return false;

        try
        {
            // Calculate screen position
            var rootPanel = PopupSource.FindRootPanel();
            if (rootPanel == null) return false;

            // Get the source panel's screen rect
            var rect = PopupSource.Box.Rect;
            
            // Calculate popup size (use a reasonable default for now)
            int popupWidth = (int)(rect.Width > 0 ? rect.Width : 200);
            int popupHeight = 200; // Will be adjusted by content

            // Calculate screen position based on positioning mode
            int screenX = 0;
            int screenY = 0;

            switch (Position)
            {
                case PositionMode.BelowStretch:
                case PositionMode.BelowLeft:
                case PositionMode.BelowCenter:
                    screenX = (int)(rect.Left);
                    screenY = (int)(rect.Bottom + PopupSourceOffset);
                    break;

                case PositionMode.AboveLeft:
                case PositionMode.AboveCenter:
                    screenX = (int)(rect.Left);
                    screenY = (int)(rect.Top - popupHeight - PopupSourceOffset);
                    break;

                case PositionMode.Left:
                case PositionMode.LeftBottom:
                    screenX = (int)(rect.Left - popupWidth - PopupSourceOffset);
                    screenY = (int)(rect.Top);
                    break;

                case PositionMode.UnderMouse:
                    screenX = (int)(rootPanel.MousePos.x);
                    screenY = (int)(rootPanel.MousePos.y + PopupSourceOffset);
                    break;
            }

            // Call the factory to create the OS window
            _osWindow = OSWindowFactory(this, PopupSource, screenX, screenY, popupWidth, popupHeight);

            if (_osWindow != null)
            {
                // Mark this popup as using an OS window
                AddClass("os-window-popup");
                
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Popup] Failed to create OS window: {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Header panel that holds <see cref="TitleLabel"/> and <see cref="IconPanel"/>.
    /// </summary>
    protected Panel? Header;

    /// <summary>
    /// Label that displays <see cref="Title"/>.
    /// </summary>
    protected Label? TitleLabel;

    /// <summary>
    /// Panel that displays <see cref="Icon"/>.
    /// </summary>
    protected IconPanel? IconPanel;

    void CreateHeader()
    {
        if (Header?.IsValid() == true) return;

        Header = AddChild<Panel>("header");
        IconPanel = Header.AddChild<IconPanel>();
        TitleLabel = Header.AddChild<Label>("title");
    }

    /// <summary>
    /// If set, will add an unselectable header with given text and <see cref="Icon"/>.
    /// </summary>
    public string? Title
    {
        get => TitleLabel?.Text;
        set
        {
            CreateHeader();
            if (TitleLabel != null) TitleLabel.Text = value;
        }
    }

    /// <summary>
    /// If set, will add an unselectable header with given icon and <see cref="Title"/>.
    /// </summary>
    public string? Icon
    {
        get => IconPanel?.Text;
        set
        {
            CreateHeader();
            if (IconPanel != null) IconPanel.Text = value;
        }
    }

    /// <summary>
    /// Closes all panels, marks this one as a success and closes it.
    /// </summary>
    public void Success()
    {
        AddClass("success");
        BasePopup.CloseAll();
    }

    /// <summary>
    /// Closes all panels, marks this one as a failure and closes it.
    /// </summary>
    public void Failure()
    {
        AddClass("failure");
        BasePopup.CloseAll();
    }

    /// <summary>
    /// Add an option to this popup with given text and click action.
    /// </summary>
    public Panel AddOption(string text, Action? action = null)
    {
        return AddChild(new Button(text, () =>
        {
            BasePopup.CloseAll();
            action?.Invoke();
        }));
    }

    /// <summary>
    /// Add an option to this popup with given text, icon and click action.
    /// </summary>
    public Panel AddOption(string text, string icon, Action? action = null)
    {
        var btn = new Button(text, () => { BasePopup.CloseAll(); action?.Invoke(); });
        btn.Icon = icon;
        return AddChild(btn);
    }

    /// <summary>
    /// Move selection in given direction.
    /// </summary>
    /// <param name="dir">Positive numbers move selection downwards, negative - upwards.</param>
    public void MoveSelection(int dir)
    {
        var currentIndex = GetChildIndex(SelectedChild);

        if (currentIndex >= 0) currentIndex += dir;
        else if (currentIndex < 0) currentIndex = dir == 1 ? 0 : -1;

        SelectedChild?.SetClass("active", false);
        SelectedChild = GetChild(currentIndex, true);
        SelectedChild?.SetClass("active", true);
    }

    public override void Tick()
    {
        base.Tick();

        if (CloseWhenParentIsHidden && PopupSource != null && !PopupSource.IsValid())
        {
            Delete();
            return;
        }

        // Only position if we're not using an OS window
        if (_osWindow == null)
        {
            PositionMe(false);
        }
    }

    public override void OnDeleted()
    {
        // Close OS window if we have one
        if (_osWindow != null)
        {
            // Call Close() method if it exists via reflection
            try
            {
                var closeMethod = _osWindow.GetType().GetMethod("Close");
                closeMethod?.Invoke(_osWindow, null);
            }
            catch
            {
                // Ignore errors during cleanup
            }
            _osWindow = null;
        }

        base.OnDeleted();
    }

    public override void OnLayout(ref Rect layoutRect)
    {
        var padding = 10;
        var rootPanel = FindRootPanel();
        if (rootPanel == null) return;

        var h = rootPanel.PanelBounds.Height - padding;
        var w = rootPanel.PanelBounds.Width - padding;

        if (layoutRect.Bottom > h)
        {
            layoutRect.Top -= layoutRect.Bottom - h;
            layoutRect.Bottom -= layoutRect.Bottom - h;
        }

        if (layoutRect.Right > w)
        {
            layoutRect.Left -= layoutRect.Right - w;
            layoutRect.Right -= layoutRect.Right - w;
        }
    }

    void PositionMe(bool isInitial)
    {
        if (PopupSource == null) return;

        var rect = PopupSource.Box.Rect;
        var scale = PopupSource.ScaleFromScreen;
        rect.Left *= scale;
        rect.Top *= scale;
        rect.Right *= scale;
        rect.Bottom *= scale;

        var rootPanel = FindRootPanel();
        if (rootPanel == null) return;

        var w = rootPanel.PanelBounds.Width * PopupSource.ScaleFromScreen;
        var h = rootPanel.PanelBounds.Height * PopupSource.ScaleFromScreen;

        Style.MaxHeight = rootPanel.PanelBounds.Height - 50;

        switch (Position)
        {
            case PositionMode.Left:
                {
                    Style.Left = null;
                    Style.Right = ((w - rect.Left) + PopupSourceOffset);
                    Style.Top = rect.Top + rect.Height * 0.5f;
                    break;
                }
            case PositionMode.LeftBottom:
                {
                    Style.Left = null;
                    Style.Right = ((w - rect.Left) + PopupSourceOffset);
                    Style.Top = null;
                    Style.Bottom = (h - rect.Bottom);
                    break;
                }

            case PositionMode.AboveLeft:
                {
                    Style.Left = rect.Left;
                    Style.Bottom = (Parent?.Box.Rect.Height ?? h) - rect.Top + PopupSourceOffset;
                    break;
                }

            case PositionMode.AboveCenter:
                {
                    Style.Left = rect.Left + rect.Width * 0.5f;
                    Style.Bottom = (Parent?.Box.Rect.Height ?? h) - rect.Top + PopupSourceOffset;
                    break;
                }

            case PositionMode.BelowLeft:
                {
                    Style.Left = rect.Left;
                    Style.Top = rect.Bottom + PopupSourceOffset;
                    break;
                }

            case PositionMode.BelowCenter:
                {
                    Style.Left = rect.Center.x; // centering is done via styles
                    Style.Top = rect.Bottom + PopupSourceOffset;
                    break;
                }

            case PositionMode.BelowStretch:
                {
                    Style.Left = rect.Left;
                    Style.Width = rect.Width;
                    Style.Top = rect.Bottom + PopupSourceOffset;
                    break;
                }

            case PositionMode.UnderMouse:
                {
                    if (isInitial)
                    {
                        var mousePos = rootPanel.MousePos;
                        Style.Left = mousePos.x * PopupSource.ScaleFromScreen;
                        Style.Top = (mousePos.y + PopupSourceOffset) * PopupSource.ScaleFromScreen;
                    }
                    break;
                }
        }

        Style.Dirty();
    }
}
