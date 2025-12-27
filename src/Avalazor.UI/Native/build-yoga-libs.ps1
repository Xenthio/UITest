# PowerShell script to download prebuilt Yoga native library for Windows
# Falls back to building from source if download fails

Write-Host "Avalazor Yoga Native Library Setup for Windows" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host ""

# Try to download prebuilt binary first
Write-Host "Attempting to download prebuilt yoga.dll from NPM package..." -ForegroundColor Yellow

try {
    # Download the yoga-layout NPM package tarball
    $npmUrl = "https://registry.npmjs.org/yoga-layout/-/yoga-layout-3.1.0.tgz"
    Write-Host "Downloading from: $npmUrl" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $npmUrl -OutFile "yoga-layout.tgz"
    
    # Extract the tarball
    Write-Host "Extracting package..." -ForegroundColor Yellow
    tar -xzf yoga-layout.tgz
    
    # Look for the Windows DLL in the package
    $dllPath = "package\build\Release\yoga.dll"
    if (Test-Path $dllPath) {
        Write-Host "Found prebuilt yoga.dll!" -ForegroundColor Green
        Copy-Item $dllPath "yoga.dll" -Force
        
        # Cleanup
        Remove-Item -Recurse -Force "package" -ErrorAction SilentlyContinue
        Remove-Item "yoga-layout.tgz" -ErrorAction SilentlyContinue
        
        Write-Host "Successfully installed prebuilt yoga.dll!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "Prebuilt DLL not found in NPM package. Trying alternative sources..." -ForegroundColor Yellow
    }
    
    # Cleanup failed attempt
    Remove-Item -Recurse -Force "package" -ErrorAction SilentlyContinue
    Remove-Item "yoga-layout.tgz" -ErrorAction SilentlyContinue
} catch {
    Write-Host "Failed to download from NPM: $_" -ForegroundColor Yellow
}

# Try alternative: yoga-layout-prebuilt package
Write-Host "Trying yoga-layout-prebuilt package..." -ForegroundColor Yellow
try {
    $npmUrl2 = "https://registry.npmjs.org/yoga-layout-prebuilt/-/yoga-layout-prebuilt-1.10.0.tgz"
    Invoke-WebRequest -Uri $npmUrl2 -OutFile "yoga-prebuilt.tgz"
    tar -xzf yoga-prebuilt.tgz
    
    $dllPath = "package\build\Release\yoga.dll"
    if (Test-Path $dllPath) {
        Write-Host "Found prebuilt yoga.dll from alternative source!" -ForegroundColor Green
        Copy-Item $dllPath "yoga.dll" -Force
        Remove-Item -Recurse -Force "package" -ErrorAction SilentlyContinue
        Remove-Item "yoga-prebuilt.tgz" -ErrorAction SilentlyContinue
        Write-Host "Successfully installed prebuilt yoga.dll!" -ForegroundColor Green
        exit 0
    }
    
    Remove-Item -Recurse -Force "package" -ErrorAction SilentlyContinue
    Remove-Item "yoga-prebuilt.tgz" -ErrorAction SilentlyContinue
} catch {
    Write-Host "Failed to download from alternative source: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Prebuilt binaries not available. Building from source..." -ForegroundColor Yellow
Write-Host "This requires Visual Studio 2022 with C++ tools and CMake." -ForegroundColor Yellow
Write-Host ""

# Download Yoga source
Write-Host "Downloading Yoga v3.1.0 source..." -ForegroundColor Yellow
$yogaUrl = "https://github.com/facebook/yoga/archive/refs/tags/v3.1.0.tar.gz"
Invoke-WebRequest -Uri $yogaUrl -OutFile "yoga.tar.gz"

# Extract
Write-Host "Extracting..." -ForegroundColor Yellow
tar -xzf yoga.tar.gz

# Build
Write-Host "Building with CMake..." -ForegroundColor Yellow
Set-Location yoga-3.1.0
New-Item -ItemType Directory -Force -Path "build" | Out-Null
Set-Location build

cmake .. -G "Visual Studio 17 2022" -A x64 -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF -DCMAKE_WINDOWS_EXPORT_ALL_SYMBOLS=ON
if ($LASTEXITCODE -ne 0) {
    Write-Host "CMake configuration failed. Make sure Visual Studio 2022 with C++ tools is installed." -ForegroundColor Red
    exit 1
}

cmake --build . --config Release --target yogacore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}

# Copy DLL (check multiple possible locations including bin directory)
Write-Host "Copying yoga.dll..." -ForegroundColor Yellow
$dllLocations = @(
    "Release\yoga.dll",
    "Release\yogacore.dll",
    "yoga\Release\yoga.dll",
    "yoga\Release\yogacore.dll",
    "lib\Release\yoga.dll",
    "lib\Release\yogacore.dll",
    "bin\Release\yoga.dll",
    "bin\Release\yogacore.dll",
    "yoga\bin\Release\yoga.dll",
    "yoga\bin\Release\yogacore.dll"
)

$copied = $false
foreach ($location in $dllLocations) {
    if (Test-Path $location) {
        Write-Host "Found yoga.dll at $location" -ForegroundColor Cyan
        Copy-Item $location "..\..\yoga.dll" -Force
        $copied = $true
        break
    }
}

if (-not $copied) {
    Write-Host "ERROR: Could not find yoga.dll in expected locations." -ForegroundColor Red
    Write-Host "Searched locations:" -ForegroundColor Yellow
    foreach ($location in $dllLocations) {
        Write-Host "  - $location" -ForegroundColor Yellow
    }
    Write-Host "Build directory contents:" -ForegroundColor Yellow
    Get-ChildItem -Recurse -Filter "*.dll" | ForEach-Object { Write-Host "  Found: $($_.FullName)" -ForegroundColor Cyan }
    Write-Host "" -ForegroundColor Yellow
    Write-Host "Build directory .lib files:" -ForegroundColor Yellow
    Get-ChildItem -Recurse -Filter "*.lib" | ForEach-Object { Write-Host "  Found: $($_.FullName)" -ForegroundColor Cyan }
    Write-Host "" -ForegroundColor Yellow
    Write-Host "NOTE: If only yogacore.lib exists, the shared library build failed." -ForegroundColor Red
    Write-Host "This may be due to Yoga v3.1.0 not supporting BUILD_SHARED_LIBS properly." -ForegroundColor Red
    Write-Host "Trying alternative approach: Building all targets to generate DLL..." -ForegroundColor Yellow
    
    # Try building all targets which might generate the DLL
    Set-Location ..
    Remove-Item -Recurse -Force build
    New-Item -ItemType Directory -Force -Path "build" | Out-Null
    Set-Location build
    cmake .. -G "Visual Studio 17 2022" -A x64 -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF -DCMAKE_WINDOWS_EXPORT_ALL_SYMBOLS=ON
    cmake --build . --config Release
    
    # Try to find DLL again
    foreach ($location in $dllLocations) {
        if (Test-Path $location) {
            Write-Host "Found yoga.dll at $location after rebuild" -ForegroundColor Cyan
            Copy-Item $location "..\..\yoga.dll" -Force
            $copied = $true
            break
        }
    }
    
    if (-not $copied) {
        Write-Host "ERROR: Still could not find yoga.dll. Yoga v3.1.0 may not support building as DLL." -ForegroundColor Red
        Write-Host "Please try using a prebuilt yoga.dll from React Native or Flutter." -ForegroundColor Yellow
        exit 1
    }
}

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
Set-Location ..\..
# Remove-Item -Recurse -Force "yoga-3.1.0"
# Remove-Item "yoga.tar.gz"

Write-Host "Build complete! yoga.dll is ready." -ForegroundColor Green
