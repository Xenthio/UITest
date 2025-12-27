#!/bin/bash
# Script to build Facebook Yoga native library from source
# Requires CMake and C++ compiler (GCC or Clang)

set -e

echo "Avalazor Yoga Native Library Setup"
echo "===================================="
echo ""

# Determine platform
PLATFORM=$(uname -s)
ARCH=$(uname -m)

echo "Platform: $PLATFORM"
echo "Architecture: $ARCH"
echo ""
echo "NOTE: This script builds Yoga from source."
echo "Required: CMake 3.15+ and C++ compiler (GCC/Clang)"
echo ""

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Determine target library name
case "$PLATFORM" in
    Linux*)
        TARGET="libyoga.so"
        ;;
    Darwin*)
        TARGET="libyoga.dylib"
        ;;
    *)
        echo "Unsupported platform: $PLATFORM"
        exit 1
        ;;
esac

# Create temp directory
TEMP_DIR=$(mktemp -d)
cd "$TEMP_DIR"

# Download Yoga source
echo "Downloading Yoga v3.1.0 source..."
curl -L "https://github.com/facebook/yoga/archive/refs/tags/v3.1.0.tar.gz" -o yoga.tar.gz

# Extract
echo "Extracting..."
tar -xzf yoga.tar.gz

# Build
echo "Building with CMake..."
cd yoga-3.1.0
mkdir -p build
cd build

cmake .. -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF
if [ $? -ne 0 ]; then
    echo "CMake configuration failed."
    echo "Make sure CMake and a C++ compiler are installed."
    exit 1
fi

cmake --build . --config Release -- -j$(nproc 2>/dev/null || echo 4)
if [ $? -ne 0 ]; then
    echo "Build failed."
    exit 1
fi

# Find and copy the library
echo "Searching for built library..."
LIB_FOUND=false

# Search common locations
for location in \
    "yoga/libyoga.so" \
    "yoga/libyogacore.so" \
    "yoga/libyoga.dylib" \
    "yoga/libyogacore.dylib" \
    "lib/libyoga.so" \
    "lib/libyogacore.so" \
    "lib/libyoga.dylib" \
    "lib/libyogacore.dylib" \
    "libyoga.so" \
    "libyogacore.so" \
    "libyoga.dylib" \
    "libyogacore.dylib"
do
    if [ -f "$location" ]; then
        echo "Found at: $location"
        cp "$location" "$SCRIPT_DIR/$TARGET"
        LIB_FOUND=true
        break
    fi
done

if [ "$LIB_FOUND" = false ]; then
    echo ""
    echo "WARNING: Could not find shared library"
    echo "Yoga v3.1.0 may have built as static library instead."
    echo ""
    echo "Found .a files:"
    find . -name "*.a" | head -5
    echo ""
    echo "Suggested solutions:"
    echo "1. Modify Yoga's CMakeLists.txt to use SHARED instead of STATIC"
    echo "2. Use an older Yoga version with better shared library support"
    echo "3. Extract library from React Native installation"
    echo ""
    exit 1
fi

# Cleanup
cd "$SCRIPT_DIR"
rm -rf "$TEMP_DIR"

echo ""
echo "Successfully built and installed $TARGET!"
echo "Location: $SCRIPT_DIR/$TARGET"
