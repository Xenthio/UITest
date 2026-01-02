# Fazor - Desktop Blazor/Razor Framework

**Fazor** is a desktop implementation of a Blazor/Razor-like system for building cross-platform desktop applications (Windows, macOS, Linux) using only C# and Razor syntax with SCSS styling support.

## 🎯 Key Features

- **Razor-Only Development**: Write your entire UI in Razor - no AXAML, no XAML, just Razor components
- **Full IntelliSense Support**: Complete Razor IntelliSense in Visual Studio (component autocomplete, parameters, directives)
- **Automatic Transpilation**: Razor files are automatically transpiled to C# at build time  
- **SCSS Styling**: Full SCSS support with variables, nesting, and mixins
- **XGUI Theme System**: Complete controls and themes ported from XGUI-3 - use them as-is or create your own
- **Cross-Platform**: Runs on Windows, macOS, and Linux
- **Zero Boilerplate**: Minimal setup required - you can only work with Razor

## 🚀 Quick Start

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Xenthio/UITest.git
cd UITest
```

2. Initialize submodules:
```bash
git submodule update --init --recursive
```

3. Build the solution:
```bash
dotnet build Fazor.sln
```

4. Run the example:
```bash
cd examples/SimpleDesktopApp
dotnet run
```

### Troubleshooting Submodule Issues

If you encounter errors like `fatal: not a git repository: thirdparty/RichTextKit/../../.git/modules/thirdparty/RichTextKit`, this means your git submodule cache is corrupted. To fix this:

**On Windows (PowerShell or Git Bash):**
```bash
# Remove the corrupted git module cache
rm -rf .git/modules/thirdparty
# Remove the submodule directory
rm -rf thirdparty/RichTextKit
# Re-initialize the submodule
git submodule update --init --recursive
```

**On Linux/macOS:**
```bash
# Remove the corrupted git module cache
rm -rf .git/modules/thirdparty
# Remove the submodule directory
rm -rf thirdparty/RichTextKit
# Re-initialize the submodule
git submodule update --init --recursive
```

If issues persist, you may need to do a fresh clone:
```bash
cd ..
rm -rf UITest
git clone --recurse-submodules https://github.com/Xenthio/UITest.git
cd UITest
```

### Your First Fazor App

1. Create a new console project:
```bash
dotnet new console -n MyFazorApp
cd MyFazorApp
```

2. Add Fazor references:
```xml
<ItemGroup>
  <ProjectReference Include="path/to/Fazor.Core/Fazor.Core.csproj" />
  <ProjectReference Include="path/to/Fazor.Runtime/Fazor.Runtime.csproj" />
  <ProjectReference Include="path/to/Fazor.Build/Fazor.Build.csproj" />
</ItemGroup>

<Import Project="path/to/Fazor.Build/build/Fazor.Build.targets" />
```

3. Create `MainApp.razor`:
```razor
@using Fazor.UI
@inherits UIComponent
@attribute [StyleSheet("/themes/Fazor.Defaults.scss")]

<div class="app">
    <h1>Hello Fazor!</h1>
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

> **Note:** The `Fazor.Defaults.scss` import provides s&box-compatible flexbox-by-default behavior, making XGUI themes work correctly.

4. Update `Program.cs`:
```csharp
using Fazor.Runtime;

FazorApplication.Run<MainApp>(args);
```

5. Build and run - that's it! No AXAML files needed!

## 📁 Project Structure

```
MyFazorApp/
├── Program.cs           # Just calls FazorApplication.Run<MainApp>()
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
- ❌ No framework boilerplate

## 🎨 Styling with SCSS

Fazor automatically compiles SCSS to CSS at build time:

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

Fazor includes a complete port of **XGUI-3 themes** that accurately mimic various UI styles. These themes work as-is - no modifications needed!

### Available Themes

**Classic Windows:**
- `Computer95.scss` - Windows 95 classic look
- `ComputerXP.scss` - Windows XP Luna theme
- `Computer7.scss` - Windows 7 Aero-inspired
- `Computer11.scss` - Windows 11 modern design

**Gaming UI:**
- `OliveGreen.scss` - Half-Life 1 / Valve style
- `Derma.scss` - Garry's Mod default UI
- `SboxDark.scss` - s&box dark theme
- `Vapour.scss` - Steam-inspired interface
- `ThinGrey.scss` - Half-Life 2 theme

**Minimal:**
- `Simple.scss` - Nothing but layout and a white border on everything only.
- `IMGUI.scss` - Dear ImGui style

### Using XGUI Themes

**Important:** XGUI themes require flexbox-by-default behavior. Always import `Fazor.Defaults.scss` first:

```razor
@attribute [StyleSheet("/themes/Fazor.Defaults.scss")]
@attribute [StyleSheet("/themes/XGUI/DefaultStyles/OliveGreen.scss")]
```

Or in your SCSS file:

```scss
@import "/themes/Fazor.Defaults.scss";
@import "/themes/XGUI/DefaultStyles/OliveGreen.scss";
```

You can also add custom overrides:

```razor
@attribute [StyleSheet("/themes/Fazor.Defaults.scss")]
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

1. **Fazor.Razor** - Razor file transpilation engine
   - Converts `.razor` files to C# code
   - **Embeds Microsoft.AspNetCore.Razor.Language from s&box for full IntelliSense**
   - Provides component autocomplete, parameter validation, and directive support

2. **Fazor.Scss** - SCSS compilation engine  
   - Compiles `.scss` files to CSS
   - Supports all SCSS features

3. **Fazor.Core** - Core UI framework
   - Base component classes
   - StyleSheet attribute system

4. **Fazor.Runtime** - Runtime infrastructure (hidden from user)
   - Handles all framework bootstrapping internally
   - Provides simple `FazorApplication.Run<T>()` API

5. **Fazor.Build** - MSBuild integration
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
Fazor: Transpiling 3 Razor file(s)...
Successfully transpiled 3 Razor file(s)
Fazor: Compiling 3 SCSS file(s)...
Successfully compiled 3 SCSS file(s)
```

## 📚 Component Examples

### Interactive Component
```razor
@using Fazor.UI
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
@using Fazor.UI
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

Fazor provides **full IntelliSense support** for Razor files in Visual Studio and compatible IDEs by embedding s&box's `Microsoft.AspNetCore.Razor.Language` implementation.

**What Works:**
- ✅ **Component Autocomplete** - IntelliSense for custom Razor components
- ✅ **Parameter IntelliSense** - Autocomplete and validation for component parameters
- ✅ **Directive Support** - Full support for `@using`, `@namespace`, `@inherits`, etc.
- ✅ **C# IntelliSense in @code blocks** - Full C# language support
- ✅ **Syntax Validation** - Real-time error checking in Razor files
- ✅ **Go to Definition** - Navigate to component definitions

**No Configuration Required:**
Simply reference `Fazor.Razor` and IntelliSense works automatically. No need for special project SDKs or configuration files.

**Credit:**
This IntelliSense implementation is ported from [s&box](https://github.com/Facepunch/sbox-public) (MIT licensed), which discovered that embedding the Razor Language Server components was necessary for full IDE support.

## 🆚 Comparison with Traditional Desktop UI Frameworks

| Feature | Traditional XAML-based | Fazor |
|---------|---------------------|----------|
| UI Markup | AXAML/XAML | Razor |
| Code-Behind | .axaml.cs/.xaml.cs files | Inline @code blocks |
| Styling | XAML styles | SCSS |
| Boilerplate | Lots (App.axaml, etc.) | None |
| Learning Curve | XAML + C# | Just C# + Razor |

## 🎯 Why Fazor?

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

The simplest possible Fazor app:
```csharp
using Fazor.Runtime;

FazorApplication.Run<MainApp>(args);
```

That's it! All framework complexity is completely hidden.

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
- **Microsoft** - AspNetCore.Razor.Language and Components

### XGUI Themes

The themes in `themes/XGUI/` are ported from [XGUI-3](https://github.com/Xenthio/XGUI-3) (MIT licensed). These themes accurately recreate various UI styles including Windows 95, XP, 7, 11, Half-Life 2, Garry's Mod, and more.

## 🔗 References

- [s&box Public Repository](https://github.com/Facepunch/sbox-public)
- [XGUI-3](https://github.com/Xenthio/XGUI-3)
