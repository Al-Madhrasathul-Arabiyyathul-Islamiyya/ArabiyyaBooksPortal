# Frontend Validation Architecture (Regle + Zod)

## Purpose
Standardize validation behavior across the Nuxt frontend so forms share:
- one validation flow,
- one error-message strategy,
- one predictable integration pattern with backend API errors.

This reduces repeated refactors when new modules/pages are added.

## Scope
- Frontend only (`BooksPortalFrontEnd/`).
- Runtime validation orchestration: **Regle**.
- Request schema/source of truth: **Zod** (`app/types/forms.ts`).
- Error display contract for form fields and global form errors.

## Design Principles
1. Keep DTO shape validation in Zod schemas.
2. Use a single app-level validation composable for page/dialog forms.
3. Resolve messages from a central catalog first, then backend fallback.
4. Do not leak technical errors to users by default.
5. Keep component-level API stable during migration.

## Core Building Blocks

### 1. Message Catalog
Create `app/utils/validation/messages.ts`:
- map rule/message keys to standardized text.
- support interpolation placeholders (`{min}`, `{max}`, etc.).

### 2. Validation Types
Create `app/types/validation.ts`:
- shared types for field error maps and global errors.
- normalized result model used by UI composables/components.

### 3. Validation Composable
Create `app/composables/useAppValidation.ts`:
- wraps Regle setup + submit/touch/reset workflow.
- normalizes Zod/Regle/backend errors.
- standard return shape (examples):
  - `errors`
  - `globalError`
  - `isValid`
  - `touchField()`
  - `touchAll()`
  - `resetValidation()`
  - `applyBackendErrors()`

### 4. Backend Error Normalizer
Create `app/utils/validation/backend-errors.ts`:
- parse backend `ApiResponse.errors` field-level entries.
- parse backend `message` as global form error fallback.
- map known backend error keys/codes to frontend catalog keys where possible.

## Error Message Strategy
Resolution order:
1. Frontend catalog message for field/rule.
2. Backend field-level message (if provided).
3. Backend business-rule/global message.
4. Generic fallback message.

This keeps user-facing text consistent while preserving important business-rule messages.

## Integration Pattern
For each form page/dialog:
1. Keep request payload schema in `app/types/forms.ts`.
2. Use `useAppValidation` for runtime form state.
3. Bind each field to normalized error state.
4. On API failure, call backend-error normalizer and display mapped errors.

## Migration Policy
- Incremental migration (high-impact forms first).
- No big-bang rewrite.
- Existing forms can continue using current logic until their migration slice.

## Validation UX Standards
- Show field errors after blur/touch or submit attempt.
- Show one global error area per form.
- Reset validation state on dialog close/reset.
- Keep message phrasing consistent across modules.

## Non-Goals
- No backend contract change in this task.
- No localization/i18n rollout in this task (catalog structure should remain i18n-ready).

## Related
- `plans/frontend-validation-migration-plan.md`
- `plans/frontend-plan.md`
- `documentation/05-frontend-structure.md`
