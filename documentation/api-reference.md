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
  | grade | string | yes |
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
  | nationalId | string? | no |
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
  | nationalId | string? | no |
  | classSectionId | int | yes |
  | parents | StudentParentRequest[]? | no |
- **Response**: `StudentResponse`

### DELETE /api/students/{id}

Delete a student.

- **Auth**: Bearer + **SuperAdmin** role
- **Response**: `string` message

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
  | publisher | string? | no |
  | publishedYear | int? | no |
  | subjectId | int | yes |
  | grade | string? | no |
- **Response**: `BookResponse`

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

- **Query**: `?pageNumber={int}&pageSize={int}&academicYearId={int}&studentId={int}`
- **Defaults**: pageNumber=1, pageSize=20
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
  | notes | string? | no |
  | items | CreateDistributionSlipItemRequest[] | yes |

  `CreateDistributionSlipItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `DistributionSlipResponse`

### DELETE /api/distributions/{id}

Cancel a distribution slip (reverses stock).

- **Response**: `string` message

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
| issuedById | int |
| issuedAt | DateTime |
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
| receivedById | int |
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

- **Query**: `?pageNumber={int}&pageSize={int}&academicYearId={int}&teacherId={int}`
- **Defaults**: pageNumber=1, pageSize=20
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
  | expectedReturnDate | DateTime? | no |
  | notes | string? | no |
  | items | TeacherIssueItemRequest[] | yes |

  `TeacherIssueItemRequest`:
  | Field | Type |
  |-------|------|
  | bookId | int |
  | quantity | int |
- **Response**: `TeacherIssueResponse`

### POST /api/TeacherIssues/{id}/return

Process a partial or full return of books from a teacher.

- **Request**: `ProcessTeacherReturnRequest`
  | Field | Type | Required |
  |-------|------|----------|
  | notes | string? | no |
  | items | TeacherReturnItemRequest[] | yes |

  `TeacherReturnItemRequest`:
  | Field | Type |
  |-------|------|
  | teacherIssueItemId | int |
  | quantity | int |
- **Response**: `TeacherReturnSlipResponse`

### DELETE /api/TeacherIssues/{id}

Cancel a teacher issue (reverses stock).

- **Response**: `string` message

### GET /api/TeacherIssues/{id}/print

Download PDF for a teacher issue.

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
| issuedById | int |
| issuedAt | DateTime |
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
| academicYearId | int |
| academicYearName | string |
| receivedById | int |
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

---

## 19. Enums

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
