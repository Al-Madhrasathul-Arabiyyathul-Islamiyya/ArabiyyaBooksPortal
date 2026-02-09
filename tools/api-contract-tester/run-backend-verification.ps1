param(
    [string]$HttpyacConfigPath = "tools/api-contract-tester/httpyac-config.json",
    [string]$InfrastructureProject = "BooksPortal/src/BooksPortal.Infrastructure/BooksPortal.Infrastructure.csproj",
    [string]$StartupProject = "BooksPortal/src/BooksPortal.API/BooksPortal.API.csproj",
    [string]$SolutionPath = "BooksPortal/BooksPortal.slnx",
    [switch]$SkipPreflight
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dotnetCliHome = Join-Path ([System.IO.Path]::GetTempPath()) "booksportal-dotnet-cli-home"
New-Item -Path $dotnetCliHome -ItemType Directory -Force | Out-Null
$env:DOTNET_CLI_HOME = $dotnetCliHome
$env:HOME = $dotnetCliHome
$env:USERPROFILE = $dotnetCliHome
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:NUGET_PACKAGES = Join-Path $dotnetCliHome ".nuget\packages"

function Stop-StaleBackendProcesses {
    param(
        [string]$ProjectPath,
        [string]$BaseUrl
    )

    $resolvedProjectPath = $null
    try {
        $resolvedProjectPath = (Resolve-Path -Path $ProjectPath).Path
    } catch {}

    $candidates = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue
    if (-not $candidates) {
        return
    }

    foreach ($process in $candidates) {
        $cmd = $process.CommandLine
        if ([string]::IsNullOrWhiteSpace($cmd)) {
            continue
        }

        $matchesProject = $false
        if ($resolvedProjectPath) {
            $matchesProject = $cmd.IndexOf($resolvedProjectPath, [StringComparison]::OrdinalIgnoreCase) -ge 0
        }

        $matchesUrl = $false
        if (-not [string]::IsNullOrWhiteSpace($BaseUrl)) {
            $matchesUrl = $cmd.IndexOf($BaseUrl, [StringComparison]::OrdinalIgnoreCase) -ge 0
        }

        if (-not $matchesProject -and -not $matchesUrl) {
            continue
        }

        try {
            Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
            Write-Host ("Stopped stale backend process PID={0}" -f $process.ProcessId)
        } catch {
            Write-Warning ("Failed to stop stale backend process PID={0}: {1}" -f $process.ProcessId, $_.Exception.Message)
        }
    }
}

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$Action
    )

    $stepStart = [DateTime]::UtcNow
    Write-Host ("[{0}] START {1}" -f $stepStart.ToString("o"), $Name)

    & $Action | Out-Host
    $exitCode = $LASTEXITCODE
    if ($null -eq $exitCode) {
        $exitCode = 0
    }

    $stepEnd = [DateTime]::UtcNow
    $status = if ($exitCode -eq 0) { "success" } else { "failed" }
    Write-Host ("[{0}] END {1} -> {2} (exit={3})" -f $stepEnd.ToString("o"), $Name, $status, $exitCode)

    return [ordered]@{
        name = $Name
        startedAtUtc = $stepStart.ToString("o")
        finishedAtUtc = $stepEnd.ToString("o")
        durationMs = [int][Math]::Round(($stepEnd - $stepStart).TotalMilliseconds)
        exitCode = [int]$exitCode
        status = $status
    }
}

$runId = [guid]::NewGuid().ToString()
$runStart = [DateTime]::UtcNow
$steps = New-Object System.Collections.Generic.List[object]
$overallExitCode = 0
$baseUrl = ""

try {
    if (-not (Test-Path $HttpyacConfigPath)) {
        throw "Config file not found: $HttpyacConfigPath"
    }
    if (-not (Test-Path $InfrastructureProject)) {
        throw "Infrastructure project not found: $InfrastructureProject"
    }
    if (-not (Test-Path $StartupProject)) {
        throw "Startup project not found: $StartupProject"
    }
    if (-not (Test-Path $SolutionPath)) {
        throw "Solution file not found: $SolutionPath"
    }

    $config = Get-Content $HttpyacConfigPath -Raw | ConvertFrom-Json
    $baseUrl = [string]$config.backend.baseUrl

    Stop-StaleBackendProcesses -ProjectPath $StartupProject -BaseUrl $baseUrl

    if (-not $SkipPreflight) {
        $steps.Add((Invoke-Step -Name "preflight_clean" -Action {
            & dotnet clean $SolutionPath
        }))
        if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "preflight_clean failed" }

        $steps.Add((Invoke-Step -Name "preflight_build" -Action {
            & dotnet build $SolutionPath
        }))
        if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "preflight_build failed" }
    }

    $steps.Add((Invoke-Step -Name "db_drop" -Action {
        & dotnet ef database drop --force --project $InfrastructureProject --startup-project $StartupProject
    }))
    if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "db_drop failed" }

    $steps.Add((Invoke-Step -Name "db_update" -Action {
        & dotnet ef database update --project $InfrastructureProject --startup-project $StartupProject
    }))
    if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "db_update failed" }

    $steps.Add((Invoke-Step -Name "httpyac_contract_suite" -Action {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File "tools/api-contract-tester/run-httpyac-contract-tests.ps1" -ConfigPath $HttpyacConfigPath
    }))
    if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "httpyac_contract_suite failed" }

    $steps.Add((Invoke-Step -Name "dotnet_test" -Action {
        & dotnet test $SolutionPath
    }))
    if ($steps[$steps.Count - 1].exitCode -ne 0) { $overallExitCode = 1; throw "dotnet_test failed" }
}
catch {
    if ($overallExitCode -eq 0) {
        $overallExitCode = 2
    }
    Write-Error $_.Exception.Message
}
finally {
    Stop-StaleBackendProcesses -ProjectPath $StartupProject -BaseUrl $baseUrl

    $logDir = "tools/api-contract-tester/logs"
    New-Item -Path $logDir -ItemType Directory -Force | Out-Null
    $logPath = Join-Path $logDir ("backend-verification-log-" + $runId + ".json")

    $runEnd = [DateTime]::UtcNow
    $log = [ordered]@{
        runId = $runId
        startedAtUtc = $runStart.ToString("o")
        finishedAtUtc = $runEnd.ToString("o")
        durationMs = [int][Math]::Round(($runEnd - $runStart).TotalMilliseconds)
        status = if ($overallExitCode -eq 0) { "success" } else { "failed" }
        exitCode = $overallExitCode
        inputs = [ordered]@{
            httpyacConfigPath = $HttpyacConfigPath
            infrastructureProject = $InfrastructureProject
            startupProject = $StartupProject
            solutionPath = $SolutionPath
        }
        steps = $steps
    }
    $log | ConvertTo-Json -Depth 10 | Set-Content -Path $logPath -Encoding UTF8

    Write-Host ("Verification log: {0}" -f $logPath)
    exit $overallExitCode
}
