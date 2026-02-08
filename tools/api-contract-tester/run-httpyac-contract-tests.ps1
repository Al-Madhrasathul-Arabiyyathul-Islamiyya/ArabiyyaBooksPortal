param(
    [string]$ConfigPath = "tools/api-contract-tester/httpyac-config.json",
    [switch]$SkipStartBackend,
    [switch]$KeepBackendRunning
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Wait-ForApi {
    param(
        [string]$HealthUrl,
        [int]$TimeoutSec
    )

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $TimeoutSec) {
        try {
            $response = Invoke-WebRequest -Uri $HealthUrl -Method GET -TimeoutSec 5 -ErrorAction Stop
            if ([int]$response.StatusCode -eq 200) {
                return $true
            }
        } catch {}
        Start-Sleep -Milliseconds 1000
    }
    return $false
}

$backendProcess = $null
$runId = [guid]::NewGuid().ToString()
$startedAt = [DateTime]::UtcNow

try {
    if (-not (Test-Path $ConfigPath)) {
        throw "Config file not found: $ConfigPath"
    }

    $config = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $baseUrl = $config.backend.baseUrl.TrimEnd("/")
    $healthUrl = "$baseUrl$($config.backend.healthPath)"
    $suitePath = $config.suite.filePath

    if (-not (Test-Path $suitePath)) {
        throw "Suite file not found: $suitePath"
    }

    if (-not $SkipStartBackend) {
        $backendArgs = @(
            "run"
            "--project"
            [string]$config.backend.projectPath
            "--urls"
            [string]$config.backend.baseUrl
        )
        $backendProcess = Start-Process -FilePath "dotnet" -ArgumentList $backendArgs -PassThru -NoNewWindow
    }

    if (-not (Wait-ForApi -HealthUrl $healthUrl -TimeoutSec ([int]$config.backend.startupTimeoutSeconds))) {
        throw "Backend did not become healthy in time: $healthUrl"
    }

    $runSuffix = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
    $runKey = $runSuffix.Substring($runSuffix.Length - 4)
    $runYear = 3000 + [int]$runKey

    $httpyacArgs = @(
        "send"
        $suitePath
        "--all"
        "--json"
        "--output"
        "none"
        "--output-failed"
        "response"
        "--var"
        "baseUrl=$baseUrl"
        "--var"
        "adminEmail=$($config.auth.adminEmail)"
        "--var"
        "adminPassword=$($config.auth.adminPassword)"
        "--var"
        "runSuffix=$runSuffix"
        "--var"
        "runKey=$runKey"
        "--var"
        "runYear=$runYear"
    )

    $rawOutput = & httpyac @httpyacArgs
    $httpyacExitCode = $LASTEXITCODE
    $httpyacJson = $rawOutput -join [Environment]::NewLine
    $parsed = $httpyacJson | ConvertFrom-Json

    $entries = New-Object System.Collections.Generic.List[object]
    foreach ($request in $parsed.requests) {
        $status = "success"
        if ($request.summary.erroredTests -gt 0) {
            $status = "error"
        } elseif ($request.summary.failedTests -gt 0) {
            $status = "fail"
        } elseif ($request.summary.skippedTests -gt 0 -and $request.summary.successTests -eq 0) {
            $status = "skipped"
        }

        $entries.Add([ordered]@{
            request = $request.name
            status = $status
            totalTests = $request.summary.totalTests
            successTests = $request.summary.successTests
            failedTests = $request.summary.failedTests
            erroredTests = $request.summary.erroredTests
            skippedTests = $request.summary.skippedTests
        })
    }

    $outputDir = [string]$config.logging.outputDir
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
    $logPath = Join-Path $outputDir ("httpyac-contract-log-" + $runId + ".json")

    $structured = [ordered]@{
        runId = $runId
        startedAtUtc = $startedAt.ToString("o")
        finishedAtUtc = [DateTime]::UtcNow.ToString("o")
        suiteFile = $suitePath
        baseUrl = $baseUrl
        vars = [ordered]@{
            runSuffix = $runSuffix
            runKey = $runKey
            runYear = $runYear
        }
        httpyacExitCode = $httpyacExitCode
        summary = $parsed.summary
        requests = $entries
        raw = $parsed
    }

    $structured | ConvertTo-Json -Depth 20 | Set-Content -Path $logPath -Encoding UTF8

    Write-Host "httpyac contract suite finished."
    Write-Host "Log file: $logPath"
    Write-Host ("Summary: requests={0}, failed={1}, errored={2}" -f $parsed.summary.totalRequests, $parsed.summary.failedRequests, $parsed.summary.erroredRequests)

    if ($httpyacExitCode -ne 0) {
        exit $httpyacExitCode
    }
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 2
}
finally {
    if ($backendProcess -and -not $KeepBackendRunning) {
        try {
            if (-not $backendProcess.HasExited) {
                Stop-Process -Id $backendProcess.Id -Force
            }
        } catch {}
    }
}
