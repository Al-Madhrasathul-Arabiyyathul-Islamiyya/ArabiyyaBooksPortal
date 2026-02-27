# 09 - Pre-v1 Bootstrap and Readiness Rollout

## Purpose
Deliver a mandatory setup/readiness system before v1 deployment so production instances cannot run with missing critical configuration.

Status legend: `[ ]` Todo | `[~]` In progress | `[x]` Done

## Scope
- In scope:
  - Backend setup state + readiness evaluation + setup endpoints + operation guards.
  - Frontend setup wizard + persistent readiness banner + route/action gating.
  - API/docs/test updates for readiness contracts.
- Out of scope:
  - SignalR/WebSocket realtime updates (separate post-v1 plan).
  - Broad settings-page redesign.

## Phase 1 - Backend

### Slice 1.1 Setup state model
- [ ] Add persisted setup-state model (singleton) for status and completed steps.
- [ ] Add readiness evaluation service for required prerequisites.
- [ ] Add machine-readable missing-step issue model.

### Slice 1.2 Setup APIs
- [ ] Implement setup status endpoint (`GET /api/setup/status`).
- [ ] Implement setup step mutation endpoints (`/start`, `/super-admin`, `/slip-templates/confirm`, `/hierarchy/initialize`, `/reference-formats/initialize`, `/complete`).
- [ ] Enforce role policy: write operations restricted to `SuperAdmin`.

### Slice 1.3 Operation guardrails
- [ ] Add centralized readiness guard for operation-critical write endpoints.
- [ ] Return structured `409` with code `SETUP_INCOMPLETE` and missing-step metadata.

### Slice 1.4 Data/migration policy
- [ ] If DB context changes are introduced, create a **fresh migration snapshot** (pre-v1 policy).
- [ ] Add compatibility backfill logic for existing databases:
  - [ ] auto-mark setup completed when prerequisites already satisfied.
  - [ ] mark in-progress with diagnostics otherwise.

### Slice 1.5 Backend verification
- [ ] Unit tests for readiness evaluator and setup transitions.
- [ ] Integration tests for setup endpoints and readiness guards.
- [ ] Contract tests for setup payloads and error envelope.

## Phase 2 - Frontend

### Slice 2.1 Setup wizard UI
- [ ] Add setup flow for `SuperAdmin` with step-driven status/actions.
- [ ] Support confirmation of existing data and guided remediation of missing items.

### Slice 2.2 Persistent readiness UX
- [ ] Add permanent readiness banner in admin and operations areas while incomplete.
- [ ] Show missing prerequisites and next action links.
- [ ] Keep Admin role read-only for setup execution.

### Slice 2.3 Route/action gating
- [ ] Block operation create/process actions while readiness incomplete.
- [ ] Keep safe read-only pages accessible.
- [ ] Normalize and display `SETUP_INCOMPLETE` backend errors via standard error handlers.

### Slice 2.4 Frontend verification
- [ ] Unit tests for readiness state/store/composable.
- [ ] E2E tests:
  - [ ] incomplete setup blocks operations
  - [ ] completed setup enables operations
  - [ ] role visibility/gating for setup actions

## Documentation and Tracking
- [ ] Update `documentation/api-reference.md` with setup endpoints and readiness errors.
- [ ] Cross-link architecture and this plan from `documentation/README.md` and `plans/README.md`.
- [ ] Keep progress updates only in this numbered plan file.

## Acceptance Criteria
- [ ] Production instances cannot process operations before setup completion.
- [ ] SuperAdmin can complete setup fully from UI/API without manual DB edits.
- [ ] Admin can monitor readiness but cannot mutate setup.
- [ ] Tests pass for setup workflow and readiness guard scenarios.

