# Avalazor - Desktop Blazor/Razor Framework with XGUI Themes

**Avalazor** (Avalonia + Razor) is a desktop implementation of a Blazor/Razor-like system for building cross-platform desktop applications (Windows, macOS, Linux) using only C# and Razor syntax with SCSS styling support - **no AXAML required**!

**Now featuring XGUI-3 themes** - Use authentic UI themes that mimic Windows 95, XP, 7, 11, Half-Life 2, Garry's Mod, and more!

## 🎯 Key Features

- **Razor-Only Development**: Write your entire UI in Razor - no AXAML, no XAML, just Razor components
- **Full IntelliSense Support**: Complete Razor IntelliSense in Visual Studio (component autocomplete, parameters, directives)
- **Automatic Transpilation**: Razor files are automatically transpiled to C# at build time  
- **SCSS Styling**: Full SCSS support with variables, nesting, and mixins
- **XGUI Theme System**: Complete themes ported from XGUI-3 - use them as-is or create your own
- **Cross-Platform**: Runs on Windows, macOS, and Linux
- **Zero Avalonia Boilerplate**: All Avalonia setup is hidden - you only work with Razor

## 🚀 Quick Start

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Xenthio/UITest.git
cd UITest
```

2. Build the solution:
```bash
dotnet build Avalazor.sln
```

3. Run the example:
```bash
cd examples/SimpleDesktopApp
dotnet run
```

### Your First Avalazor App

1. Create a new console project:
```bash
dotnet new console -n MyAvalazorApp
cd MyAvalazorApp
```

2. Add Avalazor references:
```xml
<ItemGroup>
  <ProjectReference Include="path/to/Avalazor.Core/Avalazor.Core.csproj" />
  <ProjectReference Include="path/to/Avalazor.Runtime/Avalazor.Runtime.csproj" />
  <ProjectReference Include="path/to/Avalazor.Build/Avalazor.Build.csproj" />
</ItemGroup>

<Import Project="path/to/Avalazor.Build/build/Avalazor.Build.targets" />
```

3. Create `MainApp.razor`:
```razor
@using Avalazor.UI
@inherits UIComponent
@attribute [StyleSheet("/themes/Avalazor.Defaults.scss")]

<div class="app">
    <h1>Hello Avalazor!</h1>
    <button @onclick="HandleClick">Clicks: @count</button>
</div>

@code {
    private int count = 0;

    private void HandleClick()
    {
        count++;
    }
}
```

> **Note:** The `Avalazor.Defaults.scss` import provides s&box-compatible flexbox-by-default behavior, making XGUI themes work correctly.

4. Update `Program.cs`:
```csharp
using Avalazor.Runtime;

AvalazorApplication.Run<MainApp>(args);
```

5. Build and run - that's it! No AXAML files needed!

## 📁 Project Structure

```
MyAvalazorApp/
├── Program.cs           # Just calls AvalazorApplication.Run<MainApp>()
├── MainApp.razor        # Your root component
├── MainApp.scss         # Styling for your app
└── Components/
    ├── Button.razor     # Reusable components
    └── Button.scss
```

**What you DON'T need:**
- ❌ No App.axaml
- ❌ No App.axaml.cs
- ❌ No MainWindow.axaml  
- ❌ No MainWindow.axaml.cs
- ❌ No Avalonia boilerplate

## 🎨 Styling with SCSS

Avalazor automatically compiles SCSS to CSS at build time:

**MyComponent.scss:**
```scss
$primary-color: #007acc;

.my-component {
    background-color: $primary-color;
    padding: 20px;

    button {
        color: white;
        border: none;

        &:hover {
            opacity: 0.8;
        }
    }
}
```

Associate the stylesheet with your component:
```razor
@attribute [StyleSheet("MyComponent.scss")]
```

## 🎭 XGUI Themes

Avalazor includes a complete port of **XGUI-3 themes** that accurately mimic various UI styles. These themes work as-is - no modifications needed!

### Available Themes

**Classic Windows:**
- `Computer95.scss` - Windows 95 classic look
- `ComputerXP.scss` - Windows XP Luna theme
- `Computer7.scss` - Windows 7 Aero-inspired
- `Computer11.scss` - Windows 11 modern design

**Gaming UI:**
- `OliveGreen.scss` - Half-Life 2 / Valve style
- `Derma.scss` - Garry's Mod default UI
- `SboxDark.scss` - s&box dark theme
- `Vapour.scss` - Steam-inspired interface

**Minimal:**
- `Simple.scss` - Clean, minimal design
- `ThinGrey.scss` - Lightweight grey theme
- `IMGUI.scss` - Dear ImGui style

### Using XGUI Themes

**Important:** XGUI themes require flexbox-by-default behavior. Always import `Avalazor.Defaults.scss` first:

```razor
@attribute [StyleSheet("/themes/Avalazor.Defaults.scss")]
@attribute [StyleSheet("/themes/XGUI/DefaultStyles/OliveGreen.scss")]
```

Or in your SCSS file:

```scss
@import "/themes/Avalazor.Defaults.scss";
@import "/themes/XGUI/DefaultStyles/OliveGreen.scss";
```

You can also add custom overrides:

```razor
@attribute [StyleSheet("/themes/Avalazor.Defaults.scss")]
@attribute [StyleSheet("/themes/XGUI/DefaultStyles/ComputerXP.scss")]
@attribute [StyleSheet("MyCustomOverrides.scss")]
```

### Window Decorations

XGUI themes include full window decoration support with titlebar, control buttons, and window chrome:

**Window Structure Expected:**
```razor
<div class="Window">
    <div class="TitleBar">
        <div class="TitleElements">
            <div class="TitleIcon"></div>
            <div class="TitleLabel">My Window</div>
            <div class="TitleSpacer"></div>
            <div class="Control MinimiseButton">_</div>
            <div class="Control MaximiseButton">□</div>
            <div class="Control CloseButton">×</div>
        </div>
        <div class="TitleBackground"></div>
    </div>
    <div class="window-content">
        <!-- Your content here -->
    </div>
</div>
```

The themes handle:
- **Title bar styling** with proper colors for active/inactive states
- **Control buttons** (minimize, maximize, close) with hover effects
- **Window borders** matching the theme style
- **Resizer handles** where applicable
- **Focus states** (unfocused windows have dimmed titlebars)

### Creating XGUI-Compatible Themes

XGUI themes use a modular structure with FunctionStyles (base component styles) and DefaultStyles (complete themes):

```scss
// MyTheme.scss
$base-colour: #your-color;
$default-text-colour: #your-text;

@import "/themes/XGUI/FunctionStyles/FunctionStyles.scss";
@import "/themes/XGUI/DefaultStyles/BaseStyles/VGUI.scss";
```

See `themes/XGUI/README.md` for detailed theme documentation.

## 🏗️ Architecture

### Core Libraries

1. **Avalazor.Razor** - Razor file transpilation engine
   - Converts `.razor` files to C# code
   - **Embeds Microsoft.AspNetCore.Razor.Language from s&box for full IntelliSense**
   - Provides component autocomplete, parameter validation, and directive support

2. **Avalazor.Scss** - SCSS compilation engine  
   - Compiles `.scss` files to CSS
   - Supports all SCSS features

3. **Avalazor.Core** - Core UI framework
   - Base component classes
   - StyleSheet attribute system

4. **Avalazor.Runtime** - Avalonia integration (hidden from user)
   - Handles all Avalonia bootstrapping internally
   - Provides simple `AvalazorApplication.Run<T>()` API

5. **Avalazor.Build** - MSBuild integration
   - Automatic Razor transpilation
   - Automatic SCSS compilation

## 🔧 Build Process

When you build your project:

1. **Razor Transpilation**: All `.razor` files are transpiled to `.razor.g.cs` files
2. **SCSS Compilation**: All `.scss` files are compiled to `.css` files
3. **Compilation**: Generated C# files are compiled with your project
4. **Output**: A single executable with all your UI code

Example build output:
```
Avalazor: Transpiling 3 Razor file(s)...
Successfully transpiled 3 Razor file(s)
Avalazor: Compiling 3 SCSS file(s)...
Successfully compiled 3 SCSS file(s)
```

## 📚 Component Examples

### Interactive Component
```razor
@using Avalazor.UI
@inherits UIComponent

<div class="counter">
    <h2>@Title</h2>
    <button @onclick="Increment">Count: @count</button>
</div>

@code {
    [Parameter]
    public string Title { get; set; } = "Counter";

    private int count = 0;

    private void Increment() => count++;
}
```

### Component with Children
```razor
@using Avalazor.UI
@inherits UIComponent

<div class="panel">
    <header>@Header</header>
    <div class="content">
        @ChildContent
    </div>
</div>

@code {
    [Parameter]
    public string Header { get; set; } = "";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

## 💡 IntelliSense & IDE Support

Avalazor provides **full IntelliSense support** for Razor files in Visual Studio and compatible IDEs by embedding s&box's `Microsoft.AspNetCore.Razor.Language` implementation.

**What Works:**
- ✅ **Component Autocomplete** - IntelliSense for custom Razor components
- ✅ **Parameter IntelliSense** - Autocomplete and validation for component parameters
- ✅ **Directive Support** - Full support for `@using`, `@namespace`, `@inherits`, etc.
- ✅ **C# IntelliSense in @code blocks** - Full C# language support
- ✅ **Syntax Validation** - Real-time error checking in Razor files
- ✅ **Go to Definition** - Navigate to component definitions

**No Configuration Required:**
Simply reference `Avalazor.Razor` and IntelliSense works automatically. No need for special project SDKs or configuration files.

**Credit:**
This IntelliSense implementation is ported from [s&box](https://github.com/Facepunch/sbox-public) (MIT licensed), which discovered that embedding the Razor Language Server components was necessary for full IDE support.

## 🆚 Comparison with Traditional Avalonia

| Feature | Traditional Avalonia | Avalazor |
|---------|---------------------|----------|
| UI Markup | AXAML | Razor |
| Code-Behind | .axaml.cs files | Inline @code blocks |
| Styling | XAML styles | SCSS |
| Boilerplate | Lots (App.axaml, etc.) | None |
| Learning Curve | XAML + C# | Just C# + Razor |

## 🎯 Why Avalazor?

**If you're familiar with Blazor/Razor:**
- Use the same component model you know
- No need to learn XAML
- Familiar `@code` blocks and `@onclick` syntax

**If you're coming from s&box:**
- Same Razor transpilation system
- Same SCSS workflow
- Familiar component structure
- **XGUI themes work directly - port your UI easily!**

**If you want simplicity:**
- One language (C#) for everything
- No AXAML ceremony
- Just write Razor and go!

## 📖 Documentation

### Program Entry Point

The simplest possible Avalazor app:
```csharp
using Avalazor.Runtime;

AvalazorApplication.Run<MainApp>(args);
```

That's it! Avalonia is completely hidden.

### Component Lifecycle

Components inherit from `UIComponent`:
```csharp
protected override void OnInitialized()
{
    // Component initialized
}

protected override int BuildHash()
{
    // Return hash for change detection
    return HashCode.Combine(myState);
}
```

## 🤝 Contributing

Contributions are welcome! This project is open source and licensed under MIT.

## 📜 License

MIT License - See LICENSE file

Based on the s&box Razor system by Facepunch Studios (MIT licensed):
- https://github.com/Facepunch/sbox-public

## 🙏 Credits

- **s&box** by Facepunch Studios - Original Razor transpilation system
- **XGUI-3** by Xenthio - Theme system and reference implementation
- **Avalonia** - Cross-platform UI framework
- **Microsoft** - AspNetCore.Razor.Language and Components

### XGUI Themes

The themes in `themes/XGUI/` are ported from [XGUI-3](https://github.com/Xenthio/XGUI-3) (MIT licensed). These themes accurately recreate various UI styles including Windows 95, XP, 7, 11, Half-Life 2, Garry's Mod, and more.

## 🔗 References

- [s&box Public Repository](https://github.com/Facepunch/sbox-public)
- [XGUI-3](https://github.com/Xenthio/XGUI-3)
- [Avalonia UI](https://avaloniaui.net/)
