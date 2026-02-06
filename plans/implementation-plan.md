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

## Module 9: Print & Enhancements
> PDF generation, configurable reference numbers, DB-stored labels, PDF storage, active academic year

Branch: `feature/module-9-print`

### 9.0 Documentation & Housekeeping
- [x] Add `documentation/07-reference-number-system.md`
- [x] Add `documentation/08-slip-templates-and-storage.md`
- [~] Update `plans/implementation-plan.md` to reflect enhancements
- [ ] Update `documentation/03-api-specification.md` with new endpoints

### 9.1 Basic PdfService (initial scaffold)
- [x] `IPdfService` interface (Application layer)
- [x] `PdfService` implementation (API layer, basic English templates)
- [x] QuestPDF Community license config in Program.cs
- [x] DI registration
- [x] Print endpoints in controllers (Distribution, Return, TeacherIssue)
- [x] Unit tests (6 tests: PDF generation for all slip types)

### 9.2 Active Academic Year Endpoint (Feature E)
- [~] Add `GetActiveAsync()` to `IAcademicYearService` interface
- [ ] Implement in `AcademicYearService` (`.FirstOrDefaultAsync(y => y.IsActive)`)
- [ ] Add `GET /api/academicyears/active` to `AcademicYearsController`

### 9.3 Customizable Reference Numbers (Feature A)
> See `documentation/07-reference-number-system.md` for full specification

#### New Entities
- [ ] `SlipType` enum (Distribution=1, Return=2, TeacherIssue=3, TeacherReturn=4)
- [ ] `ReferenceNumberFormat` entity (SlipType, AcademicYearId, FormatTemplate, PaddingWidth)
- [ ] `ReferenceNumberFormatConfiguration` (unique constraint on SlipType+AcademicYearId)
- [ ] `TeacherReturnSlip` entity (ReferenceNo, TeacherIssueId, ReceivedById, ReceivedAt, Notes, PdfFilePath)
- [ ] `TeacherReturnSlipItem` entity (TeacherReturnSlipId, TeacherIssueItemId, BookId, Quantity)
- [ ] `TeacherReturnSlipConfiguration` + `TeacherReturnSlipItemConfiguration`

#### Service Changes
- [ ] Rewrite `IReferenceNumberService` — new signature: `GenerateAsync(SlipType, int academicYearId)`
- [ ] Rewrite `ReferenceNumberService` — query format, fallback to default, template parsing
- [ ] Update `DistributionService.CreateAsync` to use new signature
- [ ] Update `ReturnService.CreateAsync` to use new signature
- [ ] Update `TeacherIssueService.CreateAsync` to use new signature
- [ ] Update `TeacherIssueService.ProcessReturnAsync` — create `TeacherReturnSlip` with ref number

#### Admin API
- [ ] `IReferenceNumberFormatService` + `ReferenceNumberFormatService`
- [ ] DTOs: `ReferenceNumberFormatResponse`, `CreateReferenceNumberFormatRequest`
- [ ] Validator: `CreateReferenceNumberFormatRequestValidator`
- [ ] `ReferenceNumberFormatsController` [Admin] — CRUD + DELETE [SuperAdmin]

#### Infrastructure
- [ ] Add DbSets (ReferenceNumberFormat, TeacherReturnSlip, TeacherReturnSlipItem)
- [ ] Migration: `AddReferenceNumberFormatAndTeacherReturnSlip`

#### Tests
- [ ] Unit tests: template parsing (internal static method)
- [ ] Unit tests: default fallback, token substitution, padding width variations

### 9.4 Slip Template Settings — DB-Stored Labels (Feature B)
> See `documentation/08-slip-templates-and-storage.md` for full specification

#### Entity
- [ ] `SlipTemplateSetting` entity (Category, Key, Value, SortOrder)
- [ ] `SlipTemplateSettingConfiguration` (unique constraint on Category+Key)

#### Admin API
- [ ] `ISlipTemplateSettingService` + `SlipTemplateSettingService`
- [ ] DTOs: `SlipTemplateSettingResponse`, `UpdateSlipTemplateSettingRequest`
- [ ] `SlipTemplateSettingsController` [Admin] — GET (list/filter), GET (by id), PUT (update), POST /reset [SuperAdmin]

#### Seeding
- [ ] Seed default Thaana labels in `SeedData.cs` (Common + per slip type)

#### Infrastructure
- [ ] Add DbSet for SlipTemplateSetting
- [ ] Migration: `AddSlipTemplateSetting`

### 9.5 PDF Template Redesign — Thaana (Feature C)
> Depends on 9.3 + 9.4. Designs in `./designs/` folder.

#### Font & Assets
- [ ] Copy `designs/faruma.ttf` to `API/Resources/Fonts/`
- [ ] Register Faruma font with QuestPDF `FontManager` in Program.cs
- [ ] Embed `designs/logo.svg` as resource

#### PdfService Rewrite
- [ ] Rewrite `PdfService` — A4 landscape, dual-copy layout
- [ ] Load labels from `SlipTemplateSetting` via injected service (cached with `IMemoryCache`)
- [ ] RTL support: `.ContentFromRightToLeft()` for Thaana sections
- [ ] Distribution slip template (5-col: Term, Publisher, AcademicYear, SubjectCode, BookTitle)
- [ ] Student Return slip template (5-col, different title)
- [ ] Teacher Issue slip template (4-col, no Term)
- [ ] Teacher Return slip template (4-col)
- [ ] Header: logo (top-right), school name centered in Thaana
- [ ] Signature blocks: Name, ID No, Phone, Signature, Date, Time — Thaana labels

#### IPdfService Update
- [ ] Add `GenerateTeacherReturnSlip(...)` method to interface

#### Tests
- [ ] Update existing PDF tests for new template structure
- [ ] New tests for PDF generation with Thaana labels

### 9.6 PDF Storage on Disk (Feature D)
> Depends on 9.5. See `documentation/08-slip-templates-and-storage.md`.

#### Entity Changes
- [ ] Add `PdfFilePath` (string?) to `DistributionSlip`
- [ ] Add `PdfFilePath` (string?) to `ReturnSlip`
- [ ] Add `PdfFilePath` (string?) to `TeacherIssue`
- [ ] (`TeacherReturnSlip.PdfFilePath` already defined in 9.3)

#### Configuration
- [ ] Add `SlipStorage:BasePath` to `appsettings.json`

#### Service Changes
- [ ] `DistributionService.CreateAsync` — generate PDF + save to disk + store path
- [ ] `ReturnService.CreateAsync` — same
- [ ] `TeacherIssueService.CreateAsync` — same
- [ ] `TeacherIssueService.ProcessReturnAsync` — same (teacher return slip)

#### Print Endpoint Changes
- [ ] `GET /distributions/{id}/print` — serve stored file (fallback: regenerate)
- [ ] `GET /returns/{id}/print` — same
- [ ] `GET /teacher-issues/{id}/print` — same
- [ ] `GET /teacher-issues/{id}/return/{returnSlipId}/print` — teacher return slip

#### Infrastructure
- [ ] Migration: `AddPdfFilePath` (nullable string to 3 existing tables)
- [ ] Directory structure: `{BasePath}/{SlipType}/{AcademicYear}/{ReferenceNo}.pdf`

### 9.7 Final Verification
- [ ] All existing unit tests still pass
- [ ] All new unit tests pass
- [ ] Build: 0 errors, 0 warnings
- [ ] Merge to dev, tag v0.9.0

---

## Module 10: Frontend (Nuxt 4)
> To be detailed after backend completion

### Setup
- [ ] Initialize Nuxt 4 project
- [ ] Configure PrimeVue
- [ ] Setup Pinia stores
- [ ] Create API composables

### Pages
- [ ] Login
- [ ] Dashboard
- [ ] Master data pages
- [ ] Book management
- [ ] Distribution
- [ ] Returns
- [ ] Teacher issues
- [ ] Reports

---

## Deployment
- [ ] Dockerfile.api
- [ ] Dockerfile.client
- [ ] docker-compose.yml
- [ ] Traefik configuration

---

## Deferred Items
- **Slip revision** (edit within 24h of creation) → implement post-frontend
- **Frontend pages** for reference number format management
- **Frontend pages** for slip template settings management

---

## Current Focus
> Module 9 enhancements in progress on `feature/module-9-print`

Modules 1–8 complete and merged to dev (v0.1.0 – v0.8.0).
Hotfix applied: master data deletion restricted to SuperAdmin.
64 unit tests + 1 integration test passing.

**Implementation order:** 9.0 → 9.1 → 9.2 → 9.3 → 9.4 → 9.5 → 9.6 → 9.7
