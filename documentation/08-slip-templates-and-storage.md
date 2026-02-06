# Slip Templates & PDF Storage

## Overview

The system generates PDF slips for four transaction types: Distribution, Student Return, Teacher Issue, and Teacher Return. Templates use Dhivehi (Thaana script) for labels and headings, which are stored in the database and configurable via admin API.

## Slip Types & Designs

| Type | Design File | Layout | Table Columns |
|------|------------|--------|---------------|
| Distribution | `distribution slip.png` | A4 landscape, dual copy | Term, Publisher, AcademicYear, SubjectCode, BookTitle |
| Student Return | `student return.png` | A4 landscape, dual copy | Term, Publisher, AcademicYear, SubjectCode, BookTitle |
| Teacher Issue | `teacher issue slip.png` | A4 landscape, dual copy | Publisher, AcademicYear, SubjectCode, BookTitle |
| Teacher Return | `teacher return.png` | A4 landscape, dual copy | Publisher, AcademicYear, SubjectCode, BookTitle |

## Template Layout

Each slip is printed on A4 landscape paper with two identical copies side-by-side (school copy + recipient copy), separated by a cut line.

Each copy contains (top to bottom):

1. **Header**: School logo (top-right), emblem (top-left), school name in Thaana (centered)
2. **Subtitle**: Department name in Thaana
3. **Slip title**: Type-specific title in Thaana
4. **Reference number**: `Ref No: ___` (English)
5. **Info section**: Date and recipient details with Thaana labels
6. **Data table**: Thaana column headers, English/numeric data
7. **Signature blocks**: Two signature areas with fields for Name, ID No, Phone, Signature, Date, Time — all labels in Thaana

## Configurable Labels (SlipTemplateSetting)

All Thaana text is stored in the `SlipTemplateSettings` table, organized by category and key. Labels are cached in memory for performance.

### Data Model

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key (BaseEntity) |
| Category | string | Grouping: "Common", "Distribution", "Return", "TeacherIssue", "TeacherReturn" |
| Key | string | Label identifier (e.g., "SchoolName", "Title", "ColBookTitle") |
| Value | string | The display text (Thaana or English) |
| SortOrder | int | Display ordering in admin UI |

**Unique constraint**: `(Category, Key)`

### Categories & Keys

**Common** (shared across all slip types):
- `SchoolName` — School name in Thaana
- `SchoolSubtitle` — Department subtitle
- `LabelRefNo` — "Ref No:" label
- `LabelName`, `LabelIdNo`, `LabelPhone`, `LabelSignature`, `LabelDate`, `LabelTime`
- `SignatureIssuedBy`, `SignatureReceivedBy`

**Per slip type** (Distribution, Return, TeacherIssue, TeacherReturn):
- `Title` — Slip title
- `DateLabel` — Date section label
- Various info/table column headers specific to each type

### API Endpoints [Admin]

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sliptemplatesettings` | List all (filter: ?category=) |
| GET | `/api/sliptemplatesettings/{id}` | Get by ID |
| PUT | `/api/sliptemplatesettings/{id}` | Update value |
| POST | `/api/sliptemplatesettings/reset` | Reset to defaults [SuperAdmin] |

## Font

The Faruma font (`faruma.ttf`) is used for Thaana rendering. It is embedded as a resource in the API project and registered with QuestPDF's FontManager at startup.

## PDF Storage

### Configuration

```json
{
  "SlipStorage": {
    "BasePath": "D:\\SlipStorage"
  }
}
```

In Docker environments, use a volume-mounted path like `/app/storage/slips`.

### Directory Structure

```
{BasePath}/
  Distribution/{AcademicYear}/
    {ReferenceNo}.pdf
  Return/{AcademicYear}/
    {ReferenceNo}.pdf
  TeacherIssue/{AcademicYear}/
    {ReferenceNo}.pdf
  TeacherReturn/{AcademicYear}/
    {ReferenceNo}.pdf
```

### Storage Flow

1. Slip is created with a reference number
2. PDF is generated from the template
3. PDF bytes are saved to disk at the computed path
4. File path is stored in the slip entity's `PdfFilePath` field
5. Print endpoints serve the stored file (with fallback to regenerate if missing)

### Entity Changes

A nullable `PdfFilePath` (string) field is added to:
- `DistributionSlip`
- `ReturnSlip`
- `TeacherIssue`
- `TeacherReturnSlip`

### Print Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/distributions/{id}/print` | Download distribution slip PDF |
| GET | `/api/returns/{id}/print` | Download return slip PDF |
| GET | `/api/teacher-issues/{id}/print` | Download teacher issue slip PDF |
| GET | `/api/teacher-issues/{id}/return/{returnSlipId}/print` | Download teacher return slip PDF |

## Teacher Return Slip

Teacher returns now produce a separate `TeacherReturnSlip` entity with its own reference number (SlipType.TeacherReturn), in addition to updating the `TeacherIssueItem.ReturnedQuantity` as before.

### TeacherReturnSlip Entity

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key (BaseEntity) |
| ReferenceNo | string | Generated reference number |
| TeacherIssueId | int (FK) | The teacher issue being returned against |
| ReceivedById | int | Staff who received the return |
| ReceivedAt | DateTime | When the return was received |
| Notes | string? | Optional notes |
| PdfFilePath | string? | Path to stored PDF |

### TeacherReturnSlipItem Entity

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key (BaseEntity) |
| TeacherReturnSlipId | int (FK) | Parent return slip |
| TeacherIssueItemId | int (FK) | Which issued item is being returned |
| BookId | int (FK) | The book being returned |
| Quantity | int | Number of copies returned |

## Deferred: Slip Revision

The ability to edit/revise a slip within 24 hours of creation is planned for post-frontend implementation.
