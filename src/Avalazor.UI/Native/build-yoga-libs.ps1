# PowerShell script to build Yoga native library for Windows from source
# Requires Visual Studio 2022 with C++ tools and CMake

Write-Host "Avalazor Yoga Native Library Setup for Windows" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green
Write-Host ""
Write-Host "NOTE: This script builds Yoga from source." -ForegroundColor Yellow
Write-Host "Required: Visual Studio 2022 with C++ tools and CMake" -ForegroundColor Yellow
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

# Copy DLL (check multiple possible locations)
Write-Host "Copying yoga library..." -ForegroundColor Yellow
$dllLocations = @(
    "Release\yoga.dll",
    "Release\yogacore.dll",
    "yoga\Release\yoga.dll",
    "yoga\Release\yogacore.dll",
    "lib\Release\yoga.dll",
    "lib\Release\yogacore.dll",
    "bin\Release\yoga.dll",
    "bin\Release\yogacore.dll"
)

$copied = $false
foreach ($location in $dllLocations) {
    if (Test-Path $location) {
        Write-Host "Found at $location" -ForegroundColor Cyan
        Copy-Item $location "..\..\yoga.dll" -Force
        $copied = $true
        break
    }
}

if (-not $copied) {
    Write-Host ""
    Write-Host "WARNING: Could not find yoga.dll" -ForegroundColor Red
    Write-Host "Yoga v3.1.0 may have built as static library instead." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Found .lib files:" -ForegroundColor Yellow
    Get-ChildItem -Recurse -Filter "*.lib" | Select-Object -First 5 | ForEach-Object { 
        Write-Host "  $($_.FullName)" -ForegroundColor Cyan 
    }
    Write-Host ""
    Write-Host "Suggested solutions:" -ForegroundColor Yellow
    Write-Host "1. Modify Yoga's CMakeLists.txt to use SHARED instead of STATIC" -ForegroundColor Yellow
    Write-Host "2. Use an older Yoga version with better DLL support" -ForegroundColor Yellow
    Write-Host "3. Extract yoga.dll from React Native Windows installation" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# Cleanup
Set-Location ..\..
Remove-Item -Recurse -Force yoga-3.1.0 -ErrorAction SilentlyContinue
Remove-Item yoga.tar.gz -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Successfully built and installed yoga.dll!" -ForegroundColor Green
Write-Host "Location: $(Get-Location)\yoga.dll" -ForegroundColor Cyan
