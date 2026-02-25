# Books Portal - Versioning and Release

## Scope

This document defines how versioning works for:
- Backend .NET projects (`BooksPortal/src/*` and `BooksPortal/tests/*`)
- Frontend Nuxt app (`BooksPortalFrontEnd`)
- Repository-level releases (git tags on `master`)

## Version Model

- Base scheme: `MAJOR.MINOR.PATCH`
- Current baseline: `0.10.0`

### Backend (.NET)

- Centralized in `BooksPortal/Directory.Build.props`.
- Applied to all SDK-style projects under `BooksPortal/`.
- Fields:
  - `VersionPrefix`
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`
  - `InformationalVersion`

### Backend NuGet Package Versions

- Central Package Management is enabled in `BooksPortal/Directory.Packages.props`.
- All package versions are defined once via `<PackageVersion ... />`.
- Individual `.csproj` files reference packages without `Version`.

### Frontend (Nuxt)

- Frontend app version is tracked in `BooksPortalFrontEnd/package.json`:
  - `"version": "0.10.0"`

### Git Tags (Repository Release)

- Git tags represent the integrated repository release on `master`.
- Tag format: `vMAJOR.MINOR.PATCH` (example: `v0.10.0`).
- Tags are created after merges to `master` and verification.

## Release Workflow

1. Merge short-lived backend/frontend branches into `dev/backend` or `dev/frontend`.
2. Merge `dev/backend` and/or `dev/frontend` into `develop` when those changes are needed for continuation.
3. Promote `develop` into `master` via **squash merge** for release integration.
4. Update version numbers (backend + frontend) on `master` if a release boundary is reached.
5. Commit version update on `master`.
6. Create annotated tag on `master` (`vX.Y.Z`).
7. Rebase `develop` onto `master`, then rebase `dev/backend` and `dev/frontend` onto `develop`.

## Rules

- Keep frontend and backend version aligned by default.
- Use one commit for version bump changes.
- Do not tag from non-`master` branches.
- Do not skip central package version updates when adding new .NET packages.
- Keep `AGENTS.md` local-only (gitignored).

## Verification Gates

- Before commit/finalization on any branch:
  - Run all available linters.
  - Run all available type checks.
  - Run all relevant tests and verification scripts.
  - If you add or modify test contracts/test cases, run them and confirm they pass.
- Before promoting changes to `develop` or `master`:
  - Run the full available test/verification suite for the affected area.
  - Resolve failures first; do not promote with known failing checks.

### Frontend Test Gate Matrix

- Feature branch minimum:
  - `bun run lint`
  - `bunx nuxi typecheck`
  - `bun run test:smoke`
- CI/integration branch gate:
  - `bun run test:ci`
  - `test:ci` runs unit + component + e2e and excludes `@known-gap` tests.
- Release gate before `master` promotion:
  - `bun run lint`
  - `bunx nuxi typecheck`
  - `bun run test:unit`
  - `bun run test:nuxt`
  - `bun run test:e2e`

## Native Git Hooks (No Husky)

Repository-standard hooks are stored in `.githooks/` and do not require Node/Husky at root level.

- Install once per clone:
  - PowerShell: `pwsh -File tools/git-hooks/install-hooks.ps1`
  - Bash: `bash tools/git-hooks/install-hooks.sh`
- Hook behavior:
  - `pre-commit`:
    - Formats backend staged files using `dotnet format --include ...`.
    - Formats frontend staged files using `bunx eslint --fix ...`.
  - `pre-push`:
    - If backend files changed, runs backend build and tests.
    - If frontend files changed, runs frontend lint, typecheck, unit tests, and Nuxt tests.

This ensures project-specific formatting/tests run automatically based on changed paths.
