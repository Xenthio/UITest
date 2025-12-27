# PowerShell script to build Yoga native library on Windows
# Requires: Visual Studio 2022 with C++ tools, CMake

Write-Host "Building Yoga v3.1.0 for Windows..." -ForegroundColor Green

# Download Yoga
Write-Host "Downloading Yoga v3.1.0..." -ForegroundColor Yellow
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

cmake .. -G "Visual Studio 18 2026" -A x64 -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF
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
    "yoga\Release\yoga.dll",
    "lib\Release\yoga.dll",
    "bin\Release\yoga.dll",
    "yoga\bin\Release\yoga.dll"
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
    cmake .. -G "Visual Studio 18 2026" -A x64 -DBUILD_SHARED_LIBS=ON -DBUILD_TESTING=OFF -DYOGA_BUILD_SHARED=ON
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
