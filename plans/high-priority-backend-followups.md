# High Priority Backend Follow-ups (Phase 5)

## Year-scoped streaming lookups for slip flows
- Add students lookup scoped by `academicYearId` with lightweight payload (`id`, `fullName`, `indexNo`, class display name, linked parent summary).
- Add parents lookup scoped by `studentId` (plus optional `search`) to avoid global parent scans for slip selection.
- Add books lookup scoped by `academicYearId` with current available stock for fast selector streaming.
- Add teachers lookup endpoint optimized for operations (`id`, `fullName`, `nationalId`, assignment summary) with optional `academicYearId` and `search` for fast teacher-issue modal selection.
- Add teacher-issue outstanding lookup optimized for returns (`issueId`, `referenceNo`, `teacher`, outstanding items) to avoid loading full paginated issue payloads when processing returns.

## Slip print payload completeness
- Populate parent/guardian print fields consistently (name, national ID, phone, relationship where template expects them).
- Populate operation staff identity fields as display names (issued/received by), not ID-only values.
- Ensure placeholder mapping is consistent for both Distribution and Return slip templates.

## Slip lifecycle finalization/processing
- Add explicit lifecycle endpoint(s) to mark a slip as `Processed`/`Finalized` (for example, `POST /api/Distributions/{id}/finalize`, `POST /api/Returns/{id}/finalize`, and teacher equivalents if applicable).
- Enforce rule: finalized slips cannot be cancelled.
- Include finalized status and finalized-at/by metadata in response DTOs so frontend can disable Cancel and show auditable state.
- Add explicit status model for distribution and teacher-issue slips: `Processing`, `Finalized`, `Cancelled`.
- Default operations UI list behavior: exclude `Cancelled` by default (show `Processing` + `Finalized` only).
- Include status in generated slip file naming and regenerate file name when state changes.
- Add visual `CANCELLED` watermark/stamp in generated PDF output when slip status is `Cancelled`.
- Add revision capability while status is `Processing` (editable items/quantities/linked parties), with strict stock-delta reconciliation and full audit trail.
