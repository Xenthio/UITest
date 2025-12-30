namespace Sandbox.UI;

/// <summary>
/// Base class for windows in the application.
/// Derive from this class to create windows that can interact with the native window system.
/// Manages window properties like title, size, and controls, and creates the appropriate UI structure.
/// Based on XGUI-3's Window implementation.
/// </summary>
[Library("window")]
public class Window : Panel
{
    private string _title = "Window";
    private object? _nativeWindow; // Reference to the native window (e.g., AvalazorWindow)

    /// <summary>
    /// The window title displayed in the native window title bar and optional in-window title bar
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnTitleChanged();
        }
    }

    /// <summary>
    /// The title bar of the window (if HasTitleBar is true)
    /// </summary>
    public TitleBar? TitleBar { get; set; }

    /// <summary>
    /// Initial width of the native window (used when creating the window)
    /// </summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>
    /// Initial height of the native window (used when creating the window)
    /// </summary>
    public int WindowHeight { get; set; } = 720;

    /// <summary>
    /// Position of the in-window panel (for floating window UI elements)
    /// Not the same as the native window position
    /// </summary>
    public Vector2 Position { get; set; } = Vector2.Zero;

    /// <summary>
    /// Size of the in-window panel (for floating window UI elements)
    /// </summary>
    public Vector2 Size { get; set; } = Vector2.Zero;

    /// <summary>
    /// Minimum size of the in-window panel
    /// </summary>
    public Vector2 MinSize { get; set; } = new Vector2(100, 50);

    /// <summary>
    /// Whether to use custom chrome (in-window title bar).
    /// When false (default), no title bar is drawn.
    /// When true, draws a title bar inside the window.
    /// Can also be controlled via CSS: --custom-chrome: true;
    /// </summary>
    public bool HasCustomChrome { get; set; } = false;
    
    /// <summary>
    /// Whether the window has a title bar (legacy property, use HasCustomChrome instead)
    /// </summary>
    [Obsolete("Use HasCustomChrome instead")]
    public bool HasTitleBar { get; set; } = false;

    /// <summary>
    /// Whether the window has control buttons
    /// </summary>
    public bool HasControls { get; set; } = true;

    /// <summary>
    /// Whether the window has a minimize button
    /// </summary>
    public bool HasMinimise { get; set; } = false;

    /// <summary>
    /// Whether the window has a maximize button
    /// </summary>
    public bool HasMaximise { get; set; } = false;

    /// <summary>
    /// Whether the window has a close button
    /// </summary>
    public bool HasClose { get; set; } = true;

    /// <summary>
    /// Whether the window can be resized
    /// </summary>
    public bool IsResizable { get; set; } = true;

    /// <summary>
    /// Whether the window can be dragged
    /// </summary>
    public bool IsDraggable { get; set; } = true;

    /// <summary>
    /// Whether to automatically focus the window on creation
    /// </summary>
    public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// The close button control
    /// </summary>
    public Button? ControlsClose { get; set; }

    /// <summary>
    /// The minimize button control
    /// </summary>
    public Button? ControlsMinimise { get; set; }

    /// <summary>
    /// The maximize button control
    /// </summary>
    public Button? ControlsMaximise { get; set; }

    /// <summary>
    /// The main content panel of the window
    /// </summary>
    public Panel? WindowContent { get; set; }

    /// <summary>
    /// Whether the window is minimized
    /// </summary>
    public bool IsMinimised { get; set; } = false;

    /// <summary>
    /// Whether the window is maximized
    /// </summary>
    public bool IsMaximised { get; set; } = false;

    private Vector2 _preMinimisedSize;
    private Vector2 _preMinimisedPos;
    private Vector2 _preMaximisedSize;
    private Vector2 _preMaximisedPos;

    private bool _dragging = false;
    private float _dragOffsetX = 0;
    private float _dragOffsetY = 0;

    public Window()
    {
        AddClass("panel");
        AddClass("window");
        ElementName = "window";

        // Fill the entire root panel by default
        Style.Position = PositionMode.Absolute;
        Style.FlexDirection = FlexDirection.Column;
        Style.Left = 0;
        Style.Top = 0;
        Style.Width = Length.Percent(100);
        Style.Height = Length.Percent(100);

        // Don't create TitleBar here - let CreateTitleBar handle it
        // This allows for truly optional custom title bar
    }

    /// <summary>
    /// Called when the window title changes.
    /// Override this to handle title updates in the native window.
    /// </summary>
    protected virtual void OnTitleChanged()
    {
        // Update the title bar if it exists
        if (TitleBar != null)
        {
            TitleBar.SetTitle(Title);
        }

        // Update the native window title if we have a reference
        UpdateNativeWindowTitle();
    }

    /// <summary>
    /// Sets a reference to the native window for dynamic updates.
    /// Called by the application framework.
    /// </summary>
    public void SetNativeWindow(object nativeWindow)
    {
        _nativeWindow = nativeWindow;
        UpdateNativeWindowTitle();
    }

    /// <summary>
    /// Updates the native window title if possible
    /// </summary>
    private void UpdateNativeWindowTitle()
    {
        // Try to update the native window title through reflection to avoid direct dependency
        if (_nativeWindow != null)
        {
            var setTitleMethod = _nativeWindow.GetType().GetMethod("SetTitle");
            if (setTitleMethod != null)
            {
                setTitleMethod.Invoke(_nativeWindow, new object[] { Title });
            }
        }
    }

    /// <summary>
    /// Create a content panel for the window
    /// </summary>
    public Panel CreateWindowContentPanel()
    {
        var contentPanel = AddChild(new Panel(this, "window-content"));
        WindowContent = contentPanel;
        return contentPanel;
    }

    protected override void OnAfterTreeRender(bool firstTime)
    {
        base.OnAfterTreeRender(firstTime);

        if (firstTime)
        {
            // Process root element attributes from Razor markup
            // This allows automatic property binding from <root> element
            ProcessRootElementAttributes();

            // Find window content (look for first non-titlebar child or explicit window-content)
            foreach (var child in Children)
            {
                if (child != null && child.HasClass("window-content"))
                {
                    WindowContent = child;
                    break;
                }
            }

            // If no explicit window-content found, wrap all children (except titlebar) in one
            if (WindowContent == null && ChildrenCount > 0)
            {
                // Create content panel and move all current children into it
                var contentPanel = new Panel();
                contentPanel.AddClass("window-content");
                var childrenToMove = Children.ToList();
                
                foreach (var child in childrenToMove)
                {
                    if (child != TitleBar && child != null)
                    {
                        child.Parent = null;
                        contentPanel.AddChild(child);
                    }
                }
                
                AddChild(contentPanel);
                WindowContent = contentPanel;
            }

            // Create title bar after processing attributes
            CreateTitleBar();

            if (AutoFocus)
            {
                FocusWindow();
                AutoFocus = false;
            }

            // Apply initial size if set
            if (Size != Vector2.Zero)
            {
                Style.Width = Size.x;
                Style.Height = Size.y;
            }
        }

        // Ensure title bar is first child
        if (TitleBar != null && TitleBar.IsValid())
        {
            SetChildIndex(TitleBar, 0);
        }
    }

    /// <summary>
    /// Process attributes from the root element in Razor markup.
    /// This allows declarative property binding like &lt;root title="Hello" windowwidth="800"&gt;
    /// </summary>
    private void ProcessRootElementAttributes()
    {
        // Check for 'title' attribute
        var titleAttr = GetAttribute("title");
        if (!string.IsNullOrEmpty(titleAttr))
        {
            Title = titleAttr;
        }

        // Check for native window size attributes
        var windowWidthAttr = GetAttribute("windowwidth");
        if (!string.IsNullOrEmpty(windowWidthAttr) && int.TryParse(windowWidthAttr, out int ww))
        {
            WindowWidth = ww;
        }

        var windowHeightAttr = GetAttribute("windowheight");
        if (!string.IsNullOrEmpty(windowHeightAttr) && int.TryParse(windowHeightAttr, out int wh))
        {
            WindowHeight = wh;
        }

        // Check for window control flags
        var hasMinimiseAttr = GetAttribute("hasminimise");
        if (!string.IsNullOrEmpty(hasMinimiseAttr))
        {
            HasMinimise = hasMinimiseAttr == "true" || hasMinimiseAttr == "1";
        }

        var hasMaximiseAttr = GetAttribute("hasmaximise");
        if (!string.IsNullOrEmpty(hasMaximiseAttr))
        {
            HasMaximise = hasMaximiseAttr == "true" || hasMaximiseAttr == "1";
        }

        var hasCloseAttr = GetAttribute("hasclose");
        if (!string.IsNullOrEmpty(hasCloseAttr))
        {
            HasClose = hasCloseAttr == "true" || hasCloseAttr == "1";
        }

        var hasTitleBarAttr = GetAttribute("hastitlebar");
        if (!string.IsNullOrEmpty(hasTitleBarAttr))
        {
            HasTitleBar = hasTitleBarAttr == "true" || hasTitleBarAttr == "1";
        }

        var hasCustomChromeAttr = GetAttribute("hascustomchrome");
        if (!string.IsNullOrEmpty(hasCustomChromeAttr))
        {
            HasCustomChrome = hasCustomChromeAttr == "true" || hasCustomChromeAttr == "1";
        }

        // Check for size and position attributes
        var widthAttr = GetAttribute("width");
        if (!string.IsNullOrEmpty(widthAttr) && float.TryParse(widthAttr, out float width))
        {
            Size = new Vector2(width, Size.y);
        }

        var heightAttr = GetAttribute("height");
        if (!string.IsNullOrEmpty(heightAttr) && float.TryParse(heightAttr, out float height))
        {
            Size = new Vector2(Size.x, height);
        }

        var xAttr = GetAttribute("x");
        if (!string.IsNullOrEmpty(xAttr) && float.TryParse(xAttr, out float x))
        {
            Position = new Vector2(x, Position.y);
        }

        var yAttr = GetAttribute("y");
        if (!string.IsNullOrEmpty(yAttr) && float.TryParse(yAttr, out float y))
        {
            Position = new Vector2(Position.x, y);
        }

        // Check for draggable and resizable
        var isDraggableAttr = GetAttribute("isdraggable");
        if (!string.IsNullOrEmpty(isDraggableAttr))
        {
            IsDraggable = isDraggableAttr == "true" || isDraggableAttr == "1";
        }

        var isResizableAttr = GetAttribute("isresizable");
        if (!string.IsNullOrEmpty(isResizableAttr))
        {
            IsResizable = isResizableAttr == "true" || isResizableAttr == "1";
        }
    }

    /// <summary>
    /// Create and configure the title bar with controls.
    /// Only creates title bar if HasCustomChrome is true OR if theme CSS sets --custom-chrome: true
    /// </summary>
    public void CreateTitleBar()
    {
        // Check if theme requests custom chrome via CSS custom property
        bool themeRequestsChrome = false;
        if (ComputedStyle != null)
        {
            // Check for --custom-chrome CSS variable
            var customChromeVar = ComputedStyle.GetCustomProperty("--custom-chrome");
            themeRequestsChrome = customChromeVar == "true" || customChromeVar == "1";
        }

        // Don't create title bar unless explicitly requested
        if (!HasCustomChrome && !HasTitleBar && !themeRequestsChrome) return;

        // Create TitleBar if it doesn't exist
        if (TitleBar == null)
        {
            TitleBar = new TitleBar();
            TitleBar.ParentWindow = this;
            AddChild(TitleBar);
        }

        // Set title
        TitleBar.SetTitle(Title);

        // Create control buttons
        if (HasControls)
        {
            var titleElements = TitleBar.TitleElements;
            if (titleElements != null)
            {
                if (HasMinimise)
                {
                    ControlsMinimise = titleElements.AddChild(new Button());
                    ControlsMinimise.AddClass("control");
                    ControlsMinimise.AddClass("minimisebutton");
                    ControlsMinimise.Text = "0"; // Marlett font character
                    ControlsMinimise.OnClick += () => Minimise();
                }

                if (HasMaximise)
                {
                    ControlsMaximise = titleElements.AddChild(new Button());
                    ControlsMaximise.AddClass("control");
                    ControlsMaximise.AddClass("maximisebutton");
                    ControlsMaximise.Text = "1"; // Marlett font character
                    ControlsMaximise.OnClick += () => Maximise();
                }

                if (HasClose)
                {
                    ControlsClose = titleElements.AddChild(new Button());
                    ControlsClose.AddClass("control");
                    ControlsClose.AddClass("closebutton");
                    ControlsClose.Text = "r"; // Marlett font character
                    ControlsClose.OnClick += () => Close();
                }
            }
        }
    }

    /// <summary>
    /// Minimize the window
    /// </summary>
    public void Minimise()
    {
        if (!IsMinimised)
        {
            _preMinimisedSize = new Vector2(Box.Rect.Width, Box.Rect.Height);
            _preMinimisedPos = Position;

            // Calculate position for minimized window (bottom of parent)
            if (Parent != null)
            {
                var offset = 0f;
                // Offset for other minimized windows
                foreach (var child in Parent.Children)
                {
                    if (child is Window win && win.IsMinimised && win != this)
                    {
                        offset += 180;
                    }
                }
                Position = new Vector2(offset, Parent.Box.Rect.Height - 30);
            }

            Style.Width = 180;
            Style.Height = 30;
            IsMinimised = true;
        }
        else
        {
            // Restore
            Style.Width = _preMinimisedSize.x;
            Style.Height = _preMinimisedSize.y;
            Position = _preMinimisedPos;
            IsMinimised = false;
        }
    }

    /// <summary>
    /// Maximize the window
    /// </summary>
    public void Maximise()
    {
        if (!IsMaximised)
        {
            _preMaximisedSize = new Vector2(Box.Rect.Width, Box.Rect.Height);
            _preMaximisedPos = Position;

            Position = Vector2.Zero;

            if (Parent != null)
            {
                Style.Width = Parent.Box.Rect.Width;
                Style.Height = Parent.Box.Rect.Height;
            }

            IsMaximised = true;
        }
        else
        {
            // Restore
            Style.Width = _preMaximisedSize.x;
            Style.Height = _preMaximisedSize.y;
            Position = _preMaximisedPos;
            IsMaximised = false;
        }
    }

    /// <summary>
    /// Close the window
    /// </summary>
    public void Close()
    {
        OnClose();
        OnCloseAction?.Invoke();
        Delete();
    }

    /// <summary>
    /// Action to invoke when window is closed
    /// </summary>
    public Action? OnCloseAction { get; set; }

    /// <summary>
    /// Called when the window is closed. Override to handle close events.
    /// </summary>
    public virtual void OnClose()
    {
        // Override this in derived classes
    }

    /// <summary>
    /// Focus this window and bring it to front
    /// </summary>
    public void FocusWindow()
    {
        // Note: AcceptsFocus and Focus() are not implemented in base Panel yet
        // TODO: Implement focus system
        
        // Move to front by changing child index
        if (Parent != null)
        {
            Parent.SetChildIndex(this, 0);
        }
    }

    /// <summary>
    /// Start dragging the window
    /// </summary>
    public void StartDrag(Vector2 mousePos)
    {
        if (!IsDraggable) return;

        _dragOffsetX = mousePos.x - Box.Rect.Left;
        _dragOffsetY = mousePos.y - Box.Rect.Top;
        _dragging = true;
    }

    /// <summary>
    /// Stop dragging the window
    /// </summary>
    public void StopDrag()
    {
        _dragging = false;
    }

    /// <summary>
    /// Update drag position
    /// </summary>
    public void UpdateDrag(Vector2 mousePos)
    {
        if (!_dragging) return;

        Position = new Vector2(
            mousePos.x - _dragOffsetX,
            mousePos.y - _dragOffsetY
        );
    }

    public override void Tick()
    {
        base.Tick();

        // Only apply position/size override if explicitly set (non-zero)
        // This allows floating window behavior when needed
        if (Position != Vector2.Zero || Size != Vector2.Zero)
        {
            Style.Position = PositionMode.Absolute;
            
            if (Position != Vector2.Zero)
            {
                Style.Left = Position.x;
                Style.Top = Position.y;
            }
            
            if (Size != Vector2.Zero)
            {
                Style.Width = Size.x;
                Style.Height = Size.y;
            }
        }

        // Update classes
        SetClass("minimised", IsMinimised);
        SetClass("maximised", IsMaximised);
        SetClass("unfocused", !HasFocus);
    }

    public override void SetProperty(string name, string value)
    {
        switch (name)
        {
            case "title":
                Title = value;
                return;

            case "windowwidth":
                if (int.TryParse(value, out int ww))
                    WindowWidth = ww;
                return;

            case "windowheight":
                if (int.TryParse(value, out int wh))
                    WindowHeight = wh;
                return;

            case "hastitlebar":
                HasTitleBar = value == "true" || value == "1";
                SetClass("notitlebar", !HasTitleBar);
                
                // Dynamically create or remove title bar
                if (HasTitleBar && TitleBar == null)
                {
                    CreateTitleBar();
                }
                else if (!HasTitleBar && TitleBar != null)
                {
                    TitleBar.Delete();
                    TitleBar = null;
                }
                return;

            case "hascustomchrome":
                HasCustomChrome = value == "true" || value == "1";
                SetClass("customchrome", HasCustomChrome);
                
                // Dynamically create or remove title bar
                if (HasCustomChrome && TitleBar == null)
                {
                    CreateTitleBar();
                }
                else if (!HasCustomChrome && TitleBar != null)
                {
                    TitleBar.Delete();
                    TitleBar = null;
                }
                return;

            case "hasminimise":
                HasMinimise = value == "true" || value == "1";
                return;

            case "hasmaximise":
                HasMaximise = value == "true" || value == "1";
                return;

            case "hasclose":
                HasClose = value == "true" || value == "1";
                return;

            case "isresizable":
                IsResizable = value == "true" || value == "1";
                return;

            case "isdraggable":
                IsDraggable = value == "true" || value == "1";
                return;

            case "autofocus":
                AutoFocus = value == "true" || value == "1";
                return;

            case "x":
                if (float.TryParse(value, out float x))
                    Position = new Vector2(x, Position.y);
                return;

            case "y":
                if (float.TryParse(value, out float y))
                    Position = new Vector2(Position.x, y);
                return;

            case "minwidth":
                if (float.TryParse(value, out float minWidth))
                    MinSize = new Vector2(minWidth, MinSize.y);
                return;

            case "minheight":
                if (float.TryParse(value, out float minHeight))
                    MinSize = new Vector2(MinSize.x, minHeight);
                return;
        }

        base.SetProperty(name, value);
    }
}
