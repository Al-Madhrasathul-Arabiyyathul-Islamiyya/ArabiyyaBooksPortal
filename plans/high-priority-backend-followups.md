# High Priority Backend Follow-ups (Phase 5)

## Year-scoped streaming lookups for slip flows
- [x] Add students lookup scoped by `academicYearId` with lightweight payload (`id`, `fullName`, `indexNo`, class display name, linked parent summary).
- [x] Add parents lookup scoped by `studentId` (plus optional `search`) to avoid global parent scans for slip selection.
- [x] Add books lookup scoped by `academicYearId` with current available stock for fast selector streaming.
- [x] Add teachers lookup endpoint optimized for operations (`id`, `fullName`, `nationalId`, assignment summary) with optional `academicYearId` and `search` for fast teacher-issue modal selection.
- [x] Add teacher-issue outstanding lookup optimized for returns (`issueId`, `referenceNo`, `teacher`, outstanding items) to avoid loading full paginated issue payloads when processing returns.

## Slip print payload completeness
- [x] Populate parent/guardian print fields consistently (name, national ID, phone where template expects them) for Distribution slips.
- [x] Populate operation staff identity fields as display names (issued/received by), not ID-only values.
- [x] Ensure placeholder mapping is consistent for both Distribution and Return slip templates.
- [x] Include teacher national ID in teacher issue and teacher return print payloads.
- [x] Add optional date/time input fields for slip generation requests and render them in PDFs (`DD-MM-YYYY`, `HH:mm`).

## Slip lifecycle finalization/processing
- [x] Add explicit lifecycle endpoint(s) to mark a slip as `Processed`/`Finalized` (implemented for Distribution and TeacherIssue).
- [x] Enforce rule: finalized slips cannot be cancelled.
- [x] Include finalized status and finalized-at/by metadata in response DTOs so frontend can disable Cancel and show auditable state.
- [x] Add explicit status model for distribution and teacher-issue slips: `Processing`, `Finalized`, `Cancelled`.
- [x] Default operations UI list behavior: exclude `Cancelled` by default (show `Processing` + `Finalized` only).
- [x] Include status in generated slip file naming and regenerate file name when state changes.
- [x] Add visual `CANCELLED` watermark/stamp in generated PDF output when slip status is `Cancelled`.
- [x] Add revision capability while status is `Processing` (editable items/quantities/linked parties), with strict stock-delta reconciliation and full audit trail.

## Revision implementation track (current branch)
- [x] Add distribution revision endpoint (`PUT /api/distributions/{id}`).
- [x] Add teacher-issue revision endpoint (`PUT /api/TeacherIssues/{id}`).
- [x] Enforce lifecycle guard: only `Processing` slips can be revised.
- [x] Recompute and apply stock deltas atomically during revision.
- [x] Regenerate/rewrite slip PDF after successful revision.
- [x] Add/extend service/unit/integration tests for revision flows and lifecycle lockouts.
- [x] Add explicit aggregated revision audit snapshot (before/after payload blob) in addition to the existing entity-level audit logs.
