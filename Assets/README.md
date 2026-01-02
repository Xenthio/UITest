# Global Assets Folder

This folder contains built-in assets (themes, fonts, images) that are shared across all projects in the solution.

## Structure

```
Assets/
├── themes/          # Built-in UI themes (XGUI, defaults)
│   ├── XGUI/       # XGUI theme system
│   └── *.scss      # Base theme files
├── fonts/          # Font files (.ttf, .otf)
└── images/         # Shared image assets
```

## Usage

Projects can access these global assets by importing the `Assets.props` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Import global assets -->
  <Import Project="..\..\Assets.props" />
  
  <!-- Your project configuration -->
  <PropertyGroup>
    <!-- ... -->
  </PropertyGroup>
</Project>
```

This automatically copies all global assets to the project's output directory.

## Global vs Project Assets

### Global Assets (`/Assets/`)
- **Purpose**: Built-in, shared resources used across multiple projects
- **Examples**: 
  - XGUI theme system
  - Default Avalazor styles
  - Built-in fonts (Roboto, etc.)
  - Common icons and UI elements
- **Location**: Root of solution (`/Assets/`)
- **Usage**: Import via `Assets.props`

### Project Assets (`ProjectName/Assets/`)
- **Purpose**: Project-specific resources
- **Examples**:
  - Custom themes for a specific app
  - App-specific images and icons
  - Project branding assets
- **Location**: Inside project folder (`examples/SimpleDesktopApp/Assets/`)
- **Usage**: Configured in project's `.csproj` file

## How It Works

When `Assets.props` is imported:
1. All files in `/Assets/**/*` are included as Content items
2. Files are copied to build output directory (bin/Debug, bin/Release)
3. Files are included in publish output and single-file executables
4. The `Assets/` prefix is stripped, so `Assets/themes/file.scss` becomes `themes/file.scss` in output

## File Resolution Priority

When loading stylesheets or images, the framework searches in this order:
1. **Project Assets** - `ProjectName/Assets/`
2. **Global Assets** - `/Assets/` (via Assets.props)
3. **Assembly location** - Where DLLs are located
4. **Current directory** - Runtime working directory

This means project-specific assets override global ones if they have the same path.

## Adding Global Assets

### Adding a Built-in Theme

1. Place theme files in `/Assets/themes/`:
   ```
   Assets/
   └── themes/
       └── MyBuiltInTheme/
           ├── MyTheme.scss
           └── Resources/
               └── icons.png
   ```

2. Projects automatically have access via StyleSheet attribute:
   ```csharp
   [StyleSheet("/themes/MyBuiltInTheme/MyTheme.scss")]
   ```

### Adding Fonts

1. Place font files in `/Assets/fonts/`:
   ```
   Assets/
   └── fonts/
       ├── Roboto-Regular.ttf
       └── OpenSans-Bold.ttf
   ```

2. Fonts are automatically available to the renderer

### Adding Shared Images

1. Place images in `/Assets/images/`:
   ```
   Assets/
   └── images/
       └── shared/
           └── logo.png
   ```

2. Reference in themes or code:
   ```scss
   background-image: url("images/shared/logo.png");
   ```

## Migration Guide

### From `/themes/` (old) → `/Assets/themes/` (new)

The old structure:
```
/themes/
  └── XGUI/
```

New structure:
```
/Assets/
  └── themes/
      └── XGUI/
```

Projects using `[StyleSheet("/themes/...")]` continue to work - the path is resolved from the output directory where Assets are copied.

### From `/fonts/` (old) → `/Assets/fonts/` (new)

Similar migration - fonts are copied to output at build time.

## Best Practices

1. **Minimize global assets**: Only include truly shared resources
2. **Version control**: Commit all global assets to repository
3. **Optimize files**: Compress images and minify resources
4. **Document usage**: Add README files for complex asset structures
5. **Naming conventions**: Use lowercase, hyphen-separated names for cross-platform compatibility

## Troubleshooting

### Assets not appearing in output
1. Verify `Assets.props` is imported in your `.csproj`
2. Check import path is correct relative to project location
3. Run `dotnet clean` then `dotnet build`

### File path conflicts
If project assets conflict with global assets (same path), the project asset takes precedence during file copy. Consider renaming one to avoid confusion.

### Missing assets at runtime
Ensure you're running from the correct directory where assets were copied, or use absolute paths resolved at runtime.
