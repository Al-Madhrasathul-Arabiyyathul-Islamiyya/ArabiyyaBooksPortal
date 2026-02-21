# 01 - Frontend Core Roadmap

## Purpose
Primary execution roadmap for frontend work. This is the canonical day-to-day plan.

Status legend: `[ ]` Todo | `[~]` In progress | `[x]` Done

## Current State Snapshot
- [x] Phases 1-3 are implemented (foundation, shared components, master data).
- [x] Phase 4 books is functionally implemented with list/create/edit/detail/stock operations.
- [x] Phase 5/6 operations are implemented and primary end-to-end verification is complete.
- [x] Phase 7 reports/settings alignment verified against current paginated contracts.
- [ ] Phase 8 verification debt closure across all modules is not fully signed off.

## Backend Contract Adoption Matrix
### Adopted
- [x] Paginated report endpoints consumed in admin report pages.
- [x] Paginated master-data endpoints consumed in major list pages.
- [x] Slip cancellation and print flows are wired in detail pages.

### Partial
- [~] Slip lifecycle states (`Processing`, `Finalized`, `Cancelled`) need complete UX exposure and guard behavior consistency across distribution/returns/teacher flows.
- [~] Revision workflows for processing slips need full frontend completion.
- [x] Settings/reports UX verification completed against backend pagination and role gates.
- [~] Streaming lookup endpoints are available in backend; ensure all operational selectors consistently use scoped lookups instead of broader list queries.
- [~] Staff/parent print-field prefill is delivered in backend but requires full frontend flow verification across all slip types.

### Not started / pending
- [ ] Final frontend support for all backend follow-ups in `plans/04-high-priority-backend-followups.md` that affect operational screens.
- [ ] Consolidated full regression pass for all role-based flows.

## Execution Queue (Priority Order)
### Slice 1 - Verification debt closure (high)
- [x] Close Phase 5 verification checklist:
  - [x] Distribution list/create/detail/print/cancel/finalize validated manually.
  - [x] Returns list/create/detail/print/cancel/finalize validated manually.
  - [x] Stock reconciliation validated after create/cancel/revision paths.
- [x] Close Phase 6 verification checklist:
  - [x] Teacher issue create/detail/returns/cancel/finalize validated.
  - [x] Outstanding quantities and status transitions validated.
- [x] Close Phase 7/8 report/settings verification:
  - [x] Report pages load with paginated endpoints and filters.
  - [x] Settings pages behave with current backend contracts and role gates.
  - [x] Audit log flow verified against final API contract.

### Slice 2 - Lifecycle and revision UX completion
- [~] Add explicit lifecycle state handling in relevant pages:
  - [x] disable cancel when finalized on distribution/teacher-issue detail pages
  - [x] expose finalized/cancelled metadata in distribution and teacher-issue detail views
  - [x] ensure list filters default to exclude cancelled where required
- [~] Add/complete processing-state revision UX where backend supports revision.
  - [ ] Pending: frontend editing flows for processing slips are not yet exposed in detail/create pages.

### Slice 3 - Frontend consistency pass
- [ ] Ensure all table/list endpoints use consistent paginated adapters.
- [ ] Ensure all forms use standardized validation composable (see `plans/02-frontend-validation-migration.md`).
- [ ] Ensure all user-facing errors are backend-normalized friendly messages.

### Slice 4 - Pre-release verification bundle
- [ ] Lint/typecheck/build pass.
- [ ] End-to-end operational smoke pass:
  - [ ] Admin + User role routing and guards
  - [ ] Slip generation and PDF open/download
  - [ ] Reports export paths
- [ ] Update plan/docs/basic-memory with final status.

## Acceptance Criteria
- [ ] All open checkboxes in slices 1-4 are completed or explicitly deferred with rationale.
- [ ] No contract mismatch between frontend API usage and `documentation/api-reference.md`.
- [ ] No stale phase statements in `documentation/05-frontend-structure.md` and `documentation/README.md`.
