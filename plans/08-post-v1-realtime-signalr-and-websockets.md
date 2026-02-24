# 08 - Post-v1 Realtime SignalR and WebSocket Enhancements

## Preconditions
Implement only after:
- [ ] `v1.0.0` released from `master`.
- [ ] Core backup/report scheduler foundation from Plan 05 is stable in production-like usage.

## Scope
- Add realtime server-to-client updates for long-running and monitoring workflows.
- Prioritized use cases:
  - Bulk import progress and completion notifications (replace polling fallback-first).
  - Backup/scheduled-report job status updates for admin monitoring.
  - High-latency reporting workflows where user currently waits on manual refresh.
- Transport:
  - SignalR hub(s) for authenticated app clients.
  - Fallback policy for disconnected clients (graceful resume with status snapshot API).

## Status
- [ ] Not started (post-v1 enhancement).

## Execution Phases
- [ ] Hub contract and event taxonomy design
- [ ] Backend hub/authz implementation
- [ ] Backend event publishers for bulk-import and scheduler pipelines
- [ ] Frontend realtime client composable and store integration
- [ ] UX integration: notifier tray + reports/admin monitoring views
- [ ] Polling fallback/degradation strategy
- [ ] Test coverage (unit/integration/e2e smoke)
- [ ] Observability and rollout controls

## Acceptance Criteria
- [ ] Bulk import progress updates stream to UI without manual refresh.
- [ ] Completion/failure events include actionable summary data.
- [ ] Admin monitoring views receive realtime scheduler/backup status.
- [ ] Reconnect flow restores current job state via snapshot + resumes stream.
- [ ] Feature can be disabled via config and falls back to existing polling behavior.
