# Git Hooks Setup

This repository uses native Git hooks via `core.hooksPath` and `.githooks/` (no Husky dependency).

## Install

- PowerShell:
  - `pwsh -File tools/git-hooks/install-hooks.ps1`
- Bash:
  - `bash tools/git-hooks/install-hooks.sh`

## Hook behavior

- `pre-commit`
  - Backend (`BooksPortal/*`): runs `dotnet format` only for staged backend files.
  - Frontend (`BooksPortalFrontEnd/*`): runs `bunx eslint --fix` only for staged frontend files.
- `pre-push`
  - Backend changes: `dotnet build` + backend unit/integration tests.
  - Frontend changes: `bun run lint`, `bunx nuxi typecheck`, `bun run test:unit`, `bun run test:nuxt`.

## Notes

- Hooks are path-aware and only run relevant checks for changed project areas.
- Full release verification before merge to `develop`/`master` is still required by workflow policy.
