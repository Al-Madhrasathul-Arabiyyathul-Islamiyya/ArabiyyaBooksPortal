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

## Module 9: Print Module
> PDF generation for slips

### QuestPDF Templates
- [ ] Distribution slip template (A5, dual copy)
- [ ] Return slip template
- [ ] Teacher issue slip template
- [ ] Print endpoints in controllers

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

## Current Focus
> Module 9: Print

Modules 1-8 complete and merged to dev (v0.1.0 through v0.8.0).
