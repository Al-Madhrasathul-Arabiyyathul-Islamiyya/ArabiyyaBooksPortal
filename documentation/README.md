# Books Portal - Technical Documentation

## Overview

Books Portal is a comprehensive web-based textbook management system for schools. It tracks book inventory, handles distribution to students via parents, manages returns, facilitates teacher loans, and provides detailed reporting and audit capabilities.

---

## Documentation Index

| Document | Description |
|----------|-------------|
| [01-architecture-overview.md](./01-architecture-overview.md) | Solution architecture, technology stack, project structure |
| [02-database-schema.md](./02-database-schema.md) | Entity definitions, SQL schema, relationships |
| [03-api-specification.md](./03-api-specification.md) | REST API endpoints, request/response formats |
| [04-implementation-plan.md](./04-implementation-plan.md) | Phased development plan, tasks, deliverables |
| [05-frontend-structure.md](./05-frontend-structure.md) | Nuxt project structure, components, composables |
| [06-schema-addendum-unicode-timezone](./06-schema-addendum-unicode-timezone.md) | Schema addendums for unicode and timezone |

---

## Quick Reference

### Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 LTS, C# 14, Entity Framework Core |
| Database | SQL Server 2022 |
| Frontend | Nuxt 4.3.0, Vue 3, TypeScript |
| UI Library | PrimeVue 4.x |
| Deployment | Docker, Traefik |

### Key Features

1. **Book Administration** - Register books, manage inventory, track stock
2. **Distribution** - Issue books to students via parents, generate slips
3. **Returns** - Receive books back, assess condition, update inventory
4. **Teacher Issues** - Loan books to teachers with tracking
5. **Reports** - Stock summaries, audit logs, visual dashboards

### User Roles

| Role | Permissions |
|------|-------------|
| SuperAdmin | Full access, user management |
| Admin | All operations except user management |
| User | Standard operations (distribution, returns, etc.) |

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- SQL Server 2022
- Docker & Docker Compose

### Development Setup

```bash
# Clone repository
git clone https://github.com/school/books-portal.git
cd books-portal

# Backend
cd src/BooksPortal.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend
cd client
npm install
npm run dev
```

### Docker Deployment

```bash
cd docker
cp .env.example .env
# Edit .env with your settings
docker-compose up -d
```

---

## Project Timeline

| Phase | Module | Duration |
|-------|--------|----------|
| 1 | Core Infrastructure | 1 week |
| 2 | Authentication | 1 week |
| 3 | Master Data | 1 week |
| 4 | Book Management | 1 week |
| 5 | Distribution | 1 week |
| 6 | Returns | 1 week |
| 7 | Teacher Issues | 1 week |
| 8 | Reports & Dashboard | 1.5 weeks |
| 9 | Print Module | 0.5 week |

**Total: 9-10 weeks**

---

## Contact

For questions or clarifications, contact the development team.
