# Native Yoga Library

This directory contains the native Yoga layout engine library required by Avalazor.UI.

## Automated Setup (Recommended)

Avalazor uses Facebook's Yoga layout engine via P/Invoke for maximum performance flexbox layout. The build scripts automatically download prebuilt binaries or build from source.

### Quick Start

#### Linux / macOS

```bash
cd src/Avalazor.UI/Native
chmod +x build-yoga-libs.sh
./build-yoga-libs.sh
```

The script will:
1. **First attempt**: Download prebuilt library from NPM package (fast!)
2. **Fallback**: Build from source if prebuilt not available

#### Windows

```powershell
cd src/Avalazor.UI/Native
.\build-yoga-libs.ps1
```

The script will:
1. **First attempt**: Download prebuilt `yoga.dll` from NPM package (fast!)
2. **Fallback**: Build from source using Visual Studio (requires VS 2022 + C++ tools + CMake)

## What Happens

### Prebuilt Binary Download (Default)

The scripts first try to download prebuilt binaries from the `yoga-layout` NPM package:
- ✅ **No compilation required**
- ✅ **Fast - completes in seconds**
- ✅ **No build tools needed**
- ✅ **Works on all platforms**

### Build from Source (Fallback)

If prebuilt binaries aren't available, the scripts automatically fall back to building from source.

**Requirements for building from source:**
- **CMake** (version 3.13 or higher)
- **C++ compiler**:
  - Linux: GCC or Clang
  - macOS: Xcode Command Line Tools
  - Windows: Visual Studio 2022 with C++ tools
- **curl** and **tar** (usually pre-installed)

## Manual Download (Alternative)

If automatic download fails, you can manually obtain prebuilt binaries:

1. **From NPM Package** (recommended):
   ```bash
   npm pack yoga-layout@3.1.0
   # Extract yoga.dll / libyoga.so / libyoga.dylib from package
   ```

2. **From React Native** - Extract from React Native Windows/Android releases

3. **Build in Docker** - Use a Docker container if you don't have the toolchain

## Verification

After running the script, you should have one of these files in this directory:
- `libyoga.so` (Linux)
- `libyoga.dylib` (macOS)
- `yoga.dll` (Windows)

The library will be automatically copied to the output directory during build.

## Troubleshooting

**"yoga library not found" error:**
- Ensure the native library is in this directory
- Check that the filename matches your platform (`.so`, `.dylib`, or `.dll`)
- On Linux, you may need to add the library path to `LD_LIBRARY_PATH`

**Download failures:**
- Check your internet connection
- Try running the script again (temporary network issues)
- Use manual download method as alternative

**Build from source failures:**
- Ensure CMake 3.13+ is installed: `cmake --version`
- Windows: Ensure Visual Studio 2022 with C++ tools is installed
- Try building with verbose output: `cmake --build . --verbose`

**NOTE**: Yoga v3.1.0's CMake configuration has limitations. The prebuilt binary download method is strongly recommended for Windows.

## Architecture

Avalazor uses s&box's approach:
- Native Yoga C++ library (Facebook's official implementation)
- P/Invoke bindings in C# (`Yoga.cs`, `YogaWrapper.cs`)
- No managed wrapper dependencies
- Maximum performance with full Yoga feature support

This is the same architecture used by s&box for their UI system.
