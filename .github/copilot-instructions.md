# Fazor AI Instructions

You are working with **Fazor** (formerly Avalazor), a desktop UI framework that implements a Blazor/Razor-like system for building cross-platform desktop applications using **only C# and Razor syntax** with SCSS styling. It is heavily inspired by and compatible with the S&box UI system.

## 🏗 Architecture Overview

- **Core Philosophy**: "Razor-Only Development". No XAML, no AXAML. UI is defined in `.razor` files which are transpiled to C# classes inheriting from `Sandbox.UI.Panel`.
- **Rendering**: Uses **SkiaSharp** for rendering via `Avalazor.UI` and `Sandbox.UI.Skia`.
- **Layout**: Uses Facebook's **Yoga** layout engine (Flexbox) via `Avalazor.UI/Native`.
- **Windowing**: Uses **Silk.NET** for window management and input handling.
- **Styling**: Full SCSS support. Styles are parsed and applied to Panels.
- **Component System**: Components are `Panel`s. The root of the application is a `RootPanel`.
- **S&box mirroring**: The UI system is designed to be as close to S&box's UI system as possible for compatibility and ease of porting. Code should be ported from S&box wherever possible.

### Key Projects
- `Avalazor.UI`: The main runtime library. Handles window creation, input, and the bridge between native windowing and the UI system.
- `Sandbox.UI`: The core UI framework (Panel system, Styling, Events). Ported/adapted from S&box.
- `Avalazor.Build`: MSBuild tasks for transpiling Razor to C# and handling SCSS.
- `SimpleDesktopApp`: Example application demonstrating usage.

## 🧩 Component Structure (`Panel`)

Every UI component is a `Panel` (in `Sandbox.UI`).
- **Lifecycle**: `Tick()`, `OnParentChanged()`, `OnDeleted()`.
- **Styling**: `Style` property (direct access), `ComputedStyle` (final calculated style), `StyleSheet` (attached stylesheets).
- **Layout**: `Box` (computed layout rects), `YogaNode` (flexbox layout node).
- **Input**: `OnClick`, `OnMouseOver`, `OnMouseMove`, etc.
- **Razor Integration**: `BuildRenderTree`, `StateHasChanged`.

## 🛠 Developer Workflows

### Creating a New Component
1. Create a `.razor` file (e.g., `MyComponent.razor`).
2. Inherit from `Panel` (default) or a specific subclass.
3. Use standard Razor syntax (`@if`, `@foreach`, `@code`).
4. Use SCSS for styling (either `<style>` block or separate `.scss` file).

**Example `MyComponent.razor`:**
```razor
@using Sandbox.UI
@inherits Panel

<root class="my-component">
    <div class="header">@Title</div>
    <div class="content" onclick=@OnContentClick>
        Click me! Count: @Count
    </div>
</root>

@code {
    public string Title { get; set; } = "Hello";
    public int Count { get; set; }

    void OnContentClick() {
        Count++;
        StateHasChanged(); // Re-render
    }
}
```

**Example `MyWindow.razor`:**
```razor
@using Sandbox.UI
@inherits Window

<root title="My Window" hasminimise="true" hasmaximise="true" hasclose="true" windowwidth="400" windowheight="300">
    <div class="window-content">
        <div class="header">@Title</div>
        <div class="content" onclick=@OnContentClick>
            Click me! Count: @Count
        </div>
    </div>
</root>

@code {
    public string Title { get; set; } = "Hello";
    public int Count { get; set; }

    void OnContentClick() {
        Count++;
        StateHasChanged(); // Re-render
    }
}
```

### Styling
- Use **SCSS**.
- Styles are scoped to the component if defined in `<style>`.
- Use `LoadStyleSheet("path/to/style.scss")` in the constructor or `@attribute [StyleSheet]`.
- **Flexbox** is the layout model.

### Running the App
- The entry point uses `AvalazorApplication.RunPanel<MainApp>()`.
- **Do not** use `RunComponent<T>`, it is deprecated.
- Ensure `Avalazor.Build` is built before the app project (handled by project references).

## ⚠️ Important Conventions & Gotchas

- **Namespace**: Most UI core classes are in `Sandbox.UI` (compatibility with S&box).
- **Transpilation**: Razor files are transpiled to `.razor.g.cs` in `obj/` or `Avalazor/Generated/`. If you see errors about missing classes, ensure the build has run to generate these.
- **Input Handling**: Input is event-based (`OnClick`, etc.). `RootPanel` dispatches input to children.
- **Threading**: UI operations should generally happen on the main thread.
- **Native Dependencies**: The project requires the native Yoga library. The build script should handle this, but be aware of `Avalazor.UI/Native`.
- **S&box Compatibility**: Many APIs mirror S&box's UI system. If you know S&box UI, you know Fazor.
- **Porting Strategy**: When implementing features or fixing bugs, prefer porting code directly from S&box (https://github.com/Facepunch/sbox-public) (specifically `Sandbox.Engine/Systems/UI`) whenever possible. Maintain the same class names, method signatures, and behavior to ensure compatibility and ease of porting. Some controls may come from XGUI-3 (https://github.com/Xenthio/XGUI-3).
- **Fix considerations**: When fixing bugs and changing something from s&box, consider "Is it like that in S&box?" and check the S&box source, if it isn't, the problem you're trying to fix might lie elsewhere, possibly from something else that doesn't match s&box.

## 🔍 Debugging
- **Panel Inspector**: There is a `PanelInspector` (ported from XGUI) available for debugging UI layout and styles.
- **Console Output**: `Console.WriteLine` is your friend. The framework logs layout and render tree info.
- **Visual Studio**: IntelliSense for Razor should work if the project is set up correctly.

## 📦 External Dependencies
- **Silk.NET**: Windowing, Input, OpenGL.
- **SkiaSharp**: 2D Graphics.
- **Facebook Yoga**: Layout engine (native DLL required).
