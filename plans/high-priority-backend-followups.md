# High Priority Backend Follow-ups (Phase 5)

## Year-scoped streaming lookups for slip flows
- Add students lookup scoped by `academicYearId` with lightweight payload (`id`, `fullName`, `indexNo`, class display name, linked parent summary).
- Add parents lookup scoped by `studentId` (plus optional `search`) to avoid global parent scans for slip selection.
- Add books lookup scoped by `academicYearId` with current available stock for fast selector streaming.

## Slip print payload completeness
- Populate parent/guardian print fields consistently (name, national ID, phone, relationship where template expects them).
- Populate operation staff identity fields as display names (issued/received by), not ID-only values.
- Ensure placeholder mapping is consistent for both Distribution and Return slip templates.
