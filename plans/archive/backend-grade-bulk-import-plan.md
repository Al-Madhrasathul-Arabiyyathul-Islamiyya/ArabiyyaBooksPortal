# Backend Refactor Plan: Grade Entity + Bulk Import

## Overview
This plan tracks backend work needed before frontend Phase 3 completion:
- Add canonical `Grade` entity and align class-section model.
- Add bulk import endpoints (XLSX) for `Books`, `Teachers`, and `Students`.
- Tighten required-field rules and keep audit actor tagging consistent.
- Update frontend API usage after backend contract changes.

**Status Legend:** `[ ]` Todo | `[~]` In Progress | `[x]` Done

## Branching
- [x] Create `refactor/grade-entity-from-dev` from `dev`
- [x] Create `feature/bulk-import-endpoints-from-dev` from `dev`
- [x] Implement model/migration work on `refactor/grade-entity-from-dev`
- [x] Merge refactor branch to `dev` after verification
- [x] Rebase `feature/bulk-import-endpoints-from-dev` onto updated `dev`

---

## Phase 1: Grade Entity and ClassSection Refactor

### 1.1 Domain + EF Model
- [x] Add `Grade` entity
  - [x] `KeystageId`, `Code`, `Name`, `NumericValue`, `SortOrder`
  - [x] Navigation to `Keystage` and `ClassSections`
- [x] Update `Keystage` to include `Grades` navigation
- [x] Update `ClassSection`
  - [x] Replace `Grade` string with `GradeId`
  - [x] Keep `AcademicYearId`, `KeystageId`, `Section`
- [x] Add `GradeConfiguration`
- [x] Update `ClassSectionConfiguration`
  - [x] Unique index `(AcademicYearId, GradeId, Section)`
  - [x] FK constraints for `GradeId` and `KeystageId`

### 1.2 Business Rules and Services
- [x] Update class-section service/validators to use `gradeId`
- [x] Enforce grade-keystage consistency (`Grade.KeystageId == request.KeystageId`)
- [x] Update class-section DTOs and response mapping (`gradeId`, `gradeName`, `gradeNumericValue`)

### 1.3 Seed Data
- [x] Seed Keystages and Grades idempotently
  - [x] Key stage 1: Grade 1, Grade 2, Grade 3
  - [x] Key stage 2: Grade 4, Grade 5, Grade 6
  - [x] Key stage 3: Grade 7, Grade 8
  - [x] Key stage 4: Grade 9, Grade 10
  - [x] Key stage 5: Grade 11, Grade 12
  - [x] Seed default classes A-D for all grades on active academic year

### 1.4 Required Field Tightening
- [x] Student: `nationalId` required (DTO, validator, service path)
- [x] Parent: keep and enforce `nationalId` required
- [x] Book: make `publisher` required
- [x] Book: make `publishedYear` required

### 1.5 Audit Actor Standardization
- [x] Store actor email for authenticated changes (`AuditLog.UserName`)
- [x] Add system tags for non-user operations
  - [x] `system:seed`
  - [x] `system:bulk-import`
- [x] Keep IP tracking out of scope

### 1.6 Migration
- [x] Create migration for `Grade` table and `ClassSection.GradeId`
- [x] Reset migrations and create fresh baseline migration
- [x] Remove old string grade column

### Phase 1 Verification
- [x] `dotnet build` passes
- [x] `dotnet test` passes (unit + integration)
- [ ] Existing class-section endpoints pass with new contract
- [x] Seed process creates expected keystage/grade structure

---

## Phase 2: Bulk Import Endpoints (XLSX)

### 2.1 Import Flow Design
- [ ] Implement validate/commit flow for each entity
  - [ ] `POST /api/books/bulk/validate`
  - [ ] `POST /api/books/bulk/commit`
  - [ ] `POST /api/teachers/bulk/validate`
  - [ ] `POST /api/teachers/bulk/commit`
  - [ ] `POST /api/students/bulk/validate`
  - [ ] `POST /api/students/bulk/commit`
- [ ] Enforce conflict policy: reject conflicting rows (no upsert)
- [ ] Commit in single transaction (all-or-nothing)

### 2.2 Import Rules
- [ ] Books import includes `academicYearId` + `quantity`
  - [ ] Create book row
  - [ ] Create initial stock-entry row
- [ ] Teachers conflicts by `nationalId` rejected
- [ ] Students conflicts by `indexNo` rejected
- [ ] Student import requires `nationalId`

### 2.3 Templates and Samples
- [ ] Add template download endpoints
  - [ ] `GET /api/import-templates/books`
  - [ ] `GET /api/import-templates/teachers`
  - [ ] `GET /api/import-templates/students`
- [ ] Add sample XLSX files for each entity

### 2.4 Import Reporting
- [ ] Return row-level validation/commit report
  - [ ] success rows
  - [ ] failed rows
  - [ ] failure reason codes/messages
- [ ] Persist import job summary and actor tag

### Phase 2 Verification
- [ ] Bulk validate returns expected row-level errors
- [ ] Bulk commit rejects conflicts and rolls back on failure
- [ ] Bulk commit succeeds on valid file and returns report

---

## Phase 3: API Docs, Contracts, and Frontend Alignment

### 3.1 API Reference
- [ ] Update `documentation/api-reference.md`
  - [ ] Grade entity and lookup endpoints
  - [ ] Class-section request/response contract changes
  - [ ] Required-field changes (student/book)
  - [ ] Bulk import and template endpoints
  - [ ] Example reports for success/failure

### 3.2 Contract Tests
- [ ] Update httpyac suite for changed contracts
- [ ] Add coverage for new bulk/template endpoints

### 3.3 Frontend API Usage Alignment
- [ ] Update frontend plan/docs with backend contract deltas
- [ ] Add frontend tasks for:
  - [ ] class-section `gradeId` usage
  - [ ] grade lookup consumption
  - [ ] required book field updates (`publisher`, `publishedYear`)
  - [ ] required student `nationalId`
  - [ ] bulk import UI wiring for books/teachers/students

### Phase 3 Verification
- [ ] httpyac contract suite passes
- [ ] docs and implementation are aligned
- [ ] frontend impact checklist is complete and actionable

---

## Progress Log
- [x] Plan initialized
- [x] Backend working branches created from `dev`
- [x] Grade refactor implementation completed
- [x] Bulk import implementation started
