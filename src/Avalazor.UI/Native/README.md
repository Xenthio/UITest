# Native Yoga Library

This directory contains the native Yoga layout engine library required by Avalazor.UI.

## Building the Native Library

Avalazor uses Facebook's Yoga layout engine via P/Invoke for maximum performance flexbox layout. You need to build the native library once before using Avalazor.

### Prerequisites

- **CMake** (version 3.13 or higher)
- **C++ compiler** (GCC, Clang, or MSVC)
- **curl** and **tar** (usually pre-installed on Unix systems)

### Build Instructions

#### Linux / macOS

```bash
cd src/Avalazor.UI/Native
chmod +x build-yoga-libs.sh
./build-yoga-libs.sh
```

This will:
1. Download Yoga source from GitHub
2. Build the native library using CMake
3. Copy the resulting library to this directory:
   - Linux: `libyoga.so`
   - macOS: `libyoga.dylib`

#### Windows

**Option 1: Using PowerShell script (recommended)**

Requires Visual Studio 2022 with C++ tools and CMake:

```powershell
cd src/Avalazor.UI/Native
.\build-yoga-libs.ps1
```

This will automatically download, build, and copy `yoga.dll`.

**Option 2: Using WSL**

If you have Windows Subsystem for Linux installed:

```bash
# Inside WSL
cd /mnt/c/path/to/UITest/src/Avalazor.UI/Native
chmod +x build-yoga-libs.sh
./build-yoga-libs.sh
# Rename libyoga.so to yoga.dll for Windows use
```

### Prebuilt Binaries (Alternative)

If you have trouble building from source, you can obtain prebuilt binaries:

1. **From React Native** - React Native includes prebuilt Yoga binaries in their npm packages
2. **From Yoga releases** - Check [facebook/yoga releases](https://github.com/facebook/yoga/releases)
3. **Build in Docker** - Use a Docker container to build if you don't have the toolchain

### Verifying the Build

After building, you should have one of these files in this directory:
- `libyoga.so` (Linux)
- `libyoga.dylib` (macOS)
- `yoga.dll` (Windows)

The library will be automatically copied to the output directory during build.

### Troubleshooting

**"yoga library not found" error:**
- Ensure the native library is in this directory
- Check that the filename matches your platform (`.so`, `.dylib`, or `.dll`)
- On Linux, you may need to add the library path to `LD_LIBRARY_PATH`

**Build failures:**
- Ensure CMake 3.13+ is installed: `cmake --version`
- Ensure you have a C++ compiler installed
- Try building with verbose output: `cmake --build . --verbose`

### Architecture

Avalazor uses s&box's approach:
- Native Yoga C++ library (Facebook's official implementation)
- P/Invoke bindings in C# (`Yoga.cs`, `YogaWrapper.cs`)
- No managed wrapper dependencies
- Maximum performance with full Yoga feature support

This is the same architecture used by s&box for their UI system.
