# UITest - Desktop Blazor/Razor Framework

A desktop implementation of a Blazor/Razor-like system for building cross-platform desktop applications (Windows, macOS, Linux) using C# and Razor syntax with SCSS styling support.

## Overview

UITest brings the power of Razor components to desktop application development, inspired by and based on the [s&box](https://github.com/Facepunch/sbox-public) Razor system (MIT licensed). It provides:

- **Razor-to-C# Transpilation**: Write UI components in Razor syntax that get automatically transpiled to C# at build time
- **SCSS Compilation**: Style your components using SCSS with full support for variables, nesting, and mixins
- **Automatic Build Integration**: MSBuild tasks handle all transpilation automatically
- **Cross-Platform**: Works on Windows, macOS, and Linux

## Architecture

The framework consists of several components:

### Core Libraries

1. **UITest.Razor** - Razor file transpilation engine
   - Based on `Microsoft.AspNetCore.Razor.Language`
   - Converts `.razor` files to C# code
   - Supports folder-based namespacing

2. **UITest.Scss** - SCSS compilation engine
   - Uses `SharpScss` for SCSS-to-CSS compilation
   - Supports imports, variables, and nesting
   - Compiles `.scss` files to `.css` at build time

3. **UITest.Core** - Core UI framework
   - Base component classes
   - `StyleSheetAttribute` for automatic stylesheet loading
   - Component lifecycle management

4. **UITest.Build** - MSBuild integration
   - Automatic Razor transpilation during build
   - Automatic SCSS compilation during build
   - Incremental build support

## Getting Started

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Xenthio/UITest.git
cd UITest
```

2. Build the solution:
```bash
dotnet build
```

3. Run the example application:
```bash
cd examples/SimpleDesktopApp
dotnet run
```

### Creating a New Project

1. Create a new project and add references:
```xml
<ItemGroup>
  <ProjectReference Include="path/to/UITest.Core/UITest.Core.csproj" />
  <ProjectReference Include="path/to/UITest.Build/UITest.Build.csproj" />
</ItemGroup>
```

2. Create a Razor component (e.g., `MyComponent.razor`):
```razor
@using UITest.UI
@inherits UIComponent
@attribute [StyleSheet("MyComponent.scss")]

<div class="my-component">
    <h2>@Title</h2>
    <button @onclick="HandleClick">Click me!</button>
</div>

@code {
    [Parameter]
    public string Title { get; set; } = "Hello World";

    private void HandleClick()
    {
        Console.WriteLine("Button clicked!");
    }
}
```

3. Create a stylesheet (e.g., `MyComponent.scss`):
```scss
.my-component {
    padding: 20px;
    background-color: #f5f5f5;

    h2 {
        color: #007acc;
        margin-bottom: 10px;
    }

    button {
        background-color: #007acc;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 4px;

        &:hover {
            background-color: #005a9e;
        }
    }
}
```

4. Build your project - the Razor file will be automatically transpiled to C# and the SCSS will be compiled to CSS.

## How It Works

### Razor Transpilation

The transpilation process follows these steps:

1. **Discovery**: The build system finds all `.razor` files in your project
2. **Namespace Injection**: If enabled, namespaces are automatically added based on folder structure
3. **Transpilation**: The Razor processor converts the markup and code to pure C#
4. **Compilation**: The generated C# is compiled along with your other source files

Example input (`MyComponent.razor`):
```razor
@using UITest.UI
@inherits UIComponent

<div class="container">
    <p>@Message</p>
</div>

@code {
    public string Message { get; set; } = "Hello!";
}
```

Generated output (`MyComponent.razor.g.cs`):
```csharp
using UITest.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace YourNamespace
{
    public partial class MyComponent : UIComponent
    {
        protected override void BuildRenderTree(RenderTreeBuilder __builder)
        {
            // Generated rendering code...
        }

        public string Message { get; set; } = "Hello!";
    }
}
```

### SCSS Compilation

The SCSS compilation process:

1. **Discovery**: The build system finds all `.scss` files (excluding partials starting with `_`)
2. **Compilation**: Each SCSS file is compiled to CSS
3. **Output**: CSS files are placed in the output directory

### StyleSheet Attribute

The `[StyleSheet]` attribute (inspired by s&box) allows you to associate stylesheets with components:

```csharp
@attribute [StyleSheet("MyStyles.scss")]
@attribute [StyleSheet("themes/dark.scss")]
```

Multiple stylesheets can be specified using multiple attributes.

## Examples

See the `examples/SimpleDesktopApp` directory for a complete working example that demonstrates:

- Razor component creation
- SCSS styling with variables and nesting
- Component composition
- Build-time transpilation

## Project Structure

```
UITest/
├── src/
│   ├── UITest.Razor/         # Razor transpilation engine
│   ├── UITest.Scss/          # SCSS compilation engine
│   ├── UITest.Core/          # Core UI framework
│   └── UITest.Build/         # MSBuild tasks and targets
├── examples/
│   └── SimpleDesktopApp/     # Example application
└── README.md
```

## Technical Details

### Dependencies

- **.NET 8.0** - Target framework
- **Microsoft.AspNetCore.Razor.Language** - Razor parsing and code generation
- **Microsoft.AspNetCore.Components** - Component model
- **SharpScss** - SCSS compilation
- **Avalonia UI** - Cross-platform UI framework (for examples)

### Build Process

The build process is integrated via MSBuild targets:

1. `UITestTranspileRazor` - Runs before `CoreCompile` to transpile Razor files
2. `UITestCompileScss` - Runs before `Build` to compile SCSS files
3. `UITestCleanGenerated` - Runs before `Clean` to remove generated files

Generated files are placed in `$(IntermediateOutputPath)UITest/Generated/` by default.

## Comparison with s&box

This framework is based on the s&box Razor system but adapted for desktop applications:

| Feature | s&box | UITest |
|---------|-------|--------|
| Razor Transpilation | ✅ | ✅ |
| SCSS Support | ✅ | ✅ |
| Build Integration | ✅ | ✅ |
| Runtime Environment | Game Engine | Desktop Apps |
| Target Platform | Source 2 | Windows/macOS/Linux |
| License | MIT | MIT |

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT License - see LICENSE file for details.

Based on the s&box Razor system by Facepunch Studios (MIT licensed):
- https://github.com/Facepunch/sbox-public

## Credits

- **s&box** by Facepunch Studios - Original Razor transpilation system
- **XGUI-3** by Xenthio - Reference implementation and examples
- **Microsoft** - AspNetCore.Razor.Language and Components libraries
- **SharpScss** - SCSS compilation library

## References

- [s&box Public Repository](https://github.com/Facepunch/sbox-public)
- [XGUI-3](https://github.com/Xenthio/XGUI-3)
- [XGUI-3 Test Repository](https://github.com/Xenthio/xgui-3_test)
- [Microsoft Razor SDK](https://github.com/dotnet/razor)