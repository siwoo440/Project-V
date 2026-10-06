# UI theme post-processing. Run from anywhere:
#   powershell -ExecutionPolicy Bypass -File Tools\UIThemeGenerator\process_theme.ps1
# 1) unzips ui_theme_*.zip (this folder) into raw\
# 2) cuts the sheets listed in theme_spec.txt into pieces under Assets\ProjectV\Art
# 3) writes preview\*_preview.png and preview\report.txt for checking
# Comments stay ASCII because Windows PowerShell 5.1 reads scripts without BOM as ANSI.
param(
    [string]$OutRoot = "",
    [string]$RawDir = "",
    [string]$PreviewDir = ""
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = (Resolve-Path (Join-Path $here "..\..")).Path

if (-not $OutRoot) { $OutRoot = Join-Path $repo "Assets\ProjectV\Art" }
if (-not $RawDir) { $RawDir = Join-Path $here "raw" }
if (-not $PreviewDir) { $PreviewDir = Join-Path $here "preview" }

Add-Type -AssemblyName System.IO.Compression.FileSystem
New-Item -ItemType Directory -Force $RawDir | Out-Null

foreach ($zip in Get-ChildItem -Path $here -Filter "ui_theme_*.zip") {
    $archive = [System.IO.Compression.ZipFile]::OpenRead($zip.FullName)

    try {
        foreach ($entry in $archive.Entries) {
            if (-not $entry.Name) { continue }
            if ($entry.Name -notmatch "\.(png|jpg|jpeg)$") { continue }
            if ($entry.FullName -match "(^|/)split/") { continue }  # pieces pre-cut by the generator are not needed

            $target = Join-Path $RawDir $entry.Name
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $target, $true)
        }
    }
    finally {
        $archive.Dispose()
    }

    Write-Output ("UNZIP " + $zip.Name)
}

$source = ""

foreach ($file in Get-ChildItem -Path $here -Filter "Process*.cs" | Sort-Object Name) {
    $source += [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8) + "`n"
}

# Merge the partial files into one compilation unit: keep a single copy of each using line.
$usings = [regex]::Matches($source, "(?m)^using [^;]+;\s*$") | ForEach-Object { $_.Value.Trim() } | Sort-Object -Unique
$body = [regex]::Replace($source, "(?m)^using [^;]+;\s*$", "")
$code = ($usings -join "`n") + "`n" + $body

Add-Type -TypeDefinition $code -ReferencedAssemblies System.Drawing

$report = [ThemeProc]::Run($RawDir, (Join-Path $here "theme_spec.txt"), $OutRoot, $PreviewDir)
Write-Output $report
