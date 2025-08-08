# Build Release Configuration Script for DbAutoChat.Win
param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "bin\Release\net8.0-windows",
    [switch]$CreateInstaller = $false
)

Write-Host "Building DbAutoChat.Win in $Configuration configuration..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Yellow
dotnet publish --configuration $Configuration --no-build --output $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

# Copy additional files
Write-Host "Copying additional files..." -ForegroundColor Yellow
Copy-Item "README.md" -Destination $OutputPath -Force
Copy-Item "appsettings.json" -Destination $OutputPath -Force

# Create logs directory
$logsPath = Join-Path $OutputPath "logs"
if (!(Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force
}

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Output location: $OutputPath" -ForegroundColor Cyan

if ($CreateInstaller) {
    Write-Host "Creating installer package..." -ForegroundColor Yellow
    
    # Create a simple ZIP package
    $zipPath = "DbAutoChat.Win-v1.0.zip"
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    Compress-Archive -Path "$OutputPath\*" -DestinationPath $zipPath
    Write-Host "Installer package created: $zipPath" -ForegroundColor Green
}

Write-Host "Release build process completed!" -ForegroundColor Green