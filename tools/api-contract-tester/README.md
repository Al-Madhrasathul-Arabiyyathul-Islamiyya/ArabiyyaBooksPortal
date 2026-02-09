# API Contract Tester

This tool validates `documentation/api-reference.md` against the running backend and emits a structured JSON log.

## What it does

- Starts the backend API (`dotnet run`) unless `-SkipStartBackend` is used.
- Waits for `GET /health`.
- Logs in with SuperAdmin credentials from config.
- If login fails, continues in unauthenticated mode and validates auth enforcement (`401/403`) on protected routes.
- Parses documented endpoints from `documentation/api-reference.md`.
- Loads Swagger (`/swagger/v1/swagger.json`) and reports doc vs implementation drift.
- Sweeps documented routes and records outcomes:
  - `success`: route reachable and contract shape is acceptable.
  - `warn`: validation/business-rule failures or not found on placeholder IDs.
  - `fail`: route missing, method mismatch, auth failure, or server errors.
- Writes a structured log JSON file to `tools/api-contract-tester/logs`.

## Usage

```powershell
pwsh ./tools/api-contract-tester/run-api-contract-tests.ps1
```

## httpyac Contract Suite

The `httpyac` suite uses real request bodies and chained auth variables.

Files:
- `tools/api-contract-tester/http/contract-suite.http`
- `tools/api-contract-tester/httpyac-config.json`
- `tools/api-contract-tester/run-httpyac-contract-tests.ps1`

Run:

```powershell
pwsh ./tools/api-contract-tester/run-httpyac-contract-tests.ps1
```

This writes a structured log:
- `tools/api-contract-tester/logs/httpyac-contract-log-<runId>.json`

Coverage enforcement:
- Every documented endpoint in `documentation/api-reference.md` must have a `# @covers METHOD /path` entry in the suite.
- The runner fails before execution if documented route coverage is below 100% (except explicit entries in `coverage.ignoreDocumentedRoutes`).

## One-Command Backend Verification

This runner does a complete local verification pass in one command:
- runs a preflight `dotnet clean` + `dotnet build`,
- stops stale API processes,
- recreates the database from migrations,
- runs the full `httpyac` contract suite,
- runs backend unit and integration tests,
- writes a structured log file.

Run:

```powershell
pwsh ./tools/api-contract-tester/run-backend-verification.ps1
```

Optional:

```powershell
pwsh ./tools/api-contract-tester/run-backend-verification.ps1 -SkipPreflight
```

Structured log:
- `tools/api-contract-tester/logs/backend-verification-log-<runId>.json`

## Frontend-Ready Mock Capture

Generates frontend-consumable response snapshots from a real backend run.

What it does:
- optionally starts the backend,
- optionally runs the full `httpyac` contract suite to seed deterministic data,
- captures frontend-critical success payloads and representative business-rule error payloads,
- writes JSON snapshots and an index file for frontend use.

Run:

```powershell
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1
```

Output:
- `BooksPortalFrontEnd/app/mocks/api-capture/index.json`
- `BooksPortalFrontEnd/app/mocks/api-capture/*.json`

Useful flags:

```powershell
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1 -SkipContractSuite
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1 -SkipStartBackend
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1 -KeepBackendRunning
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1 -RestartBackend
pwsh ./tools/api-contract-tester/run-frontend-mock-capture.ps1 -ForceStopOnExit
```

Notes:
- `-RestartBackend` force-kills stale backend processes matching the API project/base URL, then starts a fresh backend for capture and kills it during cleanup.
- `-ForceStopOnExit` always performs a final stale-backend cleanup pass before script exit.

### Optional flags

```powershell
pwsh ./tools/api-contract-tester/run-api-contract-tests.ps1 -SkipStartBackend
pwsh ./tools/api-contract-tester/run-api-contract-tests.ps1 -ConfigPath ./tools/api-contract-tester/api-contract-config.json
pwsh ./tools/api-contract-tester/run-api-contract-tests.ps1 -KeepBackendRunning
```

## Config

Edit `tools/api-contract-tester/api-contract-config.json`:

- `backend.baseUrl` and `backend.projectPath`
- `auth.superAdmin`
- `placeholders` for path variables
- `test.disruptiveEndpoints` and timeout values
- `requestTemplates` for endpoints that require request bodies

Important:
- Keep mutating user/auth endpoints in `disruptiveEndpoints` to avoid accidentally deactivating admin users in local databases.

## Exit codes

- `0`: no failures
- `1`: one or more failures detected
- `2`: execution/runtime error
