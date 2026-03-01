# Backend Pagination Enhancements Plan

## Scope
- Align high-volume list endpoints to consistent paginated contracts.
- Keep export endpoints non-paginated.
- Sync API docs and contract tests with implementation changes.

---

## Slice 1: Reports Pagination
- [x] Update `ReportsController` GET endpoints to accept `pageNumber` and `pageSize`.
- [x] Add page size guard (`max=100`) for reports endpoints.
- [x] Update `IReportService` list methods to return `PaginatedList<T>`.
- [x] Update `ReportService` list methods to support pagination.
- [x] Keep report export endpoints unchanged and full-data.

## Slice 2: Books Stock History Pagination
- [x] Update `GET /api/books/{id}/stock-entries` to paginated response.
- [x] Update `GET /api/books/{id}/stock-movements` to paginated response.
- [x] Add page query params and guard in controller.
- [x] Update `IBookService` and `BookService` signatures and behavior.

## Slice 3: User List Pagination + Filters
- [x] Update `GET /api/users` to paginated response.
- [x] Add optional filters: `search`, `isActive`, `role`.
- [x] Add page query params and guard in controller.
- [x] Update `IUserService` and `UserService` signatures and behavior.

## Slice 4: Class Section List Pagination + Filters
- [x] Update `GET /api/ClassSections` to paginated response.
- [x] Add optional filters: `academicYearId`, `keystageId`, `gradeId`, `search`.
- [x] Add page query params and guard in controller.
- [x] Update `IClassSectionService` and `ClassSectionService` signatures and behavior.

## Slice 5: Docs and Contracts
- [x] Update `documentation/api-reference.md` for changed endpoints.
- [x] Update `tools/api-contract-tester/http/contract-suite.http` assertions for changed response shapes.
- [x] Update `tools/api-contract-tester/run-frontend-mock-capture.ps1` request params where needed.

## Slice 6: Verification
- [x] Run backend build.
- [x] Run backend tests.
- [x] Run contract suite (or targeted requests) for updated endpoints.

---

## Notes
- This plan intentionally prioritizes endpoint contract alignment first.
- Frontend endpoint consumers must be updated to consume paginated shapes for changed endpoints.
- Contract verification run: `tools/api-contract-tester/logs/backend-verification-log-16bd491c-1f4b-47c5-90a9-b5a87a778c41.json`
