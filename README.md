# Arabiyya Academic Books Portal

Monorepo for the Arabiyya school textbook operations and administration system.

## Release Baseline

- Backend version: `1.0.0` (`BooksPortal/Directory.Build.props`)
- Frontend version: `1.0.0` (`BooksPortalFrontEnd/package.json`)
- Primary release branch: `master`

## Repository Structure

- `BooksPortal/`:
  ASP.NET Core API + application/domain/infrastructure layers + tests.
- `BooksPortalFrontEnd/`:
  Nuxt 4 frontend + BFF API routes + Playwright/Vitest tests.
- `documentation/`:
  architecture, API contract, planning, and release docs.
- `infra/`:
  Docker Compose, local reverse proxy (Traefik), and environment templates.
- `tools/`:
  developer scripts and git hook installer.

## Core Services

- Backend API: `BooksPortal/src/BooksPortal.API`
- Frontend app: `BooksPortalFrontEnd`
- Database: SQL Server (external/host-hosted or managed)

## Local Development (Non-Docker)

1. Backend:
   `dotnet run --project BooksPortal/src/BooksPortal.API`
2. Frontend:
   `cd BooksPortalFrontEnd`
   `bun install`
   `bun run dev`

## Local Docker Quick Start

1. Initialize database and app principal.

```powershell
./infra/scripts/Initialize-BooksPortalDatabase.ps1 `
  -UseWindowsAuth `
  -SqlServer "localhost,1433" `
  -DatabaseName "BooksPortal" `
  -AppAuthMode SqlLogin `
  -AppLoginName "booksportal_app" `
  -AppLoginPassword "ChangeThis!123" `
  -GrantDbOwner
```

2. Create local override file (untracked):

```powershell
Copy-Item infra/docker-compose.local.sample.yml infra/docker-compose.local.yml
```

3. Configure secrets and certs in `infra/docker-compose.local.yml` and `infra/certs/`.

4. Add hosts entries:
   - `127.0.0.1 bp.arabiyya.local`
   - `127.0.0.1 api-bp.arabiyya.local`

5. Build and run:

```powershell
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml build api
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml build frontend
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml up -d
```

6. Access:
   - Frontend: `https://bp.arabiyya.local`
   - API health: `https://api-bp.arabiyya.local/health`
   - Traefik dashboard: `http://localhost:8081`

## Initial Setup Flow

For fresh production-style databases, the app uses Setup Center:

1. Bootstrap SuperAdmin account
2. Configure/confirm slip templates
3. Configure/confirm master data hierarchy and active academic year
4. Configure/confirm reference number formats
5. Complete setup

Until setup completes, operations are blocked by readiness guards.

## Verification Commands

Backend:
- `dotnet build BooksPortal/BooksPortal.slnx`
- `dotnet test BooksPortal/BooksPortal.slnx`

Frontend:
- `cd BooksPortalFrontEnd`
- `bun run lint`
- `bunx nuxi typecheck`
- `bun run test:unit`
- `bun run test:e2e:smoke` (or full e2e where needed)

## Documentation Index

- Architecture and design: `documentation/01-architecture-overview.md`
- API contract: `documentation/api-reference.md`
- Frontend structure: `documentation/05-frontend-structure.md`
- Backend structure: `documentation/06-backend-structure.md`
- Versioning and release: `documentation/09-versioning-and-release.md`
- Bootstrap setup architecture: `documentation/13-bootstrap-and-readiness-architecture.md`

## Security and Deployment Notes

- Keep production secrets out of repo.
- Use environment/secret manager for certificates, JWT signing material, and DB credentials.
- Prefer non-destructive schema evolution for migrations (additive changes and phased cutover).
