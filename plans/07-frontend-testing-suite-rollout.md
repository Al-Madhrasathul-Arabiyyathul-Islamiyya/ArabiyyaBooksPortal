# 07 - Frontend Testing Suite Rollout

## Purpose
Build a comprehensive, modular frontend test suite (unit + e2e) that can be implemented incrementally and merged safely into `dev/frontend`.

Status legend: `[ ]` Todo | `[~]` In progress | `[x]` Done

## Branching Model
- Base integration branch: `testing/frontend-suite-core` (parent: `dev/frontend`)
- Module implementation branches (parent: `testing/frontend-suite-core`):
  - `testing/mod-01-unit-composables`
  - `testing/mod-02-e2e-auth-navigation-smoke`
  - `testing/mod-03-e2e-operations-lifecycle-smoke`
  - `testing/mod-04-e2e-reports-settings-smoke`
  - `testing/mod-05-component-behavior-critical`
  - `testing/mod-06-regression-contract-and-ci`

## Data and Gate Strategy
- E2E data strategy: **Seeded DB baseline**
- Merge gate strategy: **Smoke gate only**
  - Required for feature merges: lint + typecheck + `test:smoke`
  - Full e2e suite: run before release / nightly / major integration

## Module Checklist

### Module 01 - Unit baseline
- [x] Harness established (Vitest config + setup file)
- [x] `usePagination` unit tests
- [x] `useSlipLifecycle` unit tests
- [x] Add unit tests for auth store state transitions (mock API)
- [x] Add unit tests for validation helper edge cases

### Module 02 - E2E auth/navigation smoke
- [x] Playwright config and browser setup command
- [x] Auth guard smoke tests:
  - [x] unauthenticated redirect from `/distribution`
  - [x] unauthenticated redirect from `/admin/reports/distributions`
  - [x] login page render smoke
- [ ] Add role-gate smoke (`admin` vs `user`) using seeded accounts

### Module 03 - E2E operations lifecycle smoke
- [x] Distribution lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [x] Returns lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [x] Teacher issue lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [~] Teacher return lifecycle deterministic checks
  - Current behavior appears to auto-finalize immediately after creation.
  - Test remains intentionally failing for strict lifecycle parity verification.
  - Defer fix to post-suite hardening pass (backend + frontend lifecycle alignment).
- [x] Print action availability checks

### Module 04 - E2E reports/settings smoke
- [x] Report list load checks
- [x] Pagination interaction checks
- [x] Filters apply/reset checks
- [x] Settings pages load and role gate checks
- [x] Audit log list smoke check

### Module 05 - Critical component behavior
- [x] Action button behavior tests
- [x] Lifecycle badge/icon rendering tests
- [x] Common table shell sticky behavior tests (if componentized)
- [x] Dialog interaction smoke for common form containers

### Module 06 - Regression and CI wiring
- [x] Script-level test selection commands in `package.json`
- [ ] Add CI docs for when to run `test:smoke` vs `test:all`
- [ ] Add optional suite tags/grep conventions for focused runs
- [ ] Add release checklist alignment in docs/plans

## Run Commands
- Full unit: `bun run test:unit`
- Full e2e: `bun run test:e2e`
- Smoke gate: `bun run test:smoke`
- Unit subset:
  - `bun run test:unit:lifecycle`
  - `bun run test:unit:pagination`
  - `bun run test:unit -- tests/unit/<file>.spec.ts`
- E2E subset:
  - `bun run test:e2e:auth`
  - `bun run test:e2e:reports-settings`
  - `bun run test:e2e -- tests/e2e/<file>.spec.ts`
  - `bun run test:e2e -- --grep "auth guards"`

## Acceptance Criteria
- [ ] Modules 01-06 complete or explicitly deferred with rationale.
- [ ] `test:smoke` stable and used as minimum merge gate.
- [ ] `test:all` stable before release promotion.
- [ ] Documentation and plan status synced with implemented coverage.
