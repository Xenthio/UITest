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

cmake .. -G "Visual Studio 17 2022" -A x64 -DBUILD_SHARED_LIBS=ON -DYOGA_BUILD_TESTS=OFF
if ($LASTEXITCODE -ne 0) {
    Write-Host "CMake configuration failed. Make sure Visual Studio 2022 with C++ tools is installed." -ForegroundColor Red
    exit 1
}

cmake --build . --config Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}

# Copy DLL (check multiple possible locations)
Write-Host "Copying yoga.dll..." -ForegroundColor Yellow
$dllLocations = @(
    "Release\yoga.dll",
    "yoga\Release\yoga.dll",
    "lib\Release\yoga.dll"
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
    exit 1
}

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
Set-Location ..\..
# Remove-Item -Recurse -Force "yoga-3.1.0"
# Remove-Item "yoga.tar.gz"

Write-Host "Build complete! yoga.dll is ready." -ForegroundColor Green
