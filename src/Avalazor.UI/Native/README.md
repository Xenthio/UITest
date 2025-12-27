# Native Yoga Library

This directory contains the native Yoga layout engine library required by Avalazor.UI.

## Setup

Avalazor uses Facebook's Yoga layout engine via P/Invoke for maximum performance flexbox layout. You need to obtain the native library file before building the project.

### Quick Start

#### Option 1: Build from Source (Recommended)

**Linux / macOS:**
```bash
cd src/Avalazor.UI/Native
chmod +x build-yoga-libs.sh
./build-yoga-libs.sh
```

**Windows:**
```powershell
cd src/Avalazor.UI/Native
.\build-yoga-libs.ps1
```

**Build Requirements:**
- **CMake** 3.15+ (`cmake --version`)
- **C++ compiler**:
  - Linux: GCC or Clang (`gcc --version`)
  - macOS: Xcode Command Line Tools (`xcode-select --install`)
  - Windows: Visual Studio 2022 with C++ Desktop Development workload
- Internet connection to download Yoga source

The scripts will:
1. Download Yoga v3.1.0 source from GitHub
2. Configure with CMake
3. Build the library
4. Copy to this directory

**Note**: Yoga v3.1.0's CMake config may build as a static library (.lib/.a) instead of shared (.dll/.so/.dylib). The scripts will warn you if this happens.

#### Option 2: Manual Download

Since Yoga v3.1.0 doesn't easily build as a shared library, you can manually obtain prebuilt binaries:

**From React Native:**
1. Install React Native for your platform
2. Find `yoga.dll` (Windows), `libyoga.so` (Linux), or `libyoga.dylib` (macOS)
3. Place in this directory

**From Community Builds:**
1. Search GitHub for "yoga prebuilt binaries"
2. Download the appropriate file for your platform
3. Place in this directory

**Build from Source with Modifications:**
1. Download Yoga v3.1.0: https://github.com/facebook/yoga/releases/tag/v3.1.0
2. Modify `yoga/CMakeLists.txt`: Change `add_library(yogacore STATIC` to `SHARED`
3. Build with CMake
4. Copy resulting library here

### Required Files

After setup, you should have **one** of these files in this directory:
- `libyoga.so` - Linux shared library
- `libyoga.dylib` - macOS dynamic library
- `yoga.dll` - Windows dynamic-link library

The MSBuild configuration will automatically copy it to the output directory during build.

## Why NPM Packages Don't Have Binaries

The NPM packages (`yoga-layout`, `yoga-layout-prebuilt`) only contain:
- JavaScript/TypeScript wrapper code
- WASM binaries for browsers
- **No native .dll/.so/.dylib files**

This is why manual acquisition or building from source is required.

## Architecture

Avalazor uses s&box's P/Invoke approach:
- Native Yoga C++ library (Facebook's official implementation v3.1.0)
- Direct P/Invoke bindings in C# (`Yoga.cs`, `YogaWrapper.cs`, `YGNodeRef.cs`)
- No managed wrapper dependencies
- Maximum performance with full Yoga feature support

This is the same architecture used by s&box for their UI system.

## Troubleshooting

**Build fails with "yogacore.lib" instead of DLL:**
- Yoga v3.1.0's CMake defaults to static library
- Solution: Modify Yoga's CMakeLists.txt to use SHARED, or use prebuilt binary

**"yoga library not found" runtime error:**
- Ensure the native library file is in this directory
- Check filename matches your platform
- On Linux: May need to add to LD_LIBRARY_PATH or use `ldconfig`

**Windows: Visual Studio not found:**
- Install Visual Studio 2022
- Include "Desktop development with C++" workload
- Ensure CMake can find it

**macOS: Command Line Tools not found:**
- Run: `xcode-select --install`
- Accept the license if prompted

**Linux: Compiler not found:**
- Debian/Ubuntu: `sudo apt install build-essential cmake`
- Fedora/RHEL: `sudo dnf install gcc-c++ cmake`
- Arch: `sudo pacman -S base-devel cmake`

## For More Information

- Yoga GitHub: https://github.com/facebook/yoga
- s&box Panel system reference
- Avalazor documentation: See repository README
