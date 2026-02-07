# BooksPortal Implementation Plan

## Overview
School book distribution management system built with .NET 10 Clean Architecture and Nuxt 4 frontend.

**Status Legend:** `[ ]` Todo | `[~]` In Progress | `[x]` Done

---

## Module 1: Core Infrastructure
> Foundation: BaseEntity, DbContext, Audit, Common patterns

### Domain Layer
- [x] Create `Common/BaseEntity.cs` (audit fields, soft delete)
- [x] Create `Common/IAuditableEntity.cs` interface
- [x] Create `Enums/` folder with all enums (MovementType, BookCondition, Term, etc.)

### Infrastructure Layer
- [x] Create `Data/BooksPortalDbContext.cs`
- [x] Configure SaveChangesAsync override for auto audit fields
- [x] Create `Data/Configurations/` folder
- [x] Setup global query filter for soft deletes
- [x] Create `Identity/Staff.cs` (extends IdentityUser<int>)

### Application Layer
- [x] Create `Common/Interfaces/IRepository.cs`
- [x] Create `Common/Interfaces/IUnitOfWork.cs`
- [x] Create `Common/Models/ApiResponse.cs` (standard response wrapper)
- [x] Create `Common/Models/PaginatedList.cs`
- [x] Setup Mapster configuration

### API Layer
- [x] Configure Program.cs (services, middleware, Swagger)
- [x] Create `appsettings.json` with connection strings
- [x] Setup Serilog configuration
- [x] Create base controller with common functionality

### Tests
- [x] Create `BooksPortal.UnitTests` project
- [x] Create `BooksPortal.IntegrationTests` project

---

## Module 2: Authentication & Users
> JWT auth, user management, roles

### Backend
- [x] Configure ASP.NET Core Identity in Program.cs
- [x] Configure JWT Bearer authentication
- [x] Create `IAuthService` interface
- [x] Create `AuthService` implementation (Login, Refresh, Logout)
- [x] Create `IUserService` interface
- [x] Create `UserService` implementation
- [x] Create `AuthController` (login, refresh, logout, me, change-password)
- [x] Create `UsersController` (CRUD)
- [x] Create DTOs: LoginRequest, TokenResponse, UserDto, CreateUserDto
- [x] Create validators for auth DTOs
- [x] Seed default SuperAdmin user

### Frontend (Phase 2 - Later)
- [ ] Setup Nuxt 4 project with PrimeVue
- [ ] Create auth store
- [ ] Create login page
- [ ] Create auth middleware

---

## Module 3: Master Data
> Reference data entities - Academic Years, Keystages, Subjects, Classes, Students, Parents, Teachers

### 3.1 Academic Year
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.2 Keystage
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.3 Subject
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.4 ClassSection
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.5 Student
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.6 Parent
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.7 StudentParent Junction
- [x] Entity, Configuration

### 3.8 Teacher
- [x] Entity, Configuration, DTOs, Service, Controller, Validator

### 3.9 TeacherAssignment
- [x] Entity, Configuration, DTOs, Service, Controller

### 3.10 Lookup Endpoints
- [x] `LookupsController` for dropdown data

---

## Module 4: Book Management
> Book catalog, stock entries, stock movements

### Entities
- [x] Entity: `Book.cs` (with stock fields)
- [x] Entity: `StockEntry.cs`
- [x] Entity: `StockMovement.cs`
- [x] Configurations for all entities

### Services
- [x] `IBookService` interface
- [x] `BookService` implementation
  - [x] CRUD operations
  - [x] `AddStockAsync` method
  - [x] `AdjustStockAsync` method
  - [x] `SearchAsync` method

### Controllers & DTOs
- [x] `BooksController`
- [x] DTOs: BookResponse, CreateBookRequest, AddStockRequest, AdjustStockRequest, StockEntryResponse, StockMovementResponse
- [x] Validators

### Tests
- [ ] Unit tests for stock calculations
- [ ] Integration tests for stock operations

---

## Module 5: Distribution
> Create distribution slips, stock deduction

### Entities
- [x] Entity: `DistributionSlip.cs`
- [x] Entity: `DistributionSlipItem.cs`
- [x] Entity: `ReferenceCounter.cs` (shared by Modules 5, 6, 7)
- [x] Configurations

### Services
- [x] `IReferenceNumberService` interface + implementation
- [x] `IDistributionService` interface
- [x] `DistributionService` implementation
  - [x] Create slip with stock validation
  - [x] Reference number generation (DST{YEAR}{SEQ})
  - [x] Cancel slip with stock reversal

### Controllers & DTOs
- [x] `DistributionsController`
- [x] DTOs: DistributionSlipResponse, CreateDistributionSlipRequest, etc.
- [x] Validators

### Wiring
- [x] DbSets in BooksPortalDbContext
- [x] DI registrations
- [x] Migration

### Tests
- [x] Unit tests (6 tests)
- [ ] Integration tests

---

## Module 6: Returns
> Return slips, stock restoration, condition tracking

### Entities
- [x] Entity: `ReturnSlip.cs`
- [x] Entity: `ReturnSlipItem.cs`
- [x] Configurations

### Services
- [x] `IReturnService` interface
- [x] `ReturnService` implementation
  - [x] Create return with condition assessment
  - [x] Reference number generation (RTN{YEAR}{SEQ})
  - [x] Update damage/lost counters

### Controllers & DTOs
- [x] `ReturnsController`
- [x] DTOs
- [x] Validators

### Tests
- [x] Unit tests (15 tests: apply/reverse for all conditions, round-trip, create)
- [ ] Integration tests

---

## Module 7: Teacher Issues
> Teacher book loans, partial returns

### Entities
- [x] Entity: `TeacherIssue.cs`
- [x] Entity: `TeacherIssueItem.cs` (with OutstandingQuantity computed property)
- [x] Configurations

### Services
- [x] `ITeacherIssueService` interface
- [x] `TeacherIssueService` implementation
  - [x] Issue books to teachers (WithTeachers increment)
  - [x] Process partial returns (WithTeachers decrement, status update)
  - [x] Status tracking (Active → Partial → Returned)
  - [x] Cancel (reverses only outstanding quantities)

### Controllers & DTOs
- [x] `TeacherIssuesController` (CRUD + POST /{id}/return)
- [x] DTOs (TeacherIssueResponse, CreateTeacherIssueRequest, ProcessTeacherReturnRequest)
- [x] Validators

### Tests
- [x] Unit tests (13 tests: DetermineStatus, OutstandingQuantity, stock effects, validation)
- [ ] Integration tests

---

## Module 8: Reports & Audit
> Stock reports, distribution reports, audit logs, Excel export

### Services
- [x] `IReportService` interface
- [x] `ReportService` implementation
  - [x] Stock summary (with subject/grade filter)
  - [x] Distribution summary (by academic year, date range)
  - [x] Teacher outstanding (with teacher filter)
  - [x] Student history (distributions + returns)
  - [x] Excel export via ClosedXML (stock, distribution, teacher outstanding)

### Controllers
- [x] `ReportsController` (JSON + Excel export endpoints)
- [x] `AuditLogsController` [Admin] (paginated, filterable)

### Audit Log
- [x] Entity: `AuditLog.cs` (created in Module 1)
- [x] Auto-write in DbContext.SaveChangesAsync (Module 1)
- [x] `IAuditLogService` / `AuditLogService` query interface

---

## Module 9: Print & Enhancements ✅
> PDF generation, configurable reference numbers, DB-stored labels, PDF storage, active academic year

Branch: `feature/module-9-print` → merged to dev → merged to master (v0.9.0)

### 9.0 Documentation & Housekeeping ✅
- [x] Add `documentation/07-reference-number-system.md`
- [x] Add `documentation/08-slip-templates-and-storage.md`
- [x] Update `plans/implementation-plan.md` to reflect enhancements
- [ ] Update `documentation/03-api-specification.md` with new endpoints (deferred)

### 9.1 Basic PdfService (initial scaffold) ✅
- [x] `IPdfService` interface (Application layer)
- [x] `PdfService` implementation (API layer)
- [x] QuestPDF Community license config in Program.cs
- [x] DI registration
- [x] Print endpoints in controllers (Distribution, Return, TeacherIssue, TeacherReturn)

### 9.2 Active Academic Year Endpoint ✅
- [x] `GetActiveAsync()` on `IAcademicYearService` + implementation
- [x] `GET /api/academicyears/active` endpoint

### 9.3 Customizable Reference Numbers ✅
- [x] `SlipType` enum (Distribution, Return, TeacherIssue, TeacherReturn)
- [x] `ReferenceNumberFormat` entity + configuration
- [x] `TeacherReturnSlip` + `TeacherReturnSlipItem` entities + configurations
- [x] Rewritten `ReferenceNumberService` with format template + token parsing
- [x] Updated Distribution/Return/TeacherIssue services to use new signature
- [x] `TeacherIssueService.ProcessReturnAsync` creates `TeacherReturnSlip` with ref number
- [x] Admin API: `ReferenceNumberFormatsController` [Admin] — CRUD + DELETE [SuperAdmin]
- [x] Unit tests: template parsing, fallback, token substitution

### 9.4 Slip Template Settings — DB-Stored Labels ✅
- [x] `SlipTemplateSetting` entity + configuration (Category/Key/Value/SortOrder)
- [x] `ISlipTemplateSettingService` + `SlipTemplateSettingService`
- [x] `SlipTemplateSettingsController` [Admin] — GET, PUT, POST /reset [SuperAdmin]
- [x] Seed default Thaana labels in `SeedData.cs`
- [x] Unit tests for default labels

### 9.5 PDF Template Redesign — Thaana ✅
- [x] Faruma font + logo bundled as build assets (`Assets/` with `CopyToOutputDirectory`)
- [x] PNG logo (SVG rendered as black silhouette in QuestPDF)
- [x] Faruma font registered in Program.cs from `BaseDirectory/Assets/Faruma.ttf`
- [x] A4 landscape, dual-copy layout, RTL Thaana text
- [x] All 4 slip templates: Distribution, Return, TeacherIssue, TeacherReturn
- [x] Header: PNG logo + school name/subtitle in Thaana
- [x] Consistent row heights (MinHeight 12), BookTitle right-aligned
- [x] Signature blocks pinned to page bottom (`ExtendVertical().AlignBottom()`)
- [x] Tested up to 16 items per slip

### 9.6 PDF Storage on Disk ✅
- [x] `PdfFilePath` added to DistributionSlip, ReturnSlip, TeacherIssue, TeacherReturnSlip
- [x] `SlipStorage:BasePath` in appsettings.json
- [x] `SlipStorageService` with `SanitizeFileName` (handles slashes in ref numbers)
- [x] Auto-generation on slip creation, stored file served with fallback to regenerate
- [x] Path format: `{BasePath}/{SlipType}/{AcademicYear}/{ReferenceNo}.pdf`

### 9.7 Final Verification ✅
- [x] 88 unit tests passing + 1 integration test
- [x] Build: 0 errors, 0 warnings
- [x] Merged to dev, merged to master, tagged v0.9.0

---

## Module 10: Frontend (Nuxt 4)
> Feature-based modular implementation — see `plans/frontend-plan.md`

### Status: Planning

---

## Deployment
- [ ] Dockerfile.api
- [ ] Dockerfile.client
- [ ] docker-compose.yml
- [ ] Traefik configuration

---

## Deferred Items
- **Slip revision** (edit within 24h of creation) → implement post-frontend
- **API spec docs** (`documentation/03-api-specification.md`) — update with Module 9 endpoints
- **appsettings.json audit** — environment-specific overrides for DB, JWT, storage paths

---

## Current Focus
> Backend complete (Modules 1–9). Moving to frontend (Module 10).

Modules 1–9 complete and merged to master (v0.1.0 – v0.9.0).
Hotfix applied: master data deletion restricted to SuperAdmin.
88 unit tests + 1 integration test passing.
