# Books Portal - API Specification

## Base URL

```
https://books.school.local/api/v1
```

## Authentication

All endpoints except `/auth/login` require a valid JWT Bearer token.

```
Authorization: Bearer {token}
```

## Common Response Formats

### Success Response

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully"
}
```

### Paginated Response

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Error Response

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    { "field": "title", "message": "Title is required" }
  ]
}
```

---

## Module 1: Authentication

### POST /auth/login

Authenticate user and receive JWT token.

**Request:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZn...",
    "expiresAt": "2025-02-06T10:30:00Z",
    "user": {
      "id": 1,
      "username": "admin",
      "fullName": "Administrator",
      "email": "admin@school.local",
      "roles": ["Admin"]
    }
  }
}
```

### POST /auth/refresh

Refresh JWT token.

**Request:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZn..."
}
```

### POST /auth/logout

Invalidate refresh token.

### GET /auth/me

Get current user profile.

### PUT /auth/change-password

Change current user's password.

**Request:**
```json
{
  "currentPassword": "oldPassword",
  "newPassword": "newPassword123",
  "confirmPassword": "newPassword123"
}
```

---

## Module 2: User Management

*Requires: SuperAdmin or Admin role*

### GET /users

Get paginated list of users.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number (default: 1) |
| pageSize | int | Items per page (default: 20) |
| search | string | Search by name, username, email |
| role | string | Filter by role |
| isActive | bool | Filter by active status |

### GET /users/{id}

Get user by ID.

### POST /users

Create new user.

**Request:**
```json
{
  "username": "johndoe",
  "password": "tempPassword123",
  "fullName": "John Doe",
  "email": "john@school.local",
  "nationalId": "A123456",
  "designation": "Librarian",
  "roles": ["User"]
}
```

### PUT /users/{id}

Update user.

### DELETE /users/{id}

Soft delete user.

### POST /users/{id}/reset-password

Reset user password (Admin only).

### GET /roles

Get list of available roles.

---

## Module 3: Academic Year

### GET /academic-years

Get all academic years.

### GET /academic-years/active

Get current active academic year.

### GET /academic-years/{id}

Get academic year by ID.

### POST /academic-years

Create new academic year.

**Request:**
```json
{
  "name": "2025-2026",
  "year": 2025,
  "startDate": "2025-01-01",
  "endDate": "2025-12-31"
}
```

### PUT /academic-years/{id}

Update academic year.

### POST /academic-years/{id}/activate

Set as active academic year.

---

## Module 4: Master Data

### Keystages

#### GET /keystages
#### GET /keystages/{id}
#### POST /keystages

```json
{
  "code": "KS1",
  "name": "Key Stage 1",
  "sortOrder": 1
}
```

#### PUT /keystages/{id}
#### DELETE /keystages/{id}

---

### Subjects

#### GET /subjects
#### GET /subjects/{id}
#### POST /subjects

```json
{
  "code": "MAT",
  "name": "Mathematics"
}
```

#### PUT /subjects/{id}
#### DELETE /subjects/{id}

---

### Class Sections

#### GET /class-sections

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| academicYearId | int | Filter by academic year |
| keystageId | int | Filter by keystage |
| grade | string | Filter by grade |

#### GET /class-sections/{id}
#### POST /class-sections

```json
{
  "academicYearId": 1,
  "keystageId": 2,
  "grade": "Grade 5",
  "section": "A"
}
```

#### PUT /class-sections/{id}
#### DELETE /class-sections/{id}

---

### Students

#### GET /students

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| classSectionId | int | Filter by class |
| search | string | Search by name or index |

#### GET /students/{id}
#### GET /students/by-index/{indexNo}
#### POST /students

```json
{
  "fullName": "Ahmed Ali",
  "indexNo": "STU2025001",
  "nationalId": "A123456",
  "classSectionId": 1,
  "parents": [
    {
      "parentId": 1,
      "isPrimary": true
    }
  ]
}
```

#### PUT /students/{id}
#### DELETE /students/{id}

---

### Parents

#### GET /parents

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| search | string | Search by name, phone, national ID |

#### GET /parents/{id}
#### GET /parents/by-national-id/{nationalId}
#### POST /parents

```json
{
  "fullName": "Mohamed Ali",
  "nationalId": "A654321",
  "phone": "+960 7771234",
  "relationship": "Father"
}
```

#### PUT /parents/{id}
#### DELETE /parents/{id}

---

### Teachers

#### GET /teachers
#### GET /teachers/{id}
#### POST /teachers

```json
{
  "fullName": "Sara Ahmed",
  "nationalId": "A111222",
  "email": "sara@school.local",
  "phone": "+960 7775678",
  "assignments": [
    {
      "subjectId": 1,
      "classSectionId": 1
    }
  ]
}
```

#### PUT /teachers/{id}
#### DELETE /teachers/{id}
#### GET /teachers/{id}/assignments
#### POST /teachers/{id}/assignments
#### DELETE /teachers/{id}/assignments/{assignmentId}

---

## Module 5: Book Management

### GET /books

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| search | string | Search by title, author, ISBN, code |
| subjectId | int | Filter by subject |
| grade | string | Filter by grade |
| hasStock | bool | Filter books with available stock |

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "isbn": "978-3-16-148410-0",
        "code": "MAT-G5-001",
        "title": "Mathematics Grade 5",
        "author": "Ministry of Education",
        "edition": "2024",
        "publisher": "Educational Publishing",
        "publishedYear": 2024,
        "subjectId": 1,
        "subjectName": "Mathematics",
        "grade": "Grade 5",
        "totalStock": 100,
        "distributed": 45,
        "withTeachers": 5,
        "damaged": 2,
        "lost": 1,
        "available": 47
      }
    ],
    "totalCount": 50
  }
}
```

### GET /books/{id}

### GET /books/by-code/{code}

### GET /books/search

Quick search for autocomplete.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| q | string | Search term (min 2 chars) |
| limit | int | Max results (default: 10) |

### POST /books

Create new book.

**Request:**
```json
{
  "isbn": "978-3-16-148410-0",
  "code": "MAT-G5-001",
  "title": "Mathematics Grade 5",
  "author": "Ministry of Education",
  "edition": "2024",
  "publisher": "Educational Publishing",
  "publishedYear": 2024,
  "subjectId": 1,
  "grade": "Grade 5"
}
```

### PUT /books/{id}

Update book details (not stock quantities).

### DELETE /books/{id}

Soft delete book (only if no active distributions).

---

### Stock Operations

#### POST /books/{id}/stock-entry

Add stock to a book.

**Request:**
```json
{
  "quantity": 50,
  "source": "Ministry",
  "notes": "Annual allocation"
}
```

#### GET /books/{id}/stock-entries

Get stock entry history for a book.

#### GET /books/{id}/stock-movements

Get all stock movements for a book.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| fromDate | date | Start date |
| toDate | date | End date |
| movementType | int | Filter by type |

#### POST /books/{id}/adjust-stock

Manual stock adjustment (Admin only).

**Request:**
```json
{
  "field": "damaged",
  "adjustment": 2,
  "reason": "Books found damaged in storage"
}
```

---

## Module 6: Distribution

### GET /distributions

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| academicYearId | int | Filter by academic year |
| studentId | int | Filter by student |
| classSectionId | int | Filter by class |
| term | int | Filter by term (1, 2, 3) |
| fromDate | date | Start date |
| toDate | date | End date |

### GET /distributions/{id}

### GET /distributions/by-reference/{referenceNo}

### POST /distributions

Create new distribution slip.

**Request:**
```json
{
  "term": 1,
  "studentId": 1,
  "parentId": 1,
  "notes": "",
  "items": [
    {
      "bookId": 1,
      "quantity": 1
    },
    {
      "bookId": 2,
      "quantity": 1
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "referenceNo": "DST2025000001",
    "term": 1,
    "student": {
      "id": 1,
      "fullName": "Ahmed Ali",
      "indexNo": "STU2025001",
      "className": "Grade 5 - A"
    },
    "parent": {
      "id": 1,
      "fullName": "Mohamed Ali",
      "nationalId": "A654321",
      "phone": "+960 7771234"
    },
    "issuedBy": {
      "id": 1,
      "fullName": "Administrator"
    },
    "issuedAt": "2025-02-05T10:30:00Z",
    "items": [
      {
        "bookId": 1,
        "bookTitle": "Mathematics Grade 5",
        "bookCode": "MAT-G5-001",
        "quantity": 1
      }
    ]
  },
  "message": "Distribution slip created successfully"
}
```

### DELETE /distributions/{id}

Cancel/void distribution slip (reverses stock).

### GET /distributions/{id}/print

Get printable slip data.

---

## Module 7: Returns

### GET /returns

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| academicYearId | int | Filter by academic year |
| studentId | int | Filter by student |
| classSectionId | int | Filter by class |
| fromDate | date | Start date |
| toDate | date | End date |

### GET /returns/{id}

### GET /returns/by-reference/{referenceNo}

### POST /returns

Create new return slip.

**Request:**
```json
{
  "studentId": 1,
  "returnedById": 1,
  "notes": "",
  "items": [
    {
      "bookId": 1,
      "quantity": 1,
      "condition": 1,
      "conditionNotes": ""
    },
    {
      "bookId": 2,
      "quantity": 1,
      "condition": 4,
      "conditionNotes": "Cover torn, pages water damaged"
    }
  ]
}
```

### DELETE /returns/{id}

Cancel/void return slip (reverses stock).

### GET /returns/{id}/print

Get printable slip data.

---

## Module 8: Teacher Issues

### GET /teacher-issues

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| academicYearId | int | Filter by academic year |
| teacherId | int | Filter by teacher |
| status | int | Filter by status (1=Active, 2=Partial, 3=Returned) |
| fromDate | date | Start date |
| toDate | date | End date |

### GET /teacher-issues/{id}

### GET /teacher-issues/by-reference/{referenceNo}

### POST /teacher-issues

Create new teacher issue.

**Request:**
```json
{
  "teacherId": 1,
  "expectedReturnDate": "2025-06-30",
  "notes": "For Grade 5 teaching",
  "items": [
    {
      "bookId": 1,
      "quantity": 2
    }
  ]
}
```

### POST /teacher-issues/{id}/return

Process return of teacher-issued books.

**Request:**
```json
{
  "items": [
    {
      "itemId": 1,
      "returnedQuantity": 2
    }
  ],
  "notes": "All books returned in good condition"
}
```

### DELETE /teacher-issues/{id}

Cancel/void teacher issue (reverses stock).

### GET /teacher-issues/{id}/print

Get printable slip data.

---

## Module 9: Reports

### GET /reports/stock-summary

Overall stock summary.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| subjectId | int | Filter by subject |
| grade | string | Filter by grade |

**Response:**
```json
{
  "success": true,
  "data": {
    "totalBooks": 5000,
    "totalDistributed": 2500,
    "totalWithTeachers": 150,
    "totalDamaged": 50,
    "totalLost": 25,
    "totalAvailable": 2275,
    "booksBySubject": [
      { "subject": "Mathematics", "total": 1000, "available": 455 }
    ],
    "booksByGrade": [
      { "grade": "Grade 5", "total": 800, "available": 380 }
    ]
  }
}
```

### GET /reports/distribution-summary

Distribution statistics.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| academicYearId | int | Academic year |
| fromDate | date | Start date |
| toDate | date | End date |
| groupBy | string | day, week, month |

### GET /reports/movement-log

Stock movement audit log.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| bookId | int | Filter by book |
| movementType | int | Filter by type |
| fromDate | date | Start date |
| toDate | date | End date |
| pageNumber | int | Page number |
| pageSize | int | Items per page |

### GET /reports/class-distribution

Distribution by class.

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| academicYearId | int | Academic year |
| classSectionId | int | Filter by class |

### GET /reports/student-history/{studentId}

Complete book history for a student.

### GET /reports/teacher-outstanding

List of outstanding teacher issues.

### GET /reports/export/{reportType}

Export report as Excel.

**Report Types:** stock-summary, distributions, returns, teacher-issues, movements

---

## Module 10: Audit Log

### GET /audit-logs

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| entityType | string | Filter by entity |
| action | string | Filter by action |
| userId | int | Filter by user |
| fromDate | datetime | Start date |
| toDate | datetime | End date |

### GET /audit-logs/{id}

Get audit log detail with full old/new values.

---

## Lookup Endpoints

Utility endpoints for dropdowns and autocomplete.

### GET /lookups/subjects
### GET /lookups/keystages
### GET /lookups/grades
### GET /lookups/class-sections
### GET /lookups/academic-years
### GET /lookups/terms
### GET /lookups/book-conditions
### GET /lookups/movement-types

---

## Error Codes

| Code | Description |
|------|-------------|
| 400 | Bad Request - Validation error |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Business rule violation |
| 500 | Internal Server Error |

### Business Error Examples

```json
{
  "success": false,
  "message": "Insufficient stock",
  "errors": [
    {
      "field": "items[0].quantity",
      "message": "Book 'Mathematics Grade 5' only has 5 available, requested 10"
    }
  ]
}
```

```json
{
  "success": false,
  "message": "Cannot delete book with active distributions",
  "errors": [
    {
      "field": "id",
      "message": "Book has 25 copies currently distributed. Return all copies before deleting."
    }
  ]
}
```
