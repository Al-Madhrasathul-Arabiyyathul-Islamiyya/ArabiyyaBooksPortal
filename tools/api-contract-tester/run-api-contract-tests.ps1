param(
    [string]$ConfigPath = "tools/api-contract-tester/api-contract-config.json",
    [switch]$SkipStartBackend,
    [switch]$KeepBackendRunning
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function ConvertTo-HashtableRecursive {
    param([object]$InputObject)

    if ($null -eq $InputObject) {
        return $null
    }

    if ($InputObject -is [System.Collections.IDictionary]) {
        $ht = @{}
        foreach ($k in $InputObject.Keys) {
            $ht[$k] = ConvertTo-HashtableRecursive -InputObject $InputObject[$k]
        }
        return $ht
    }

    if ($InputObject -is [System.Collections.IEnumerable] -and -not ($InputObject -is [string])) {
        $list = New-Object System.Collections.ArrayList
        foreach ($item in $InputObject) {
            [void]$list.Add((ConvertTo-HashtableRecursive -InputObject $item))
        }
        return ,$list.ToArray()
    }

    if ($InputObject -is [pscustomobject]) {
        $ht = @{}
        foreach ($prop in $InputObject.PSObject.Properties) {
            $ht[$prop.Name] = ConvertTo-HashtableRecursive -InputObject $prop.Value
        }
        return $ht
    }

    return $InputObject
}

function New-RunContext {
    return [ordered]@{
        RunId = [guid]::NewGuid().ToString()
        StartedAtUtc = [DateTime]::UtcNow
        Entries = New-Object System.Collections.Generic.List[object]
        Warnings = New-Object System.Collections.Generic.List[string]
        Errors = New-Object System.Collections.Generic.List[string]
    }
}

function Add-LogEntry {
    param(
        [hashtable]$Ctx,
        [string]$Category,
        [string]$Method,
        [string]$PathTemplate,
        [string]$ResolvedPath,
        [int]$StatusCode,
        [string]$Outcome,
        [string]$Message,
        [object]$Details = $null
    )

    $Ctx.Entries.Add([ordered]@{
        timestampUtc = [DateTime]::UtcNow.ToString("o")
        category = $Category
        method = $Method
        pathTemplate = $PathTemplate
        resolvedPath = $ResolvedPath
        statusCode = $StatusCode
        outcome = $Outcome
        message = $Message
        details = $Details
    })
}

function Normalize-Template {
    param([string]$Path)
    if ([string]::IsNullOrWhiteSpace($Path)) { return $Path }
    $p = $Path -replace "\{([^}:]+):[^}]+\}", '{$1}'
    return $p.Trim().ToLowerInvariant()
}

function Replace-PathPlaceholders {
    param(
        [string]$PathTemplate,
        [hashtable]$Placeholders
    )

    return ([regex]::Replace($PathTemplate, "\{([^}]+)\}", {
        param($match)
        $token = $match.Groups[1].Value
        $name = ($token -split ":")[0]
        if ($Placeholders.ContainsKey($name)) {
            return [string]$Placeholders[$name]
        }
        return "1"
    }))
}

function Get-DocumentedEndpoints {
    param([string]$DocPath)

    $lines = Get-Content -Path $DocPath
    $endpoints = New-Object System.Collections.Generic.List[object]

    $headingIndexes = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match "^### (GET|POST|PUT|DELETE|PATCH)\s+(.+)$") {
            $headingIndexes += $i
        }
    }

    for ($h = 0; $h -lt $headingIndexes.Count; $h++) {
        $start = $headingIndexes[$h]
        $end = if ($h -lt $headingIndexes.Count - 1) { $headingIndexes[$h + 1] - 1 } else { $lines.Count - 1 }

        $line = $lines[$start]
        if ($line -notmatch "^### (GET|POST|PUT|DELETE|PATCH)\s+(.+)$") { continue }

        $method = $matches[1].Trim().ToUpperInvariant()
        $path = $matches[2].Trim()
        $authLine = $null

        for ($j = $start; $j -le $end; $j++) {
            if ($lines[$j] -match "^\- \*\*Auth\*\*:\s*(.+)$") {
                $authLine = $matches[1].Trim()
                break
            }
        }

        $requiresAuth = $true
        if ($authLine -and $authLine.ToLowerInvariant().Contains("none")) {
            $requiresAuth = $false
        }

        $endpoints.Add([ordered]@{
            method = $method
            path = $path
            normalizedPath = (Normalize-Template -Path $path)
            requiresAuth = $requiresAuth
        })
    }

    return $endpoints
}

function Invoke-JsonRequest {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Token,
        [object]$Body = $null,
        [int]$TimeoutSec = 30
    )

    $headers = @{ "Accept" = "application/json" }
    if (-not [string]::IsNullOrWhiteSpace($Token)) {
        $headers["Authorization"] = "Bearer $Token"
    }

    $params = @{
        Uri = $Url
        Method = $Method
        Headers = $headers
        TimeoutSec = $TimeoutSec
        ErrorAction = "Stop"
    }

    if ($null -ne $Body) {
        $params["ContentType"] = "application/json"
        $params["Body"] = ($Body | ConvertTo-Json -Depth 12)
    }

    $supportsSkipHttpErrorCheck = (Get-Command Invoke-WebRequest).Parameters.ContainsKey("SkipHttpErrorCheck")
    if ($supportsSkipHttpErrorCheck) {
        $params["SkipHttpErrorCheck"] = $true
    }

    try {
        $response = Invoke-WebRequest @params
        $contentType = ""
        if ($response.Headers -and $response.Headers["Content-Type"]) {
            $contentType = [string]$response.Headers["Content-Type"]
        }

        $parsed = $null
        if ($response.Content -and $contentType -like "*application/json*") {
            try { $parsed = $response.Content | ConvertFrom-Json } catch {}
        }

        return [ordered]@{
            statusCode = [int]$response.StatusCode
            contentType = $contentType
            body = $parsed
            rawBody = [string]$response.Content
        }
    } catch {
        $webEx = $_.Exception
        if ($webEx.Response) {
            $resp = $webEx.Response

            # PowerShell 7: HttpResponseMessage
            if ($resp -is [System.Net.Http.HttpResponseMessage]) {
                $raw = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                $ct = ""
                if ($resp.Content -and $resp.Content.Headers -and $resp.Content.Headers.ContentType) {
                    $ct = [string]$resp.Content.Headers.ContentType
                }
                $parsed = $null
                if ($raw -and $ct -like "*application/json*") {
                    try { $parsed = $raw | ConvertFrom-Json } catch {}
                }
                return [ordered]@{
                    statusCode = [int]$resp.StatusCode
                    contentType = $ct
                    body = $parsed
                    rawBody = $raw
                }
            }

            # Windows PowerShell: HttpWebResponse
            if ($resp.GetType().GetMethod("GetResponseStream")) {
                $stream = $resp.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $raw = $reader.ReadToEnd()
                $parsed = $null
                try { $parsed = $raw | ConvertFrom-Json } catch {}
                return [ordered]@{
                    statusCode = [int]$resp.StatusCode
                    contentType = [string]$resp.ContentType
                    body = $parsed
                    rawBody = $raw
                }
            }
        }
        throw
    }
}

function Assert-ApiWrapper {
    param([object]$Body)
    if ($null -eq $Body) { return $false }
    $props = $Body.PSObject.Properties.Name
    return ($props -contains "success") -and ($props -contains "data")
}

function Get-SwaggerEndpoints {
    param(
        [string]$BaseUrl,
        [int]$TimeoutSec
    )

    $swaggerUrl = "$BaseUrl/swagger/v1/swagger.json"
    $json = Invoke-JsonRequest -Method "GET" -Url $swaggerUrl -Token "" -TimeoutSec $TimeoutSec
    if ($json.statusCode -ne 200 -or $null -eq $json.body) {
        throw "Failed to load swagger: $swaggerUrl"
    }

    $result = New-Object System.Collections.Generic.List[object]
    foreach ($pathProp in $json.body.paths.PSObject.Properties) {
        $path = [string]$pathProp.Name
        foreach ($methodProp in $pathProp.Value.PSObject.Properties) {
            $method = [string]$methodProp.Name
            $result.Add([ordered]@{
                method = $method.ToUpperInvariant()
                path = $path
                normalizedPath = (Normalize-Template -Path $path)
            })
        }
    }

    return $result
}

function Wait-ForApi {
    param(
        [string]$HealthUrl,
        [int]$TimeoutSec,
        [int]$RequestTimeoutSec
    )

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $TimeoutSec) {
        try {
            $res = Invoke-JsonRequest -Method "GET" -Url $HealthUrl -Token "" -TimeoutSec $RequestTimeoutSec
            if ($res.statusCode -eq 200) {
                return $true
            }
        } catch {}
        Start-Sleep -Milliseconds 1000
    }
    return $false
}

function New-DefaultBody {
    param(
        [string]$Method,
        [string]$PathTemplate,
        [hashtable]$Config,
        [hashtable]$RuntimeState
    )

    $key = "$Method $PathTemplate"
    if ($Config.requestTemplates.ContainsKey($key)) {
        return $Config.requestTemplates[$key]
    }

    if ($key -eq "POST /api/auth/refresh" -and $RuntimeState.ContainsKey("authTokens")) {
        return [ordered]@{
            accessToken = $RuntimeState.authTokens.accessToken
            refreshToken = $RuntimeState.authTokens.refreshToken
        }
    }
    if ($key -eq "POST /api/auth/login" -and $RuntimeState.ContainsKey("superAdminCredentials")) {
        return [ordered]@{
            email = $RuntimeState.superAdminCredentials.email
            password = $RuntimeState.superAdminCredentials.password
        }
    }

    if ($Method -in @("POST", "PUT", "PATCH")) {
        return [ordered]@{}
    }

    return $null
}

function Try-LoginSuperAdmin {
    param(
        [hashtable]$Config,
        [string]$BaseUrl
    )

    $email = [string]$Config.auth.superAdmin.email
    $candidates = New-Object System.Collections.Generic.List[string]

    if ($Config.auth.superAdmin.ContainsKey("passwordCandidates")) {
        foreach ($p in $Config.auth.superAdmin.passwordCandidates) {
            if (-not [string]::IsNullOrWhiteSpace([string]$p)) {
                $candidates.Add([string]$p)
            }
        }
    }

    if ($Config.auth.superAdmin.ContainsKey("password") -and -not [string]::IsNullOrWhiteSpace([string]$Config.auth.superAdmin.password)) {
        $p = [string]$Config.auth.superAdmin.password
        if (-not $candidates.Contains($p)) {
            $candidates.Insert(0, $p)
        }
    }

    if ($candidates.Count -eq 0) {
        throw "No superAdmin password configured. Set auth.superAdmin.password or passwordCandidates."
    }

    foreach ($pw in $candidates) {
        $attempt = Invoke-JsonRequest `
            -Method "POST" `
            -Url "$BaseUrl/api/auth/login" `
            -Token "" `
            -Body ([ordered]@{
                email = $email
                password = $pw
            }) `
            -TimeoutSec 30

        if ($attempt.statusCode -eq 200 -and $null -ne $attempt.body -and $attempt.body.success) {
            return [ordered]@{
                login = $attempt
                passwordUsed = $pw
            }
        }
    }

    throw "SuperAdmin login failed for all configured password candidates."
}

$ctx = New-RunContext
$backendProcess = $null
$stdoutFile = $null
$stderrFile = $null

try {
    if (-not (Test-Path $ConfigPath)) {
        throw "Config file not found: $ConfigPath"
    }

    $rawConfig = Get-Content $ConfigPath -Raw | ConvertFrom-Json
    $config = ConvertTo-HashtableRecursive -InputObject $rawConfig
    $baseUrl = $config.backend.baseUrl.TrimEnd("/")
    $healthUrl = "$baseUrl$($config.backend.healthPath)"
    $docPath = $config.documentation.path

    if (-not (Test-Path $docPath)) {
        throw "API reference not found: $docPath"
    }

    if (-not $SkipStartBackend) {
        $stdoutFile = Join-Path $env:TEMP "api-contract-run-$($ctx.RunId)-stdout.log"
        $stderrFile = Join-Path $env:TEMP "api-contract-run-$($ctx.RunId)-stderr.log"
        $args = @(
            "run"
            "--project"
            $config.backend.projectPath
            "--urls"
            $config.backend.baseUrl
        )

        $backendProcess = Start-Process -FilePath "dotnet" -ArgumentList $args -PassThru -NoNewWindow -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile
    }

    if (-not (Wait-ForApi -HealthUrl $healthUrl -TimeoutSec $config.backend.startupTimeoutSeconds -RequestTimeoutSec $config.test.requestTimeoutSeconds)) {
        throw "Backend did not become healthy in time: $healthUrl"
    }

    $runtime = @{
        authAvailable = $false
    }

    try {
        $loginResult = Try-LoginSuperAdmin -Config $config -BaseUrl $baseUrl
        $authLogin = $loginResult.login
        $runtime.authAvailable = $true
        $runtime.authTokens = $authLogin.body.data
        $runtime.superAdminCredentials = @{
            email = [string]$config.auth.superAdmin.email
            password = [string]$loginResult.passwordUsed
        }
    }
    catch {
        $ctx.Warnings.Add("SuperAdmin login failed; continuing in unauthenticated mode.")
    }

    $documented = Get-DocumentedEndpoints -DocPath $docPath
    $swagger = Get-SwaggerEndpoints -BaseUrl $baseUrl -TimeoutSec $config.test.requestTimeoutSeconds

    $docSet = @{}
    foreach ($d in $documented) { $docSet["$($d.method) $($d.normalizedPath)"] = $true }
    $swaggerSet = @{}
    foreach ($s in $swagger) { $swaggerSet["$($s.method) $($s.normalizedPath)"] = $true }

    foreach ($k in $docSet.Keys) {
        if (-not $swaggerSet.ContainsKey($k)) {
            Add-LogEntry -Ctx $ctx -Category "doc-vs-swagger" -Method ($k.Split(" ")[0]) -PathTemplate ($k.Substring($k.IndexOf(" ") + 1)) -ResolvedPath "" -StatusCode 0 -Outcome "fail" -Message "Documented endpoint not found in Swagger"
        }
    }
    foreach ($k in $swaggerSet.Keys) {
        if (-not $docSet.ContainsKey($k)) {
            Add-LogEntry -Ctx $ctx -Category "doc-vs-swagger" -Method ($k.Split(" ")[0]) -PathTemplate ($k.Substring($k.IndexOf(" ") + 1)) -ResolvedPath "" -StatusCode 0 -Outcome "warn" -Message "Swagger endpoint not documented"
        }
    }

    $sweep = $documented | Sort-Object @{ Expression = { if ($_.path -eq "/api/auth/logout") { 1 } else { 0 } } }, method, path
    foreach ($ep in $sweep) {
        if ($config.test.skipDisruptiveEndpoints -and $ep.path -in $config.test.disruptiveEndpoints) {
            Add-LogEntry -Ctx $ctx -Category "route-sweep" -Method $ep.method -PathTemplate $ep.path -ResolvedPath "" -StatusCode 0 -Outcome "skipped" -Message "Skipped disruptive endpoint by config."
            continue
        }

        $resolvedPath = Replace-PathPlaceholders -PathTemplate $ep.path -Placeholders $config.placeholders
        $url = "$baseUrl$resolvedPath"
        $body = New-DefaultBody -Method $ep.method -PathTemplate $ep.path -Config $config -RuntimeState $runtime
        $token = ""
        if ($ep.requiresAuth -and $runtime.authAvailable) {
            $token = [string]$runtime.authTokens.accessToken
        }

        $response = $null
        try {
            $response = Invoke-JsonRequest -Method $ep.method -Url $url -Token $token -Body $body -TimeoutSec $config.test.requestTimeoutSeconds
        }
        catch {
            Add-LogEntry -Ctx $ctx -Category "route-sweep" -Method $ep.method -PathTemplate $ep.path -ResolvedPath $resolvedPath -StatusCode 0 -Outcome "fail" -Message ("Unhandled request exception: " + $_.Exception.Message)
            continue
        }

        $status = [int]$response.statusCode
        $hasPlaceholder = $ep.path -match "\{[^}]+\}"
        $isJson = $response.contentType -like "*application/json*"

        $outcome = "success"
        $message = "Endpoint reachable."

        if ($status -ge 500) {
            $outcome = "fail"
            $message = "Server error."
        }
        elseif ($status -eq 405) {
            $outcome = "fail"
            $message = "Method not allowed."
        }
        elseif ($status -eq 404) {
            if ($hasPlaceholder) {
                $outcome = "warn"
                $message = "Not found with placeholder sample values."
            } else {
                $outcome = "fail"
                $message = "Route not found."
            }
        }
        elseif ($ep.requiresAuth -and $status -in @(401, 403)) {
            if ($runtime.authAvailable) {
                $outcome = "fail"
                $message = "Auth failed for authenticated endpoint."
            } else {
                $outcome = "success"
                $message = "Auth enforcement confirmed (unauthenticated mode)."
            }
        }
        elseif ($status -in @(400, 409, 422)) {
            $outcome = "warn"
            $message = "Validation/business-rule rejection."
        }
        elseif ($isJson -and $status -ge 200 -and $status -lt 300 -and -not (Assert-ApiWrapper -Body $response.body)) {
            $outcome = "fail"
            $message = "JSON response does not match ApiResponse wrapper."
        }

        Add-LogEntry -Ctx $ctx -Category "route-sweep" -Method $ep.method -PathTemplate $ep.path -ResolvedPath $resolvedPath -StatusCode $status -Outcome $outcome -Message $message -Details @{
            requiresAuth = $ep.requiresAuth
            responseContentType = $response.contentType
        }
    }

    $summary = [ordered]@{
        runId = $ctx.RunId
        startedAtUtc = $ctx.StartedAtUtc.ToString("o")
        finishedAtUtc = [DateTime]::UtcNow.ToString("o")
        baseUrl = $baseUrl
        documentationPath = $docPath
        totals = [ordered]@{
            all = $ctx.Entries.Count
            success = @($ctx.Entries | Where-Object { $_.outcome -eq "success" }).Count
            warn = @($ctx.Entries | Where-Object { $_.outcome -eq "warn" }).Count
            fail = @($ctx.Entries | Where-Object { $_.outcome -eq "fail" }).Count
            skipped = @($ctx.Entries | Where-Object { $_.outcome -eq "skipped" }).Count
        }
        entries = $ctx.Entries
        warnings = $ctx.Warnings
    }

    $outputDir = $config.logging.outputDir
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
    $outPath = Join-Path $outputDir ("api-contract-log-" + $ctx.RunId + ".json")
    $summary | ConvertTo-Json -Depth 20 | Set-Content -Path $outPath -Encoding UTF8

    Write-Host "API contract sweep finished."
    Write-Host "Log file: $outPath"
    Write-Host "Summary: success=$($summary.totals.success), warn=$($summary.totals.warn), fail=$($summary.totals.fail), skipped=$($summary.totals.skipped)"

    if ($summary.totals.fail -gt 0) {
        exit 1
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
