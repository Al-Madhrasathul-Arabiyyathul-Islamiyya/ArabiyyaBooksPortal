// API endpoint paths
export const API = {
  auth: {
    login: '/auth/login',
    refresh: '/auth/refresh',
    logout: '/auth/logout',
    me: '/auth/me',
    changePassword: '/auth/change-password',
  },
  academicYears: {
    base: '/academic-years',
    active: '/academic-years/active',
    activate: (id: number) => `/academic-years/${id}/activate`,
    byId: (id: number) => `/academic-years/${id}`,
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
    base: '/class-sections',
    byId: (id: number) => `/class-sections/${id}`,
  },
  students: {
    base: '/students',
    byId: (id: number) => `/students/${id}`,
  },
  parents: {
    base: '/parents',
    byId: (id: number) => `/parents/${id}`,
  },
  teachers: {
    base: '/teachers',
    byId: (id: number) => `/teachers/${id}`,
    assignments: (id: number) => `/teachers/${id}/assignments`,
    assignmentById: (teacherId: number, assignmentId: number) =>
      `/teachers/${teacherId}/assignments/${assignmentId}`,
  },
  lookups: {
    academicYears: '/lookups/academic-years',
    keystages: '/lookups/keystages',
    subjects: '/lookups/subjects',
    classSections: '/lookups/class-sections',
    teachers: '/lookups/teachers',
    parents: '/lookups/parents',
  },
  books: {
    base: '/books',
    byId: (id: number) => `/books/${id}`,
    stock: (id: number) => `/books/${id}/stock`,
    adjustStock: (id: number) => `/books/${id}/adjust-stock`,
    stockEntries: (id: number) => `/books/${id}/stock-entries`,
    stockMovements: (id: number) => `/books/${id}/stock-movements`,
  },
  distributions: {
    base: '/distributions',
    byId: (id: number) => `/distributions/${id}`,
    cancel: (id: number) => `/distributions/${id}/cancel`,
    print: (id: number) => `/distributions/${id}/print`,
  },
  returns: {
    base: '/returns',
    byId: (id: number) => `/returns/${id}`,
    cancel: (id: number) => `/returns/${id}/cancel`,
    print: (id: number) => `/returns/${id}/print`,
  },
  teacherIssues: {
    base: '/teacher-issues',
    byId: (id: number) => `/teacher-issues/${id}`,
    return: (id: number) => `/teacher-issues/${id}/return`,
    cancel: (id: number) => `/teacher-issues/${id}/cancel`,
    print: (id: number) => `/teacher-issues/${id}/print`,
    printReturn: (id: number, returnId: number) =>
      `/teacher-issues/${id}/returns/${returnId}/print`,
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
  },
  referenceNumberFormats: {
    base: '/reference-number-formats',
    byId: (id: number) => `/reference-number-formats/${id}`,
  },
  slipTemplateSettings: {
    base: '/slip-template-settings',
    byId: (id: number) => `/slip-template-settings/${id}`,
    reset: '/slip-template-settings/reset',
  },
  auditLogs: {
    base: '/audit-logs',
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
  staff: 'Staff',
} as const

// Local storage keys
export const STORAGE_KEYS = {
  accessToken: 'bp_access_token',
  refreshToken: 'bp_refresh_token',
  tokenExpiry: 'bp_token_expiry',
} as const
