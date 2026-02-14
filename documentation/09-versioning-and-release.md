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

1. Merge feature branches into `dev`.
2. Merge `dev` into `master`.
3. Update version numbers (backend + frontend) if release boundary reached.
4. Commit version update on `master`.
5. Create annotated tag on `master` (`vX.Y.Z`).
6. Rebase `dev` onto `master`, then `frontend` onto `dev`.

## Rules

- Keep frontend and backend version aligned by default.
- Use one commit for version bump changes.
- Do not tag from non-`master` branches.
- Do not skip central package version updates when adding new .NET packages.
