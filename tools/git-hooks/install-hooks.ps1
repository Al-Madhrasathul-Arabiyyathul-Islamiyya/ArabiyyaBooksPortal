param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$hooksPath = Join-Path $repoRoot ".githooks"

if (-not (Test-Path $hooksPath)) {
    throw "Hooks directory not found: $hooksPath"
}

$current = git config --local --get core.hooksPath 2>$null
if ($current -and -not $Force) {
    Write-Host "core.hooksPath is already set to '$current'. Use -Force to override."
    exit 0
}

git config --local core.hooksPath ".githooks"
Write-Host "Configured core.hooksPath=.githooks"
