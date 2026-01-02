# Assets Folder

This folder contains all static assets (images, themes, resources) for the application.

## Structure

```
Assets/
├── themes/          # UI themes and stylesheets
│   ├── XGUI/       # XGUI theme system
│   └── *.scss      # Individual theme files
└── images/         # Image assets for UI elements
```

## How It Works

### Build & Publish Behavior

All files in the `Assets/` folder are:
- Copied to the build output directory (bin/Debug or bin/Release)
- Copied to the publish directory for distribution
- Embedded in single-file publish executables
- Files appear at the root of the output (e.g., `Assets/themes/` becomes `themes/`)

This is configured in the `.csproj` file:

```xml
<ItemGroup>
  <Content Include="Assets\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
    <Link>%(RecursiveDir)%(Filename)%(Extension)</Link>
  </Content>
</ItemGroup>
```

### StyleSheet Resolution

The `[StyleSheet]` attribute searches for files in the following order:
1. `{BaseDirectory}/Assets/{path}` - Assets subdirectory (new structure)
2. `{BaseDirectory}/{path}` - Direct path (backward compatibility)
3. Assembly location
4. Current working directory

Example usage:
```csharp
// Resolves to Assets/themes/XGUI/DefaultStyles/ComputerXP.scss
[StyleSheet("/themes/XGUI/DefaultStyles/ComputerXP.scss")]

// Relative to component location  
[StyleSheet("MyComponent.scss")]
```

### Image/Texture Resolution

Images referenced in CSS (via `url()`) or code are searched in:
1. Exact path if it exists
2. `{BaseDirectory}/Assets/{path}` - Assets subdirectory (new structure)
3. `{BaseDirectory}/{path}` - Direct path
4. `{BaseDirectory}/assets/{path}` (legacy, lowercase)
5. `{BaseDirectory}/wwwroot/{path}` (legacy)

Example in SCSS:
```scss
.button {
    background-image: url("themes/XGUI/Resources/button_bg.png");
    border-image: url("themes/XGUI/Resources/button_border.png") 5 / 5px;
}
```

## Adding New Assets

### Adding Themes

1. Place theme `.scss` files in `Assets/themes/`
2. Reference in Razor components:
   ```csharp
   @attribute [StyleSheet("/themes/MyTheme.scss")]
   ```

### Adding Images

1. Place image files in `Assets/images/` (or any subfolder)
2. Reference in CSS:
   ```scss
   background-image: url("images/my-image.png");
   ```

### Adding Resources

For theme-specific resources (icons, borders, etc.):
1. Create a subfolder under `Assets/themes/`
   - Example: `Assets/themes/XGUI/Resources/`
2. Place image files there
3. Reference in theme SCSS files:
   ```scss
   $icon-close: url("themes/XGUI/Resources/icon_close.png");
   ```

## Migration from Root Folders

Previously, themes and resources were in root-level folders:
- `/themes/` → Now in `Assets/themes/`
- Images scattered in various locations → Now in `Assets/images/`

The framework maintains backward compatibility with these paths during the transition.

## Best Practices

1. **Organize by type**: Keep themes, images, and other resource types in separate subfolders
2. **Use relative paths**: Reference assets relative to the output root (e.g., `themes/...`)
3. **Lowercase names**: Use lowercase filenames for cross-platform compatibility
4. **Small file sizes**: Optimize images before adding to keep build size small
5. **Version control**: Commit all assets to the repository for team collaboration

## Troubleshooting

### Stylesheet not found
- Verify the file exists in `Assets/themes/`
- Check the path in `[StyleSheet]` matches the output location
- Run `dotnet build` to ensure assets are copied

### Image not loading
- Check console for "Failed to load texture" messages
- Verify file path in CSS `url()` is correct
- Ensure image file is in `Assets/` and copied to output

### Build not copying assets
- Verify `.csproj` has the `<Content Include="Assets\**\*">` section
- Clean and rebuild: `dotnet clean && dotnet build`
