# Frontend Validation Migration Plan (Regle + Zod)

## Objective
Standardize form validation across the Nuxt frontend by introducing a shared validation module/composable with consistent field/global error behavior and message formatting.

## Scope
- Frontend (`BooksPortalFrontEnd/`) only.
- Validation runtime: Regle.
- Schema contracts: Zod (`app/types/forms.ts`).
- Shared messaging and backend-error normalization.

## Deliverables
1. `app/utils/validation/messages.ts`
2. `app/utils/validation/backend-errors.ts`
3. `app/types/validation.ts`
4. `app/composables/useAppValidation.ts`
5. Migration of high-impact forms (Auth + Phase 3 admin forms first)
6. Documentation sync in `documentation/10-frontend-validation-architecture.md`

## Phased Migration

### Phase V1 - Foundation
- [ ] Add validation message catalog with rule/key mapping.
- [ ] Add shared validation types for field/global errors.
- [ ] Add backend-error normalizer (`ApiResponse.errors` + fallback `message`).
- [ ] Add `useAppValidation` composable with:
  - [ ] `errors`
  - [ ] `globalError`
  - [ ] `isValid`
  - [ ] `touchField`
  - [ ] `touchAll`
  - [ ] `resetValidation`
  - [ ] `applyBackendErrors`

### Phase V2 - First Migration Slice
- [ ] Migrate `app/pages/login.vue`.
- [ ] Migrate Phase 3 modal forms:
  - [ ] Academic Years
  - [ ] Keystages
  - [ ] Subjects
  - [ ] Class Sections
  - [ ] Students
  - [ ] Parents
  - [ ] Teachers
- [ ] Keep UX behavior stable (no visual regressions).

### Phase V3 - Expansion
- [ ] Migrate Phase 4+ forms (books, slips, settings).
- [ ] Remove redundant per-page ad-hoc validation helpers.
- [ ] Consolidate shared field-level message keys.

### Phase V4 - Validation Pass
- [ ] Regle-first pass for all forms using shared composable.
- [ ] Optional schema cleanup where Zod/raw checks are duplicated.
- [ ] Ensure standardized global error blocks in all complex forms.

## Acceptance Criteria
- Consistent field and form-level error rendering across migrated pages.
- Backend business-rule errors displayed as user-safe messages.
- No technical raw errors shown directly to end users.
- `bunx nuxi typecheck` passes after each migration slice.
- `bun run build` passes on merged slices.

## Notes
- i18n is out of scope for now, but message catalog structure should remain translation-ready.
- Do not change backend contracts as part of this plan.
