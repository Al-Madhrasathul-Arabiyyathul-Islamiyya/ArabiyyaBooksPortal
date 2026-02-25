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
   - Set certificate passwords for:
     - `JwtSettings__CertificatePassword`
     - `DataProtection__CertificatePassword`
   - Keep `infra/certs/` populated with your local certs:
     - `cert.pem` / `key.pem` (TLS cert for Traefik)
     - `signing.pfx` (JWT signing certificate)
     - `pfx_cert.pfx` (DataProtection key encryption certificate)
     - `signing-key.pem` / `signing-cert.pem` (frontend session cookie signing)

4. Ensure hostnames resolve locally (hosts file):
   - `127.0.0.1 bp.arabiyya.local`
   - `127.0.0.1 api-bp.arabiyya.local`

5. Build and run stack:

```powershell
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml build api
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml build frontend
docker compose -f infra/docker-compose.yml -f infra/docker-compose.local.yml up -d
```

6. Access:
- Frontend: `https://bp.arabiyya.local`
- API health: `https://api-bp.arabiyya.local/health`
- Traefik dashboard: `http://localhost:8081`

## Infra Layout

- `infra/docker-compose.yml`: baseline stack definition.
- `infra/docker-compose.local.sample.yml`: template for local overrides.
- `infra/docker-compose.local.yml`: local-only override (gitignored in `infra/.gitignore`).
- `infra/traefik/dynamic.yml`: TLS certificate mapping for local Traefik.
- `infra/env/.env.example`: starter env template.
- `infra/scripts/Initialize-BooksPortalDatabase.ps1`: DB/user bootstrap helper.

## Notes

- For production deployments, move secrets to environment/secret manager and avoid committed defaults.
- Current docker stack assumes SQL Server runs externally (host VM/server or managed SQL).
