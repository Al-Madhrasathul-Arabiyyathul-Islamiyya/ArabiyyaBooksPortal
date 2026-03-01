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
- Table UX baseline:
  - Action columns should use consistent icon+tooltip affordances.
  - Prefer in-card table scrolling for dense admin grids.
  - Use `primevue/skeleton` placeholders for table-loading states (pending rollout).
- Post-core UX enhancement:
  - Evaluate PrimeVue `AutoComplete` for search-heavy fields, entity pickers, parent linking, and teacher assignment selectors.

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
- [x] Implementation completed (contract/type/store alignment + bulk-import UI readiness)

## Validation Standardization Track
> Cross-cutting validation standardization is tracked separately and executed incrementally with feature work.
- [~] Follow `plans/frontend-validation-migration-plan.md` for shared Regle + Zod validation module rollout.

### 2.7.1 Route and BFF Contract Sync
- [x] Verify BFF catch-all/proxy supports new backend routes:
  - [x] `GET /lookups/grades`
  - [x] `POST /books/bulk/validate`, `POST /books/bulk/commit`
  - [x] `POST /teachers/bulk/validate`, `POST /teachers/bulk/commit`
  - [x] `POST /students/bulk/validate`, `POST /students/bulk/commit`
  - [x] `GET /import-templates/books|teachers|students`
- [x] Add explicit typed API helpers/composables for bulk validate/commit + template download flows

### 2.7.2 DTO/Form Type Alignment
- [x] Update frontend schemas/types for backend changes:
  - [x] ClassSection create/update uses `gradeId` (not free-text `grade`)
  - [x] ClassSection response includes `gradeId`
  - [x] Student create/update requires `nationalId`
  - [x] Book create/update requires `publisher` + `publishedYear`
- [x] Regenerate/refresh local API mock captures for changed request/response shapes

### 2.7.3 Lookup and Store Alignment
- [x] Add grade lookups to lookup store (`GET /lookups/grades`, optional `keystageId`)
- [x] Cascade dropdown behavior in class-section forms: keystage -> grades
- [x] Keep existing role/route constants aligned with backend route casing and path conventions

### 2.7.4 UI Readiness for Bulk Import
- [x] Add admin UI placeholders/actions for bulk import in:
  - [x] Books
  - [x] Teachers
  - [x] Students
- [x] Add template download actions and validation-report rendering design
- [x] Ensure commit flow reflects backend all-or-nothing transaction semantics

### Phase 2.7 Verification
- [x] Contract calls against updated backend succeed for grades and bulk endpoints
- [x] No frontend usage remains on removed/legacy payload fields (`grade` string in class-section requests)
- [x] Typecheck/lint pass after schema/store/composable updates

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
- [x] `app/pages/admin/master-data/students/[id].vue` deferred (N/A for current modal-first flow)
  - [x] Default create/edit path uses modal from index page
  - [x] Fields handled in index modal: fullName, indexNo, nationalId, classSectionId (dropdown)
  - [x] Parent linking section implemented in index modal: add/remove parents with isPrimary toggle
  - [x] Parent search/select implemented in index modal (Parents API)
  - [x] Zod validation implemented in index modal
  - [x] Delete (SuperAdmin, confirm) implemented in index modal

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
- [x] `app/pages/admin/master-data/teachers/[id].vue` deferred (N/A for current modal-first flow)
  - [x] Create/Edit form handled in index modal
  - [x] Assignments section implemented in index page dialog (subject + classSection)
  - [x] Add assignment dialog implemented in index page
  - [x] Remove assignment (confirm) implemented in index page
  - [x] Delete teacher (SuperAdmin, confirm) implemented in index page

### Phase 3 Verification
- [x] All 7 master data sections have working list + create + edit + delete
- [x] Academic year activate works and refreshes list state
- [x] Student parent linking works (add/remove/toggle primary)
- [x] Teacher assignment management works
- [x] Pagination works for students, parents, teachers
- [x] SuperAdmin-only delete buttons hidden for other roles

---

## Phase 4: Book Management
> Book catalog, stock entries, stock movements, adjust stock

### 4.1 Book Pages
- [x] `app/pages/admin/books/index.vue`
  - [x] Paginated DataTable with search + subject filter dropdown
  - [x] Columns: code, title, subjectName, grade, totalStock, distributed, available (StockBadge)
  - [x] Row click → navigate to detail page
  - [x] "New Book" button → create/edit modal flow
- [x] `app/pages/admin/books/create.vue`
  - [x] Replaced by modal-first create/edit flow in `books/index.vue` to keep UX consistency
  - [x] Fields: isbn, code, title, author, edition, publisher, publishedYear, subjectId (dropdown), grade
  - [x] Zod validation (code + title required)
- [x] `app/pages/admin/books/[id].vue`
  - [x] Book detail header (title, code, stock summary)
  - [x] Stock entries + stock movements sections with DataTables and stock action buttons
  - [x] Edit button → navigate to edit form
  - [x] Delete button (SuperAdmin, confirm)
- [ ] Enhancement: allow entering initial stock quantity directly in create-book flow once stock workflow finalization is complete.

### 4.2 Book Components
- [x] `app/components/books/StockBadge.vue`
  - [x] Color-coded PrimeVue Tag: green (>=10), yellow (<10), red (0)
  - [x] Props: `available: number`, `showLabel?: boolean`
- [x] `app/components/books/StockEntryDialog.vue`
  - [x] Dialog form: academicYearId (dropdown), quantity, source, notes
  - [x] POST to `/books/{id}/stock-entry` (wired through parent page handler)
  - [x] Emits submit/cancel events to parent for refresh flow
- [x] `app/components/books/StockAdjustDialog.vue`
  - [x] Dialog form: academicYearId, movementType (dropdown: MarkDamaged, MarkLost, Adjustment, WriteOff), quantity, notes
  - [x] Only visible to Admin+ roles (enforced by parent action visibility)
  - [x] POST to `/books/{id}/adjust-stock` (wired through parent page handler)
- [x] `app/components/books/BookSelector.vue`
  - [x] Search input → calls `GET /books/search?q=` → shows results
  - [x] Click result → adds to selected list with quantity=1
  - [x] Selected books table: title, code, available (StockBadge), quantity (InputNumber, max=available), remove button
  - [x] v-model: `SelectedBook[]` (id, title, code, available, quantity)
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
- [x] `app/components/slips/StudentLookup.vue`
  - [x] Search students by name/indexNo (paginated API call)
  - [x] Debounced live search while typing (no explicit "Search" click required)
  - [x] Support year-scoped queries via `academicYearId`
  - [x] Allow clearing selected student from lookup UI
  - [x] Select student → emit selected student object
  - [x] Shows className and nationalId
  - [x] If no result found, show inline "Create Student" action (modal trigger event)
- [x] `app/components/slips/ParentLookup.vue`
  - [x] When student is selected, auto-load student's parents
  - [x] If only one parent, auto-select
  - [x] If multiple, show dropdown
  - [x] Also allow debounced manual parent search
  - [x] Allow clearing selected parent from lookup UI
  - [x] If no result found, show inline "Create Parent" action (modal trigger event)
- [x] `app/components/slips/ConditionSelector.vue`
  - [x] Dropdown for BookCondition enum (Good, Fair, Poor, Damaged, Lost)
  - [x] Optional conditionNotes text field
- [x] `app/components/slips/SlipDetail.vue`
  - [x] Reusable slip detail layout: header info (reference, date, year) + items table + notes + action buttons

### 5.2 Distribution Pages
- [x] `app/pages/distribution/index.vue`
  - [x] Paginated DataTable with filters: academicYearId, studentId (search)
  - [x] Columns: referenceNo, studentName, studentClassName, parentName, issuedAt, item count
  - [x] Search by reference number (GET `/distributions/by-reference/{ref}`)
  - [x] Row click → detail page
- [x] `app/pages/distribution/create.vue`
  - [x] Modal-first flow + two-column create UX:
    1. Select academic year + term (from lookups)
    2. Left column: Book selector (multi-select with quantities)
    3. Right column: slip preview + modal triggers for student/parent selection
    4. Notes (optional)
  - [x] Non-admin users are locked to active academic year
  - [x] Zod validation: year, term, student, parent, at least one book item
  - [x] POST to `/distributions` → on success navigate to detail page
- [x] `app/pages/distribution/[id].vue`
  - [x] Slip detail: reference, year, term, student info, parent info, issued date
  - [x] Items table: bookCode, bookTitle, quantity
  - [x] Print button (GET `/{id}/print` → open PDF)
  - [x] Cancel button (DELETE, with confirm dialog, reverses stock)

### 5.3 Return Pages
- [x] `app/pages/returns/index.vue`
  - [x] Same pattern as distribution index
  - [x] Columns: referenceNo, studentName, studentClassName, returnedByName, receivedAt, item count
- [x] `app/pages/returns/create.vue`
  - [x] Modal-first flow with preview panel:
    1. Select academic year
    2. Left column: BookSelector + per-item ConditionSelector
    3. Right column: return preview + modal triggers for student/returned-by selection
    4. Notes
  - [x] Non-admin users are locked to active academic year
  - [x] Each item has: bookId, quantity, condition (ConditionSelector), conditionNotes
  - [x] POST to `/returns`
- [x] `app/pages/returns/[id].vue`
  - [x] Slip detail with condition column in items table
  - [x] Print + Cancel buttons

### Phase 5 Verification
- [ ] Distribution: list → create → detail → print → cancel flow works
- [ ] Returns: list → create → detail → print → cancel flow works
- [ ] Stock updates reflected after distribution/return creation
- [ ] PDF print opens in new tab
- [ ] Reference number search works
- [ ] Student/parent lookup and auto-selection works
- [x] Inline create student/parent flows are available directly in lookup modals (no admin navigation required).

---

## Phase 6: Teacher Issues
> Issue books to teachers, process partial/full returns

### 6.1 Teacher Issue Pages
- [x] `app/pages/teacher-issues/index.vue`
  - [x] Paginated DataTable with filters: academicYearId, teacherId (search)
  - [x] Columns: referenceNo, teacherName, issuedAt, expectedReturnDate, status (badge), item count
  - [x] Status badges: Active (blue), Partial (yellow), Returned (green), Overdue (red)
- [x] `app/pages/teacher-issues/create.vue`
  - [x] Form:
    1. Select academic year
    2. Teacher lookup (search by name)
    3. BookSelector (multi-select with quantities)
    4. Expected return date (DatePicker, optional)
    5. Notes
  - [x] POST to `/teacher-issues`
- [x] `app/pages/teacher-issues/[id].vue`
  - [x] Issue detail: reference, teacher, year, issuedAt, expectedReturnDate, status
  - [x] Items table: bookCode, bookTitle, quantity, returnedQuantity, outstandingQuantity, returnedAt
  - [x] "Process Return" button (navigates to return processing) - only if outstanding exists
  - [x] Print button
  - [x] Cancel button (only if outstanding items exist)

### 6.2 Teacher Return Dialog
- [x] `app/pages/teacher-returns/index.vue` integrated return dialog
  - [x] Shows outstanding items only (outstandingQuantity > 0)
  - [x] Per item: quantity to return (max = outstanding)
  - [x] Notes field
  - [x] POST to `/teacher-issues/{id}/return` with `ProcessTeacherReturnRequest`
  - [x] On success: show return slip reference, refresh list

### 6.3 Teacher Lookup
- [x] `app/components/slips/TeacherLookup.vue`
  - [x] Search teachers by name
  - [x] Select teacher -> emit teacher object
  - [x] If no result found, show inline "Create Teacher" action (modal)


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
- [x] `app/components/reports/ExportButton.vue`
  - [x] Props: `url` (export endpoint), `filename`
  - [x] Fetches blob via API, triggers browser download
  - [x] Loading state during download
- [x] `app/components/reports/ReportFilters.vue`
  - [x] Reusable filter bar: academic year, subject, grade, date range, teacher
  - [x] Emits filter params

### 7.3 Report Pages
- [x] `app/pages/admin/reports/index.vue` — report hub with cards linking to each report
- [x] `app/pages/admin/reports/stock-summary.vue`
  - [x] Filters: subjectId, grade
  - [x] DataTable: code, title, subjectName, grade, totalStock, distributed, withTeachers, damaged, lost, available
  - [x] ExportButton -> `GET /reports/export/stock-summary`
- [x] `app/pages/admin/reports/distributions.vue`
  - [x] Filters: academicYearId (required), from date, to date
  - [x] DataTable: referenceNo, studentName, studentIndexNo, parentName, issuedAt, totalBooks
  - [x] ExportButton -> `GET /reports/export/distribution-summary`
- [x] `app/pages/admin/reports/teacher-outstanding.vue`
  - [x] Filters: teacherId (optional)
  - [x] DataTable: referenceNo, teacherName, bookTitle, bookCode, quantity, returnedQuantity, outstanding, status, issuedAt, expectedReturnDate
  - [x] ExportButton -> `GET /reports/export/teacher-outstanding`
- [x] `app/pages/admin/reports/student-history.vue`
  - [x] Student search/select input
  - [x] DataTable: type (Distribution/Return), referenceNo, date, bookTitle, bookCode, quantity, condition
  - [x] No export endpoint (view-only)

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
- [x] `app/pages/admin/settings/users/index.vue`
  - [x] DataTable: userName, email, fullName, roles (tags), isActive (badge), createdAt
  - [x] Toggle active button (POST `/{id}/toggle-active`)
  - [x] "New User" button
  - [x] Route guard: Admin+ only
- [x] `app/pages/admin/settings/users/[id].vue`
  - [x] Replaced by modal-first create/edit flow in `users/index.vue` for UX consistency
  - [x] Create/Edit form: userName (create only), email, password (create only), fullName, nationalId, designation, roles (MultiSelect), isActive
  - [x] Role assignment section (PUT `/{id}/roles`)

### 8.2 Reference Number Formats
- [x] `app/pages/admin/settings/reference-formats.vue`
  - [x] DataTable: slipType, academicYearName, formatTemplate, paddingWidth
  - [x] Create/Edit dialog: slipType (dropdown), academicYearId (dropdown), formatTemplate, paddingWidth
  - [x] Help text explaining tokens: `{year}`, `{autonum}`
  - [x] Live preview visualization: template tokens + generated examples in create/edit dialog
  - [x] Delete (SuperAdmin, confirm)
  - [x] Route guard: Admin+ only
  - [ ] Enhancement backlog: support configurable starting number/offset per format for manual pre-existing references

### 8.3 Slip Template Settings
- [x] `app/pages/admin/settings/slip-templates.vue`
  - [x] Grouped by category (DataTable with grouping or Accordion)
  - [x] Inline edit: value + sortOrder per row
  - [x] Save individual setting (PUT `/{id}`)
  - [x] "Reset to Defaults" button (POST `/reset`, SuperAdmin only, confirm)
  - [x] Route guard: Admin+ only

### 8.4 Audit Log
- [x] `app/pages/admin/audit-log/index.vue`
  - [x] Paginated DataTable
  - [x] Filters: entityType, action, userId, date range (from/to)
  - [x] Columns: timestamp, action, entityType, entityId, userName
  - [x] Expandable row for oldValues/newValues (JSON display)
  - [x] Route guard: Admin+ only

### 8.5 User Profile
- [x] `app/pages/admin/settings/profile.vue`
  - [x] Display current user info (from auth store)
  - [x] Change password form (currentPassword, newPassword, confirmPassword)
  - [x] POST to `/auth/change-password`
  - [x] Route guard: Admin+ only

### Phase 8 Verification
- [ ] User CRUD works, role assignment works (UI + API wiring complete; run manual verification)
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


