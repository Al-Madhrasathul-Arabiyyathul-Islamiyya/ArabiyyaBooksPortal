# Books Portal - Architecture Overview

## Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend Framework | .NET | 10.0 LTS |
| Backend Language | C# | 14 |
| Database | SQL Server | 2022 |
| ORM | Entity Framework Core | 10.x |
| Frontend Framework | Nuxt | 4.3.0 |
| Frontend Language | TypeScript | 5.x |
| UI Components | PrimeVue | 4.x |
| State Management | Pinia | 2.x |
| Containerization | Docker | Latest |
| Reverse Proxy | Traefik | Latest |

---

## Architecture Pattern

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation                          │
│                    (Nuxt 4 SPA Client)                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                         API Layer                            │
│                  (ASP.NET Core Web API)                      │
│         Controllers, Middleware, Filters, Auth               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│       Services, DTOs, Interfaces, Validation, Mapping        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│            Entities, Enums, Domain Logic                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│     EF Core DbContext, Repositories, External Services       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Database                              │
│                    SQL Server 2022                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Solution Structure

```
BooksPortal/
├── src/
│   ├── BooksPortal.Domain/                 # Domain entities, enums
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── Common/
│   │
│   ├── BooksPortal.Application/            # Business logic, interfaces
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Mappings/
│   │   │   └── Behaviors/
│   │   ├── Features/
│   │   │   ├── Auth/
│   │   │   ├── Books/
│   │   │   ├── Distribution/
│   │   │   ├── Returns/
│   │   │   ├── TeacherIssues/
│   │   │   ├── MasterData/
│   │   │   └── Reports/
│   │   └── DTOs/
│   │
│   ├── BooksPortal.Infrastructure/         # Data access, external services
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   └── BooksPortalDbContext.cs
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── Identity/
│   │
│   └── BooksPortal.API/                    # Web API host
│       ├── Controllers/
│       ├── Middleware/
│       ├── Filters/
│       └── Program.cs
│
├── client/                                  # Nuxt 4 Frontend
│   ├── assets/
│   ├── components/
│   │   ├── common/
│   │   ├── books/
│   │   ├── distribution/
│   │   ├── returns/
│   │   ├── teachers/
│   │   └── reports/
│   ├── composables/
│   ├── layouts/
│   ├── middleware/
│   ├── pages/
│   ├── plugins/
│   ├── stores/
│   ├── types/
│   └── nuxt.config.ts
│
├── tests/
│   ├── BooksPortal.UnitTests/
│   └── BooksPortal.IntegrationTests/
│
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.client
│   └── docker-compose.yml
│
└── docs/
    └── (documentation files)
```

---

## Key Design Decisions

### 1. Authentication Strategy

**Phase 1 (Current):** ASP.NET Core Identity with JWT Bearer tokens
- Username/password authentication
- Role-based authorization (SuperAdmin, Admin, User)
- JWT tokens for API access
- Refresh token support

**Phase 2 (Future):** OpenID Connect integration
- ASP.NET Core Identity already supports external providers
- Add OpenID Connect configuration when ready
- Existing users can be linked to external identities

### 2. Academic Year Segregation

- All transactional data includes `AcademicYearId` foreign key
- Application context tracks "active" academic year
- Users can switch academic year context for viewing historical data
- Reports can span multiple years or filter by specific year

### 3. Stock Calculation Strategy

Stock quantities are **denormalized** on the `Book` entity for performance:
- `TotalStock`, `Distributed`, `WithTeachers`, `Damaged`, `Lost`, `Available`
- Updated via database triggers OR application service layer
- `StockMovement` table provides audit trail for recalculation if needed

### 4. Soft Deletes

Critical entities use soft delete pattern:
- `IsDeleted` flag + `DeletedAt` timestamp
- Global query filter excludes deleted records by default
- Audit log captures who deleted and when

### 5. Audit Logging

Two-tier approach:
- **Entity-level:** EF Core interceptors capture Create/Update/Delete
- **Action-level:** Explicit logging for business operations (Distribution, Returns, etc.)

---

## Security Considerations

| Concern | Implementation |
|---------|----------------|
| Authentication | JWT Bearer tokens with short expiry + refresh tokens |
| Authorization | Role-based + resource-based where needed |
| Password Storage | ASP.NET Core Identity (PBKDF2 by default) |
| API Security | HTTPS only, CORS configured for Nuxt client |
| SQL Injection | EF Core parameterized queries |
| XSS | Nuxt built-in sanitization + CSP headers |
| CSRF | Not applicable (JWT, not cookies) |
| Rate Limiting | ASP.NET Core rate limiting middleware |

---

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     School Intranet                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Traefik                               │
│              (Reverse Proxy / Load Balancer)                 │
│                   books.school.local                         │
└─────────────────────────────────────────────────────────────┘
                    │                   │
                    ▼                   ▼
        ┌───────────────────┐ ┌───────────────────┐
        │   Nuxt Client     │ │   .NET API        │
        │   (Container)     │ │   (Container)     │
        │   Port 3000       │ │   Port 5000       │
        └───────────────────┘ └───────────────────┘
                                        │
                                        ▼
                              ┌───────────────────┐
                              │   SQL Server      │
                              │   (Host/Container)│
                              │   Port 1433       │
                              └───────────────────┘
```

---

## External Libraries

### Backend (.NET)

| Package | Purpose |
|---------|---------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | User management, authentication |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT token validation |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider |
| `FluentValidation.AspNetCore` | Request validation |
| `Mapster` | Object mapping (faster than AutoMapper) |
| `Serilog.AspNetCore` | Structured logging |
| `QuestPDF` | PDF generation for slips |
| `ClosedXML` | Excel export for reports |

### Frontend (Nuxt)

| Package | Purpose |
|---------|---------|
| `@primevue/nuxt-module` | PrimeVue integration |
| `@pinia/nuxt` | State management |
| `@vueuse/nuxt` | Vue composition utilities |
| `zod` | Runtime type validation |
| `dayjs` | Date manipulation |
