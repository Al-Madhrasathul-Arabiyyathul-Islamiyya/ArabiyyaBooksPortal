# Post-v1 Backup and Scheduled Reports Plan

## Preconditions

Implement only after:
- [ ] Core functionality is complete.
- [ ] System is service-ready (backend + frontend).
- [ ] Release tag `v1.0.0` is published from `master`.

## Scope

- Scheduled database and files backup.
- Scheduled report export generation (Excel).
- Destinations:
  - Windows network share
  - Google Drive folder (OAuth user integration)
- Delta-only execution based on persistent hash/checkpoint state.
- Admin monitoring UI and role-gated operations.

## Phase A: Foundation and Data Model

- [ ] Add scheduler options section to `appsettings.json` and strongly typed options.
- [ ] Add new entities/migration:
  - [ ] `ScheduledJobDefinition`
  - [ ] `ScheduledJobRun`
  - [ ] `BackupJobState`
- [ ] Add scheduler worker service (`BackgroundService`).
- [ ] Add per-job execution lock to prevent overlap.

## Phase B: Destination Adapters

- [ ] Implement network share destination writer.
- [ ] Implement Google Drive destination writer using OAuth user flow.
- [ ] Add token encryption and secure secret handling.

## Phase C: Backup Jobs

- [ ] Database dump job.
- [ ] Files backup job.
- [ ] ZIP packaging with timestamp naming.
- [ ] Destination-level write verification (hash/check).

## Phase D: Scheduled Report Exports

- [ ] Report export job orchestration (Excel).
- [ ] Job profile definitions for selected reports.
- [ ] Destination write support for report artifacts.

## Phase E: Delta Execution

- [ ] Implement source hash/checkpoint calculator.
- [ ] Implement no-delta skip behavior.
- [ ] Ensure backup state-table churn is excluded from delta triggers.

## Phase F: Admin Monitoring and Operations UI

- [ ] Add `/admin/settings/backup` page.
- [ ] Add status summary cards and per-job table.
- [ ] Add run history with failure detail panel.
- [ ] Add role gates:
  - [ ] `SuperAdmin` can run/pause/resume/retry and auth/reauth.
  - [ ] `Admin` read-only access only.

## Phase G: APIs and Authorization

- [ ] Read APIs (`Admin`, `SuperAdmin`):
  - [ ] `GET /api/admin/backup/jobs`
  - [ ] `GET /api/admin/backup/jobs/{id}/runs`
- [ ] Mutating APIs (`SuperAdmin` only):
  - [ ] `POST /api/admin/backup/jobs/{id}/run-now`
  - [ ] `POST /api/admin/backup/jobs/{id}/retry`
  - [ ] `POST /api/admin/backup/jobs/{id}/pause`
  - [ ] `POST /api/admin/backup/jobs/{id}/resume`
  - [ ] `POST /api/admin/backup/google-drive/auth/start`
  - [ ] `POST /api/admin/backup/google-drive/auth/complete`
  - [ ] `POST /api/admin/backup/google-drive/reauth`

## Phase H: Reliability and Operations

- [ ] Retry/backoff policy for transient failures.
- [ ] Partial destination success tracking.
- [ ] Retention/pruning policy for old artifacts and run logs.
- [ ] Structured operational logs and audit entries.

## Test Checklist

- [ ] Scheduler executes jobs at configured times.
- [ ] Scheduler skips when no source delta.
- [ ] Scheduler runs when source delta exists.
- [ ] Network share destination write success/failure coverage.
- [ ] Google Drive upload success/failure coverage.
- [ ] Multi-destination partial failure behavior.
- [ ] Role-based API/UI access coverage (`Admin` vs `SuperAdmin`).
- [ ] ZIP naming convention and checksum verification coverage.

## Acceptance Criteria

- [ ] Scheduled jobs run stably without duplicate executions.
- [ ] Backups/reports are uploaded only on change.
- [ ] SuperAdmin can manually operate jobs and OAuth lifecycle.
- [ ] Admin can monitor status/history without mutating access.
- [ ] Failures are visible, diagnosable, and retryable.
