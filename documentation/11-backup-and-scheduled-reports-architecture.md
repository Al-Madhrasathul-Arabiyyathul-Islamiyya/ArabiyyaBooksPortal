# Backup and Scheduled Reports Architecture (Post-v1.0.0)

## Scope and Activation Gate

This feature set is planned for **post-release hardening** and should be implemented only after:
- Core backend and frontend functionality is complete.
- System is considered service-ready.
- Repository release tag `v1.0.0` is published from `master`.

## Objectives

1. Scheduled backups for database dumps and selected file artifacts.
2. Scheduled report generation and export (Excel).
3. Destination delivery to:
   - Windows network share (UNC path)
   - Google Drive folder (Google Workspace) using OAuth user flow
4. Delta-only execution to avoid redundant backup/export runs.
5. Admin-facing operational visibility and SuperAdmin operational control.

## Runtime Architecture

Primary runtime is an app-hosted scheduler in backend (`BackgroundService`/`IHostedService`):
- Loads job definitions from config + persistent job metadata.
- Evaluates schedules.
- Computes delta fingerprints before execution.
- Executes destination writes with retry policy.
- Persists run history and state transitions.

## Job Categories

### Backup Jobs
- Database dump backup.
- File backup (for configured folders/files such as generated slips and other operational artifacts).

### Report Jobs
- Scheduled report generation from existing report APIs/services.
- Export format: Excel (`.xlsx`).

## Destinations

### Network Share
- Writes ZIP artifacts to configurable UNC path.
- Supports per-job subfolders and retention policy.

### Google Drive
- OAuth user-flow integration (service accounts unavailable).
- One-time interactive consent to acquire refresh token.
- Headless refresh-token execution for scheduled uploads.

## Delta-Only Backup/Export Policy

Each job computes source fingerprint/checkpoint:
- File jobs: hash + metadata checkpoint of source set.
- DB dump jobs: hash of produced dump artifact.
- Report jobs: hash of produced export content.

Execution rules:
- If no delta since last successful run, mark run as `SkippedNoDelta`.
- If delta exists, produce artifact and write to destinations.

State persistence:
- Persist checkpoints/hashes in dedicated state table (`BackupJobState`).
- Changes to scheduler state tables themselves **must not** trigger backup execution.

## Artifact Packaging and Naming

- Package artifacts as ZIP.
- Naming convention:
  - `<jobType>-<scope>-yyyyMMdd-HHmmss.zip`
  - Example: `backup-database-20260214-231500.zip`

## Security and Secrets

- Encrypt OAuth refresh token at rest.
- Do not log secrets/tokens.
- Restrict share write permissions to dedicated service identity.
- Restrict operational APIs by role.

## Failure and Retry Model

- Retry transient destination failures with bounded backoff.
- Support partial destination success recording (for multi-destination jobs).
- Persist structured error detail in run history.
- Keep failed artifact metadata for troubleshooting.

## Frontend Operational UI

Route:
- `/admin/settings/backup`

Role behavior:
- `SuperAdmin`:
  - Manual run-now
  - Pause/resume jobs
  - Retry failed jobs
  - Google OAuth connect/reauth
- `Admin`:
  - Read-only job status and history views

Primary UI panels:
- Overall backup health summary.
- Per-job status table (last success/failure, next run, destination status).
- Run history and error details.

## Planned API Surface

Read (Admin+):
- `GET /api/admin/backup/jobs`
- `GET /api/admin/backup/jobs/{id}/runs`

Operations (SuperAdmin):
- `POST /api/admin/backup/jobs/{id}/run-now`
- `POST /api/admin/backup/jobs/{id}/retry`
- `POST /api/admin/backup/jobs/{id}/pause`
- `POST /api/admin/backup/jobs/{id}/resume`
- `POST /api/admin/backup/google-drive/auth/start`
- `POST /api/admin/backup/google-drive/auth/complete`
- `POST /api/admin/backup/google-drive/reauth`

## Configuration (`appsettings.json`)

Add a dedicated section (example shape):

```json
{
  "BackupScheduler": {
    "Enabled": true,
    "Jobs": [],
    "NetworkShare": {},
    "GoogleDrive": {}
  }
}
```

Specific keys (cron/interval, destinations, retention, OAuth settings, timezones) should be formalized in the implementation plan and options classes.
