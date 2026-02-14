# BooksPortal API Reference

## Overview

| Item | Detail |
|------|--------|
| Base URL | `https://{host}/api` |
| Auth | JWT Bearer token in `Authorization: Bearer {token}` header |
| Content-Type | `application/json` |

### Routing Note

- Default convention in this API is `[Route("api/[controller]")]`.
- No kebab-case route transformer is configured.
- Controller-name paths are canonical (for example: `/api/AcademicYears`, `/api/ClassSections`, `/api/TeacherIssues`).
- ASP.NET Core routing is case-insensitive, but hyphenated aliases are not configured.

### Standard Response Wrapper

All endpoints return:

```json
{
  "success": true,
  "data": T,
  "message": "optional message",
  "errors": [{ "field": "Name", "message": "Required" }]
}
```

Type: `ApiResponse<T>` where `T` is the documented response type.

### Pagination Format

Paginated endpoints return `PaginatedList<T>`:

```json
{
  "items": [ ... ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPrevious": false,
  "hasNext": true
}
```

### Frontend Payload Freeze

- **Freeze scope**: Auth, Lookups, Students, Books, Distributions, Returns, and frontend-critical report endpoints.
- **Freeze status**: Active
- **Last validated**: 2026-02-09
- **Validation run**: `tools/api-contract-tester/logs/httpyac-contract-log-8838ab0c-93e8-4237-9003-46b28cd0cdbb.json`
- **Rule**: For the scoped endpoints, example payloads in this document are treated as frontend contracts.

---

## 1. Auth

Base path: `/api/auth`

### POST /api/auth/login

Login and obtain JWT tokens.

- **Auth**: None (AllowAnonymous)
- **Request**: `LoginRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | email | string | yes |
  | password | string | yes |
- **Response**: `TokenResponse`
  | Field | Type |
  |-------|------|
  | accessToken | string |
  | refreshToken | string |
  | expiresAt | DateTime |

### POST /api/auth/refresh

Refresh an expired access token.

- **Auth**: None (AllowAnonymous)
- **Request**: `RefreshTokenRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | accessToken | string | yes |
  | refreshToken | string | yes |
- **Response**: `TokenResponse` (same as login)

### POST /api/auth/logout

Revoke the current user's refresh token.

- **Auth**: Bearer (any authenticated user)
- **Request**: None (uses current user ID from token)
- **Response**: `bool` with message "Logged out successfully."

### GET /api/auth/me

Get current user profile.

- **Auth**: Bearer (any authenticated user)
- **Response**: `UserProfileResponse`
  | Field | Type |
  |-------|------|
  | id | int |
  | userName | string |
  | email | string |
  | fullName | string |
  | nationalId | string? |
  | designation | string? |
  | roles | string[] |

### POST /api/auth/change-password

Change current user's password.

- **Auth**: Bearer (any authenticated user)
- **Request**: `ChangePasswordRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | currentPassword | string | yes |
  | newPassword | string | yes |
- **Response**: `bool` with message

---

## 2. Academic Years

Base path: `/api/AcademicYears`

All endpoints require **Bearer auth**.

### GET /api/AcademicYears

List all academic years.

- **Response**: `AcademicYearResponse[]`

### GET /api/AcademicYears/{id}

Get academic year by ID.

- **Response**: `AcademicYearResponse`

### POST /api/AcademicYears

Create a new academic year.

- **Request**: `CreateAcademicYearRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | name | string | yes |
  | year | int | yes |
  | startDate | DateTime | yes |
  | endDate | DateTime | yes |
- **Response**: `AcademicYearResponse`

### PUT /api/AcademicYears/{id}

Update an academic year.

- **Request**: `UpdateAcademicYearRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | name | string | yes |
  | year | int | yes |
  | startDate | DateTime | yes |
  | endDate | DateTime | yes |
- **Response**: `AcademicYearResponse`

### POST /api/AcademicYears/{id}/activate

Set an academic year as active (deactivates others).

- **Response**: `string` message

### GET /api/AcademicYears/active

Get the currently active academic year. Returns 404 if none is active.

- **Response**: `AcademicYearResponse` (or 404)

### DELETE /api/AcademicYears/{id}

Delete an academic year (soft delete).

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message
- **Behavior**: returns business-rule error if the academic year is referenced by existing records.

**AcademicYearResponse**:

| Field | Type |
|-------|------|
| id | int |
| name | string |
| year | int |
| startDate | DateTime |
| endDate | DateTime |
| isActive | bool |

---

## 3. Keystages

Base path: `/api/keystages`

All endpoints require **Bearer auth**.

### GET /api/keystages

List all keystages.

- **Response**: `KeystageResponse[]`

### GET /api/keystages/{id}

Get keystage by ID.

- **Response**: `KeystageResponse`

### POST /api/keystages

Create a keystage.

- **Request**: `CreateKeystageRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | code | string | yes |
  | name | string | yes |
  | sortOrder | int | yes |
- **Response**: `KeystageResponse`

### PUT /api/keystages/{id}

Update a keystage.

- **Request**: `CreateKeystageRequest` (same as create)
- **Response**: `KeystageResponse`

### DELETE /api/keystages/{id}

Delete a keystage.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

**KeystageResponse**:

| Field | Type |
|-------|------|
| id | int |
| code | string |
| name | string |
| sortOrder | int |

---

## 4. Subjects

Base path: `/api/subjects`

All endpoints require **Bearer auth**.

### GET /api/subjects

List all subjects.

- **Response**: `SubjectResponse[]`

### GET /api/subjects/{id}

Get subject by ID.

- **Response**: `SubjectResponse`

### POST /api/subjects

Create a subject.

- **Request**: `CreateSubjectRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | name | string | yes |
  | code | string | yes |
- **Response**: `SubjectResponse`

### PUT /api/subjects/{id}

Update a subject.

- **Request**: `CreateSubjectRequest` (same as create)
- **Response**: `SubjectResponse`

### DELETE /api/subjects/{id}

Delete a subject.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

**SubjectResponse**:

| Field | Type |
|-------|------|
| id | int |
| name | string |
| code | string |

---

## 5. Class Sections

Base path: `/api/ClassSections`

All endpoints require **Bearer auth**.

### GET /api/ClassSections

List class sections with optional filter.

- **Query**: `?academicYearId={int}`
- **Response**: `ClassSectionResponse[]`

### GET /api/ClassSections/{id}

Get class section by ID.

- **Response**: `ClassSectionResponse`

### POST /api/ClassSections

Create a class section.

- **Request**: `CreateClassSectionRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | keystageId | int | yes |
  | gradeId | int | yes |
  | section | string | yes |
- **Response**: `ClassSectionResponse`

### PUT /api/ClassSections/{id}

Update a class section.

- **Request**: `CreateClassSectionRequest` (same as create)
- **Response**: `ClassSectionResponse`

### DELETE /api/ClassSections/{id}

Delete a class section.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

**ClassSectionResponse**:

| Field | Type |
|-------|------|
| id | int |
| academicYearId | int |
| academicYearName | string |
| keystageId | int |
| keystageName | string |
| gradeId | int |
| grade | string |
| section | string |
| displayName | string (computed: "{grade} {section}") |
| studentCount | int |

---

## 6. Students

Base path: `/api/students`

All endpoints require **Bearer auth**.

### GET /api/students

List students (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&classSectionId={int}&search={string}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<StudentResponse>`

### GET /api/students/{id}

Get student by ID.

- **Response**: `StudentResponse`

### POST /api/students

Create a student.

- **Request**: `CreateStudentRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | fullName | string | yes |
  | indexNo | string | yes |
  | nationalId | string | yes |
  | classSectionId | int | yes |
  | parents | StudentParentRequest[]? | no |

  `StudentParentRequest`:
  | Field | Type |
  |-------|------|
  | parentId | int |
  | isPrimary | bool |
- **Response**: `StudentResponse`

### PUT /api/students/{id}

Update a student.

- **Request**: `UpdateStudentRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | fullName | string | yes |
  | nationalId | string | yes |
  | classSectionId | int | yes |
  | parents | StudentParentRequest[]? | no |
- **Response**: `StudentResponse`

### DELETE /api/students/{id}

Delete a student.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

### POST /api/students/bulk/validate

Validate student import payload before commit.

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

### POST /api/students/bulk/commit

Commit student import payload in a single transaction (reject-on-conflict, no upsert).

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

**StudentResponse**:

| Field | Type |
|-------|------|
| id | int |
| fullName | string |
| indexNo | string |
| nationalId | string? |
| classSectionId | int |
| classSectionDisplayName | string |
| parents | StudentParentResponse[] |

**StudentParentResponse**:

| Field | Type |
|-------|------|
| parentId | int |
| fullName | string |
| nationalId | string |
| phone | string? |
| relationship | string? |
| isPrimary | bool |

---

## 7. Parents

Base path: `/api/parents`

All endpoints require **Bearer auth**.

### GET /api/parents

List parents (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&search={string}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<ParentResponse>`

### GET /api/parents/{id}

Get parent by ID.

- **Response**: `ParentResponse`

### POST /api/parents

Create a parent.

- **Request**: `CreateParentRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | fullName | string | yes |
  | nationalId | string | yes |
  | phone | string? | no |
  | relationship | string? | no |
- **Response**: `ParentResponse`

### PUT /api/parents/{id}

Update a parent.

- **Request**: `CreateParentRequest` (same as create)
- **Response**: `ParentResponse`

### DELETE /api/parents/{id}

Delete a parent.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message
- **Behavior**: returns business-rule error if the parent is referenced by existing records.

**ParentResponse**:

| Field | Type |
|-------|------|
| id | int |
| fullName | string |
| nationalId | string |
| phone | string? |
| relationship | string? |

---

## 8. Teachers

Base path: `/api/teachers`

All endpoints require **Bearer auth**.

### GET /api/teachers

List teachers (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&search={string}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<TeacherResponse>`

### GET /api/teachers/{id}

Get teacher by ID.

- **Response**: `TeacherResponse`

### POST /api/teachers

Create a teacher.

- **Request**: `CreateTeacherRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | fullName | string | yes |
  | nationalId | string | yes |
  | email | string? | no |
  | phone | string? | no |
- **Response**: `TeacherResponse`

### PUT /api/teachers/{id}

Update a teacher.

- **Request**: `CreateTeacherRequest` (same as create)
- **Response**: `TeacherResponse`

### DELETE /api/teachers/{id}

Delete a teacher.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

### POST /api/teachers/{id}/assignments

Add a subject/class assignment to a teacher.

- **Request**: `CreateTeacherAssignmentRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | subjectId | int | yes |
  | classSectionId | int | yes |
- **Response**: `TeacherAssignmentResponse`

### DELETE /api/teachers/{id}/assignments/{assignmentId}

Remove an assignment from a teacher.

- **Response**: `string` message

### POST /api/teachers/bulk/validate

Validate teacher import payload before commit.

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

### POST /api/teachers/bulk/commit

Commit teacher import payload in a single transaction (reject-on-conflict, no upsert).

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

**TeacherResponse**:

| Field | Type |
|-------|------|
| id | int |
| fullName | string |
| nationalId | string |
| email | string? |
| phone | string? |
| assignments | TeacherAssignmentResponse[] |

**TeacherAssignmentResponse**:

| Field | Type |
|-------|------|
| id | int |
| subjectId | int |
| subjectName | string |
| classSectionId | int |
| classSectionDisplayName | string |

---

## 9. Lookups

Base path: `/api/lookups`

All endpoints require **Bearer auth**. Returns lightweight id/name pairs for dropdowns.

### GET /api/lookups/academic-years

- **Response**: `LookupResponse[]`

### GET /api/lookups/keystages

- **Response**: `LookupResponse[]`

### GET /api/lookups/grades

- **Query**: `?keystageId={int}` (optional)
- **Response**: `LookupResponse[]`

### GET /api/lookups/subjects

- **Response**: `LookupResponse[]`

### GET /api/lookups/class-sections

- **Query**: `?academicYearId={int}`
- **Response**: `LookupResponse[]`

### GET /api/lookups/terms

- **Response**: `LookupResponse[]` (enum values)

### GET /api/lookups/book-conditions

- **Response**: `LookupResponse[]` (enum values)

### GET /api/lookups/movement-types

- **Response**: `LookupResponse[]` (enum values)

**LookupResponse**:

| Field | Type |
|-------|------|
| id | int |
| name | string |

---

## 10. Books

Base path: `/api/books`

All endpoints require **Bearer auth**.

### GET /api/books

List books (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&subjectId={int}&search={string}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<BookResponse>`

### GET /api/books/{id}

Get book by ID.

- **Response**: `BookResponse`

### POST /api/books

Create a book.

- **Request**: `CreateBookRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | isbn | string? | no |
  | code | string | yes |
  | title | string | yes |
  | author | string? | no |
  | edition | string? | no |
  | publisher | string | yes (default value in domain model is `Other`) |
  | publishedYear | int | yes |
  | subjectId | int | yes |
  | grade | string? | no |
- **Response**: `BookResponse`

### POST /api/books/bulk/validate

Validate book import payload before commit.

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

### POST /api/books/bulk/commit

Commit book import payload in a single transaction (reject-on-conflict, no upsert).

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `multipart/form-data` with `file` (`.xlsx`)
- **Response**: `BulkImportReport`

### PUT /api/books/{id}

Update a book.

- **Request**: `CreateBookRequest` (same as create)
- **Response**: `BookResponse`

### DELETE /api/books/{id}

Delete a book.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

### POST /api/books/{id}/stock-entry

Add stock entry for a book.

- **Request**: `AddStockRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | quantity | int | yes |
  | source | string? | no |
  | notes | string? | no |
- **Response**: `BookResponse`

### GET /api/books/{id}/stock-entries

List stock entries for a book.

- **Response**: `StockEntryResponse[]`

### GET /api/books/{id}/stock-movements

List stock movements for a book.

- **Response**: `StockMovementResponse[]`

### POST /api/books/{id}/adjust-stock

Adjust stock (mark damaged, lost, write-off, or general adjustment).

- **Auth**: Bearer + **SuperAdmin** or **Admin** role
- **Request**: `AdjustStockRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | movementType | MovementType (int) | yes |
  | quantity | int | yes |
  | notes | string? | no |
- **Response**: `string` message

### GET /api/books/search

Search books by query string.

- **Query**: `?q={string}`
- **Response**: `BookResponse[]`

**BookResponse**:

| Field | Type |
|-------|------|
| id | int |
| isbn | string? |
| code | string |
| title | string |
| author | string? |
| edition | string? |
| publisher | string? |
| publishedYear | int? |
| subjectId | int |
| subjectName | string |
| grade | string? |
| totalStock | int |
| distributed | int |
| withTeachers | int |
| damaged | int |
| lost | int |
| available | int (computed) |

**StockEntryResponse**:

| Field | Type |
|-------|------|
| id | int |
| bookId | int |
| academicYearId | int |
| quantity | int |
| source | string? |
| notes | string? |
| enteredById | int |
| enteredAt | DateTime |

**StockMovementResponse**:

| Field | Type |
|-------|------|
| id | long |
| bookId | int |
| movementType | MovementType (int) |
| quantity | int |
| referenceId | int? |
| referenceType | string? |
| notes | string? |
| processedById | int |
| processedAt | DateTime |

---

## 11. Distribution Slips

Base path: `/api/distributions`

All endpoints require **Bearer auth**.

### GET /api/distributions

List distribution slips (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&academicYearId={int}&studentId={int}&includeCancelled={bool}`
- **Defaults**: pageNumber=1, pageSize=20
- **Default behavior**: cancelled slips are excluded unless `includeCancelled=true`.
- **Response**: `PaginatedList<DistributionSlipResponse>`

### GET /api/distributions/{id}

Get distribution slip by ID.

- **Response**: `DistributionSlipResponse`

### GET /api/distributions/by-reference/{referenceNo}

Get distribution slip by reference number.

- **Response**: `DistributionSlipResponse`

### POST /api/distributions

Create a distribution slip.

- **Request**: `CreateDistributionSlipRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | term | Term (int) | yes |
  | studentId | int | yes |
  | parentId | int | yes |
  | issuedDate | DateOnly? | no |
  | issuedTime | TimeOnly? | no |
  | notes | string? | no |
  | items | CreateDistributionSlipItemRequest[] | yes |

  `CreateDistributionSlipItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `DistributionSlipResponse`

### PUT /api/distributions/{id}

Revise a distribution slip while it is still in `Processing` state.

- **Request**: `UpdateDistributionSlipRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | term | Term (int) | yes |
  | studentId | int | yes |
  | parentId | int | yes |
  | issuedDate | DateOnly? | no |
  | issuedTime | TimeOnly? | no |
  | notes | string? | no |
  | items | UpdateDistributionSlipItemRequest[] | yes |

  `UpdateDistributionSlipItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `DistributionSlipResponse`
- **Rule**: only slips with `lifecycleStatus=Processing` can be revised.

### DELETE /api/distributions/{id}

Cancel a distribution slip (reverses stock).

- **Response**: `string` message
- **Rule**: finalized slips cannot be cancelled.

### POST /api/distributions/{id}/finalize

Finalize a distribution slip.

- **Response**: `string` message
- **Rule**: cancelled slips cannot be finalized.

### GET /api/distributions/{id}/print

Download PDF for a distribution slip.

- **Response**: `application/pdf` file

**DistributionSlipResponse**:

| Field | Type |
|-------|------|
| id | int |
| referenceNo | string |
| academicYearId | int |
| academicYearName | string |
| term | Term (int) |
| studentId | int |
| studentName | string |
| studentIndexNo | string |
| studentClassName | string |
| studentNationalId | string? |
| parentId | int |
| parentName | string |
| parentNationalId | string? |
| parentPhone | string? |
| parentRelationship | string? |
| issuedById | int |
| issuedByName | string |
| issuedByDesignation | string? |
| issuedAt | DateTime |
| lifecycleStatus | SlipLifecycleStatus (int) |
| finalizedById | int? |
| finalizedAt | DateTime? |
| cancelledById | int? |
| cancelledAt | DateTime? |
| notes | string? |
| pdfFilePath | string? |
| items | DistributionSlipItemResponse[] |

**DistributionSlipItemResponse**:

| Field | Type |
|-------|------|
| id | int |
| bookId | int |
| bookCode | string |
| bookTitle | string |
| quantity | int |

---

## 12. Return Slips

Base path: `/api/returns`

All endpoints require **Bearer auth**.

### GET /api/returns

List return slips (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&academicYearId={int}&studentId={int}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<ReturnSlipResponse>`

### GET /api/returns/{id}

Get return slip by ID.

- **Response**: `ReturnSlipResponse`

### GET /api/returns/by-reference/{referenceNo}

Get return slip by reference number.

- **Response**: `ReturnSlipResponse`

### POST /api/returns

Create a return slip.

- **Request**: `CreateReturnSlipRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | studentId | int | yes |
  | returnedById | int | yes |
  | receivedDate | DateOnly? | no |
  | receivedTime | TimeOnly? | no |
  | notes | string? | no |
  | items | CreateReturnSlipItemRequest[] | yes |

  `CreateReturnSlipItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
  | condition | BookCondition (int) |
  | conditionNotes | string? |
- **Response**: `ReturnSlipResponse`

### DELETE /api/returns/{id}

Cancel a return slip (reverses stock).

- **Response**: `string` message

### GET /api/returns/{id}/print

Download PDF for a return slip.

- **Response**: `application/pdf` file

**ReturnSlipResponse**:

| Field | Type |
|-------|------|
| id | int |
| referenceNo | string |
| academicYearId | int |
| academicYearName | string |
| studentId | int |
| studentName | string |
| studentIndexNo | string |
| studentClassName | string |
| studentNationalId | string? |
| returnedById | int |
| returnedByName | string |
| returnedByNationalId | string? |
| returnedByPhone | string? |
| returnedByRelationship | string? |
| receivedById | int |
| receivedByName | string |
| receivedByDesignation | string? |
| receivedAt | DateTime |
| notes | string? |
| pdfFilePath | string? |
| items | ReturnSlipItemResponse[] |

**ReturnSlipItemResponse**:

| Field | Type |
|-------|------|
| id | int |
| bookId | int |
| bookTitle | string |
| bookCode | string |
| quantity | int |
| condition | BookCondition (int) |
| conditionNotes | string? |

---

## 13. Teacher Issues

Base path: `/api/TeacherIssues`

All endpoints require **Bearer auth**.

### GET /api/TeacherIssues

List teacher issues (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&academicYearId={int}&teacherId={int}&includeCancelled={bool}`
- **Defaults**: pageNumber=1, pageSize=20
- **Default behavior**: cancelled slips are excluded unless `includeCancelled=true`.
- **Response**: `PaginatedList<TeacherIssueResponse>`

### GET /api/TeacherIssues/{id}

Get teacher issue by ID.

- **Response**: `TeacherIssueResponse`

### POST /api/TeacherIssues

Create a teacher issue.

- **Request**: `CreateTeacherIssueRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | academicYearId | int | yes |
  | teacherId | int | yes |
  | issuedDate | DateOnly? | no |
  | issuedTime | TimeOnly? | no |
  | expectedReturnDate | DateTime? | no |
  | notes | string? | no |
  | items | TeacherIssueItemRequest[] | yes |

  `TeacherIssueItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `TeacherIssueResponse`

### PUT /api/TeacherIssues/{id}

Revise a teacher issue while it is still in `Processing` state.

- **Request**: `UpdateTeacherIssueRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | teacherId | int | yes |
  | issuedDate | DateOnly? | no |
  | issuedTime | TimeOnly? | no |
  | expectedReturnDate | DateTime? | no |
  | notes | string? | no |
  | items | UpdateTeacherIssueItemRequest[] | yes |

  `UpdateTeacherIssueItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `TeacherIssueResponse`
- **Rule**: only slips with `lifecycleStatus=Processing` can be revised.

### POST /api/TeacherIssues/{id}/return

Process a partial or full return of books from a teacher.

- **Request**: `ProcessTeacherReturnRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | receivedDate | DateOnly? | no |
  | receivedTime | TimeOnly? | no |
  | notes | string? | no |
  | items | TeacherReturnItemRequest[] | yes |

  `TeacherReturnItemRequest`:
  | Field | Type |
  |-------|------|
  | teacherIssueItemId | int |
  | quantity | int |
- **Response**: `TeacherReturnSlipResponse`

### DELETE /api/TeacherIssues/{id}

Cancel a teacher issue (reverses outstanding stock).

- **Response**: `string` message
- **Rule**: finalized slips cannot be cancelled.

### POST /api/TeacherIssues/{id}/finalize

Finalize a teacher issue.

- **Response**: `string` message
- **Rule**: cancelled slips cannot be finalized.

### GET /api/TeacherIssues/{id}/print

Download PDF for a teacher issue.

- **Response**: `application/pdf` file

### GET /api/TeacherIssues/{id}/return/print

Download PDF for the latest teacher return slip created for the teacher issue.

- **Response**: `application/pdf` file

**TeacherIssueResponse**:

| Field | Type |
|-------|------|
| id | int |
| referenceNo | string |
| academicYearId | int |
| academicYearName | string |
| teacherId | int |
| teacherName | string |
| teacherNationalId | string? |
| issuedById | int |
| issuedByName | string |
| issuedByDesignation | string? |
| issuedAt | DateTime |
| lifecycleStatus | SlipLifecycleStatus (int) |
| finalizedById | int? |
| finalizedAt | DateTime? |
| cancelledById | int? |
| cancelledAt | DateTime? |
| expectedReturnDate | DateTime? |
| status | TeacherIssueStatus (int) |
| notes | string? |
| pdfFilePath | string? |
| items | TeacherIssueItemResponse[] |

**TeacherIssueItemResponse**:

| Field | Type |
|-------|------|
| id | int |
| bookId | int |
| bookTitle | string |
| bookCode | string |
| quantity | int |
| returnedQuantity | int |
| outstandingQuantity | int |
| returnedAt | DateTime? |
| receivedById | int? |

**TeacherReturnSlipResponse**:

| Field | Type |
|-------|------|
| id | int |
| referenceNo | string |
| teacherIssueId | int |
| teacherName | string |
| teacherNationalId | string? |
| academicYearId | int |
| academicYearName | string |
| receivedById | int |
| receivedByName | string |
| receivedByDesignation | string? |
| receivedAt | DateTime |
| notes | string? |
| pdfFilePath | string? |
| items | TeacherReturnSlipItemResponse[] |

**TeacherReturnSlipItemResponse**:

| Field | Type |
|-------|------|
| id | int |
| bookId | int |
| bookTitle | string |
| bookCode | string |
| quantity | int |

---

## 14. Reports

Base path: `/api/reports`

All endpoints require **Bearer auth**.

### GET /api/reports/stock-summary

Stock summary report.

- **Query**: `?subjectId={int}&grade={string}`
- **Response**: `StockSummaryReport[]`

### GET /api/reports/distribution-summary

Distribution summary report.

- **Query**: `?academicYearId={int}&from={DateTime}&to={DateTime}`
- `academicYearId` is **required**
- **Response**: `DistributionSummaryReport[]`

### GET /api/reports/teacher-outstanding

Teacher outstanding books report.

- **Query**: `?teacherId={int}`
- **Response**: `TeacherOutstandingReport[]`

### GET /api/reports/student-history/{studentId}

Student book history.

- **Response**: `StudentHistoryReport[]`

### GET /api/reports/export/stock-summary

Export stock summary as Excel.

- **Query**: `?subjectId={int}&grade={string}`
- **Response**: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`

### GET /api/reports/export/distribution-summary

Export distribution summary as Excel.

- **Query**: `?academicYearId={int}&from={DateTime}&to={DateTime}`
- **Response**: `.xlsx` file

### GET /api/reports/export/teacher-outstanding

Export teacher outstanding as Excel.

- **Query**: `?teacherId={int}`
- **Response**: `.xlsx` file

**StockSummaryReport**:

| Field | Type |
|-------|------|
| bookId | int |
| code | string |
| title | string |
| subjectName | string |
| grade | string? |
| totalStock | int |
| distributed | int |
| withTeachers | int |
| damaged | int |
| lost | int |
| available | int |

**DistributionSummaryReport**:

| Field | Type |
|-------|------|
| slipId | int |
| referenceNo | string |
| studentName | string |
| studentIndexNo | string |
| parentName | string |
| issuedAt | DateTime |
| totalBooks | int |

**TeacherOutstandingReport**:

| Field | Type |
|-------|------|
| issueId | int |
| referenceNo | string |
| teacherName | string |
| bookTitle | string |
| bookCode | string |
| quantity | int |
| returnedQuantity | int |
| outstanding | int |
| status | TeacherIssueStatus (int) |
| issuedAt | DateTime |
| expectedReturnDate | DateTime? |

**StudentHistoryReport**:

| Field | Type |
|-------|------|
| type | string |
| referenceNo | string |
| date | DateTime |
| bookTitle | string |
| bookCode | string |
| quantity | int |
| condition | string? |

---

## 15. Users

Base path: `/api/users`

All endpoints require **Bearer auth** + **SuperAdmin** or **Admin** role.

### GET /api/users

List all users.

- **Response**: `UserResponse[]`

### GET /api/users/{id}

Get user by ID.

- **Response**: `UserResponse`

### POST /api/users

Create a new user.

- **Request**: `CreateUserRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | userName | string | yes |
  | email | string | yes |
  | password | string | yes |
  | fullName | string | yes |
  | nationalId | string? | no |
  | designation | string? | no |
  | roles | string[] | yes |
- **Response**: `UserResponse`

### PUT /api/users/{id}

Update a user.

- **Request**: `UpdateUserRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | email | string | yes |
  | fullName | string | yes |
  | nationalId | string? | no |
  | designation | string? | no |
  | isActive | bool | yes |
- **Response**: `UserResponse`

### POST /api/users/{id}/toggle-active

Toggle user active/inactive status.

- **Response**: `bool` with message
- **Behavior**: the configured main SuperAdmin account cannot be deactivated.

### PUT /api/users/{id}/roles

Assign roles to a user.

- **Request**: `string[]` (list of role names)
- **Response**: `bool` with message

**UserResponse**:

| Field | Type |
|-------|------|
| id | int |
| userName | string |
| email | string |
| fullName | string |
| nationalId | string? |
| designation | string? |
| isActive | bool |
| roles | string[] |
| createdAt | DateTime |

---

## 16. Reference Number Formats

Base path: `/api/ReferenceNumberFormats`

All endpoints require **Bearer auth** + **SuperAdmin** or **Admin** role.

### GET /api/ReferenceNumberFormats

List all formats.

- **Query**: `?slipType={SlipType}&academicYearId={int}`
- **Response**: `ReferenceNumberFormatResponse[]`

### GET /api/ReferenceNumberFormats/{id}

Get format by ID.

- **Response**: `ReferenceNumberFormatResponse`

### POST /api/ReferenceNumberFormats

Create a format.

- **Request**: `CreateReferenceNumberFormatRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | slipType | SlipType (int) | yes |
  | academicYearId | int | yes |
  | formatTemplate | string | yes |
  | paddingWidth | int | yes (default 6) |
- **Response**: `ReferenceNumberFormatResponse`

### PUT /api/ReferenceNumberFormats/{id}

Update a format.

- **Request**: `CreateReferenceNumberFormatRequest` (same as create)
- **Response**: `ReferenceNumberFormatResponse`

### DELETE /api/ReferenceNumberFormats/{id}

Delete a format.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

**ReferenceNumberFormatResponse**:

| Field | Type |
|-------|------|
| id | int |
| slipType | SlipType (int) |
| academicYearId | int |
| academicYearName | string |
| formatTemplate | string |
| paddingWidth | int |

---

## 17. Slip Template Settings

Base path: `/api/SlipTemplateSettings`

All endpoints require **Bearer auth** + **SuperAdmin** or **Admin** role.

### GET /api/SlipTemplateSettings

List all settings.

- **Query**: `?category={string}`
- **Response**: `SlipTemplateSettingResponse[]`

### GET /api/SlipTemplateSettings/{id}

Get setting by ID.

- **Response**: `SlipTemplateSettingResponse`

### PUT /api/SlipTemplateSettings/{id}

Update a setting value.

- **Request**: `UpdateSlipTemplateSettingRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | value | string | yes |
  | sortOrder | int | yes |
- **Response**: `SlipTemplateSettingResponse`

### POST /api/SlipTemplateSettings/reset

Reset all template settings to defaults.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

**SlipTemplateSettingResponse**:

| Field | Type |
|-------|------|
| id | int |
| category | string |
| key | string |
| value | string |
| sortOrder | int |

---

## 18. Audit Logs

Base path: `/api/AuditLogs`

All endpoints require **Bearer auth** + **SuperAdmin** or **Admin** role.

### GET /api/AuditLogs

List audit logs (paginated).

- **Query**: `?pageNumber={int}&pageSize={int}&entityType={string}&action={string}&userId={int}&from={DateTime}&to={DateTime}`
- **Defaults**: pageNumber=1, pageSize=20
- **Response**: `PaginatedList<AuditLogResponse>`
- **Notable action values**:
  - `CREATE`, `UPDATE`, `DELETE`
  - `REVISION_SNAPSHOT` (explicit before/after aggregate snapshots for slip revisions)

**AuditLogResponse**:

| Field | Type |
|-------|------|
| id | long |
| action | string |
| entityType | string |
| entityId | string |
| oldValues | string? (JSON) |
| newValues | string? (JSON) |
| userId | int? |
| userName | string? |
| timestamp | DateTime |

#### GET `/api/AuditLogs?action=REVISION_SNAPSHOT` Example Success
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1589,
        "action": "REVISION_SNAPSHOT",
        "entityType": "DistributionSlip",
        "entityId": "42",
        "oldValues": "{\"id\":42,\"referenceNo\":\"DST2026000042\",\"items\":[{\"bookId\":21,\"quantity\":1}]}",
        "newValues": "{\"id\":42,\"referenceNo\":\"DST2026000042\",\"items\":[{\"bookId\":21,\"quantity\":2}]}",
        "userId": 1,
        "userName": "admin@booksportal.local",
        "timestamp": "2026-02-14T10:24:18Z"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "message": null,
  "errors": null
}
```

---

## 19. Frontend Payload Freeze (2026-02-09)

The following examples are frozen for frontend integration. All JSON examples use the standard `ApiResponse<T>` envelope unless stated otherwise.

### Auth (`/api/auth`)

#### POST `/api/auth/login` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt...",
    "refreshToken": "refresh...",
    "expiresAt": "2026-02-16T03:04:11Z"
  },
  "message": null,
  "errors": null
}
```

#### POST `/api/auth/login` Example Error (frozen)
```json
{
  "success": false,
  "data": null,
  "message": "Invalid email or password.",
  "errors": null
}
```

#### POST `/api/auth/refresh` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt...",
    "refreshToken": "refresh...",
    "expiresAt": "2026-02-16T03:10:00Z"
  },
  "message": null,
  "errors": null
}
```

#### GET `/api/auth/me` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userName": "superadmin",
    "email": "admin@booksportal.local",
    "fullName": "Super Admin",
    "nationalId": null,
    "designation": null,
    "roles": ["SuperAdmin"]
  },
  "message": null,
  "errors": null
}
```

#### POST `/api/auth/change-password` Example Success (frozen)
```json
{
  "success": true,
  "data": true,
  "message": "Password changed successfully.",
  "errors": null
}
```

#### POST `/api/auth/change-password` Example Business Rule Error (frozen)
```json
{
  "success": false,
  "data": null,
  "message": "Optimistic concurrency failure, object has been modified.",
  "errors": null
}
```

#### POST `/api/auth/logout` Example Success (frozen)
```json
{
  "success": true,
  "data": true,
  "message": "Logged out successfully.",
  "errors": null
}
```

### Lookups (`/api/lookups/*`)

Frozen routes:
- `GET /api/lookups/academic-years`
- `GET /api/lookups/keystages`
- `GET /api/lookups/subjects`
- `GET /api/lookups/class-sections`
- `GET /api/lookups/terms`
- `GET /api/lookups/book-conditions`
- `GET /api/lookups/movement-types`
- `GET /api/lookups/operations/students?academicYearId={id}&search={term?}&take={n?}`
- `GET /api/lookups/operations/parents?studentId={id}&search={term?}&take={n?}`
- `GET /api/lookups/operations/books?academicYearId={id}&search={term?}&take={n?}`
- `GET /api/lookups/operations/teachers?academicYearId={id?}&search={term?}&take={n?}`
- `GET /api/lookups/operations/teacher-issues/outstanding?academicYearId={id?}&teacherId={id?}&search={term?}&take={n?}`

#### Example Success (frozen)
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "2025/2026" }
  ],
  "message": null,
  "errors": null
}
```

### Students (`/api/students`)

Frozen routes:
- `GET /api/students`
- `GET /api/students/{id}`
- `POST /api/students`
- `PUT /api/students/{id}`
- `DELETE /api/students/{id}`

#### GET `/api/students` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 12,
        "indexNo": "IDX-1001",
        "fullName": "Student One",
        "classSectionId": 3,
        "nationalId": null
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "message": null,
  "errors": null
}
```

#### POST/PUT `/api/students` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 12,
    "indexNo": "IDX-1001",
    "fullName": "Student One",
    "classSectionId": 3,
    "nationalId": null
  },
  "message": null,
  "errors": null
}
```

#### DELETE `/api/students/{id}` Example Success (frozen)
```json
{
  "success": true,
  "data": "Student deleted successfully.",
  "message": null,
  "errors": null
}
```

#### DELETE `/api/students/{id}` Example Business Rule Error (frozen)
```json
{
  "success": false,
  "data": null,
  "message": "Cannot delete student because it is referenced by existing records.",
  "errors": null
}
```

### Books (`/api/books`)

Frozen routes:
- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `POST /api/books/{id}/stock-entry`
- `GET /api/books/{id}/stock-entries`
- `GET /api/books/{id}/stock-movements`
- `POST /api/books/{id}/adjust-stock`
- `GET /api/books/search`

#### GET `/api/books` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 21,
        "code": "BK-001",
        "title": "Sample Book",
        "subjectId": 2,
        "grade": "Grade 6",
        "availableStock": 10
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "message": null,
  "errors": null
}
```

#### POST/PUT `/api/books` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 21,
    "code": "BK-001",
    "title": "Sample Book",
    "subjectId": 2,
    "grade": "Grade 6"
  },
  "message": null,
  "errors": null
}
```

#### POST `/api/books/{id}/stock-entry` Example Success (frozen)
```json
{
  "success": true,
  "data": true,
  "message": "Stock entry recorded successfully.",
  "errors": null
}
```

#### GET `/api/books/{id}/stock-entries` and GET `/api/books/{id}/stock-movements` Example Success (frozen)
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "bookId": 21,
      "quantity": 5,
      "movementType": 1,
      "notes": null,
      "createdAt": "2026-02-09T03:04:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

#### POST `/api/books/{id}/adjust-stock` Example Success (frozen)
```json
{
  "success": true,
  "data": true,
  "message": "Stock adjusted successfully.",
  "errors": null
}
```

#### GET `/api/books/search` Example Success (frozen)
```json
{
  "success": true,
  "data": [
    {
      "id": 21,
      "code": "BK-001",
      "title": "Sample Book"
    }
  ],
  "message": null,
  "errors": null
}
```

#### DELETE `/api/books/{id}` Example Business Rule Error (frozen)
```json
{
  "success": false,
  "data": null,
  "message": "Cannot delete book because it is referenced by existing records.",
  "errors": null
}
```

### Distributions (`/api/distributions`)

Frozen routes:
- `GET /api/distributions`
- `GET /api/distributions/{id}`
- `GET /api/distributions/by-reference/{referenceNo}`
- `POST /api/distributions`
- `PUT /api/distributions/{id}`
- `DELETE /api/distributions/{id}`
- `POST /api/distributions/{id}/finalize`
- `GET /api/distributions/{id}/print`

#### GET `/api/distributions` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 9,
        "referenceNo": "DIS-2026-0001",
        "studentId": 12,
        "studentName": "Student One",
        "term": 1,
        "lifecycleStatus": 0
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "message": null,
  "errors": null
}
```

#### GET `/api/distributions/{id}` and `/by-reference/{referenceNo}` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 9,
    "referenceNo": "DIS-2026-0001",
    "studentId": 12,
    "parentId": 3,
    "parentName": "Parent One",
    "parentNationalId": "A123456",
    "parentPhone": "7777777",
    "issuedAt": "2026-02-09T03:04:30Z",
    "lifecycleStatus": 0,
    "items": [{ "bookId": 21, "bookTitle": "Sample Book", "quantity": 1 }]
  },
  "message": null,
  "errors": null
}
```

#### POST `/api/distributions` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 9,
    "referenceNo": "DIS-2026-0001"
  },
  "message": null,
  "errors": null
}
```

#### DELETE `/api/distributions/{id}` Example Success (frozen)
```json
{
  "success": true,
  "data": "Distribution slip cancelled.",
  "message": null,
  "errors": null
}
```

#### POST `/api/distributions/{id}/finalize` Example Success (frozen)
```json
{
  "success": true,
  "data": "Distribution slip finalized.",
  "message": null,
  "errors": null
}
```

#### GET `/api/distributions/{id}/print` Example Success (frozen)
- HTTP `200`
- `Content-Type: application/pdf`
- Binary body (not JSON envelope)

### Returns (`/api/returns`)

Frozen routes:
- `GET /api/returns`
- `GET /api/returns/{id}`
- `GET /api/returns/by-reference/{referenceNo}`
- `POST /api/returns`
- `DELETE /api/returns/{id}`
- `GET /api/returns/{id}/print`

#### GET `/api/returns` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 7,
        "referenceNo": "RET-2026-0001",
        "studentId": 12,
        "studentName": "Student One"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "message": null,
  "errors": null
}
```

#### GET `/api/returns/{id}` and `/by-reference/{referenceNo}` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 7,
    "referenceNo": "RET-2026-0001",
    "studentId": 12,
    "returnedById": 3,
    "receivedAt": "2026-02-09T03:04:50Z",
    "items": [{ "bookId": 21, "bookTitle": "Sample Book", "quantity": 1, "condition": 1 }]
  },
  "message": null,
  "errors": null
}
```

#### POST `/api/returns` Example Success (frozen)
```json
{
  "success": true,
  "data": {
    "id": 7,
    "referenceNo": "RET-2026-0001"
  },
  "message": null,
  "errors": null
}
```

#### DELETE `/api/returns/{id}` Example Success (frozen)
```json
{
  "success": true,
  "data": "Return slip cancelled.",
  "message": null,
  "errors": null
}
```

#### GET `/api/returns/{id}/print` Example Success (frozen)
- HTTP `200`
- `Content-Type: application/pdf`
- Binary body (not JSON envelope)

### Frontend-Critical Reports

Frozen routes:
- `GET /api/reports/distribution-summary`
- `GET /api/reports/student-history/{studentId}`

#### GET `/api/reports/distribution-summary` Example Success (frozen)
```json
{
  "success": true,
  "data": [
    {
      "slipId": 9,
      "referenceNo": "DIS-2026-0001",
      "studentName": "Student One",
      "totalBooks": 3
    }
  ],
  "message": null,
  "errors": null
}
```

#### GET `/api/reports/student-history/{studentId}` Example Success (frozen)
```json
{
  "success": true,
  "data": [
    {
      "referenceNo": "DIS-2026-0001",
      "bookTitle": "Sample Book",
      "movementType": 2,
      "quantity": 1,
      "date": "2026-02-09T03:05:00Z"
    }
  ],
  "message": null,
  "errors": null
}
```

### Freeze Validation

Re-validate after any backend contract change:

```powershell
pwsh ./tools/api-contract-tester/run-backend-verification.ps1
```

The frontend freeze is considered current only when this run passes and the scoped example payloads remain accurate.

## 20. Enums

### Term
| Value | Name |
|-------|------|
| 1 | Term1 |
| 2 | Term2 |
| 3 | Both |

### BookCondition
| Value | Name |
|-------|------|
| 1 | Good |
| 2 | Fair |
| 3 | Poor |
| 4 | Damaged |
| 5 | Lost |

### MovementType
| Value | Name |
|-------|------|
| 1 | StockEntry |
| 2 | Distribution |
| 3 | Return |
| 4 | TeacherIssue |
| 5 | TeacherReturn |
| 6 | MarkDamaged |
| 7 | MarkLost |
| 8 | Adjustment |
| 9 | WriteOff |

### SlipType
| Value | Name |
|-------|------|
| 1 | Distribution |
| 2 | Return |
| 3 | TeacherIssue |
| 4 | TeacherReturn |

### TeacherIssueStatus
| Value | Name |
|-------|------|
| 1 | Active |
| 2 | Partial |
| 3 | Returned |
| 4 | Overdue |

### UserRole
| Value | Name |
|-------|------|
| — | SuperAdmin |
| — | Admin |
| — | User |
