// API endpoint paths (relative to /api/bff)
export const API = {
  auth: {
    login: '/auth/login',
    refresh: '/auth/refresh',
    logout: '/auth/logout',
    me: '/auth/me',
    changePassword: '/auth/change-password',
  },
  academicYears: {
    base: '/AcademicYears',
    active: '/AcademicYears/active',
    activate: (id: number) => `/AcademicYears/${id}/activate`,
    byId: (id: number) => `/AcademicYears/${id}`,
  },
  keystages: {
    base: '/keystages',
    byId: (id: number) => `/keystages/${id}`,
  },
  grades: {
    base: '/grades',
    byId: (id: number) => `/grades/${id}`,
  },
  subjects: {
    base: '/subjects',
    byId: (id: number) => `/subjects/${id}`,
  },
  classSections: {
    base: '/ClassSections',
    byId: (id: number) => `/ClassSections/${id}`,
  },
  students: {
    base: '/students',
    byId: (id: number) => `/students/${id}`,
    bulkValidate: '/students/bulk/validate',
    bulkCommit: '/students/bulk/commit',
    bulkCommitAsync: '/students/bulk/commit-async',
    bulkJob: (id: string) => `/students/bulk/jobs/${id}`,
    bulkJobReport: (id: string) => `/students/bulk/jobs/${id}/report`,
  },
  parents: {
    base: '/parents',
    byId: (id: number) => `/parents/${id}`,
    bulkValidate: '/parents/bulk/validate',
    bulkCommit: '/parents/bulk/commit',
    bulkCommitAsync: '/parents/bulk/commit-async',
    bulkJob: (id: string) => `/parents/bulk/jobs/${id}`,
    bulkJobReport: (id: string) => `/parents/bulk/jobs/${id}/report`,
  },
  teachers: {
    base: '/teachers',
    byId: (id: number) => `/teachers/${id}`,
    bulkValidate: '/teachers/bulk/validate',
    bulkCommit: '/teachers/bulk/commit',
    bulkCommitAsync: '/teachers/bulk/commit-async',
    bulkJob: (id: string) => `/teachers/bulk/jobs/${id}`,
    bulkJobReport: (id: string) => `/teachers/bulk/jobs/${id}/report`,
    assignments: (id: number) => `/teachers/${id}/assignments`,
    assignmentById: (teacherId: number, assignmentId: number) =>
      `/teachers/${teacherId}/assignments/${assignmentId}`,
  },
  lookups: {
    academicYears: '/lookups/academic-years',
    keystages: '/lookups/keystages',
    grades: '/lookups/grades',
    subjects: '/lookups/subjects',
    classSections: '/lookups/class-sections',
    terms: '/lookups/terms',
    bookConditions: '/lookups/book-conditions',
    movementTypes: '/lookups/movement-types',
  },
  importTemplates: {
    books: '/import-templates/books',
    teachers: '/import-templates/teachers',
    students: '/import-templates/students',
    parents: '/import-templates/parents',
    masterDataHierarchy: '/import-templates/master-data-hierarchy',
  },
  masterDataHierarchy: {
    bulkUpsert: '/master-data/hierarchy/bulk/upsert',
  },
  books: {
    base: '/books',
    byId: (id: number) => `/books/${id}`,
    search: '/books/search',
    bulkValidate: '/books/bulk/validate',
    bulkCommit: '/books/bulk/commit',
    bulkCommitAsync: '/books/bulk/commit-async',
    bulkJob: (id: string) => `/books/bulk/jobs/${id}`,
    bulkJobReport: (id: string) => `/books/bulk/jobs/${id}/report`,
    stockEntry: (id: number) => `/books/${id}/stock-entry`,
    adjustStock: (id: number) => `/books/${id}/adjust-stock`,
    stockEntries: (id: number) => `/books/${id}/stock-entries`,
    stockMovements: (id: number) => `/books/${id}/stock-movements`,
  },
  distributions: {
    base: '/distributions',
    byId: (id: number) => `/distributions/${id}`,
    byReference: (referenceNo: string) => `/distributions/by-reference/${referenceNo}`,
    finalize: (id: number) => `/distributions/${id}/finalize`,
    print: (id: number) => `/distributions/${id}/print`,
  },
  returns: {
    base: '/returns',
    byId: (id: number) => `/returns/${id}`,
    byReference: (referenceNo: string) => `/returns/by-reference/${referenceNo}`,
    finalize: (id: number) => `/returns/${id}/finalize`,
    print: (id: number) => `/returns/${id}/print`,
  },
  teacherIssues: {
    base: '/TeacherIssues',
    byId: (id: number) => `/TeacherIssues/${id}`,
    finalize: (id: number) => `/TeacherIssues/${id}/finalize`,
    return: (id: number) => `/TeacherIssues/${id}/return`,
    returnPrint: (id: number) => `/TeacherIssues/${id}/return/print`,
    print: (id: number) => `/TeacherIssues/${id}/print`,
  },
  teacherReturns: {
    base: '/TeacherReturns',
    byId: (id: number) => `/TeacherReturns/${id}`,
    byReference: (referenceNo: string) => `/TeacherReturns/by-reference/${referenceNo}`,
    finalize: (id: number) => `/TeacherReturns/${id}/finalize`,
    print: (id: number) => `/TeacherReturns/${id}/print`,
  },
  reports: {
    stockSummary: '/reports/stock-summary',
    distributionSummary: '/reports/distribution-summary',
    teacherOutstanding: '/reports/teacher-outstanding',
    studentHistory: (studentId: number) => `/reports/student-history/${studentId}`,
    exportStock: '/reports/export/stock-summary',
    exportDistribution: '/reports/export/distribution-summary',
    exportTeacherOutstanding: '/reports/export/teacher-outstanding',
  },
  users: {
    base: '/users',
    byId: (id: number) => `/users/${id}`,
    roles: (id: number) => `/users/${id}/roles`,
    toggleActive: (id: number) => `/users/${id}/toggle-active`,
  },
  referenceNumberFormats: {
    base: '/ReferenceNumberFormats',
    byId: (id: number) => `/ReferenceNumberFormats/${id}`,
  },
  slipTemplateSettings: {
    base: '/SlipTemplateSettings',
    byId: (id: number) => `/SlipTemplateSettings/${id}`,
    reset: '/SlipTemplateSettings/reset',
  },
  auditLogs: {
    base: '/AuditLogs',
  },
} as const

// Pagination defaults
export const PAGINATION = {
  defaultPageSize: 20,
  pageSizeOptions: [10, 20, 50, 100],
} as const

// User roles
export const ROLES = {
  superAdmin: 'SuperAdmin',
  admin: 'Admin',
  user: 'User',
} as const
