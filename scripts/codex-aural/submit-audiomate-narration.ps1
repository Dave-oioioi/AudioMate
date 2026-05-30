param(
    [Parameter(Mandatory = $true)]
    [string] $Text,

    [string] $SourceName = "Codex",

    [string] $SourceId = ""
)

$ErrorActionPreference = "Stop"

function Write-FallbackText {
    param([string] $Value)

    $textPath = Join-Path $env:TEMP "codex_tts_text.txt"
    $responsePath = Join-Path $env:TEMP "codex_tts_response.txt"
    $Value | Set-Content $textPath -Encoding UTF8
    $Value | Set-Content $responsePath -Encoding UTF8
}

function Start-StandaloneAural {
    $scriptPath = Join-Path $env:USERPROFILE ".codex\skills\speech\scripts\tts_bg.ps1"
    if (Test-Path $scriptPath) {
        Start-Process powershell -WindowStyle Hidden -ArgumentList "-NoProfile -File `"$scriptPath`""
    }
}

$summary = ($Text -replace "\s+", " ").Trim()
if ($summary.Length -gt 100) {
    $summary = $summary.Substring(0, 99).TrimEnd() + "…"
}

$audioMate = Get-Process -Name "AudioMate.App" -ErrorAction SilentlyContinue
if ($null -eq $audioMate) {
    Write-FallbackText $summary
    Start-StandaloneAural
    exit 0
}

$queueDirectory = Join-Path $env:LOCALAPPDATA "AudioMate\NarrationQueue"
New-Item -ItemType Directory -Force $queueDirectory | Out-Null

$createdAt = [DateTimeOffset]::UtcNow
$id = [Guid]::NewGuid()
$request = [ordered]@{
    Id = $id
    Text = $summary
    SourceName = $SourceName
    SourceId = if ([string]::IsNullOrWhiteSpace($SourceId)) { $null } else { $SourceId }
    CreatedAt = $createdAt
}

$fileName = "{0}-{1}.json" -f $createdAt.ToString("yyyyMMddHHmmssfff"), $id.ToString("N")
$finalPath = Join-Path $queueDirectory $fileName
$tempPath = "$finalPath.tmp"

$request | ConvertTo-Json -Depth 4 | Set-Content $tempPath -Encoding UTF8
Move-Item -LiteralPath $tempPath -Destination $finalPath
