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

## Exit codes

- `0`: no failures
- `1`: one or more failures detected
- `2`: execution/runtime error
