#!/bin/bash
# Build script for Facebook Yoga native library
# This script downloads and builds the Yoga layout engine from source

set -e

echo "Building Facebook Yoga native library..."

# Determine platform
PLATFORM=$(uname -s)
ARCH=$(uname -m)

# Create temporary build directory
BUILD_DIR=$(mktemp -d)
cd "$BUILD_DIR"

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
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release

# Copy the built library to the Native directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

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
cd /
rm -rf "$BUILD_DIR"

echo "Done!"
