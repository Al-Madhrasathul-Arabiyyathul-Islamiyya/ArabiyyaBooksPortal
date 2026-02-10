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
  },
  parents: {
    base: '/parents',
    byId: (id: number) => `/parents/${id}`,
  },
  teachers: {
    base: '/teachers',
    byId: (id: number) => `/teachers/${id}`,
    bulkValidate: '/teachers/bulk/validate',
    bulkCommit: '/teachers/bulk/commit',
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
  },
  books: {
    base: '/books',
    byId: (id: number) => `/books/${id}`,
    search: '/books/search',
    bulkValidate: '/books/bulk/validate',
    bulkCommit: '/books/bulk/commit',
    stockEntry: (id: number) => `/books/${id}/stock-entry`,
    adjustStock: (id: number) => `/books/${id}/adjust-stock`,
    stockEntries: (id: number) => `/books/${id}/stock-entries`,
    stockMovements: (id: number) => `/books/${id}/stock-movements`,
  },
  distributions: {
    base: '/distributions',
    byId: (id: number) => `/distributions/${id}`,
    byReference: (referenceNo: string) => `/distributions/by-reference/${referenceNo}`,
    print: (id: number) => `/distributions/${id}/print`,
  },
  returns: {
    base: '/returns',
    byId: (id: number) => `/returns/${id}`,
    byReference: (referenceNo: string) => `/returns/by-reference/${referenceNo}`,
    print: (id: number) => `/returns/${id}/print`,
  },
  teacherIssues: {
    base: '/TeacherIssues',
    byId: (id: number) => `/TeacherIssues/${id}`,
    return: (id: number) => `/TeacherIssues/${id}/return`,
    print: (id: number) => `/TeacherIssues/${id}/print`,
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
