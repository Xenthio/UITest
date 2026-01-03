# Graphics Backend Selection

Avalazor supports multiple graphics backends for rendering the UI:

## Available Backends

### OpenGL (Default, Fully Functional)
- **Status**: ✅ Fully implemented and tested
- **Platform Support**: Linux, macOS, Windows
- **Use Case**: Default cross-platform rendering
- **Stability**: Production-ready

The OpenGL backend uses OpenGL 3.3 Core Profile and integrates with SkiaSharp's OpenGL backend. This is the most mature and well-tested backend.

### Vulkan (Experimental)
- **Status**: ⚠️ Skeleton implementation, not functional
- **Platform Support**: Linux, Windows (when implemented)
- **Use Case**: Modern low-level graphics API with better multi-threading support
- **Stability**: Not yet functional - requires complete GRVkBackendContext setup

The Vulkan backend has a basic structure but requires significant additional work:
- Proper Vulkan function pointer setup for Skia
- Memory allocator integration
- Swap chain management
- Synchronization primitives

### DirectX 11 (Experimental)
- **Status**: ⚠️ Skeleton implementation, not functional
- **Platform Support**: Windows only (when implemented)
- **Use Case**: Native Windows DirectX rendering
- **Stability**: Not yet functional - requires D3D11 device and swap chain setup

The DirectX 11 backend is Windows-specific and requires:
- D3D11 device and context creation
- DXGI swap chain setup
- Proper render target management
- Integration with SkiaSharp's D3D11 backend

## Selecting a Backend

### In Code

```csharp
using Avalazor.UI;

// OpenGL (default)
var window = new NativeWindow(
    width: 1280, 
    height: 720, 
    title: "My App"
);

// Explicitly specify OpenGL
var windowGL = new NativeWindow(
    width: 1280, 
    height: 720, 
    title: "My App",
    backendType: GraphicsBackendType.OpenGL
);

// Vulkan (not yet functional)
var windowVk = new NativeWindow(
    width: 1280, 
    height: 720, 
    title: "My App",
    backendType: GraphicsBackendType.Vulkan
);

// DirectX 11 (not yet functional, Windows only)
var windowD3D = new NativeWindow(
    width: 1280, 
    height: 720, 
    title: "My App",
    backendType: GraphicsBackendType.DirectX11
);
```

### Platform Restrictions

The DirectX11 backend can only be used on Windows. Attempting to use it on other platforms will throw a `PlatformNotSupportedException`.

## Implementation Status

| Backend | Device Init | Context Creation | Surface Creation | Rendering | Status |
|---------|-------------|------------------|------------------|-----------|--------|
| OpenGL | ✅ | ✅ | ✅ | ✅ | Production |
| Vulkan | ✅ | ❌ | ❌ | ❌ | Skeleton Only |
| DirectX 11 | ❌ | ❌ | ❌ | ❌ | Skeleton Only |

## Contributing

Contributions to complete the Vulkan and DirectX 11 backends are welcome! Key areas that need work:

### Vulkan Backend
1. Complete `GRVkBackendContext` initialization with proper function pointers
2. Implement Vulkan surface creation and swap chain management
3. Set up command buffers and synchronization
4. Memory management and resource cleanup

### DirectX 11 Backend
1. Complete D3D11 device and swap chain creation
2. Implement `GRContext.CreateD3D()` integration
3. Set up render target views and back buffer management
4. Handle resize and present operations

## References

- [SkiaSharp Documentation](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/)
- [Vulkan Tutorial](https://vulkan-tutorial.com/)
- [DirectX 11 Programming Guide](https://docs.microsoft.com/en-us/windows/win32/direct3d11/dx-graphics-overviews)
- [Silk.NET Documentation](https://github.com/dotnet/Silk.NET)
