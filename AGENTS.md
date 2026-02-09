# AGENTS.md

This file defines how coding agents should operate in this repository for maximum accuracy and speed.

## 1) Scope

Primary focus: frontend work in `./BooksPortalFrontEnd/` (Module 10).
Backend references are allowed when needed for API contract alignment.

## 2) Source of Truth Priority (highest to lowest)

1. `basic-memory` notes:
- `progress/module-10-progress`
- `decisions/design-decisions`
- `architecture/frontend-setup`
- `workflow/frontend-git-workflow`
2. Current code in `./BooksPortalFrontEnd/`
3. `plans/frontend-plan.md`
4. `documentation/api-reference.md`
5. Older docs that may be stale:
- `documentation/05-frontend-structure.md`
- `BooksPortalFrontEnd/README.md`
- `plans/implementation-plan.md` frontend status section

When sources conflict, follow this priority order and note the conflict in your response.

## 3) Current Project State (locked)

- Module 10 status: In Progress
- Phase 1: Complete
- Phase 2: Complete
- Phase 3: Next to start
- Previous Phase 3 implementation was reverted and must not be treated as done.

## 4) Frontend Technical Baseline

- Framework: Nuxt 4 SSR
- Package manager: Bun only (`bun`, `bunx`)
- UI: PrimeVue 4 (Aura preset)
- CSS: Tailwind CSS v4 via `@tailwindcss/vite` + `tailwindcss-primeui`
- State: Pinia + Pinia Colada
- Validation: Regle + Zod
- Auth model: JWT in cookies (SSR compatible)
- API base: `runtimeConfig.public.apiBase = http://localhost:5071/api`
- Dark mode: `@nuxtjs/color-mode`, `.dark` selector
- Thaana font: `public/fonts/Faruma.ttf`

## 5) Required Conventions

- Use `useCookie()` auth cookies:
- `bp_access_token` (7d)
- `bp_refresh_token` (30d)
- `bp_token_expiry` (7d)

- Do not use localStorage for auth.
- Do not add `@nuxtjs/tailwindcss`.
- `nuxt-csurf` may be introduced only as part of explicit backend/frontend CSRF alignment work.
- Keep app-specific wrappers as:
- `useAppToast`
- `useAppConfirm`
- Use endpoint constants from `app/utils/constants.ts` instead of hardcoded strings.
- Keep role checks consistent with backend role values (`SuperAdmin`, `Admin`, `User`) and verify any use of `Staff`.

## 6) Session Startup Checklist (every new session)

1. Read `basic-memory`:
- `memory://progress/*`
- `memory://decisions/*`
- `memory://architecture/*`
2. Read:
- `plans/frontend-plan.md`
- `CLAUDE.md`
3. Inspect current frontend code only for the area being changed.
4. Restate active phase and success criteria before editing.

## 7) Frontend Execution Loop (Phase-oriented)

1. Pick one phase/task from `plans/frontend-plan.md`.
2. Confirm required API endpoints in `documentation/api-reference.md`.
3. Implement with reusable composables/components first.
4. Verify SSR safety (no browser globals on server path).
5. Verify role visibility and route guard behavior.
6. Run `bun run build` after meaningful batches.
7. Update `basic-memory` progress note with concrete done/not-done state.

## 8) Git Workflow Rules

- Never work directly on `master`. Always create a task branch (`hotfix/*`, `refactor/*`, `feature/*`) and merge back after verification.
- For frontend phase work, branch from `feature/module-10-frontend`.
- Do not work directly on `feature/module-10-frontend`.
- Merge completed phase branches back into `feature/module-10-frontend` after build/test pass.

## 8.1) Elevated Command Convention

- In this Codex environment, run `git` commands with elevated sandbox permissions.
- Run raw `dotnet` CLI commands with elevated sandbox permissions.
- Prefer dotnet MCP tools when available, but if shelling out to `dotnet`, keep it elevated.
- Run PowerShell scripts that invoke `dotnet` (build/test/ef/run) with elevated sandbox permissions.
- Prefer single direct commands over long chained one-liners for `git`/`dotnet` operations to maximize approval reuse.

## 9) Verification Standard for Frontend Changes

Minimum checks after each task:
- `bun run build` passes
- No auth regressions (login redirect + protected routes)
- No dark mode regression
- No PrimeVue/Tailwind style collision introduced
- No duplicate composable auto-import conflicts
- API calls use `useApi` and constants

## 10) Known Drift / Pitfalls

- `documentation/05-frontend-structure.md` reflects older planned structure and versions; do not treat as implemented state.
- `BooksPortalFrontEnd/README.md` still mentions `nuxt-csurf` and "not started" status; this is stale.
- `plans/implementation-plan.md` frontend status is stale versus `plans/frontend-plan.md` + memory progress.
- In `app/utils/constants.ts`, role constant uses `Staff`; backend docs use `User`. Validate before introducing role-gated behavior.
- Some endpoint constants include `cancel` routes for slips; verify against live API reference before wiring UI actions.

## 11) Definition of Done (for each phase chunk)

- Planned checklist items implemented for that chunk
- Build passes
- Critical UX path manually sanity checked
- Progress updated in `basic-memory` with exact phase status
- Any source drift discovered is recorded in memory notes
