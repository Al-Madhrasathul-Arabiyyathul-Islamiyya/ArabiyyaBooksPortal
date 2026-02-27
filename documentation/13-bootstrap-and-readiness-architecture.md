# 13 - Bootstrap and Readiness Architecture (Pre-v1)

## Purpose
Define a mandatory pre-v1 initialization and readiness model so production cannot run critical workflows with missing baseline configuration.

## Problem
Relying on startup seeders for business configuration causes two classes of risk:
- Runtime behavior can differ across environments based on seeding rules.
- Critical operational prerequisites may be missing while the app appears healthy.

## Goals
- Require explicit initial setup before normal operations.
- Provide a single readiness status contract for backend and frontend.
- Block operation-critical actions when setup is incomplete.
- Keep seeders minimal and non-destructive in production.

## Non-Goals
- Realtime updates (SignalR/WebSockets) are out of scope for this phase.
- Broad redesign of all settings pages is out of scope.
- Destructive schema/migration operations are out of scope.

## Required Readiness Checklist (Blocking)
1. SuperAdmin account is initialized and active.
2. Slip template settings exist and are confirmed.
3. Academic years exist and exactly one is active.
4. Keystages, grades, and class sections are configured.
5. Reference number templates exist for all required slip types in the active academic year.

## Backend Design
### Setup State
Introduce a persisted setup-state model (singleton record), e.g.:
- `Status`: `NotStarted | InProgress | Completed`
- `CompletedSteps`
- `LastEvaluatedAtUtc`
- `CompletedAtUtc`
- `SchemaVersion`

### Readiness Evaluator
Central service computes checklist status from current DB state and returns:
- overall status
- per-step status
- blocking issues with machine-readable keys
- human-readable resolution hints

### API Contract Additions
- `GET /api/setup/status`
- `POST /api/setup/start`
- `POST /api/setup/super-admin`
- `POST /api/setup/slip-templates/confirm`
- `POST /api/setup/hierarchy/initialize`
- `POST /api/setup/reference-formats/initialize`
- `POST /api/setup/complete`

All setup write endpoints: `SuperAdmin` only.

### Guardrails
Operation-critical endpoints must reject with structured `409` when readiness is incomplete:
- `code: SETUP_INCOMPLETE`
- `missingSteps: [...]`
- `hints: [...]`

Read-only endpoints stay available where safe.

## Frontend Design
### Setup Wizard (Admin)
- New `SuperAdmin` setup flow page (step-driven).
- Binds directly to `/api/setup/status` and setup step endpoints.
- Supports both "create defaults" and "confirm existing configuration" flows.

### Persistent Readiness Visibility
- Permanent, non-dismissible warning banner when setup is incomplete.
- Visible in both admin and operations areas.
- Includes missing prerequisites and action link to setup page for authorized users.

### Route and Action Gating
- Block create/process operation flows while setup incomplete.
- Preserve read-only views where possible.

## Seeding Policy After This Change
- Startup seeders should only provide technical baseline data.
- Business configuration ownership moves to setup workflow.
- Existing operator-managed values must not be overwritten on startup.

## Migration and Compatibility
- If DB model changes are required, use a **fresh migration snapshot** (project policy pre-v1).
- Backfill logic at startup:
  - if environment already satisfies readiness, mark as completed.
  - otherwise mark in-progress with missing-step diagnostics.
- No destructive data loss path.

## Observability and Audit
- Log setup step transitions with correlation IDs.
- Audit setup completion and critical setup edits.
- Expose clear diagnostics in readiness status payload.

## Acceptance Criteria
- Incomplete setup cannot execute critical operational actions.
- SuperAdmin can finish all prerequisites without direct DB edits.
- Admin sees readiness state (read-only), cannot perform setup writes.
- Frontend and backend contracts are aligned and documented in `documentation/api-reference.md`.

