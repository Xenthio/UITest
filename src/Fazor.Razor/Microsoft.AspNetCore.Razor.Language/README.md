# Microsoft.AspNetCore.Razor.Language Implementation

This directory contains the `Microsoft.AspNetCore.Razor.Language` namespace implementation ported from [s&box](https://github.com/Facepunch/sbox-public) (MIT licensed).

## Purpose

This implementation provides full IntelliSense support for Razor files in Visual Studio and other IDEs by embedding the Razor Language Server components directly.

## Source

- **Original Source**: s&box engine (https://github.com/Facepunch/sbox-public/tree/master/engine/Sandbox.Razor/Microsoft.AspNetCore.Razor.Language)
- **License**: MIT (same as s&box)
- **Author**: Facepunch Studios

## Why Embed This?

s&box discovered that to get full Razor IntelliSense working in Visual Studio, they needed to implement their own `Microsoft.AspNetCore.Razor.Language` namespace rather than relying on the NuGet package. This provides:

1. **Component IntelliSense** - Autocomplete for custom Razor components
2. **Parameter IntelliSense** - Autocomplete and validation for component parameters
3. **Directive IntelliSense** - Full support for `@using`, `@namespace`, etc.
4. **Syntax Validation** - Real-time error checking in Razor files

## Files Included

This implementation includes ~434 C# files covering:
- Razor syntax parsing
- Code generation
- Component model
- Tag helpers
- Directive support
- Language server protocol integration

## Usage

Projects that reference `Fazor.Razor` automatically get this implementation and full IntelliSense support for Razor files without any additional configuration.

## Updates

This is a snapshot from s&box. If s&box updates their Razor implementation, we may need to sync those changes here.
