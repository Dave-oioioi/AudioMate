$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$assetDir = Join-Path $root "src\AudioMate.App\Assets"
New-Item -ItemType Directory -Force $assetDir | Out-Null

Add-Type -AssemblyName System.Drawing

function New-RoundedRectPath {
    param(
        [float] $X,
        [float] $Y,
        [float] $Width,
        [float] $Height,
        [float] $Radius
    )

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = $Radius * 2
    [void]$path.AddArc($X, $Y, $diameter, $diameter, 180, 90)
    [void]$path.AddArc($X + $Width - $diameter, $Y, $diameter, $diameter, 270, 90)
    [void]$path.AddArc($X + $Width - $diameter, $Y + $Height - $diameter, $diameter, $diameter, 0, 90)
    [void]$path.AddArc($X, $Y + $Height - $diameter, $diameter, $diameter, 90, 90)
    [void]$path.CloseFigure()
    return $path
}

function New-IconPng {
    param(
        [int] $Size,
        [string] $OutputPath
    )

    $bitmap = [System.Drawing.Bitmap]::new($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $margin = [Math]::Max(1, [int]($Size * 0.07))
    $radius = $Size * 0.24
    $rect = [System.Drawing.RectangleF]::new($margin, $margin, $Size - ($margin * 2), $Size - ($margin * 2))
    $roundedPath = New-RoundedRectPath $rect.X $rect.Y $rect.Width $rect.Height $radius

    $background = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        $rect,
        [System.Drawing.Color]::FromArgb(255, 10, 24, 42),
        [System.Drawing.Color]::FromArgb(255, 0, 190, 214),
        [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal)
    $graphics.FillPath($background, $roundedPath)

    $highlight = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        $rect,
        [System.Drawing.Color]::FromArgb(72, 255, 255, 255),
        [System.Drawing.Color]::FromArgb(0, 255, 255, 255),
        [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
    $graphics.FillPath($highlight, $roundedPath)

    $softShadowPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(56, 0, 0, 0), [Math]::Max(2, $Size * 0.075))
    $softShadowPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $softShadowPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $softShadowPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $symbolPen = [System.Drawing.Pen]::new([System.Drawing.Color]::White, [Math]::Max(2, $Size * 0.07))
    $symbolPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $symbolPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $symbolPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $accentPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(245, 214, 255, 250), [Math]::Max(1, $Size * 0.046))
    $accentPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $accentPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round

    $arcRect = [System.Drawing.RectangleF]::new($Size * 0.27, $Size * 0.27, $Size * 0.46, $Size * 0.42)
    $shadowArcRect = [System.Drawing.RectangleF]::new($arcRect.X, $arcRect.Y + ($Size * 0.025), $arcRect.Width, $arcRect.Height)
    $graphics.DrawArc($softShadowPen, $shadowArcRect, 198, 144)
    $graphics.DrawArc($symbolPen, $arcRect, 198, 144)

    $graphics.DrawLine($softShadowPen, $Size * 0.29, $Size * 0.54, $Size * 0.29, $Size * 0.70)
    $graphics.DrawLine($softShadowPen, $Size * 0.71, $Size * 0.54, $Size * 0.71, $Size * 0.70)
    $graphics.DrawLine($symbolPen, $Size * 0.29, $Size * 0.52, $Size * 0.29, $Size * 0.68)
    $graphics.DrawLine($symbolPen, $Size * 0.71, $Size * 0.52, $Size * 0.71, $Size * 0.68)

    $pulse = [System.Drawing.Drawing2D.GraphicsPath]::new()
    [void]$pulse.AddBezier(
        $Size * 0.38, $Size * 0.58,
        $Size * 0.43, $Size * 0.49,
        $Size * 0.46, $Size * 0.68,
        $Size * 0.50, $Size * 0.58)
    [void]$pulse.AddBezier(
        $Size * 0.50, $Size * 0.58,
        $Size * 0.54, $Size * 0.48,
        $Size * 0.58, $Size * 0.67,
        $Size * 0.63, $Size * 0.56)
    $graphics.DrawPath($accentPen, $pulse)

    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    $pulse.Dispose()
    $accentPen.Dispose()
    $symbolPen.Dispose()
    $softShadowPen.Dispose()
    $highlight.Dispose()
    $background.Dispose()
    $roundedPath.Dispose()
    $graphics.Dispose()
    $bitmap.Dispose()
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$pngPaths = @()
foreach ($size in $sizes) {
    $path = Join-Path $assetDir "AudioMate-$size.png"
    New-IconPng -Size $size -OutputPath $path
    $pngPaths += $path
}

$icoPath = Join-Path $assetDir "AudioMate.ico"
$entries = foreach ($path in $pngPaths) {
    [pscustomobject]@{
        Path = $path
        Size = [int]([System.IO.Path]::GetFileNameWithoutExtension($path).Split('-')[-1])
        Bytes = [System.IO.File]::ReadAllBytes($path)
    }
}

$stream = [System.IO.File]::Create($icoPath)
$writer = [System.IO.BinaryWriter]::new($stream)
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$entries.Count)

$offset = 6 + ($entries.Count * 16)
foreach ($entry in $entries) {
    $writer.Write([byte]$(if ($entry.Size -eq 256) { 0 } else { $entry.Size }))
    $writer.Write([byte]$(if ($entry.Size -eq 256) { 0 } else { $entry.Size }))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$entry.Bytes.Length)
    $writer.Write([UInt32]$offset)
    $offset += $entry.Bytes.Length
}

foreach ($entry in $entries) {
    $writer.Write($entry.Bytes)
}

$writer.Dispose()
$stream.Dispose()
