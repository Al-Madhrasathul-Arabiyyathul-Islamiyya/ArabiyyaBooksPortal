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
- [x] Add role-gate smoke (`admin` vs `user`) using seeded accounts

### Module 03 - E2E operations lifecycle smoke
- [x] Distribution lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [x] Returns lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [x] Teacher issue lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
- [x] Teacher return lifecycle deterministic checks (`create -> Processing -> finalize -> Finalized`)
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
- [x] Add CI docs for when to run `test:smoke` vs `test:all`
- [x] Add optional suite tags/grep conventions for focused runs
- [x] Add release checklist alignment in docs/plans

## Run Commands
- Full unit: `bun run test:unit`
- Full component (Nuxt project): `bun run test:nuxt`
- Full e2e: `bun run test:e2e`
- Smoke gate: `bun run test:smoke`
- CI gate (excludes tagged known gaps): `bun run test:ci`
- Unit subset:
  - `bun run test:unit:lifecycle`
  - `bun run test:unit:pagination`
  - `bun run test:unit -- tests/unit/<file>.spec.ts`
  - `bun run test:unit:grep -- "<pattern>"`
  - `bun run test:nuxt:grep -- "<pattern>"`
- E2E subset:
  - `bun run test:e2e:auth`
  - `bun run test:e2e:reports-settings`
  - `bun run test:e2e:smoke`
  - `bun run test:e2e:ci`
  - `bun run test:e2e -- tests/e2e/<file>.spec.ts`
  - `bun run test:e2e -- --grep "auth guards"`
  - `bun run test:e2e:grep -- "<pattern>"`

### Playwright env controls
- Default local behavior: frontend + backend web servers are auto-started by Playwright.
- Opt out of backend auto-start:
  - `PLAYWRIGHT_SKIP_BACKEND=1 bun run test:e2e:smoke`
- External environment mode:
  - set `PLAYWRIGHT_BASE_URL` and `PLAYWRIGHT_API_BASE` to use pre-running services.

## Acceptance Criteria
- [x] Modules 01-06 complete or explicitly deferred with rationale.
- [x] `test:smoke` stable and used as minimum merge gate.
- [~] `test:all` stable before release promotion.
- [x] Documentation and plan status synced with implemented coverage.
