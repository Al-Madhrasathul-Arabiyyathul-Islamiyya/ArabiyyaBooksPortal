# Books Portal - Database Schema

## Migration Safety Policy

Effective policy for all future schema changes:

- Prefer non-destructive, in-place upgradable migrations.
- Avoid destructive operations (drop/rename/alter with data loss risk) unless there is no safe alternative.
- Favor additive evolution:
  - add nullable/new columns first,
  - backfill data,
  - switch application reads/writes,
  - then retire old fields in a later migration window.
- Prefer pivot/join tables over destructive reshaping of high-volume tables when modeling changes permit.
- Every migration should preserve existing production data and support rolling forward without manual data loss operations.

## Entity Relationship Overview

```
AcademicYear ──┬── ClassSection ──┬── Student ──── StudentParent ──── Parent
               │                  └── TeacherAssignment ── Teacher
               │
Keystage ──────┘
               
Subject ───────── Book ──┬── StockEntry
                         ├── StockMovement
                         ├── DistributionSlipItem ── DistributionSlip
                         ├── ReturnSlipItem ──────── ReturnSlip
                         └── TeacherIssueItem ────── TeacherIssue

Staff (Users) ── AuditLog
```

---

## Core Entities

### AcademicYear

Defines academic year periods for data segregation.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| Name | NVARCHAR(50) | e.g., "2025-2026" |
| Year | INT | e.g., 2025 |
| StartDate | DATE | Year start |
| EndDate | DATE | Year end |
| IsActive | BIT | Current active year |

---

### Keystage

Key stages for curriculum organization.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| Code | NVARCHAR(10) | e.g., "KS1", "KS2" |
| Name | NVARCHAR(100) | e.g., "Key Stage 1" |
| SortOrder | INT | Display order |

---

### Subject

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| Code | NVARCHAR(10) | e.g., "MAT", "ENG" |
| Name | NVARCHAR(100) | Full subject name |

---

### ClassSection

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| AcademicYearId | INT FK | Reference to AcademicYear |
| KeystageId | INT FK | Reference to Keystage |
| Grade | NVARCHAR(20) | e.g., "Grade 5" |
| Section | NVARCHAR(10) | e.g., "A", "B" |
| DisplayName | COMPUTED | Grade + Section |

---

### Staff (Application Users)

Extends ASP.NET Core Identity `IdentityUser<int>`.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| UserName | NVARCHAR(256) | Login username |
| Email | NVARCHAR(256) | Email address |
| FullName | NVARCHAR(200) | Display name |
| NationalId | NVARCHAR(50) | National ID |
| Designation | NVARCHAR(100) | Job title |
| IsActive | BIT | Account status |

---

### Student

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| FullName | NVARCHAR(200) | Student name |
| IndexNo | NVARCHAR(50) | Student index (unique) |
| NationalId | NVARCHAR(50) | Optional |
| ClassSectionId | INT FK | Current class |

---

### Parent

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| FullName | NVARCHAR(200) | Parent name |
| NationalId | NVARCHAR(50) | National ID (unique) |
| Phone | NVARCHAR(20) | Contact number |
| Relationship | NVARCHAR(50) | Mother/Father/Guardian |

---

### StudentParent (Junction)

| Field | Type | Description |
|-------|------|-------------|
| StudentId | INT FK | Reference to Student |
| ParentId | INT FK | Reference to Parent |
| IsPrimary | BIT | Primary contact flag |

---

### Teacher

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| FullName | NVARCHAR(200) | Teacher name |
| NationalId | NVARCHAR(50) | National ID (unique) |
| Email | NVARCHAR(256) | Email |
| Phone | NVARCHAR(20) | Contact |

---

### TeacherAssignment

Links teachers to subjects and classes.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| TeacherId | INT FK | Reference to Teacher |
| SubjectId | INT FK | Reference to Subject |
| ClassSectionId | INT FK | Reference to ClassSection |

---

## Book Entities

### Book

Book catalog with stock quantities.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| ISBN | NVARCHAR(20) | ISBN (optional) |
| Code | NVARCHAR(50) | School code (unique) |
| Title | NVARCHAR(500) | Book title |
| Author | NVARCHAR(300) | Author(s) |
| Edition | NVARCHAR(50) | Edition |
| Publisher | NVARCHAR(200) | Publisher |
| PublishedYear | INT | Year published |
| SubjectId | INT FK | Reference to Subject |
| Grade | NVARCHAR(20) | Target grade |
| **TotalStock** | INT | Total ever entered |
| **Distributed** | INT | Currently with students |
| **WithTeachers** | INT | Currently with teachers |
| **Damaged** | INT | Marked damaged |
| **Lost** | INT | Marked lost |
| **Available** | COMPUTED | TotalStock - Distributed - WithTeachers - Damaged - Lost |

---

### StockEntry

Records each addition of books to stock.

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| BookId | INT FK | Reference to Book |
| AcademicYearId | INT FK | Reference to AcademicYear |
| Quantity | INT | Books added |
| Source | NVARCHAR(100) | e.g., "Ministry", "Purchase" |
| Notes | NVARCHAR(500) | Remarks |
| EnteredById | INT FK | Staff who entered |
| EnteredAt | DATETIME2 | Entry timestamp |

---

### StockMovement

Audit trail for all stock changes.

| Field | Type | Description |
|-------|------|-------------|
| Id | BIGINT PK | Primary key |
| BookId | INT FK | Reference to Book |
| AcademicYearId | INT FK | Reference to AcademicYear |
| MovementType | INT | See enum below |
| Quantity | INT | Positive or negative |
| ReferenceId | INT | Related slip ID |
| ReferenceType | NVARCHAR(50) | "Distribution", "Return", etc. |
| Notes | NVARCHAR(500) | Remarks |
| ProcessedById | INT FK | Staff who processed |
| ProcessedAt | DATETIME2 | Timestamp |

**MovementType Enum:**
1. StockEntry
2. Distribution
3. Return
4. TeacherIssue
5. TeacherReturn
6. MarkDamaged
7. MarkLost
8. Adjustment
9. WriteOff

---

## Transaction Entities

### DistributionSlip

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| ReferenceNo | NVARCHAR(50) | Auto-generated (unique) |
| AcademicYearId | INT FK | Reference to AcademicYear |
| Term | INT | 1=Term1, 2=Term2, 3=Both |
| StudentId | INT FK | Reference to Student |
| ParentId | INT FK | Parent who collected |
| IssuedById | INT FK | Staff who processed |
| IssuedAt | DATETIME2 | Timestamp |
| Notes | NVARCHAR(500) | Remarks |

### DistributionSlipItem

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| DistributionSlipId | INT FK | Reference to slip |
| BookId | INT FK | Reference to Book |
| Quantity | INT | Number distributed |

---

### ReturnSlip

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| ReferenceNo | NVARCHAR(50) | Auto-generated (unique) |
| AcademicYearId | INT FK | Reference to AcademicYear |
| StudentId | INT FK | Reference to Student |
| ReturnedById | INT FK | Parent who returned |
| ReceivedById | INT FK | Staff who processed |
| ReceivedAt | DATETIME2 | Timestamp |
| Notes | NVARCHAR(500) | Remarks |

### ReturnSlipItem

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| ReturnSlipId | INT FK | Reference to slip |
| BookId | INT FK | Reference to Book |
| Quantity | INT | Number returned |
| Condition | INT | 1=Good, 2=Fair, 3=Poor, 4=Damaged, 5=Lost |
| ConditionNotes | NVARCHAR(500) | Damage description |

---

### TeacherIssue

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| ReferenceNo | NVARCHAR(50) | Auto-generated (unique) |
| AcademicYearId | INT FK | Reference to AcademicYear |
| TeacherId | INT FK | Reference to Teacher |
| IssuedById | INT FK | Staff who processed |
| IssuedAt | DATETIME2 | Timestamp |
| ExpectedReturnDate | DATE | Optional due date |
| Status | INT | 1=Active, 2=Partial, 3=Returned, 4=Overdue |
| Notes | NVARCHAR(500) | Remarks |

### TeacherIssueItem

| Field | Type | Description |
|-------|------|-------------|
| Id | INT PK | Primary key |
| TeacherIssueId | INT FK | Reference to issue |
| BookId | INT FK | Reference to Book |
| Quantity | INT | Number issued |
| ReturnedQuantity | INT | Number returned |
| ReturnedAt | DATETIME2 | Return timestamp |
| ReceivedById | INT FK | Staff who received return |

---

## Audit Entity

### AuditLog

| Field | Type | Description |
|-------|------|-------------|
| Id | BIGINT PK | Primary key |
| Action | NVARCHAR(50) | CREATE, UPDATE, DELETE, etc. |
| EntityType | NVARCHAR(100) | Entity name |
| EntityId | NVARCHAR(50) | Record ID |
| OldValues | NVARCHAR(MAX) | JSON of previous values |
| NewValues | NVARCHAR(MAX) | JSON of new values |
| UserId | INT FK | Who performed action |
| UserName | NVARCHAR(256) | Username snapshot |
| Timestamp | DATETIME2 | When it happened |
| IpAddress | NVARCHAR(50) | Client IP |

---

## Common Fields (BaseEntity)

All entities except junction tables include:

| Field | Type | Description |
|-------|------|-------------|
| CreatedAt | DATETIME2 | Creation timestamp |
| CreatedBy | INT | Creator user ID |
| UpdatedAt | DATETIME2 | Last update timestamp |
| UpdatedBy | INT | Last updater user ID |
| IsDeleted | BIT | Soft delete flag |
| DeletedAt | DATETIME2 | Deletion timestamp |

---

## Reference Number Format

| Slip Type | Prefix | Example |
|-----------|--------|---------|
| Distribution | DST | DST2025000001 |
| Return | RTN | RTN2025000001 |
| Teacher Issue | TIS | TIS2025000001 |

Pattern: `{PREFIX}{YEAR}{6-DIGIT SEQUENCE}`
