# BooksPortal Frontend Implementation Plan

## Overview
Nuxt 4 SSR application for the Books Portal textbook management system. All backend APIs (Modules 1-9) are complete.

**Status Legend:** `[ ]` Todo | `[~]` In Progress | `[x]` Done

### Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Nuxt | 4.3 | Vue meta-framework (SSR mode) |
| Vue | 3.5 | Reactive UI framework |
| TypeScript | 5.x | Type safety |
| PrimeVue | 4.5 | UI component library (Aura theme) |
| Tailwind CSS | 4.1 | Utility-first CSS (via `@tailwindcss/vite`) |
| tailwindcss-primeui | Latest | PrimeVue + Tailwind CSS layer integration |
| Pinia | 3 | State management |
| Pinia Colada | 0.3 | Server-state caching / data fetching |
| Regle + Zod | Latest | Form validation (Zod v4 schemas via Standard Schema) |
| Day.js | Latest | Date handling (Indian/Maldives TZ) |
| nuxt-charts | 2.1 | Dashboard charts |
| @nuxt/icon | 2.2 | Iconify icons |
| @nuxt/image | 2.0 | Image optimization |
| @nuxt/fonts | 0.13 | Font loading (Sofia Sans, Playfair Display, Geist Mono) |

### Performance Note
- `nuxt-charts`/Unovis (including `elkjs`) significantly increases client bundle size.
- Keep dependency for now, but lazy-load chart/report components in Phase 7 (`/admin/reports/*`) and avoid importing chart modules in shared layouts/composables.

### Fonts
- **Sans**: Sofia Sans (Google Fonts via @nuxt/fonts)
- **Serif**: Playfair Display (Google Fonts via @nuxt/fonts)
- **Mono**: Geist Mono (Google Fonts via @nuxt/fonts)
- **Thaana**: Faruma (local, `public/fonts/Faruma.ttf`)

### API Reference
See `documentation/api-reference.md` for all endpoints, DTOs, and auth requirements.

### Information Architecture (Planned)
- Operational surface (all authenticated roles): `/`, `/distribution/*`, `/returns/*`, `/teacher-issues/*`
- Admin surface (Admin/SuperAdmin only): `/admin/*`
- Sidebar model:
  - Main operational menu shown by default
  - `Admin` link pinned at the bottom, visible only to `Admin`/`SuperAdmin`
  - All management, reports, settings, and audit screens nested under `/admin`

### UX Conventions (Planned)
- Prefer modal dialogs for create/edit forms where feasible to reduce navigation overhead.
- Use full-page forms only when form complexity is high (e.g., deeply nested or multi-section workflows).
- For lookup/search dropdowns, provide inline "Create new" actions when no matching record exists.
  - Priority entities: `Student`, `Parent`, `Teacher`.

---

## Phase 1: Foundation & Configuration
> Nuxt config, types, API client, auth, layouts, login page

### 1.1 Nuxt Configuration
- [x] Install Tailwind CSS v4 via `@tailwindcss/vite` plugin
- [x] Install `tailwindcss-primeui` for PrimeVue layer integration
- [x] Configure `nuxt.config.ts`
  - [x] SSR enabled (default)
  - [x] Configure PrimeVue module (Aura theme, `.dark` selector, ripple)
  - [x] Set `runtimeConfig.public.apiBase` to `http://localhost:5071/api`
  - [x] Configure `dayjs-nuxt` (locale: en, timezone: Indian/Maldives)
  - [x] Configure `@nuxtjs/color-mode` (preference: system, cookie storage, classSuffix: '')
  - [x] Configure `@nuxt/fonts` (Sofia Sans, Playfair Display, Geist Mono)
- [x] Configure `main.css` (TW4 import, tailwindcss-primeui, dark variant, @theme fonts, Faruma @font-face)
- [x] Update `app/app.vue` (`<NuxtLayout><NuxtPage /></NuxtLayout>` + Toast + ConfirmDialog)

### 1.2 TypeScript Types (Zod v4 Schemas)
- [x] `app/types/api.ts` — `ApiResponseSchema<T>`, `PaginatedListSchema<T>`, `ApiError`, `PaginationParams`
- [x] `app/types/enums.ts` — Zod enums + value maps: `Term`, `BookCondition`, `MovementType`, `SlipType`, `TeacherIssueStatus`, `UserRole`
- [x] `app/types/entities.ts` — Zod schemas + inferred types for all response DTOs:
  - [x] Auth: `TokenResponse`, `UserProfile`
  - [x] Master Data: `AcademicYear`, `Keystage`, `Subject`, `ClassSection`, `Student`, `StudentParent`, `Parent`, `Teacher`, `TeacherAssignment`, `Lookup`
  - [x] Books: `Book`, `StockEntry`, `StockMovement`
  - [x] Distribution: `DistributionSlip`, `DistributionSlipItem`
  - [x] Returns: `ReturnSlip`, `ReturnSlipItem`
  - [x] Teacher Issues: `TeacherIssue`, `TeacherIssueItem`, `TeacherReturnSlip`, `TeacherReturnSlipItem`
  - [x] Reports: `StockSummaryReport`, `DistributionSummaryReport`, `TeacherOutstandingReport`, `StudentHistoryReport`
  - [x] Settings: `ReferenceNumberFormat`, `SlipTemplateSetting`
  - [x] Users: `User`
  - [x] Audit: `AuditLog`
- [x] `app/types/forms.ts` — Zod schemas with validation for all request DTOs:
  - [x] Auth: `LoginRequest`, `RefreshTokenRequest`, `ChangePasswordRequest`
  - [x] Master Data: all create/update requests
  - [x] Books: `CreateBookRequest`, `AddStockRequest`, `AdjustStockRequest`
  - [x] Distribution: `CreateDistributionSlipRequest`, `CreateDistributionSlipItemRequest`
  - [x] Returns: `CreateReturnSlipRequest`, `CreateReturnSlipItemRequest`
  - [x] Teacher Issues: all request types
  - [x] Settings: `CreateReferenceNumberFormatRequest`, `UpdateSlipTemplateSettingRequest`
  - [x] Users: `CreateUserRequest`, `UpdateUserRequest`

### 1.3 Utilities
- [x] `app/utils/constants.ts` — API paths, pagination defaults, role names
- [x] `app/utils/formatters.ts` — enum label maps, severity maps, option lists

### 1.4 Styles
- [x] `app/assets/css/main.css` — TW4 import, tailwindcss-primeui, dark variant, @theme fonts, Faruma @font-face

### 1.5 API Client
- [x] `app/composables/useApi.ts`
  - [x] Use frontend BFF base URL (`/api/bff`)
  - [x] Use `nuxt-csurf` fetch integration for state-changing requests
  - [x] Handle 401 → attempt token refresh → retry original request
  - [x] `get`, `post`, `put`, `del`, `downloadBlob` methods

### 1.6 Auth Store & Composable
- [x] `app/stores/auth.ts` (Pinia)
  - [x] State: `user` (UserProfile | null)
  - [x] Getters: `isAuthenticated`, `roles`, `hasRole()`, `hasAnyRole()`
  - [x] Actions: `login()`, `logout()`, `fetchProfile()`, `initialize()`
  - [x] Auth state is profile-based; tokens are handled server-side in HttpOnly cookies
- [x] `app/composables/useAuth.ts` — convenience wrapper

### 1.7 Middleware
- [x] `app/middleware/auth.global.ts` — initialize auth store, redirect to `/login` if not authenticated
- [x] `app/middleware/role.ts` — check `meta.roles` against user roles

### 1.8 Layouts
- [x] `app/layouts/default.vue` — sidebar (PanelMenu) + header (breadcrumb, color toggle, user menu) + content
- [x] `app/layouts/auth.vue` — centered card with color mode toggle

### 1.9 Login Page
- [x] `app/pages/login.vue`
  - [x] Email + password fields with Zod + Regle validation
  - [x] Error display for invalid credentials
  - [x] Uses `auth` layout

### Phase 1 Verification
- [x] `bun run build` succeeds (SSR mode)
- [x] Login page renders at `/login`
- [x] Color mode toggle works on login and main layout
- [x] Dark mode respects system preference

---

## Phase 2: Common Components & Composables
> Shared UI components, remaining composables, app-wide stores

### 2.1 Composables
- [x] `app/composables/useAppToast.ts` — wraps PrimeVue `useToast()`: `showSuccess()`, `showError()`, `showInfo()`, `showWarn()`
- [x] `app/composables/useAppConfirm.ts` — wraps PrimeVue `useConfirm()`: `confirmDelete()`, `confirmAction()`
- [x] `app/composables/usePagination.ts` — `page`, `pageSize`, `onPage(event)` (0-based→1-based), `queryParams`
- [x] `app/composables/useAcademicYear.ts` — provide/inject pattern for active year context
- [x] `app/composables/usePrint.ts` — fetch PDF blob, open in new tab / download (browser-guarded)

### 2.2 Stores
- [x] `app/stores/app.ts` — `sidebarCollapsed`, `toggleSidebar()`
- [x] `app/stores/lookups.ts` — cached lookup data:
  - [x] `academicYears`, `keystages`, `subjects`, `classSections`, `terms`, `bookConditions`, `movementTypes`
  - [x] `fetchAll()` action, individual `fetchX()` actions
  - [x] `getLookupLabel(list, id)` helper

### 2.3 Layout Components
- [x] Sidebar + header integrated directly in `default.vue` layout
  - [x] PanelMenu with nested items
  - [x] Role-based item visibility (Settings only for Admin+)
  - [x] Collapsible sidebar
  - [x] Color mode toggle in header
  - [x] User menu dropdown (Profile, Logout)
- [x] `app/components/common/AppBreadcrumb.vue` — auto-generate from route path

### 2.4 Shared Components
- [x] `app/components/common/EmptyState.vue` — icon + title + subtitle + action button
- [x] `app/components/forms/FormField.vue` — label + slot + error wrapper
- [x] `app/components/forms/SearchInput.vue` — debounced search with clear button

### Phase 2 Verification
- [x] Default layout renders with sidebar + header
- [x] Color mode toggle works across all pages
- [x] Build succeeds with no duplicate import warnings

---

## Phase 2.5: BFF + Security Alignment
> SSR-first auth/session model and API route alignment before feature expansion

### 2.5.1 BFF Proxy Layer
- [x] Add Nitro API endpoints under `server/api/bff/*`
  - [x] Auth handlers: login, logout, refresh, me
  - [x] Catch-all proxy for non-auth backend routes
- [x] Add server auth session utilities
  - [x] Read/set/clear `bp_access_token`, `bp_refresh_token`, `bp_token_expiry`
  - [x] Token expiry check + refresh flow

### 2.5.2 CSRF + Client API Refactor
- [x] Add and configure `nuxt-csurf`
- [x] Refactor `useApi` to call `/api/bff` instead of direct backend API
- [x] Remove client-side bearer header logic from app code
- [x] Keep SSR-safe blob download path for print/export endpoints

### 2.5.3 Contract/Role Drift Corrections
- [x] Update endpoint constants in `app/utils/constants.ts` to match backend routes
- [x] Remove unsupported lookup constants (teachers/parents) from lookups store
- [x] Align frontend role constant with backend (`SuperAdmin`, `Admin`, `User`)

### Phase 2.5 Verification
- [x] Login and layout rendering validated with BFF flow
- [x] `.nuxt` TS checks pass (`bunx tsc --noEmit -p .nuxt/tsconfig.json`)
- [~] `bun run build` completes but may take longer in this environment

---

## Phase 2.6: Routing + Layout Surface Split
> Split operations and admin information architecture before Phase 3 implementation

### 2.6.1 Layout and Navigation Split
- [x] Keep `default` layout operations-focused (`/`, `/distribution`, `/returns`, `/teacher-issues`)
- [x] Add bottom `Admin` entry in operations sidebar (Admin/SuperAdmin only)
- [x] Introduce dedicated `admin` layout with admin-only navigation groups

### 2.6.2 Admin Route Scaffolding
- [x] Add `/admin` route shell with role guard
- [x] Add `/admin/audit-log` route shell with role guard
- [x] Place audit log in admin top-level nav (not nested under settings)

### 2.6.3 Next Phase 2.6 Tasks
- [x] Add `/admin/*` placeholder shells for master-data/books/reports/settings routes used by nav
- [x] Add admin route breadcrumbs and route metadata consistency pass
- [x] Validate role access behavior (`User` blocked from `/admin/*`)

### Phase 2.6 Verification
- [x] `bunx tsc --noEmit -p .nuxt/tsconfig.json`
- [x] `bun run build`
- [x] Manual check: user sees operations-only menu, admin sees bottom `Admin` link and admin layout

---

## Phase 2.7: Backend Contract Alignment (Post-Refactor)
> Bring frontend foundation/composables/server routes in line with backend `Grade` + bulk-import changes before completing remaining Phase 3+ screens
- [x] Alignment branch created: `feature/module-10-backend-contract-alignment`
- [~] Implementation started (plan + app title config done; contract/type/store alignment pending)

### 2.7.1 Route and BFF Contract Sync
- [ ] Verify BFF catch-all/proxy supports new backend routes:
  - [ ] `GET /lookups/grades`
  - [ ] `POST /books/bulk/validate`, `POST /books/bulk/commit`
  - [ ] `POST /teachers/bulk/validate`, `POST /teachers/bulk/commit`
  - [ ] `POST /students/bulk/validate`, `POST /students/bulk/commit`
  - [ ] `GET /import-templates/books|teachers|students`
- [ ] Add explicit typed API helpers/composables for bulk validate/commit + template download flows

### 2.7.2 DTO/Form Type Alignment
- [ ] Update frontend schemas/types for backend changes:
  - [ ] ClassSection create/update uses `gradeId` (not free-text `grade`)
  - [ ] ClassSection response includes `gradeId`
  - [ ] Student create/update requires `nationalId`
  - [ ] Book create/update requires `publisher` + `publishedYear`
- [ ] Regenerate/refresh local API mock captures for changed request/response shapes

### 2.7.3 Lookup and Store Alignment
- [ ] Add grade lookups to lookup store (`GET /lookups/grades`, optional `keystageId`)
- [ ] Cascade dropdown behavior in class-section forms: keystage -> grades
- [ ] Keep existing role/route constants aligned with backend route casing and path conventions

### 2.7.4 UI Readiness for Bulk Import
- [ ] Add admin UI placeholders/actions for bulk import in:
  - [ ] Books
  - [ ] Teachers
  - [ ] Students
- [ ] Add template download actions and validation-report rendering design
- [ ] Ensure commit flow reflects backend all-or-nothing transaction semantics

### Phase 2.7 Verification
- [ ] Contract calls against updated backend succeed for grades and bulk endpoints
- [ ] No frontend usage remains on removed/legacy payload fields (`grade` string in class-section requests)
- [ ] Typecheck/build pass after schema/store/composable updates

---

## Phase 3: Master Data
> 7 CRUD pages — each with list (DataTable) + create/edit (Dialog) + delete (confirm)

### 3.1 Academic Years
- [x] `app/pages/admin/master-data/academic-years/index.vue`
  - [x] DataTable: name, year, startDate, endDate, isActive badge
  - [x] Create/Edit dialog (name, year, startDate, endDate)
  - [x] Activate button (POST `/{id}/activate`)
  - [x] Delete button (SuperAdmin only, with confirm)
  - [x] Zod validation: name required, year required, endDate > startDate

### 3.2 Keystages
- [x] `app/pages/admin/master-data/keystages/index.vue`
  - [x] DataTable: code, name, sortOrder
  - [x] Create/Edit dialog (code, name, sortOrder)
  - [x] Delete (SuperAdmin, confirm)
  - [x] Zod validation

### 3.3 Subjects
- [x] `app/pages/admin/master-data/subjects/index.vue`
  - [x] DataTable: code, name
  - [x] Create/Edit dialog (code, name)
  - [x] Delete (SuperAdmin, confirm)

### 3.4 Class Sections
- [x] `app/pages/admin/master-data/class-sections/index.vue`
  - [x] Academic year filter dropdown (from lookups)
  - [x] DataTable: displayName, keystageName, grade, section, studentCount
  - [x] Create/Edit dialog (academicYearId, keystageId, gradeId, section)
  - [x] Delete (SuperAdmin, confirm)

### 3.5 Students
- [x] `app/pages/admin/master-data/students/index.vue`
  - [x] Paginated DataTable with search + classSectionId filter
  - [x] Columns: fullName, indexNo, nationalId, classSectionDisplayName
  - [x] Inline parent list display
- [ ] `app/pages/admin/master-data/students/[id].vue` (fallback only for complex edits)
  - [ ] Default create/edit path uses modal from index page
  - [ ] Fields: fullName, indexNo, nationalId, classSectionId (dropdown)
  - [ ] Parent linking section: add/remove parents with isPrimary toggle
  - [ ] Uses parent search/select (from Parents API)
  - [ ] Zod validation
  - [ ] Delete (SuperAdmin, confirm)

### 3.6 Parents
- [x] `app/pages/admin/master-data/parents/index.vue`
  - [x] Paginated DataTable with search
  - [x] Columns: fullName, nationalId, phone, relationship
  - [x] Create/Edit dialog (fullName, nationalId, phone, relationship)
  - [x] Delete (SuperAdmin, confirm)

### 3.7 Teachers
- [x] `app/pages/admin/master-data/teachers/index.vue`
  - [x] Paginated DataTable with search
  - [x] Columns: fullName, nationalId, email, phone
  - [x] Expandable row or link to detail for assignments
- [ ] `app/pages/admin/master-data/teachers/[id].vue` (fallback only for complex assignment management)
  - [ ] Create/Edit form (fullName, nationalId, email, phone)
  - [ ] Assignments section: DataTable of current assignments (subject + classSection)
  - [ ] Add assignment dialog (subjectId, classSectionId dropdowns)
  - [ ] Remove assignment (confirm)
  - [ ] Delete teacher (SuperAdmin, confirm)

### Phase 3 Verification
- [ ] All 7 master data sections have working list + create + edit + delete
- [ ] Academic year activate works and updates sidebar selector
- [ ] Student parent linking works (add/remove/toggle primary)
- [ ] Teacher assignment management works
- [ ] Pagination works for students, parents, teachers
- [ ] SuperAdmin-only delete buttons hidden for other roles

---

## Phase 4: Book Management
> Book catalog, stock entries, stock movements, adjust stock

### 4.1 Book Pages
- [ ] `app/pages/admin/books/index.vue`
  - [ ] Paginated DataTable with search + subject filter dropdown
  - [ ] Columns: code, title, subjectName, grade, totalStock, distributed, available (StockBadge)
  - [ ] Row click → navigate to detail page
  - [ ] "New Book" button → create page
- [ ] `app/pages/admin/books/create.vue`
  - [ ] Reused for create and edit (`/books/create?edit={id}` or separate `/books/[id]/edit`)
  - [ ] Fields: isbn, code, title, author, edition, publisher, publishedYear, subjectId (dropdown), grade
  - [ ] Zod validation (code + title required)
- [ ] `app/pages/admin/books/[id].vue`
  - [ ] Book detail header (title, code, subject, grade, stock summary)
  - [ ] TabView with 2 tabs:
    - [ ] Stock Entries tab — DataTable of stock entries + "Add Stock" button
    - [ ] Stock Movements tab — DataTable of all movements (chronological)
  - [ ] Edit button → navigate to edit form
  - [ ] Delete button (SuperAdmin, confirm)

### 4.2 Book Components
- [ ] `app/components/books/StockBadge.vue`
  - [ ] Color-coded PrimeVue Tag: green (>=10), yellow (<10), red (0)
  - [ ] Props: `available: number`, `showLabel?: boolean`
- [ ] `app/components/books/StockEntryDialog.vue`
  - [ ] Dialog form: academicYearId (dropdown), quantity, source, notes
  - [ ] POST to `/books/{id}/stock-entry`
  - [ ] Emits `created` event to refresh parent
- [ ] `app/components/books/StockAdjustDialog.vue`
  - [ ] Dialog form: academicYearId, movementType (dropdown: MarkDamaged, MarkLost, Adjustment, WriteOff), quantity, notes
  - [ ] Only visible to Admin+ roles
  - [ ] POST to `/books/{id}/adjust-stock`
- [ ] `app/components/books/BookSelector.vue`
  - [ ] Search input → calls `GET /books/search?q=` → shows results
  - [ ] Click result → adds to selected list with quantity=1
  - [ ] Selected books table: title, code, available (StockBadge), quantity (InputNumber, max=available), remove button
  - [ ] v-model: `SelectedBook[]` (id, title, code, available, quantity)
  - [ ] Used by Distribution, Returns, and Teacher Issues forms

### Phase 4 Verification
- [ ] Book list loads with pagination, search, and subject filter
- [ ] Create/edit book works with validation
- [ ] Book detail page shows stock entries and movements tabs
- [ ] Stock entry dialog adds stock and refreshes view
- [ ] Stock adjust dialog works for admin users
- [ ] BookSelector search and selection works

---

## Phase 5: Distribution & Returns
> Create slips with stock deduction, view/search slips, print PDFs

### 5.1 Shared Slip Components
- [ ] `app/components/slips/StudentLookup.vue`
  - [ ] Search students by name/indexNo (paginated API call)
  - [ ] Select student → emit selected student object
  - [ ] Shows className and nationalId
  - [ ] If no result found, show inline "Create Student" action (modal)
- [ ] `app/components/slips/ParentLookup.vue`
  - [ ] When student is selected, auto-load student's parents
  - [ ] If only one parent, auto-select
  - [ ] If multiple, show dropdown
  - [ ] Also allow manual parent search
  - [ ] If no result found, show inline "Create Parent" action (modal)
- [ ] `app/components/slips/ConditionSelector.vue`
  - [ ] Dropdown for BookCondition enum (Good, Fair, Poor, Damaged, Lost)
  - [ ] Optional conditionNotes text field
- [ ] `app/components/slips/SlipDetail.vue`
  - [ ] Reusable slip detail layout: header info (reference, date, year) + items table + notes + action buttons

### 5.2 Distribution Pages
- [ ] `app/pages/distribution/index.vue`
  - [ ] Paginated DataTable with filters: academicYearId, studentId (search)
  - [ ] Columns: referenceNo, studentName, studentClassName, parentName, issuedAt, item count
  - [ ] Search by reference number (GET `/distributions/by-reference/{ref}`)
  - [ ] Row click → detail page
- [ ] `app/pages/distribution/create.vue`
  - [ ] Step-by-step form:
    1. Select academic year + term (from lookups)
    2. StudentLookup → auto-loads parents via ParentLookup
    3. BookSelector (multi-select with quantities)
    4. Notes (optional)
  - [ ] Zod validation: year, term, student, parent, at least one book item
  - [ ] POST to `/distributions` → on success navigate to detail page
- [ ] `app/pages/distribution/[id].vue`
  - [ ] Slip detail: reference, year, term, student info, parent info, issued date
  - [ ] Items table: bookCode, bookTitle, quantity
  - [ ] Print button (GET `/{id}/print` → open PDF)
  - [ ] Cancel button (DELETE, with confirm dialog, reverses stock)

### 5.3 Return Pages
- [ ] `app/pages/returns/index.vue`
  - [ ] Same pattern as distribution index
  - [ ] Columns: referenceNo, studentName, studentClassName, returnedByName, receivedAt, item count
- [ ] `app/pages/returns/create.vue`
  - [ ] Form:
    1. Select academic year
    2. StudentLookup
    3. Select returnedById (parent/guardian dropdown from student's parents, or manual)
    4. BookSelector (with ConditionSelector per item)
    5. Notes
  - [ ] Each item has: bookId, quantity, condition (ConditionSelector), conditionNotes
  - [ ] POST to `/returns`
- [ ] `app/pages/returns/[id].vue`
  - [ ] Slip detail with condition column in items table
  - [ ] Print + Cancel buttons

### Phase 5 Verification
- [ ] Distribution: list → create → detail → print → cancel flow works
- [ ] Returns: list → create → detail → print → cancel flow works
- [ ] Stock updates reflected after distribution/return creation
- [ ] PDF print opens in new tab
- [ ] Reference number search works
- [ ] Student/parent lookup and auto-selection works

---

## Phase 6: Teacher Issues
> Issue books to teachers, process partial/full returns

### 6.1 Teacher Issue Pages
- [ ] `app/pages/teacher-issues/index.vue`
  - [ ] Paginated DataTable with filters: academicYearId, teacherId (search)
  - [ ] Columns: referenceNo, teacherName, issuedAt, expectedReturnDate, status (badge), item count
  - [ ] Status badges: Active (blue), Partial (yellow), Returned (green), Overdue (red)
- [ ] `app/pages/teacher-issues/create.vue`
  - [ ] Form:
    1. Select academic year
    2. Teacher lookup (search by name)
    3. BookSelector (multi-select with quantities)
    4. Expected return date (DatePicker, optional)
    5. Notes
  - [ ] POST to `/teacher-issues`
- [ ] `app/pages/teacher-issues/[id].vue`
  - [ ] Issue detail: reference, teacher, year, issuedAt, expectedReturnDate, status
  - [ ] Items table: bookCode, bookTitle, quantity, returnedQuantity, outstandingQuantity, returnedAt
  - [ ] "Process Return" button (opens return dialog) — only if status != Returned
  - [ ] Print button
  - [ ] Cancel button (only if outstanding items exist)

### 6.2 Teacher Return Dialog
- [ ] `app/components/teachers/ProcessReturnDialog.vue`
  - [ ] Shows outstanding items only (outstandingQuantity > 0)
  - [ ] Per item: checkbox to include, quantity to return (max = outstanding)
  - [ ] Notes field
  - [ ] POST to `/teacher-issues/{id}/return` with `ProcessTeacherReturnRequest`
  - [ ] On success: show return slip reference, refresh parent page

### 6.3 Teacher Lookup
- [ ] `app/components/slips/TeacherLookup.vue`
  - [ ] Search teachers by name
  - [ ] Select teacher → emit teacher object
  - [ ] If no result found, show inline "Create Teacher" action (modal)

### Phase 6 Verification
- [ ] Teacher issue: list → create → detail → process return → print flow works
- [ ] Partial return updates status to "Partial"
- [ ] Full return updates status to "Returned"
- [ ] Outstanding quantities calculated correctly
- [ ] Cancel reverses only outstanding quantities

---

## Phase 7: Reports & Dashboard
> Summary reports, Excel exports, dashboard overview

### 7.1 Dashboard
- [ ] `app/pages/index.vue`
  - [ ] Operational dashboard focused on daily issuance/return flows
  - [ ] Recent distributions list (latest 5)
  - [ ] Recent returns list (latest 5)
  - [ ] Quick action buttons: New Distribution, New Return, New Teacher Issue
- [ ] `app/pages/admin/index.vue`
  - [ ] Admin dashboard with summary cards and links to reports/settings
  - [ ] Visible only to Admin/SuperAdmin

### 7.2 Report Components
- [ ] `app/components/reports/ExportButton.vue`
  - [ ] Props: `url` (export endpoint), `filename`
  - [ ] Fetches blob via API, triggers browser download
  - [ ] Loading state during download
- [ ] `app/components/reports/ReportFilters.vue`
  - [ ] Reusable filter bar: academic year, subject, grade, date range, teacher
  - [ ] Emits filter params

### 7.3 Report Pages
- [ ] `app/pages/admin/reports/index.vue` — report hub with cards linking to each report
- [ ] `app/pages/admin/reports/stock-summary.vue`
  - [ ] Filters: subjectId, grade
  - [ ] DataTable: code, title, subjectName, grade, totalStock, distributed, withTeachers, damaged, lost, available
  - [ ] ExportButton → `GET /reports/export/stock-summary`
- [ ] `app/pages/admin/reports/distributions.vue`
  - [ ] Filters: academicYearId (required), from date, to date
  - [ ] DataTable: referenceNo, studentName, studentIndexNo, parentName, issuedAt, totalBooks
  - [ ] ExportButton → `GET /reports/export/distribution-summary`
- [ ] `app/pages/admin/reports/teacher-outstanding.vue`
  - [ ] Filters: teacherId (optional)
  - [ ] DataTable: referenceNo, teacherName, bookTitle, bookCode, quantity, returnedQuantity, outstanding, status, issuedAt, expectedReturnDate
  - [ ] ExportButton → `GET /reports/export/teacher-outstanding`
- [ ] `app/pages/admin/reports/student-history.vue`
  - [ ] Student search/select input
  - [ ] DataTable: type (Distribution/Return), referenceNo, date, bookTitle, bookCode, quantity, condition
  - [ ] No export endpoint (view-only)

### Phase 7 Verification
- [ ] Dashboard loads with correct totals from API
- [ ] All 4 report pages load and display data
- [ ] Filters work (especially required academicYearId for distribution summary)
- [ ] Excel export downloads .xlsx files correctly
- [ ] Student history shows both distributions and returns

---

## Phase 8: Admin & Settings
> User management, system settings, audit log, user profile

### 8.1 User Management
- [ ] `app/pages/admin/settings/users/index.vue`
  - [ ] DataTable: userName, email, fullName, roles (tags), isActive (badge), createdAt
  - [ ] Toggle active button (POST `/{id}/toggle-active`)
  - [ ] "New User" button
  - [ ] Route guard: Admin+ only
- [ ] `app/pages/admin/settings/users/[id].vue`
  - [ ] Create/Edit form: userName (create only), email, password (create only), fullName, nationalId, designation, roles (MultiSelect), isActive
  - [ ] Role assignment section (PUT `/{id}/roles`)

### 8.2 Reference Number Formats
- [ ] `app/pages/admin/settings/reference-formats.vue`
  - [ ] DataTable: slipType, academicYearName, formatTemplate, paddingWidth
  - [ ] Create/Edit dialog: slipType (dropdown), academicYearId (dropdown), formatTemplate, paddingWidth
  - [ ] Help text explaining tokens: `{year}`, `{autonum}`
  - [ ] Delete (SuperAdmin, confirm)
  - [ ] Route guard: Admin+ only

### 8.3 Slip Template Settings
- [ ] `app/pages/admin/settings/slip-templates.vue`
  - [ ] Grouped by category (DataTable with grouping or Accordion)
  - [ ] Inline edit: value + sortOrder per row
  - [ ] Save individual setting (PUT `/{id}`)
  - [ ] "Reset to Defaults" button (POST `/reset`, SuperAdmin only, confirm)
  - [ ] Route guard: Admin+ only

### 8.4 Audit Log
- [ ] `app/pages/admin/audit-log/index.vue`
  - [ ] Paginated DataTable
  - [ ] Filters: entityType, action, userId, date range (from/to)
  - [ ] Columns: timestamp, action, entityType, entityId, userName
  - [ ] Expandable row for oldValues/newValues (JSON display)
  - [ ] Route guard: Admin+ only

### 8.5 User Profile
- [ ] `app/pages/admin/settings/profile.vue`
  - [ ] Display current user info (from auth store)
  - [ ] Change password form (currentPassword, newPassword, confirmPassword)
  - [ ] POST to `/auth/change-password`
  - [ ] Route guard: Admin+ only

### Phase 8 Verification
- [ ] User CRUD works, role assignment works
- [ ] Non-admin users cannot access settings/users or audit log
- [ ] Reference number format CRUD works
- [ ] Slip template settings inline edit works
- [ ] Reset to defaults restores original values
- [ ] Audit log loads with filters and pagination
- [ ] Change password works

---

## Final Verification
- [ ] `bun run dev` starts without errors
- [ ] Full login → navigate → CRUD → logout flow works
- [ ] All master data CRUD pages functional
- [ ] Book management with stock operations works
- [ ] Distribution create → print → cancel flow works end-to-end
- [ ] Return create → print → cancel flow works end-to-end
- [ ] Teacher issue → partial return → full return flow works
- [ ] All report pages load data and export Excel
- [ ] PDF print opens correctly for all 4 slip types
- [ ] Role-based access control enforced (menu visibility + route guards)
- [ ] Responsive layout works on common screen sizes
- [ ] No TypeScript errors, no console errors
