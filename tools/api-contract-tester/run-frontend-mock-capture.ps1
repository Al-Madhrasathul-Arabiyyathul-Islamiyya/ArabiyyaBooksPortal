param(
    [string]$HttpyacConfigPath = "tools/api-contract-tester/httpyac-config.json",
    [string]$SeedFixturePath = "tools/api-contract-tester/fixtures/mock-capture-seed.json",
    [string]$OutputDir = "BooksPortalFrontEnd/app/mocks/api-capture",
    [string]$InfrastructureProject = "BooksPortal/src/BooksPortal.Infrastructure/BooksPortal.Infrastructure.csproj",
    [string]$StartupProject = "BooksPortal/src/BooksPortal.API/BooksPortal.API.csproj",
    [switch]$SkipDatabaseReset,
    [switch]$SkipStartBackend,
    [switch]$RestartBackend,
    [switch]$SkipContractSuite,
    [switch]$KeepBackendRunning,
    [switch]$ForceStopOnExit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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

function Resolve-TemplateValue {
    param(
        [string]$Value,
        [hashtable]$Vars
    )

    if ($Value -match '^\{\{([a-zA-Z0-9_]+):int\}\}$') {
        $key = $matches[1]
        if (-not $Vars.ContainsKey($key)) {
            throw "Template variable '$key' not found."
        }
        return [int]$Vars[$key]
    }

    $resolved = [regex]::Replace($Value, '\{\{([a-zA-Z0-9_]+)\}\}', {
        param($m)
        $key = $m.Groups[1].Value
        if (-not $Vars.ContainsKey($key)) {
            throw "Template variable '$key' not found."
        }
        return [string]$Vars[$key]
    })

    return $resolved
}

function Resolve-TemplateObject {
    param(
        [object]$Value,
        [hashtable]$Vars
    )

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [string]) {
        return Resolve-TemplateValue -Value $Value -Vars $Vars
    }

    if ($Value -is [System.Collections.IDictionary]) {
        $result = [ordered]@{}
        foreach ($key in $Value.Keys) {
            $result[$key] = Resolve-TemplateObject -Value $Value[$key] -Vars $Vars
        }
        return $result
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        $list = New-Object System.Collections.ArrayList
        foreach ($item in $Value) {
            [void]$list.Add((Resolve-TemplateObject -Value $item -Vars $Vars))
        }
        return @($list.ToArray())
    }

    return $Value
}

function Get-ResolvedFixture {
    param(
        [hashtable]$Fixtures,
        [string]$Name,
        [hashtable]$Vars
    )

    if (-not $Fixtures.ContainsKey($Name)) {
        throw "Fixture '$Name' not found."
    }

    return Resolve-TemplateObject -Value $Fixtures[$Name] -Vars $Vars
}

function Ensure-ArrayField {
    param(
        [System.Collections.IDictionary]$Payload,
        [string]$FieldName
    )

    if (-not $Payload.Contains($FieldName)) {
        return
    }

    $value = $Payload[$FieldName]
    if ($null -eq $value) {
        return
    }

    if ($value -is [string]) {
        $Payload[$FieldName] = @($value)
        return
    }

    if ($value -is [System.Array]) {
        return
    }

    if ($value -is [System.Collections.IEnumerable] -and -not ($value -is [System.Collections.IDictionary])) {
        $Payload[$FieldName] = @($value)
        return
    }

    $Payload[$FieldName] = @($value)
}

function Reset-DevelopmentDatabase {
    param(
        [string]$InfrastructureProjectPath,
        [string]$StartupProjectPath
    )

    & dotnet ef database drop --force --project $InfrastructureProjectPath --startup-project $StartupProjectPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef database drop failed (exit=$LASTEXITCODE)."
    }

    & dotnet ef database update --project $InfrastructureProjectPath --startup-project $StartupProjectPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef database update failed (exit=$LASTEXITCODE)."
    }
}

function Invoke-CaptureRequest {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers,
        [object]$Body = $null
    )

    $jsonBody = $null
    if ($null -ne $Body) {
        if ($Body -is [string]) {
            $jsonBody = $Body
        } else {
            $jsonBody = ($Body | ConvertTo-Json -Depth 20)
        }
    }

    try {
        $resp = Invoke-WebRequest -Uri $Uri -Method $Method -Headers $Headers -ContentType "application/json" -Body $jsonBody -ErrorAction Stop
        $raw = $resp.Content
        $parsed = $null
        try { $parsed = $raw | ConvertFrom-Json -Depth 30 } catch {}

        return [ordered]@{
            name = $Name
            method = $Method
            uri = $Uri
            statusCode = [int]$resp.StatusCode
            isError = $false
            capturedAtUtc = [DateTime]::UtcNow.ToString("o")
            body = $parsed
            rawBody = $raw
            requestBody = $jsonBody
        }
    } catch {
        $statusCode = 0
        $raw = ""
        if ($_.ErrorDetails -and $_.ErrorDetails.PSObject.Properties.Name -contains "Message") {
            $raw = [string]$_.ErrorDetails.Message
        }
        $hasResponse = $false
        if ($_.Exception -and $_.Exception.PSObject.Properties.Name -contains "Response") {
            $hasResponse = $null -ne $_.Exception.Response
        }

        if ($hasResponse) {
            try {
                $statusCode = [int]$_.Exception.Response.StatusCode
            } catch {}
            if ([string]::IsNullOrWhiteSpace($raw)) {
                try {
                    $raw = $_.Exception.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                } catch {}
            }
        }

        $parsed = $null
        if (-not [string]::IsNullOrWhiteSpace($raw)) {
            try { $parsed = $raw | ConvertFrom-Json -Depth 30 } catch {}
        }

        return [ordered]@{
            name = $Name
            method = $Method
            uri = $Uri
            statusCode = $statusCode
            isError = $true
            capturedAtUtc = [DateTime]::UtcNow.ToString("o")
            body = $parsed
            rawBody = $raw
            requestBody = $jsonBody
        }
    }
}

function Write-Capture {
    param(
        [string]$OutputDirPath,
        [hashtable]$Capture
    )

    $safe = $Capture.name.ToLowerInvariant().Replace(" ", "-").Replace("/", "-")
    $path = Join-Path $OutputDirPath ("{0}.json" -f $safe)
    ($Capture | ConvertTo-Json -Depth 40) | Set-Content -Path $path -Encoding UTF8
    return $path
}

function Find-FirstBy {
    param(
        [object[]]$Items,
        [scriptblock]$Predicate
    )

    if (-not $Items) { return $null }
    foreach ($item in $Items) {
        if (& $Predicate $item) {
            return $item
        }
    }
    return $null
}

function Assert-CaptureSuccess {
    param(
        [hashtable]$Capture,
        [string]$Name
    )

    if (-not $Capture -or $Capture.isError -or $Capture.statusCode -lt 200 -or $Capture.statusCode -ge 300) {
        $status = if ($Capture) { [string]$Capture.statusCode } else { "n/a" }
        $details = ""
        if ($Capture) {
            if (-not [string]::IsNullOrWhiteSpace([string]$Capture.rawBody)) {
                $details = [string]$Capture.rawBody
            } elseif ($Capture.body) {
                $details = ($Capture.body | ConvertTo-Json -Depth 20)
            }
        }

        $requestBodyDetails = ""
        if ($Capture -and -not [string]::IsNullOrWhiteSpace([string]$Capture.requestBody)) {
            $requestBodyDetails = [string]$Capture.requestBody
        }

        if ([string]::IsNullOrWhiteSpace($details) -and [string]::IsNullOrWhiteSpace($requestBodyDetails)) {
            throw "Capture request '$Name' failed with status $status."
        }

        if ([string]::IsNullOrWhiteSpace($requestBodyDetails)) {
            throw ("Capture request '{0}' failed with status {1}. Response: {2}" -f $Name, $status, $details)
        }

        if ([string]::IsNullOrWhiteSpace($details)) {
            throw ("Capture request '{0}' failed with status {1}. RequestBody: {2}" -f $Name, $status, $requestBodyDetails)
        }

        throw ("Capture request '{0}' failed with status {1}. RequestBody: {2} Response: {3}" -f $Name, $status, $requestBodyDetails, $details)
    }
}

$backendProcess = $null
$runId = [guid]::NewGuid().ToString()
$runStart = [DateTime]::UtcNow
$captures = New-Object System.Collections.Generic.List[object]
$baseUrl = ""
$startupProjectPath = $StartupProject

try {
    if (-not (Test-Path $HttpyacConfigPath)) {
        throw "Config file not found: $HttpyacConfigPath"
    }
    if (-not (Test-Path $SeedFixturePath)) {
        throw "Seed fixture file not found: $SeedFixturePath"
    }
    if (-not (Test-Path $InfrastructureProject)) {
        throw "Infrastructure project not found: $InfrastructureProject"
    }
    if (-not (Test-Path $StartupProject)) {
        throw "Startup project not found: $StartupProject"
    }

    $config = Get-Content -Raw $HttpyacConfigPath | ConvertFrom-Json
    $baseUrl = [string]$config.backend.baseUrl
    $startupProjectFromConfig = [string]$config.backend.projectPath
    if (-not [string]::IsNullOrWhiteSpace($startupProjectFromConfig)) {
        $startupProjectPath = $startupProjectFromConfig
    }
    if (-not (Test-Path $startupProjectPath)) {
        throw "Startup project not found: $startupProjectPath"
    }
    $healthUrl = "{0}{1}" -f $baseUrl.TrimEnd("/"), [string]$config.backend.healthPath
    $startupTimeout = [int]$config.backend.startupTimeoutSeconds
    $adminEmail = [string]$config.auth.adminEmail
    $adminPassword = [string]$config.auth.adminPassword
    $fixtures = Get-Content -Raw $SeedFixturePath | ConvertFrom-Json -AsHashtable

    if ($RestartBackend -and $SkipStartBackend) {
        throw "RestartBackend and SkipStartBackend cannot be used together."
    }

    if ($RestartBackend -and $KeepBackendRunning) {
        throw "RestartBackend and KeepBackendRunning cannot be used together."
    }

    if (-not $SkipDatabaseReset) {
        Stop-StaleBackendProcesses -ProjectPath $startupProjectPath -BaseUrl $baseUrl
        Reset-DevelopmentDatabase -InfrastructureProjectPath $InfrastructureProject -StartupProjectPath $startupProjectPath
    }

    if ($RestartBackend -or -not $SkipStartBackend) {
        Stop-StaleBackendProcesses -ProjectPath $startupProjectPath -BaseUrl $baseUrl
        $backendProcess = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", $startupProjectPath, "--urls", $baseUrl) -PassThru -NoNewWindow
    }

    if (-not (Wait-ForApi -HealthUrl $healthUrl -TimeoutSec $startupTimeout)) {
        throw "Backend did not become healthy in time: $healthUrl"
    }

    if (-not $SkipContractSuite) {
        & pwsh -NoProfile -ExecutionPolicy Bypass -File "tools/api-contract-tester/run-httpyac-contract-tests.ps1" -ConfigPath $HttpyacConfigPath -SkipStartBackend
        if ($LASTEXITCODE -ne 0) {
            throw "httpyac contract suite failed (exit=$LASTEXITCODE). Cannot produce reliable frontend mock capture."
        }
    }

    $logFile = Get-ChildItem -Path "tools/api-contract-tester/logs" -Filter "httpyac-contract-log-*.json" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    $lastLog = $null
    if ($logFile) {
        $lastLog = Get-Content -Raw $logFile.FullName | ConvertFrom-Json
        if (-not $SkipContractSuite -and ($lastLog.summary.failedRequests -gt 0 -or $lastLog.summary.erroredRequests -gt 0)) {
            throw "Latest httpyac contract log is not clean (failed=$($lastLog.summary.failedRequests), errored=$($lastLog.summary.erroredRequests))."
        }
    }

    $runSuffix = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
    $runKey = $runSuffix.Substring($runSuffix.Length - 4)
    $runYear = 2001 + ([int]$runKey % 7000)
    $tempUserPassword = "Pass@12345"
    $templateVars = @{
        runSuffix = $runSuffix
        runKey = $runKey
        runYear = $runYear
        tempUserPassword = $tempUserPassword
    }

    $loginCapture = Invoke-CaptureRequest -Name "auth-login-admin" -Method "POST" -Uri "$baseUrl/api/auth/login" -Headers @{} -Body @{
        email = $adminEmail
        password = $adminPassword
    }
    $captures.Add($loginCapture)
    if ($loginCapture.statusCode -ne 200 -or -not $loginCapture.body -or -not $loginCapture.body.data.accessToken) {
        throw "Admin login failed during mock capture."
    }

    $accessToken = [string]$loginCapture.body.data.accessToken
    $authHeader = @{ Authorization = "Bearer $accessToken" }

    # Build dedicated mock dataset for capture; do not depend on contract-suite leftovers.
    $ayCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "academicYearCreate" -Vars $templateVars
    $ayCreate = Invoke-CaptureRequest -Name "seed-academic-year-create" -Method "POST" -Uri "$baseUrl/api/AcademicYears" -Headers $authHeader -Body $ayCreatePayload
    $captures.Add($ayCreate)
    Assert-CaptureSuccess -Capture $ayCreate -Name "seed-academic-year-create"
    $academicYearId = [int]$ayCreate.body.data.id
    $templateVars["academicYearId"] = $academicYearId

    $ksCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "keystageCreate" -Vars $templateVars
    $ksCreate = Invoke-CaptureRequest -Name "seed-keystage-create" -Method "POST" -Uri "$baseUrl/api/keystages" -Headers $authHeader -Body $ksCreatePayload
    $captures.Add($ksCreate)
    Assert-CaptureSuccess -Capture $ksCreate -Name "seed-keystage-create"
    $keystageId = [int]$ksCreate.body.data.id
    $templateVars["keystageId"] = $keystageId

    # Resolve a valid existing keystage+grade pair for class section creation.
    # Grades are seeded system data and are not created through the keystage create endpoint.
    $lookupKeystagesProbe = Invoke-CaptureRequest -Name "seed-lookups-keystages-probe" -Method "GET" -Uri "$baseUrl/api/lookups/keystages" -Headers $authHeader
    $captures.Add($lookupKeystagesProbe)
    Assert-CaptureSuccess -Capture $lookupKeystagesProbe -Name "seed-lookups-keystages-probe"
    $probeKeystages = @($lookupKeystagesProbe.body.data)
    if ($probeKeystages.Count -eq 0) {
        throw "No keystages available in lookup data for class section seed capture."
    }

    $selectedKeystageId = [int]$probeKeystages[0].id
    $lookupGradesProbe = Invoke-CaptureRequest -Name "seed-lookups-grades-probe" -Method "GET" -Uri "$baseUrl/api/lookups/grades?keystageId=$selectedKeystageId" -Headers $authHeader
    $captures.Add($lookupGradesProbe)
    Assert-CaptureSuccess -Capture $lookupGradesProbe -Name "seed-lookups-grades-probe"
    $probeGrades = @($lookupGradesProbe.body.data)
    if ($probeGrades.Count -eq 0) {
        throw "No grades available for keystageId=$selectedKeystageId in lookup data."
    }

    $keystageId = $selectedKeystageId
    $gradeId = [int]$probeGrades[0].id
    $templateVars["keystageId"] = $keystageId
    $templateVars["gradeId"] = $gradeId

    $subjectCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "subjectCreate" -Vars $templateVars
    $subjectCreate = Invoke-CaptureRequest -Name "seed-subject-create" -Method "POST" -Uri "$baseUrl/api/subjects" -Headers $authHeader -Body $subjectCreatePayload
    $captures.Add($subjectCreate)
    Assert-CaptureSuccess -Capture $subjectCreate -Name "seed-subject-create"
    $subjectId = [int]$subjectCreate.body.data.id
    $templateVars["subjectId"] = $subjectId

    $classSectionCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "classSectionCreate" -Vars $templateVars
    $classSectionCreate = Invoke-CaptureRequest -Name "seed-class-section-create" -Method "POST" -Uri "$baseUrl/api/ClassSections" -Headers $authHeader -Body $classSectionCreatePayload
    $captures.Add($classSectionCreate)
    Assert-CaptureSuccess -Capture $classSectionCreate -Name "seed-class-section-create"
    $classSectionId = [int]$classSectionCreate.body.data.id
    $templateVars["classSectionId"] = $classSectionId

    $parentCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "parentCreate" -Vars $templateVars
    $parentCreate = Invoke-CaptureRequest -Name "seed-parent-create" -Method "POST" -Uri "$baseUrl/api/parents" -Headers $authHeader -Body $parentCreatePayload
    $captures.Add($parentCreate)
    Assert-CaptureSuccess -Capture $parentCreate -Name "seed-parent-create"
    $parentId = [int]$parentCreate.body.data.id
    $templateVars["parentId"] = $parentId

    $teacherCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "teacherCreate" -Vars $templateVars
    $teacherCreate = Invoke-CaptureRequest -Name "seed-teacher-create" -Method "POST" -Uri "$baseUrl/api/teachers" -Headers $authHeader -Body $teacherCreatePayload
    $captures.Add($teacherCreate)
    Assert-CaptureSuccess -Capture $teacherCreate -Name "seed-teacher-create"
    $teacherId = [int]$teacherCreate.body.data.id
    $templateVars["teacherId"] = $teacherId

    $teacherAssignmentCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "teacherAssignmentCreate" -Vars $templateVars
    $teacherAssignmentCreate = Invoke-CaptureRequest -Name "seed-teacher-assignment-create" -Method "POST" -Uri "$baseUrl/api/teachers/$teacherId/assignments" -Headers $authHeader -Body $teacherAssignmentCreatePayload
    $captures.Add($teacherAssignmentCreate)
    Assert-CaptureSuccess -Capture $teacherAssignmentCreate -Name "seed-teacher-assignment-create"

    $studentCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "studentCreate" -Vars $templateVars
    Ensure-ArrayField -Payload $studentCreatePayload -FieldName "parents"
    $studentCreate = Invoke-CaptureRequest -Name "seed-student-create" -Method "POST" -Uri "$baseUrl/api/students" -Headers $authHeader -Body $studentCreatePayload
    $captures.Add($studentCreate)
    Assert-CaptureSuccess -Capture $studentCreate -Name "seed-student-create"
    $studentId = [int]$studentCreate.body.data.id
    $templateVars["studentId"] = $studentId

    $bookCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "bookCreate" -Vars $templateVars
    $bookCreate = Invoke-CaptureRequest -Name "seed-book-create" -Method "POST" -Uri "$baseUrl/api/books" -Headers $authHeader -Body $bookCreatePayload
    $captures.Add($bookCreate)
    Assert-CaptureSuccess -Capture $bookCreate -Name "seed-book-create"
    $bookId = [int]$bookCreate.body.data.id
    $templateVars["bookId"] = $bookId

    $stockEntryCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "bookStockEntryCreate" -Vars $templateVars
    $stockEntryCreate = Invoke-CaptureRequest -Name "seed-book-stock-entry" -Method "POST" -Uri "$baseUrl/api/books/$bookId/stock-entry" -Headers $authHeader -Body $stockEntryCreatePayload
    $captures.Add($stockEntryCreate)
    Assert-CaptureSuccess -Capture $stockEntryCreate -Name "seed-book-stock-entry"

    $distributionCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "distributionCreate" -Vars $templateVars
    Ensure-ArrayField -Payload $distributionCreatePayload -FieldName "items"
    $distributionCreate = Invoke-CaptureRequest -Name "seed-distribution-create" -Method "POST" -Uri "$baseUrl/api/distributions" -Headers $authHeader -Body $distributionCreatePayload
    $captures.Add($distributionCreate)
    Assert-CaptureSuccess -Capture $distributionCreate -Name "seed-distribution-create"
    $distributionId = [int]$distributionCreate.body.data.id
    $distributionRef = [string]$distributionCreate.body.data.referenceNo
    $templateVars["distributionId"] = $distributionId
    $templateVars["distributionRef"] = $distributionRef

    $returnCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "returnCreate" -Vars $templateVars
    Ensure-ArrayField -Payload $returnCreatePayload -FieldName "items"
    $returnCreate = Invoke-CaptureRequest -Name "seed-return-create" -Method "POST" -Uri "$baseUrl/api/returns" -Headers $authHeader -Body $returnCreatePayload
    $captures.Add($returnCreate)
    Assert-CaptureSuccess -Capture $returnCreate -Name "seed-return-create"
    $returnId = [int]$returnCreate.body.data.id
    $returnRef = [string]$returnCreate.body.data.referenceNo
    $templateVars["returnId"] = $returnId
    $templateVars["returnRef"] = $returnRef

    $teacherIssueCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "teacherIssueCreate" -Vars $templateVars
    Ensure-ArrayField -Payload $teacherIssueCreatePayload -FieldName "items"
    $teacherIssueCreate = Invoke-CaptureRequest -Name "seed-teacher-issue-create" -Method "POST" -Uri "$baseUrl/api/TeacherIssues" -Headers $authHeader -Body $teacherIssueCreatePayload
    $captures.Add($teacherIssueCreate)
    Assert-CaptureSuccess -Capture $teacherIssueCreate -Name "seed-teacher-issue-create"
    $teacherIssueId = [int]$teacherIssueCreate.body.data.id
    $templateVars["teacherIssueId"] = $teacherIssueId

    $tempUserEmail = "apitest{0}@booksportal.local" -f $runSuffix
    $templateVars["tempUserEmail"] = $tempUserEmail
    $userCreatePayload = Get-ResolvedFixture -Fixtures $fixtures -Name "userCreate" -Vars $templateVars
    Ensure-ArrayField -Payload $userCreatePayload -FieldName "roles"
    $userCreate = Invoke-CaptureRequest -Name "seed-user-create" -Method "POST" -Uri "$baseUrl/api/users" -Headers $authHeader -Body $userCreatePayload
    $captures.Add($userCreate)
    Assert-CaptureSuccess -Capture $userCreate -Name "seed-user-create"
    $tempUserId = [int]$userCreate.body.data.id
    $templateVars["tempUserId"] = $tempUserId

    # Lists after seeding.
    $captures.Add((Invoke-CaptureRequest -Name "academic-years-list" -Method "GET" -Uri "$baseUrl/api/AcademicYears" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "keystages-list" -Method "GET" -Uri "$baseUrl/api/keystages" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "subjects-list" -Method "GET" -Uri "$baseUrl/api/subjects" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "class-sections-list" -Method "GET" -Uri "$baseUrl/api/ClassSections?academicYearId=$academicYearId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "parents-list" -Method "GET" -Uri "$baseUrl/api/parents?pageNumber=1&pageSize=20&search=$runSuffix" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "teachers-list" -Method "GET" -Uri "$baseUrl/api/teachers?pageNumber=1&pageSize=20&search=$runSuffix" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "students-list" -Method "GET" -Uri "$baseUrl/api/students?pageNumber=1&pageSize=20&search=$runSuffix" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "books-list" -Method "GET" -Uri "$baseUrl/api/books?pageNumber=1&pageSize=20&subjectId=$subjectId&search=$runKey" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "distributions-list" -Method "GET" -Uri "$baseUrl/api/distributions?pageNumber=1&pageSize=20&academicYearId=$academicYearId&studentId=$studentId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "returns-list" -Method "GET" -Uri "$baseUrl/api/returns?pageNumber=1&pageSize=20&academicYearId=$academicYearId&studentId=$studentId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "teacher-issues-list" -Method "GET" -Uri "$baseUrl/api/TeacherIssues?pageNumber=1&pageSize=20&academicYearId=$academicYearId&teacherId=$teacherId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "users-list" -Method "GET" -Uri "$baseUrl/api/users" -Headers $authHeader))

    # Capture frontend-critical success responses.
    $captures.Add((Invoke-CaptureRequest -Name "auth-me" -Method "GET" -Uri "$baseUrl/api/auth/me" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-academic-years" -Method "GET" -Uri "$baseUrl/api/lookups/academic-years" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-keystages" -Method "GET" -Uri "$baseUrl/api/lookups/keystages" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-grades" -Method "GET" -Uri "$baseUrl/api/lookups/grades?keystageId=$keystageId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-subjects" -Method "GET" -Uri "$baseUrl/api/lookups/subjects" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-class-sections" -Method "GET" -Uri "$baseUrl/api/lookups/class-sections?academicYearId=$academicYearId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-terms" -Method "GET" -Uri "$baseUrl/api/lookups/terms" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-book-conditions" -Method "GET" -Uri "$baseUrl/api/lookups/book-conditions" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "lookups-movement-types" -Method "GET" -Uri "$baseUrl/api/lookups/movement-types" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "students-get-by-id" -Method "GET" -Uri "$baseUrl/api/students/$studentId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "books-get-by-id" -Method "GET" -Uri "$baseUrl/api/books/$bookId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "books-search" -Method "GET" -Uri "$baseUrl/api/books/search?q=$runKey" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "books-stock-entries" -Method "GET" -Uri "$baseUrl/api/books/$bookId/stock-entries?pageNumber=1&pageSize=20" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "books-stock-movements" -Method "GET" -Uri "$baseUrl/api/books/$bookId/stock-movements?pageNumber=1&pageSize=20" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "distributions-get-by-id" -Method "GET" -Uri "$baseUrl/api/distributions/$distributionId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "distributions-get-by-reference" -Method "GET" -Uri "$baseUrl/api/distributions/by-reference/$distributionRef" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "returns-get-by-id" -Method "GET" -Uri "$baseUrl/api/returns/$returnId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "returns-get-by-reference" -Method "GET" -Uri "$baseUrl/api/returns/by-reference/$returnRef" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "teacher-issues-get-by-id" -Method "GET" -Uri "$baseUrl/api/TeacherIssues/$teacherIssueId" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "reference-number-formats-list" -Method "GET" -Uri "$baseUrl/api/ReferenceNumberFormats?slipType=Distribution&academicYearId=$academicYearId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "slip-template-settings-list" -Method "GET" -Uri "$baseUrl/api/SlipTemplateSettings" -Headers $authHeader))

    $captures.Add((Invoke-CaptureRequest -Name "reports-stock-summary" -Method "GET" -Uri "$baseUrl/api/reports/stock-summary?pageNumber=1&pageSize=20&subjectId=$subjectId&grade=G$runKey" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "reports-distribution-summary" -Method "GET" -Uri "$baseUrl/api/reports/distribution-summary?pageNumber=1&pageSize=20&academicYearId=$academicYearId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "reports-teacher-outstanding" -Method "GET" -Uri "$baseUrl/api/reports/teacher-outstanding?pageNumber=1&pageSize=20&teacherId=$teacherId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "reports-student-history" -Method "GET" -Uri "$baseUrl/api/reports/student-history/$studentId?pageNumber=1&pageSize=20" -Headers $authHeader))

    if ($tempUserId -gt 0) {
        $captures.Add((Invoke-CaptureRequest -Name "users-get-by-id" -Method "GET" -Uri "$baseUrl/api/users/$tempUserId" -Headers $authHeader))
        $captures.Add((Invoke-CaptureRequest -Name "users-roles-update" -Method "PUT" -Uri "$baseUrl/api/users/$tempUserId/roles" -Headers $authHeader -Body @("User", "Admin")))
    }

    # Capture expected business-rule error responses useful for frontend handling.
    $captures.Add((Invoke-CaptureRequest -Name "errors-superadmin-toggle-forbidden" -Method "POST" -Uri "$baseUrl/api/users/1/toggle-active" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "errors-parent-delete-referenced" -Method "DELETE" -Uri "$baseUrl/api/parents/$parentId" -Headers $authHeader))
    $captures.Add((Invoke-CaptureRequest -Name "errors-academic-year-delete-referenced" -Method "DELETE" -Uri "$baseUrl/api/AcademicYears/$academicYearId" -Headers $authHeader))

    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
    $files = New-Object System.Collections.Generic.List[object]
    foreach ($capture in $captures) {
        $filePath = Write-Capture -OutputDirPath $OutputDir -Capture $capture
        $files.Add([ordered]@{
            name = $capture.name
            statusCode = $capture.statusCode
            isError = $capture.isError
            file = $filePath
        })
    }

    $successCount = @($captures | Where-Object { -not $_.isError -and $_.statusCode -ge 200 -and $_.statusCode -lt 300 }).Count
    $errorCount = @($captures | Where-Object { $_.isError -or $_.statusCode -ge 400 }).Count

    $index = [ordered]@{
        runId = $runId
        startedAtUtc = $runStart.ToString("o")
        finishedAtUtc = [DateTime]::UtcNow.ToString("o")
        source = [ordered]@{
            baseUrl = $baseUrl
            httpyacLog = if ($logFile) { $logFile.FullName } else { $null }
            runSuffix = $runSuffix
            runKey = $runKey
            runYear = $runYear
        }
        entities = [ordered]@{
            academicYearId = $academicYearId
            keystageId = $keystageId
            subjectId = $subjectId
            classSectionId = $classSectionId
            parentId = $parentId
            teacherId = $teacherId
            studentId = $studentId
            bookId = $bookId
            distributionId = $distributionId
            returnId = $returnId
            teacherIssueId = $teacherIssueId
            tempUserId = $tempUserId
        }
        summary = [ordered]@{
            totalCaptures = $captures.Count
            successLike = $successCount
            errorLike = $errorCount
        }
        files = $files
    }

    $indexPath = Join-Path $OutputDir "index.json"
    ($index | ConvertTo-Json -Depth 40) | Set-Content -Path $indexPath -Encoding UTF8

    Write-Host "Frontend mock capture completed."
    Write-Host ("Output directory: {0}" -f $OutputDir)
    Write-Host ("Index file: {0}" -f $indexPath)
}
catch {
    $errMsg = ($_ | Out-String).Trim()
    [Console]::Error.WriteLine($errMsg)
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

    if ($RestartBackend -or $ForceStopOnExit) {
        try {
            Stop-StaleBackendProcesses -ProjectPath $startupProjectPath -BaseUrl $baseUrl
        } catch {}
    }
}
