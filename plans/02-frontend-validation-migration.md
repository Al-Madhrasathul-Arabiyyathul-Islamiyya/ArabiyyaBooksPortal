# 02 - Frontend Validation Migration (Regle + Zod)

## Purpose
Standardize frontend validation behavior and error rendering across all forms.

Status legend: `[ ]` Todo | `[~]` In progress | `[x]` Done

## Baseline
- [x] Shared message catalog created.
- [x] Shared backend-error normalizer created.
- [x] `useAppValidation` composable introduced.
- [x] Login + key Phase 3 admin forms migrated.

## Remaining Work
### V3 Expansion
- [~] Migrate remaining Phase 4+ forms:
  - [x] Books forms (create/edit + stock dialogs)
  - [x] Slips forms/pages complete migration
  - [x] Settings forms complete migration
- [~] Remove residual ad-hoc page-level validators and error extractors (cleanup pass still pending).

### V4 Validation Pass
- [ ] Regle-first pass on all operational and admin forms.
- [ ] Consolidate message keys and remove duplication.
- [ ] Ensure consistent global error surfaces on complex flows.

## Acceptance Criteria
- [~] No raw technical backend errors shown to users.
- [~] Field-level + form-level behavior is consistent across all forms.
- [x] Typecheck/build pass after migration completion.
