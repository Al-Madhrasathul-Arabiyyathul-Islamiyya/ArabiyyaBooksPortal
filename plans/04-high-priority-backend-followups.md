# 04 - High Priority Backend Follow-ups

## Purpose
Track high-impact backend follow-ups and frontend dependencies for slip flows.

Status legend: `[ ]` Todo | `[~]` In progress | `[x]` Done

## Completed Backend Work
### Streaming lookups
- [x] Academic-year scoped student lookup.
- [x] Student-scoped parent lookup.
- [x] Academic-year scoped books lookup with available stock.
- [x] Teacher lookup optimized for issue flow.
- [x] Teacher-issue outstanding lookup optimized for returns.

### Slip payload completeness
- [x] Parent/guardian print field mapping aligned.
- [x] Staff identity fields mapped for print payloads.
- [x] Teacher national ID included in teacher-slip print payload.
- [x] Optional date/time inputs supported for slip generation requests.

### Lifecycle + revision
- [x] Lifecycle statuses implemented (`Processing`, `Finalized`, `Cancelled`).
- [x] Finalized slips cannot be cancelled.
- [x] Cancelled watermark generated in PDFs.
- [x] Distribution and teacher-issue revision endpoints implemented.
- [x] Revision stock-delta reconciliation and audit snapshot implemented.

## Remaining Integration Work (frontend-dependent)
- [ ] Complete frontend lifecycle-state UX parity on all affected pages.
- [ ] Complete frontend revision UX where backend supports revision.
- [ ] Complete end-to-end verification with lifecycle/revision scenarios.

## Acceptance Criteria
- [ ] Frontend and API docs are aligned with lifecycle/revision behavior.
- [ ] Contract + manual verification include processing/finalized/cancelled scenarios.
