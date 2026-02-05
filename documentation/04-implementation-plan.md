# Books Portal - Implementation Plan

## Overview

This document outlines the phased implementation approach for the Books Portal system. Each phase is designed to be independently deployable, allowing for incremental delivery and testing.

---

## Phase Summary

| Phase | Module | Duration | Priority |
|-------|--------|----------|----------|
| 1 | Core Infrastructure | 1 week | Critical |
| 2 | Authentication & Users | 1 week | Critical |
| 3 | Master Data | 1 week | Critical |
| 4 | Book Management | 1 week | Critical |
| 5 | Distribution | 1 week | Critical |
| 6 | Returns | 1 week | Critical |
| 7 | Teacher Issues | 1 week | High |
| 8 | Reports & Dashboard | 1.5 weeks | High |
| 9 | Print Module | 0.5 week | Medium |
| 10 | OpenID Integration | Future | Low |

**Total Estimated Duration:** 9-10 weeks

---

## Phase 1: Core Infrastructure

### Objectives
- Set up solution structure
- Configure database and EF Core
- Implement base patterns and utilities
- Set up Docker configuration

### Backend Tasks

#### 1.1 Solution Setup

Create Clean Architecture solution with 4 projects:
- `BooksPortal.Domain` — Entities, enums, domain logic
- `BooksPortal.Application` — Services, DTOs, interfaces, validators
- `BooksPortal.Infrastructure` — EF Core, repositories, external services
- `BooksPortal.API` — Controllers, middleware, auth config

#### 1.2 NuGet Packages

| Project | Packages |
|---------|----------|
| Domain | (none) |
| Application | FluentValidation, Mapster |
| Infrastructure | EF Core SqlServer, Identity.EntityFrameworkCore, Serilog.Sinks.MSSqlServer |
| API | JwtBearer, FluentValidation.AspNetCore, Serilog.AspNetCore, Swashbuckle |

#### 1.3 Key Implementation Points

- **BaseEntity** — Common audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt)
- **IRepository<T>** — Generic repository interface
- **IUnitOfWork** — Transaction management
- **DbContext** — Override SaveChangesAsync to auto-populate audit fields
- **Global Query Filter** — Exclude soft-deleted records by default
- **SaveChanges Interceptor** — Auto-generate AuditLog entries on entity changes

### Frontend Tasks

#### 1.6 Nuxt Project Setup

Initialize Nuxt 4 project with dependencies:
- `@primevue/nuxt-module` — UI components
- `@pinia/nuxt` — State management
- `@vueuse/nuxt` — Composition utilities
- `@nuxtjs/tailwindcss` — Styling
- `zod` — Validation
- `dayjs` — Date handling

#### 1.7 Key Configuration

- Configure API base URL via `runtimeConfig`
- Set up PrimeVue module with custom theme
- Create `useApi` composable for authenticated API calls

### DevOps Tasks

#### 1.8 Docker Configuration

Create Docker setup with:
- `Dockerfile.api` — Multi-stage build for .NET API
- `Dockerfile.client` — Multi-stage build for Nuxt
- `docker-compose.yml` — Services: api, client, db (SQL Server), traefik

**Traefik routing:**
- `books.school.local` → Nuxt client
- `books.school.local/api/*` → .NET API

### Deliverables
- [x] Solution structure created
- [x] Base entities and interfaces
- [x] DbContext with soft delete and audit
- [x] Nuxt project initialized
- [x] Docker configuration

---

## Phase 2: Authentication & Users

### Objectives
- Implement JWT authentication
- User management (CRUD)
- Role-based authorization
- Password management

### Backend Tasks

#### 2.1 Authentication Setup

- Configure JWT Bearer authentication in Program.cs
- Create `IAuthService` with Login, Refresh, Logout methods
- Implement token generation with claims (userId, roles)
- Store refresh tokens in database
- Create auth endpoints controller

#### 2.2 User Management

- Extend ASP.NET Core Identity `IdentityUser<int>` as `Staff`
- Create `IUserService` for CRUD operations
- Implement role assignment/removal
- Password reset functionality (admin-initiated)

### Frontend Tasks

#### 2.3 Auth Implementation

- `useAuthStore` — Manages user state, token storage (cookie), login/logout
- `auth.ts` middleware — Redirects unauthenticated users to login
- `role.ts` middleware — Checks role requirements from route meta
- Login page with username/password form
- User management page (Admin only)

### Deliverables
- [ ] JWT authentication working
- [ ] Login/logout/refresh endpoints
- [ ] User CRUD endpoints
- [ ] Role management
- [ ] Login page
- [ ] Auth middleware
- [ ] User management page (Admin)

---

## Phase 3: Master Data

### Objectives
- CRUD for all reference data entities
- Lookup endpoints
- Data seeding

### Entities to Implement
1. Academic Years
2. Keystages
3. Subjects
4. Class Sections
5. Students
6. Parents
7. Student-Parent relationships
8. Teachers
9. Teacher Assignments

### Backend Pattern (Repeated for each entity)

```csharp
// Example: Subject
// 1. Entity (Domain)
// 2. Configuration (Infrastructure)
// 3. Repository interface (Application)
// 4. Repository implementation (Infrastructure)
// 5. Service interface (Application)
// 6. Service implementation (Application)
// 7. DTOs (Application)
// 8. Validator (Application)
// 9. Controller (API)
```

### Frontend Pattern

```
pages/
  master-data/
    academic-years/
      index.vue       # List with DataTable
      [id].vue        # View/Edit form
      create.vue      # Create form
    subjects/
      ...
    class-sections/
      ...
    students/
      ...
    parents/
      ...
    teachers/
      ...
```

### Deliverables
- [x] All master data CRUD working
- [x] Lookup endpoints
- [x] Frontend pages for all entities
- [x] Data validation
- [x] Seed data for testing

---

## Phase 4: Book Management

### Objectives
- Book catalog CRUD
- Stock entry functionality
- Stock calculations
- Stock movement tracking

### Backend Tasks

#### 4.1 Book Service

Key operations:
- **AddStockAsync** — Add stock entry, update Book.TotalStock, create StockMovement record
- **AdjustStockAsync** — Manual adjustment (damaged, lost), update relevant counters
- **GetStockSummaryAsync** — Calculate totals across filters

Stock update flow:
1. Validate book exists
2. Get active academic year
3. Create StockEntry record
4. Update Book quantity fields
5. Create StockMovement audit record
6. Save all in single transaction

### Frontend Tasks

#### 4.2 Key Components

- **BookSearch** — Autocomplete search by title/code/ISBN
- **StockBadge** — Color-coded availability indicator (green/yellow/red)
- **StockEntryDialog** — Form for adding stock (quantity, source, notes)
- **StockAdjustDialog** — Admin form for manual adjustments

### Deliverables
- [ ] Book CRUD
- [ ] Stock entry functionality
- [ ] Stock movement logging
- [ ] Stock summary calculations
- [ ] Book search with filters
- [ ] Stock adjustment (Admin)

---

## Phase 5: Distribution

### Objectives
- Create distribution slips
- Automatic stock deduction
- Reference number generation
- Print-ready slip generation

### Key Flow

1. Staff searches and selects student
2. System shows linked parents for selection
3. Staff selects term (1, 2, or Both)
4. Staff searches and adds books with quantities
5. System validates stock availability
6. On submit:
   - Generate reference number (DST{YEAR}{SEQUENCE})
   - Create DistributionSlip and DistributionSlipItems
   - Update Book.Distributed for each item
   - Create StockMovement records
   - Log to AuditLog
7. Return slip ready for printing

### Frontend Pages

- `/distribution` — List with filters (date, class, student)
- `/distribution/create` — Multi-step form
- `/distribution/[id]` — View details
- `/distribution/print/[id]` — Print layout

### Deliverables
- [ ] Distribution slip creation
- [ ] Student/parent lookup
- [ ] Book selection with stock check
- [ ] Stock deduction on save
- [ ] Reference number generation
- [ ] Slip cancellation (with stock reversal)
- [ ] Print preview

---

## Phase 6: Returns

### Objectives
- Create return slips
- Stock restoration
- Condition recording
- Damage/loss tracking

### Key Differences from Distribution
- Can mark books as damaged or lost
- Damaged/lost books update respective counters instead of Available
- Optional link to original distribution

### Deliverables
- [x] Return slip creation
- [x] Condition assessment per book
- [x] Stock restoration
- [x] Damage/loss recording
- [x] Print preview

---

## Phase 7: Teacher Issues

### Objectives
- Teacher book loans
- Partial returns support
- Outstanding tracking
- Overdue alerts

### Key Features
- Track issued vs returned quantities per item
- Status updates (Active → Partial → Returned)
- Expected return date tracking

### Deliverables
- [x] Teacher issue creation
- [x] Partial return processing
- [x] Outstanding list
- [x] Status tracking

---

## Phase 8: Reports & Dashboard

### Objectives
- Dashboard with key metrics
- Stock reports
- Distribution reports
- Movement audit log
- Export functionality

### Dashboard Components

- **Summary Cards** — Total books, distributed, available, with teachers
- **Distribution Chart** — Bar chart by grade/class
- **Recent Activity** — Latest distributions/returns
- **Low Stock Alert** — Books below threshold

### Reports List

| Report | Description |
|--------|-------------|
| Stock Summary | Overall inventory by subject/grade |
| Distribution Report | Distributions by date range, class, student |
| Return Report | Returns with condition breakdown |
| Teacher Outstanding | Books currently with teachers |
| Stock Movement Audit | Full movement history with filters |
| Class Distribution | Per-class book allocation summary |

### Deliverables
- [ ] Dashboard with charts
- [ ] All report pages
- [ ] Filtering and date ranges
- [ ] Excel export (using ClosedXML)
- [ ] Audit log viewer

---

## Phase 9: Print Module

### Objectives
- Printable slip generation
- A5 format support
- Dual copy (school/parent)
- PDF generation

### Implementation

Use **QuestPDF** library for PDF generation:
- Create slip templates for Distribution, Return, Teacher Issue
- A5 page size with school header
- Two copies per page (school copy, parent copy)
- Include: reference number, date, student/parent details, book list, signature lines

### Deliverables
- [ ] Distribution slip PDF
- [ ] Return slip PDF
- [ ] Teacher issue slip PDF
- [ ] Print preview in browser
- [ ] Direct print option

---

## Phase 10: OpenID Integration (Future)

### Objectives
- External identity provider support
- Link existing users to external identities
- SSO capability

### Implementation Notes

ASP.NET Core Identity already supports external providers. When ready:
- Add `AddOpenIdConnect` to authentication config
- Create account linking flow for existing users
- Add "Login with SSO" button to login page
- Handle token exchange

### Deliverables
- [ ] OpenID Connect configuration
- [ ] User linking mechanism
- [ ] SSO login option
- [ ] Token exchange flow

---

## Testing Strategy

### Unit Tests
- Service layer logic
- Validators
- Mapping profiles

### Integration Tests
- API endpoints
- Database operations
- Authentication flow

### E2E Tests (Optional)
- Critical user flows
- Using Playwright or Cypress

---

## Deployment Checklist

- [ ] Environment variables configured
- [ ] Database migrations applied
- [ ] Default admin user seeded
- [ ] SSL certificates (if needed)
- [ ] Traefik routing configured
- [ ] Backup strategy in place
- [ ] Monitoring/logging configured
