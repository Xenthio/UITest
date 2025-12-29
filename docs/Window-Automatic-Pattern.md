# Window Control - Automatic Razor Integration

## Overview

The Window control now supports automatic integration with Razor components through base class inheritance and declarative attribute binding. This allows for a cleaner, more intuitive developer experience when creating custom windows.

## Automatic Window Pattern

### Basic Usage

Create a Razor component that inherits from `Window`:

```razor
@namespace MyApp
@using Sandbox.UI
@attribute [StyleSheet("/themes/XGUI/DefaultStyles/OliveGreen.scss")]
@inherits Window

<root title="My Window" hasminimise="true" hasmaximise="true" hasclose="true">
    <h2>Window Content</h2>
    <p>Your content goes here...</p>
    <button>Click Me</button>
</root>

@code {
    protected override void OnAfterTreeRender(bool firstTime)
    {
        base.OnAfterTreeRender(firstTime);
        
        if (firstTime)
        {
            Console.WriteLine($"Window '{Title}' initialized!");
        }
    }
}
```

### Root Element Attributes

The `<root>` element supports the following attributes for automatic window configuration:

#### Window Appearance
- `title` - Window title text (string)
- `hastitlebar` - Show/hide custom title bar (boolean: "true"/"false")
- `hascontrols` - Show/hide title bar control buttons (boolean)

#### Title Bar Controls
- `hasminimise` - Show minimize button (boolean)
- `hasmaximise` - Show maximize button (boolean)
- `hasclose` - Show close button (boolean)

#### Size and Position
- `width` - Window width in pixels (number)
- `height` - Window height in pixels (number)
- `x` - X position (number)
- `y` - Y position (number)

#### Behavior
- `isdraggable` - Allow window dragging (boolean, default: true)
- `isresizable` - Allow window resizing (boolean, default: true)

### How It Works

1. **Inheritance**: Component inherits from `Window` class
2. **Attribute Processing**: On first render, `ProcessRootElementAttributes()` reads attributes from the root element
3. **Property Binding**: Attributes automatically map to Window properties
4. **Auto-wrapping**: All content inside `<root>` is automatically wrapped in a window-content panel
5. **Title Bar Creation**: If `hastitlebar` is true (default), a custom title bar with controls is automatically created

## Complete Example

See `examples/SimpleDesktopApp/ExampleWindow.razor` for a complete working example.

