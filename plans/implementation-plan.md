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
- [ ] Entity: `AcademicYear.cs`
- [ ] Configuration: `AcademicYearConfiguration.cs`
- [ ] DTOs: `AcademicYearDto`, `CreateAcademicYearDto`
- [ ] Service: `IAcademicYearService`, `AcademicYearService`
- [ ] Controller: `AcademicYearsController`
- [ ] Validator: `CreateAcademicYearValidator`
- [ ] Unit tests

### 3.2 Keystage
- [ ] Entity: `Keystage.cs`
- [ ] Configuration: `KeystageConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.3 Subject
- [ ] Entity: `Subject.cs`
- [ ] Configuration: `SubjectConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.4 ClassSection
- [ ] Entity: `ClassSection.cs` (with computed DisplayName)
- [ ] Configuration: `ClassSectionConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.5 Student
- [ ] Entity: `Student.cs`
- [ ] Configuration: `StudentConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.6 Parent
- [ ] Entity: `Parent.cs`
- [ ] Configuration: `ParentConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.7 StudentParent Junction
- [ ] Entity: `StudentParent.cs`
- [ ] Configuration: `StudentParentConfiguration.cs`

### 3.8 Teacher
- [ ] Entity: `Teacher.cs`
- [ ] Configuration: `TeacherConfiguration.cs`
- [ ] DTOs, Service, Controller, Validator
- [ ] Unit tests

### 3.9 TeacherAssignment
- [ ] Entity: `TeacherAssignment.cs`
- [ ] Configuration: `TeacherAssignmentConfiguration.cs`
- [ ] DTOs, Service, Controller
- [ ] Unit tests

### 3.10 Lookup Endpoints
- [ ] Create `LookupsController` for dropdown data

---

## Module 4: Book Management
> Book catalog, stock entries, stock movements

### Entities
- [ ] Entity: `Book.cs` (with stock fields)
- [ ] Entity: `StockEntry.cs`
- [ ] Entity: `StockMovement.cs`
- [ ] Configurations for all entities

### Services
- [ ] `IBookService` interface
- [ ] `BookService` implementation
  - [ ] CRUD operations
  - [ ] `AddStockAsync` method
  - [ ] `AdjustStockAsync` method
  - [ ] `GetStockSummaryAsync` method

### Controllers & DTOs
- [ ] `BooksController`
- [ ] DTOs: BookDto, CreateBookDto, StockEntryDto, etc.
- [ ] Validators

### Tests
- [ ] Unit tests for stock calculations
- [ ] Integration tests for stock operations

---

## Module 5: Distribution
> Create distribution slips, stock deduction

### Entities
- [ ] Entity: `DistributionSlip.cs`
- [ ] Entity: `DistributionSlipItem.cs`
- [ ] Configurations

### Services
- [ ] `IDistributionService` interface
- [ ] `DistributionService` implementation
  - [ ] Create slip with stock validation
  - [ ] Reference number generation (DST{YEAR}{SEQ})
  - [ ] Cancel slip with stock reversal

### Controllers & DTOs
- [ ] `DistributionsController`
- [ ] DTOs: DistributionSlipDto, CreateDistributionDto, etc.
- [ ] Validators

### Tests
- [ ] Unit tests
- [ ] Integration tests

---

## Module 6: Returns
> Return slips, stock restoration, condition tracking

### Entities
- [ ] Entity: `ReturnSlip.cs`
- [ ] Entity: `ReturnSlipItem.cs`
- [ ] Configurations

### Services
- [ ] `IReturnService` interface
- [ ] `ReturnService` implementation
  - [ ] Create return with condition assessment
  - [ ] Reference number generation (RTN{YEAR}{SEQ})
  - [ ] Update damage/lost counters

### Controllers & DTOs
- [ ] `ReturnsController`
- [ ] DTOs
- [ ] Validators

### Tests
- [ ] Unit tests
- [ ] Integration tests

---

## Module 7: Teacher Issues
> Teacher book loans, partial returns

### Entities
- [ ] Entity: `TeacherIssue.cs`
- [ ] Entity: `TeacherIssueItem.cs`
- [ ] Configurations

### Services
- [ ] `ITeacherIssueService` interface
- [ ] `TeacherIssueService` implementation
  - [ ] Issue books to teachers
  - [ ] Process partial returns
  - [ ] Status tracking

### Controllers & DTOs
- [ ] `TeacherIssuesController`
- [ ] DTOs
- [ ] Validators

### Tests
- [ ] Unit tests
- [ ] Integration tests

---

## Module 8: Reports & Dashboard
> Stock reports, distribution reports, audit logs

### Services
- [ ] `IReportService` interface
- [ ] `ReportService` implementation
  - [ ] Stock summary
  - [ ] Distribution summary
  - [ ] Movement log
  - [ ] Class distribution
  - [ ] Teacher outstanding

### Controllers
- [ ] `ReportsController`
- [ ] Excel export using ClosedXML

### Audit Log
- [ ] Entity: `AuditLog.cs`
- [ ] EF Core interceptor for audit logging
- [ ] `AuditLogsController`

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
> Module 1: Core Infrastructure

Starting with foundation code that all other modules depend on.
