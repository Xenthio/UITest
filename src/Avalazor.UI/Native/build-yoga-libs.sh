#!/bin/bash
# Script to download or build Facebook Yoga native library
# Attempts to download prebuilt binaries first, falls back to building from source

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

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Attempt to download prebuilt binary from NPM package
echo "Attempting to download prebuilt library from NPM package..."

# Create temp directory
TEMP_DIR=$(mktemp -d)
cd "$TEMP_DIR"

# Try downloading yoga-layout package
if curl -L "https://registry.npmjs.org/yoga-layout/-/yoga-layout-3.1.0.tgz" -o yoga-layout.tgz 2>/dev/null; then
    echo "Downloaded NPM package, extracting..."
    tar -xzf yoga-layout.tgz 2>/dev/null || true
    
    # Look for prebuilt library based on platform
    case "$PLATFORM" in
        Linux*)
            LIB_PATH="package/build/Release/libyoga.so"
            TARGET="libyoga.so"
            ;;
        Darwin*)
            LIB_PATH="package/build/Release/libyoga.dylib"
            TARGET="libyoga.dylib"
            ;;
        *)
            LIB_PATH=""
            ;;
    esac
    
    if [ -n "$LIB_PATH" ] && [ -f "$LIB_PATH" ]; then
        echo "Found prebuilt library!"
        cp "$LIB_PATH" "$SCRIPT_DIR/$TARGET"
        cd "$SCRIPT_DIR"
        rm -rf "$TEMP_DIR"
        echo "Successfully installed prebuilt $TARGET!"
        exit 0
    fi
fi

echo "Prebuilt binaries not available for $PLATFORM."
echo "Building from source..."
echo ""

cd "$TEMP_DIR"

# Download Yoga source
echo "Downloading Yoga source..."
YOGA_VERSION="3.1.0"
curl -L "https://github.com/facebook/yoga/archive/refs/tags/v${YOGA_VERSION}.tar.gz" -o yoga.tar.gz
tar -xzf yoga.tar.gz
cd "yoga-${YOGA_VERSION}"

# Build with CMake
echo "Building Yoga..."
mkdir build
cd build
cmake .. -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF
cmake --build . --config Release --target yogacore

case "$PLATFORM" in
    Linux*)
        LIBRARY="libyogacore.so"
        TARGET="libyoga.so"
        ;;
    Darwin*)
        LIBRARY="libyogacore.dylib"
        TARGET="libyoga.dylib"
        ;;
    MINGW*|MSYS*|CYGWIN*)
        LIBRARY="yogacore.dll"
        TARGET="yoga.dll"
        ;;
    *)
        echo "Unsupported platform: $PLATFORM"
        exit 1
        ;;
esac

# Find and copy the library
BUILT_LIB=$(find . -name "$LIBRARY" | head -n 1)
if [ -z "$BUILT_LIB" ]; then
    echo "Error: Could not find built library $LIBRARY"
    exit 1
fi

cp "$BUILT_LIB" "$SCRIPT_DIR/$TARGET"

echo "Success! Native library built and copied to: $SCRIPT_DIR/$TARGET"

# Cleanup
cd "$SCRIPT_DIR"
rm -rf "$TEMP_DIR"

echo "Done!"
