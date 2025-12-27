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

cmake .. -G "Visual Studio 17 2022" -A x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "CMake configuration failed. Make sure Visual Studio 2022 with C++ tools is installed." -ForegroundColor Red
    exit 1
}

cmake --build . --config Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit 1
}

# Copy DLL
Write-Host "Copying yoga.dll..." -ForegroundColor Yellow
Copy-Item "Release\yoga.dll" "..\..\yoga.dll" -Force

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Yellow
Set-Location ..\..
Remove-Item -Recurse -Force "yoga-3.1.0"
Remove-Item "yoga.tar.gz"

Write-Host "Build complete! yoga.dll is ready." -ForegroundColor Green
