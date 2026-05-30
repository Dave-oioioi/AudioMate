param(
    [string] $Version = "0.1.0",
    [string] $Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$solution = Join-Path $root "AudioMate.sln"
$project = Join-Path $root "src\AudioMate.App\AudioMate.App.csproj"
$publishDir = Join-Path $root "artifacts\publish\AudioMate-$Version-$Runtime"
$installerScript = Join-Path $root "scripts\installer\AudioMate.iss"
$installerOut = Join-Path $root "artifacts\installer"

Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $publishDir | Out-Null
New-Item -ItemType Directory -Force $installerOut | Out-Null

dotnet test $solution --configuration Release
dotnet publish $project `
    --configuration Release `
    --runtime $Runtime `
    --self-contained false `
    --output $publishDir `
    /p:Version=$Version

$iscc = Get-Command iscc -ErrorAction SilentlyContinue
$isccPath = if ($iscc) {
    $iscc.Source
} else {
    @(
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"),
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if ($isccPath) {
    & $isccPath "/DMyAppVersion=$Version" "/O$installerOut" $installerScript
} else {
    Write-Host "Inno Setup compiler (iscc) was not found. Publish output is ready at $publishDir."
    Write-Host "Install Inno Setup or open scripts\installer\AudioMate.iss manually to build the installer."
}
