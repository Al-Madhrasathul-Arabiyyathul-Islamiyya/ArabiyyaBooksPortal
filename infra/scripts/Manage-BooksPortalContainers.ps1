[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$Rebuild,
    [switch]$Frontend,
    [switch]$Backend,
    [switch]$All,
    [switch]$SkipBuild,
    [switch]$Clean,
    [switch]$Down,
    [switch]$Up,
    [switch]$Traefik,
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$EnvDirectory = "infra/env",
    [string[]]$ComposeFiles = @("infra/docker-compose.yml", "infra/docker-compose.local.yml")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ComposeFileArgs {
    param(
        [string]$Root,
        [string[]]$Files
    )

    $args = @()
    foreach ($file in $Files) {
        $fullPath = Join-Path $Root $file
        if (-not (Test-Path -LiteralPath $fullPath)) {
            throw "Compose file not found: $fullPath"
        }
        $args += @("-f", $fullPath)
    }

    return ,$args
}

function Import-EnvFile {
    param(
        [string]$FilePath
    )

    if (-not (Test-Path -LiteralPath $FilePath)) {
        return
    }

    Write-Host "Loading env vars from: $FilePath"
    foreach ($line in Get-Content -LiteralPath $FilePath) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        if ($trimmed.StartsWith("#")) { continue }

        $splitIndex = $trimmed.IndexOf("=")
        if ($splitIndex -lt 1) { continue }

        $name = $trimmed.Substring(0, $splitIndex).Trim()
        $value = $trimmed.Substring($splitIndex + 1)

        if ($value.Length -ge 2) {
            $startsWithQuote = $value.StartsWith('"') -or $value.StartsWith("'")
            $endsWithQuote = $value.EndsWith('"') -or $value.EndsWith("'")
            if ($startsWithQuote -and $endsWithQuote) {
                $value = $value.Substring(1, $value.Length - 2)
            }
        }

        [System.Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

function Invoke-Compose {
    param(
        [string[]]$ComposeArgs,
        [string[]]$CommandArgs
    )

    $display = "docker compose $($ComposeArgs -join ' ') $($CommandArgs -join ' ')"
    if (-not $PSCmdlet.ShouldProcess($display, "Run docker compose command")) {
        return
    }

    Write-Host ">> $display"
    & docker compose @ComposeArgs @CommandArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $display"
    }
}

function Get-Services {
    if ($All) {
        return @()
    }

    $services = @()
    if ($Backend) { $services += "api" }
    if ($Frontend) { $services += "frontend" }
    if ($Traefik) { $services += "traefik" }

    if ($services.Count -eq 0) {
        return @()
    }

    return $services
}

$root = (Resolve-Path $ProjectRoot).Path
$composeArgs = Get-ComposeFileArgs -Root $root -Files $ComposeFiles

$envRoot = Join-Path $root $EnvDirectory
$envBase = Join-Path $envRoot ".env"
$envLocal = Join-Path $envRoot ".env.local"

Import-EnvFile -FilePath $envBase
Import-EnvFile -FilePath $envLocal

$services = @(Get-Services)
$serviceLabel = if ($services.Count -eq 0) { "all services" } else { ($services -join ", ") }

if (-not ($Rebuild -or $Clean -or $Down -or $Up)) {
    $Up = $true
}

Write-Host "Target: $serviceLabel"

if ($Clean) {
    # Remove project containers/networks/images (local) and orphans.
    Invoke-Compose -ComposeArgs $composeArgs -CommandArgs @("down", "--remove-orphans", "--rmi", "local")
}
elseif ($Down) {
    $downArgs = @("down", "--remove-orphans")
    if ($services.Count -gt 0) {
        Write-Warning "docker compose down does not support targeting specific services; bringing full stack down."
    }
    Invoke-Compose -ComposeArgs $composeArgs -CommandArgs $downArgs
}

if ($Rebuild -and -not $SkipBuild) {
    $buildArgs = @("build")
    if ($services.Count -gt 0) {
        $buildArgs += $services
    }
    Invoke-Compose -ComposeArgs $composeArgs -CommandArgs $buildArgs
}
elseif ($Rebuild -and $SkipBuild) {
    Write-Warning "Rebuild requested with SkipBuild; skipping image build."
}

if ($Up) {
    $upArgs = @("up", "-d")
    if ($services.Count -gt 0) {
        $upArgs += $services
    }
    Invoke-Compose -ComposeArgs $composeArgs -CommandArgs $upArgs
}

Write-Host "Done."
