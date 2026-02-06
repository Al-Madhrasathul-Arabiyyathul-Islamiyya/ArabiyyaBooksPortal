# Reference Number System

## Overview

Reference numbers are unique identifiers generated for each slip (Distribution, Return, Teacher Issue, Teacher Return). The system supports admin-configurable format templates per slip type per academic year.

## Slip Types

| SlipType | Value | Default Prefix | Description |
|----------|-------|----------------|-------------|
| Distribution | 1 | DST | Student book distribution |
| Return | 2 | RTN | Student book return |
| TeacherIssue | 3 | TIS | Teacher book issue |
| TeacherReturn | 4 | TRT | Teacher book return |

## Format Templates

Administrators can configure a format template for each slip type and academic year combination. Templates support the following tokens:

| Token | Description | Example |
|-------|-------------|---------|
| `{year}` | Academic year (from `AcademicYear.Year`) | `2025` |
| `{autonum}` | Auto-incrementing sequence number, zero-padded | `000001` |
| Literal text | Preserved as-is | `MAI/`, `/bdp/` |

### Examples

| Format Template | Generated Reference |
|----------------|-------------------|
| `DST{year}{autonum}` | `DST2025000001` (default, backward-compatible) |
| `MAI/{year}/bdp/{autonum}` | `MAI/2025/bdp/000001` |
| `MAI/TRB/{year}/prefix-{autonum}` | `MAI/TRB/2025/prefix-000001` |

### Padding Width

The `PaddingWidth` field controls zero-padding on `{autonum}`. Default is 6 digits. Setting to 4 produces `0001`, setting to 8 produces `00000001`.

## Data Model

### ReferenceNumberFormat

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key (BaseEntity) |
| SlipType | SlipType (enum) | Which slip type this format applies to |
| AcademicYearId | int (FK) | Which academic year |
| FormatTemplate | string | The format template with tokens |
| PaddingWidth | int | Zero-padding width for {autonum} (default: 6) |

**Unique constraint**: `(SlipType, AcademicYearId)` — one format per slip type per year.

### ReferenceCounter

| Field | Type | Description |
|-------|------|-------------|
| Key | string (PK) | Counter key: `{SlipType}_{AcademicYearId}` |
| LastSequence | int | Last used sequence number |

## Concurrency Safety

Reference number generation uses SQL Server row-level locking (`UPDLOCK, ROWLOCK`) to prevent duplicate numbers under concurrent requests. The entire generation happens within the caller's transaction scope.

## Fallback Behavior

If no `ReferenceNumberFormat` is configured for a given slip type and academic year, the system falls back to the default format: `{PREFIX}{year}{autonum}` where PREFIX is `DST`, `RTN`, `TIS`, or `TRT`.

## API Endpoints

### Reference Number Formats [Admin]

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/referencenumberformats` | List all formats (filter: ?slipType=&academicYearId=) |
| GET | `/api/referencenumberformats/{id}` | Get format by ID |
| POST | `/api/referencenumberformats` | Create format |
| PUT | `/api/referencenumberformats/{id}` | Update format |
| DELETE | `/api/referencenumberformats/{id}` | Delete format [SuperAdmin] |
