param(
    [string] $CurrentVersion = ""
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$installerDir = Join-Path $root "artifacts\installer"

if (-not (Test-Path $installerDir)) {
    throw "Installer directory not found: $installerDir"
}

$installers = Get-ChildItem -Path $installerDir -Filter "AudioMate-Setup-v*.exe" |
    ForEach-Object {
        if ($_.BaseName -match '^AudioMate-Setup-v(?<version>\d+\.\d+\.\d+)$') {
            [pscustomobject]@{
                File = $_
                Version = [version]$Matches.version
            }
        }
    } |
    Sort-Object -Property Version -Descending

if (-not $installers) {
    throw "No AudioMate installers were found in: $installerDir"
}

$current = if ([string]::IsNullOrWhiteSpace($CurrentVersion)) {
    $installers[0].Version
} else {
    [version]$CurrentVersion
}

$previous = $installers |
    Where-Object { $_.Version -lt $current } |
    Select-Object -First 1

if (-not $previous) {
    throw "No previous AudioMate installer was found before version $current."
}

Write-Host "Installing previous AudioMate version $($previous.Version): $($previous.File.FullName)"
Start-Process -FilePath $previous.File.FullName -Wait
