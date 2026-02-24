# Arabiyya Academic Books Portal

Monorepo for backend (`BooksPortal`) and frontend (`BooksPortalFrontEnd`) services.

## Services

- Backend API: ASP.NET Core (`BooksPortal/src/BooksPortal.API`)
- Frontend: Nuxt 4 (`BooksPortalFrontEnd`)
- DB: SQL Server (external/hosted)

## Quick Start (Docker)

1. Bootstrap SQL database and application principal.

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

2. Create local Docker overrides (untracked file):

```powershell
Copy-Item infra/docker-compose.local.sample.yml infra/docker-compose.local.yml
```

3. Update secrets/connection values in `infra/docker-compose.local.yml`.

4. Build and run stack:

```powershell
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml up --build
```

5. Access:
- Frontend: `http://localhost:3000`
- API: `http://localhost:5071`

## Infra Layout

- `infra/docker-compose.yml`: baseline stack definition.
- `infra/docker-compose.local.sample.yml`: template for local overrides.
- `infra/docker-compose.local.yml`: local-only override (gitignored in `infra/.gitignore`).
- `infra/env/.env.example`: starter env template.
- `infra/scripts/Initialize-BooksPortalDatabase.ps1`: DB/user bootstrap helper.

## Notes

- For production deployments, move secrets to environment/secret manager and avoid committed defaults.
- Current docker stack assumes SQL Server runs externally (host VM/server or managed SQL).
