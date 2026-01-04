# Popup System Implementation Summary

## Overview
This implementation adds a proper popup system for dropdown panels and other popup UI elements, ported from S&box's UI framework. Popups now use intelligent positioning and automatic cleanup instead of manual positioning logic.

## What Was Implemented

### 1. Core Popup Infrastructure

#### BasePopup (src/Sandbox.UI/Controls/BasePopup.cs)
- Base class for all popup panels
- Tracks all active popups globally in a static list
- Provides `CloseAll()` method to close all popups at once
- Optional `StayOpen` flag to prevent auto-closing
- Automatic cleanup on deletion

#### Popup (src/Sandbox.UI/Controls/Popup.cs)
- Full-featured popup panel with positioning system
- 8 positioning modes:
  - `BelowStretch`: Below source, stretched to match width (default for ComboBox)
  - `BelowLeft`: Below source, left-aligned
  - `BelowCenter`: Below source, centered
  - `AboveLeft`: Above source, left-aligned
  - `AboveCenter`: Above source, centered
  - `Left`: Left of source, vertically centered
  - `LeftBottom`: Left of source, bottom-aligned
  - `UnderMouse`: At current mouse position
- Automatic repositioning on tick
- Optional header with title and icon
- Helper methods for adding option buttons
- Smart boundary detection (keeps popups within window bounds)

### 2. ComboBox Integration

Updated `src/Sandbox.UI/Controls/ComboBox.cs`:
- Now creates `Popup` instance instead of plain `Panel`
- Uses `BelowStretch` positioning mode
- Inherits all popup benefits (auto-positioning, click-outside-to-close)
- Simplified code - removed manual positioning logic

### 3. Auto-Close on Click Outside

Updated `src/Sandbox.UI/Panel/RootPanel.cs`:
- Detects mouse clicks outside of popup panels
- Automatically closes all popups when clicking elsewhere
- Preserves the clicked panel if it's inside a popup
- Uses LINQ to check panel ancestry

### 4. Future: OS-Level Popup Windows

Created infrastructure (not yet integrated):
- `src/Avalazor.UI/PopupWindow.cs`: Window class for popup windows
- `src/Avalazor.UI/PopupWindowManager.cs`: Manager for multiple popup windows
- Provides foundation for true OS-level popup windows

## Usage Examples

### Using Popup Directly
```csharp
// Create a popup below a button
var popup = new Popup(sourceButton, Popup.PositionMode.BelowLeft, 5.0f);
popup.AddOption("Option 1", () => Console.WriteLine("Selected 1"));
popup.AddOption("Option 2", () => Console.WriteLine("Selected 2"));
popup.AddOption("Option 3", () => Console.WriteLine("Selected 3"));
```

### ComboBox (Automatic)
```razor
<combobox default="Option 1">
    <option value="1">Option 1</option>
    <option value="2">Option 2</option>
    <option value="3">Option 3</option>
</combobox>
```
The ComboBox automatically creates and manages a Popup when opened.

### Custom Popup with Header
```csharp
var popup = new Popup();
popup.Title = "Select Action";
popup.Icon = "🎯";
popup.SetPositioning(sourcePanel, Popup.PositionMode.UnderMouse, 0);
popup.AddOption("Delete", "🗑", () => DeleteItem());
popup.AddOption("Edit", "✏", () => EditItem());
popup.AddOption("Copy", "📋", () => CopyItem());
```

## Testing

### Manual Testing
1. Run `examples/SimpleDesktopApp`
2. Open the "About XGUI" window
3. Test ComboBox dropdowns:
   - Click to open - dropdown appears below
   - Click outside - dropdown closes
   - Click an option - selects and closes
   - Open multiple dropdowns - each works independently

### Test File
See `examples/SimpleDesktopApp/PopupTest.razor` for a dedicated test window.

## Technical Details

### Positioning Algorithm
1. Get source panel's bounding rect
2. Apply scaling factor (DPI-aware)
3. Calculate position based on positioning mode
4. Clamp to window boundaries with padding
5. Update styles (Left, Top, Width, etc.)
6. Mark style as dirty to trigger re-layout

### Popup Lifecycle
1. **Creation**: Popup is created and adds itself to global list
2. **Positioning**: `SetPositioning()` sets parent to root, applies CSS classes
3. **Updates**: Each tick calls `PositionMe()` to update position
4. **Closing**: User clicks outside, triggering `BasePopup.CloseAll()`
5. **Cleanup**: `OnDeleted()` removes from global list

### Click-Outside-to-Close Logic
```csharp
// In RootPanel.ProcessButtonEvent()
if (pressed && button == "mouseleft")
{
    var target = Input.Hovered;
    // Close all popups if clicking outside them
    if (target != null && !target.AncestorsAndSelf.OfType<BasePopup>().Any())
    {
        BasePopup.CloseAll(target);
    }
}
```

## Current Limitations

### Single-Window Rendering
Popups render in the same window as the rest of the UI. They **cannot extend beyond the window boundaries** due to clipping by the render context. This is a fundamental limitation of single-window rendering.

### No True OS-Level Popups
Creating true popup windows that can extend beyond the main window would require:

1. **Separate Windows**: Each popup needs its own `IWindow` instance
2. **Event Loop Coordination**: Either:
   - Multiple threads (one per window)
   - Single event loop pumping messages for all windows
   - Platform-specific window message handling
3. **Graphics Context Management**:
   - D3D11: Separate device contexts or shared resources
   - OpenGL: Context sharing (platform-dependent)
   - Vulkan: Complex swapchain management
4. **Window Relationships**:
   - Parent-child window hierarchy
   - Z-order management (topmost, always-on-top)
   - Focus management
   - Position synchronization when parent moves
5. **Platform Differences**:
   - Windows: HWND parent-child relationships
   - Linux X11: Transient-for hints
   - Linux Wayland: XDG popup protocol
   - macOS: NSPanel or NSPopover

The `PopupWindow` and `PopupWindowManager` classes provide a starting point for this work, but full integration is a significant undertaking.

## Compatibility with S&box

This implementation is a direct port from S&box's UI system:
- `BasePopup`: From `engine/Sandbox.Engine/Systems/UI/Controls/BasePopup.cs`
- `Popup`: From `game/addons/base/code/UI/Popup.cs`
- Same positioning modes, same behavior, same API

This ensures that:
- UI code can be more easily ported between Fazor and S&box
- Behavior is proven and tested
- Future S&box updates can be integrated

## Future Enhancements

### Short Term
- [ ] Keyboard navigation for popup options (arrow keys, enter, escape)
- [ ] Popup animations (fade in/out, slide)
- [ ] Max height with scrolling for tall popups
- [ ] Context menu support (right-click popup)

### Long Term
- [ ] True OS-level popup windows (significant effort)
- [ ] Multiple monitor support with popup repositioning
- [ ] Popup shadows/drop shadows
- [ ] Nested popup menus (submenus)

## Conclusion

This implementation provides a robust, S&box-compatible popup system suitable for dropdowns, context menus, and other popup UI elements. While true OS-level popup windows remain a future goal, the current system provides excellent functionality within the constraints of single-window rendering.
