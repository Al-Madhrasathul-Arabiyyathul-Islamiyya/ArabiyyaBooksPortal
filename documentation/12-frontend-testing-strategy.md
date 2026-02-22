# Frontend Testing Strategy

## Scope
This document defines the practical test strategy for `BooksPortalFrontEnd` using:
- Unit tests: `Vitest`
- Browser e2e tests: `Playwright`

The rollout is intentionally modular to keep implementation localized and easy to review.

## Test Levels

### 1) Unit tests (fast feedback)
Focus:
- composables
- utility formatters
- store logic with mocked dependencies

Current examples:
- `tests/unit/usePagination.spec.ts`
- `tests/unit/useSlipLifecycle.spec.ts`

### 2) E2E smoke tests (merge confidence)
Focus:
- auth/route guards
- key workflow path sanity
- major page load and status rendering checks

Current examples:
- `tests/e2e/auth-guards.spec.ts`

### 3) Full e2e suite (release confidence)
Focus:
- operational flows for distribution/returns/teacher issue/teacher returns
- reports/settings behavior
- cross-module regression checks

## Data Strategy
Default test data strategy is **seeded DB baseline**.

Rationale:
- deterministic and quick to start
- avoids heavy setup/teardown code per test
- good fit for smoke and lifecycle verification

For mutation-heavy scenarios, API-driven setup can be added case-by-case.

## Commands

### Core
- `bun run test:unit`
- `bun run test:nuxt`
- `bun run test:e2e`
- `bun run test:all`
- `bun run test:ci`

### Focused
- `bun run test:smoke`
- `bun run test:e2e:smoke`
- `bun run test:e2e:auth`
- `bun run test:unit:lifecycle`
- `bun run test:unit:pagination`

### Pattern-based selective runs
- `bun run test:unit -- tests/unit/<file>.spec.ts`
- `bun run test:e2e -- tests/e2e/<file>.spec.ts`
- `bun run test:e2e -- --grep "<name>"`
- `bun run test:unit:grep -- "<pattern>"`
- `bun run test:nuxt:grep -- "<pattern>"`
- `bun run test:e2e:grep -- "<pattern>"`

### Playwright Runtime Defaults
- Local `Playwright` runs now start both frontend (`:3000`) and backend (`:5071`) web servers by default when `PLAYWRIGHT_BASE_URL` is not provided.
- To run browser tests against an already-running external backend/frontend pair, set:
  - `PLAYWRIGHT_BASE_URL=<frontend-url>`
  - `PLAYWRIGHT_API_BASE=<backend-api-base>`
- To keep local frontend auto-start but skip backend auto-start, set:
  - `PLAYWRIGHT_SKIP_BACKEND=1`

## Merge/Release Policy

### Minimum merge gate (feature branches)
1. `bun run lint`
2. `bunx nuxi typecheck`
3. `bun run test:smoke`
   - `test:smoke` excludes tests tagged `@known-gap` so deferred failures do not block branch gates.

### CI branch gate (`testing/*`, `dev/frontend`)
1. `bun run lint`
2. `bunx nuxi typecheck`
3. `bun run test:ci`
   - `test:ci` includes unit + nuxt component + e2e
   - `test:ci` excludes tests tagged `@known-gap` using `--grep-invert`

### Pre-release / release gate
1. `bun run lint`
2. `bunx nuxi typecheck`
3. `bun run test:unit`
4. `bun run test:nuxt`
5. `bun run test:e2e`

## Branching Convention for Test Rollout
Base branch:
- `testing/frontend-suite-core`

Per-module branches:
- `testing/mod-01-unit-composables`
- `testing/mod-02-e2e-auth-navigation-smoke`
- `testing/mod-03-e2e-operations-lifecycle-smoke`
- `testing/mod-04-e2e-reports-settings-smoke`
- `testing/mod-05-component-behavior-critical`
- `testing/mod-06-regression-contract-and-ci`

Each module branch merges into `testing/frontend-suite-core`, then core merges into `dev/frontend` once stable.

## Conventions and Decisions
- When a frontend change introduces or modifies reusable UI behavior, add/adjust component tests in the same branch (Nuxt/Vitest component-level coverage).
- Prefer deterministic assertions tied to explicit state transitions over permissive assertions that only check generic visibility.
- Keep test modules small and localized; avoid bundling unrelated test concerns in a single branch.
- Use test tags for focused and policy-driven runs:
  - `@smoke`: fast merge-confidence scenarios
  - `@operations`, `@reports`, `@settings`, `@auth`: domain-focused slices
  - `@known-gap`: intentionally deferred failures tracked in plans/docs and excluded from `test:smoke` and `test:ci`

## Release Alignment
- Frontend release verification is aligned with `documentation/09-versioning-and-release.md`.
- Before promoting to `develop`/`master`, run full release gate (`lint`, `typecheck`, `test:unit`, `test:nuxt`, `test:e2e`) and resolve failures.
