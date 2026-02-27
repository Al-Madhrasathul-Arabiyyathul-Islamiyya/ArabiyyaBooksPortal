# Books Portal - Technical Documentation

## Overview

Books Portal is a web-based textbook management system for Arabiyya school. It tracks book inventory, handles distribution to students via parents, manages returns, facilitates teacher loans, and provides reporting and audit capabilities.

---

## Documentation Index

| Document | Description |
|----------|-------------|
| [01-architecture-overview.md](./01-architecture-overview.md) | Solution architecture, technology stack, project structure |
| [02-database-schema.md](./02-database-schema.md) | Entity definitions, SQL schema, relationships |
| [03-api-specification.md](./03-api-specification.md) | Original REST API specification (design phase) |
| [04-implementation-plan.md](./04-implementation-plan.md) | Phased development plan, tasks, deliverables |
| [05-frontend-structure.md](./05-frontend-structure.md) | Nuxt project structure, components, composables |
| [06-schema-addendum-unicode-timezone.md](./06-schema-addendum-unicode-timezone.md) | Schema addendums for unicode and timezone |
| [07-reference-number-system.md](./07-reference-number-system.md) | Customizable reference number format system |
| [08-slip-templates-and-storage.md](./08-slip-templates-and-storage.md) | Slip template settings and PDF storage |
| [09-versioning-and-release.md](./09-versioning-and-release.md) | Versioning policy for backend/frontend and git release tags |
| [10-frontend-validation-architecture.md](./10-frontend-validation-architecture.md) | Frontend validation architecture (Regle + Zod + error normalization) |
| [11-backup-and-scheduled-reports-architecture.md](./11-backup-and-scheduled-reports-architecture.md) | Post-v1 scheduler architecture for backups, report exports, and admin monitoring |
| [12-frontend-testing-strategy.md](./12-frontend-testing-strategy.md) | Frontend testing strategy (Vitest + Playwright), modular rollout, and merge/release gates |
| [13-bootstrap-and-readiness-architecture.md](./13-bootstrap-and-readiness-architecture.md) | Pre-v1 bootstrap/readiness architecture to enforce mandatory initial setup before operations |
| [api-reference.md](./api-reference.md) | **Live API reference** - all endpoints, DTOs, enums |

---

## Quick Reference

### Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 LTS, C# 14, Entity Framework Core |
| Database | SQL Server 2022 |
| Frontend | Nuxt 4.3, Vue 3.5, TypeScript |
| UI Library | PrimeVue 4.5 |
| State | Pinia 3 + Pinia Colada (server-state) |
| Validation | Regle (frontend), FluentValidation (backend) |
| PDF | QuestPDF |
| Excel | ClosedXML |

### Key Features

1. **Book Administration** - Register books, manage inventory, track stock levels
2. **Distribution** - Issue books to students via parents, generate PDF slips
3. **Returns** - Receive books back, assess condition, update inventory
4. **Teacher Issues** - Loan books to teachers with expected return tracking
5. **Reports** - Stock summaries, distribution reports, Excel exports
6. **Audit** - Full audit trail with before/after values
7. **Settings** - Customizable reference numbers and slip template labels

### User Roles

| Role | Permissions |
|------|-------------|
| SuperAdmin | Full access including user management and destructive deletes |
| Admin | All operations except user management |
| User | Standard operations (distribution, returns, view reports) |

---

## Project Status

| Component | Status | Version |
|-----------|--------|---------|
| Backend (core modules + follow-ups) | Active development | v0.10.x |
| Backend Tests | Active (unit/integration/contract) | - |
| Frontend Core (Phases 1-4) | Implemented | - |
| Frontend Remaining | Verification debt + lifecycle/revision UX closure | - |

---

## Repository Structure

```
BookPortal/
  BooksPortal/               # Backend (.NET solution)
    src/
      BooksPortal.API/       # Web API, controllers
      BooksPortal.Application/  # Services, DTOs, interfaces
      BooksPortal.Domain/    # Entities, enums
      BooksPortal.Infrastructure/  # EF Core, repositories
    tests/
      BooksPortal.Tests/     # Unit tests (xUnit)
  BooksPortalFrontEnd/       # Frontend (Nuxt 4)
  documentation/             # Project documentation
  designs/                   # UI mockups
  plans/                     # Feature plans (kanban)
```

## Planning Conventions

- Canonical execution plans are in numbered files under `plans/` (`01-...`, `02-...`, ...).
- Legacy plan filenames are kept as pointer stubs; full historical content is stored in `plans/archive/`.
- Frontend active roadmap: `plans/01-frontend-core-roadmap.md`.
