import { z } from 'zod/v4'

// --- Auth ---

export const TokenResponseSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
  expiresAt: z.string(),
})
export type TokenResponse = z.infer<typeof TokenResponseSchema>

export const UserProfileSchema = z.object({
  id: z.number(),
  userName: z.string(),
  email: z.string(),
  fullName: z.string(),
  nationalId: z.string().nullable(),
  designation: z.string().nullable(),
  roles: z.array(z.string()),
})
export type UserProfile = z.infer<typeof UserProfileSchema>

// --- Master Data ---

export const AcademicYearSchema = z.object({
  id: z.number(),
  name: z.string(),
  year: z.number(),
  startDate: z.string(),
  endDate: z.string(),
  isActive: z.boolean(),
})
export type AcademicYear = z.infer<typeof AcademicYearSchema>

export const KeystageSchema = z.object({
  id: z.number(),
  code: z.string(),
  name: z.string(),
  sortOrder: z.number(),
})
export type Keystage = z.infer<typeof KeystageSchema>

export const GradeSchema = z.object({
  id: z.number(),
  name: z.string(),
  sortOrder: z.number(),
  keystageId: z.number(),
  keystageName: z.string(),
})
export type Grade = z.infer<typeof GradeSchema>

export const SubjectSchema = z.object({
  id: z.number(),
  name: z.string(),
  code: z.string(),
})
export type Subject = z.infer<typeof SubjectSchema>

export const ClassSectionSchema = z.object({
  id: z.number(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  keystageId: z.number(),
  keystageName: z.string(),
  gradeId: z.number(),
  grade: z.string(),
  section: z.string(),
  displayName: z.string(),
  studentCount: z.number(),
})
export type ClassSection = z.infer<typeof ClassSectionSchema>

export const StudentParentSchema = z.object({
  parentId: z.number(),
  fullName: z.string(),
  nationalId: z.string(),
  phone: z.string().nullable(),
  relationship: z.string().nullable(),
  isPrimary: z.boolean(),
})
export type StudentParent = z.infer<typeof StudentParentSchema>

export const StudentSchema = z.object({
  id: z.number(),
  fullName: z.string(),
  indexNo: z.string(),
  nationalId: z.string(),
  classSectionId: z.number(),
  classSectionDisplayName: z.string(),
  parents: z.array(StudentParentSchema),
})
export type Student = z.infer<typeof StudentSchema>

export const ParentSchema = z.object({
  id: z.number(),
  fullName: z.string(),
  nationalId: z.string(),
  phone: z.string().nullable(),
  relationship: z.string().nullable(),
})
export type Parent = z.infer<typeof ParentSchema>

export const TeacherAssignmentSchema = z.object({
  id: z.number(),
  subjectId: z.number(),
  subjectName: z.string(),
  classSectionId: z.number(),
  classSectionDisplayName: z.string(),
})
export type TeacherAssignment = z.infer<typeof TeacherAssignmentSchema>

export const TeacherSchema = z.object({
  id: z.number(),
  fullName: z.string(),
  nationalId: z.string(),
  email: z.string().nullable(),
  phone: z.string().nullable(),
  assignments: z.array(TeacherAssignmentSchema),
})
export type Teacher = z.infer<typeof TeacherSchema>

export const LookupSchema = z.object({
  id: z.number(),
  name: z.string(),
})
export type Lookup = z.infer<typeof LookupSchema>

// --- Books ---

export const BookSchema = z.object({
  id: z.number(),
  isbn: z.string().nullable(),
  code: z.string(),
  title: z.string(),
  author: z.string().nullable(),
  edition: z.string().nullable(),
  publisher: z.string(),
  publishedYear: z.number().int(),
  subjectId: z.number(),
  subjectName: z.string(),
  grade: z.string().nullable(),
  totalStock: z.number(),
  distributed: z.number(),
  withTeachers: z.number(),
  damaged: z.number(),
  lost: z.number(),
  available: z.number(),
})
export type Book = z.infer<typeof BookSchema>

export const StockEntrySchema = z.object({
  id: z.number(),
  bookId: z.number(),
  academicYearId: z.number(),
  quantity: z.number(),
  source: z.string().nullable(),
  notes: z.string().nullable(),
  enteredById: z.number(),
  enteredAt: z.string(),
})
export type StockEntry = z.infer<typeof StockEntrySchema>

export const StockMovementSchema = z.object({
  id: z.number(),
  bookId: z.number(),
  movementType: z.number(),
  quantity: z.number(),
  referenceId: z.number().nullable(),
  referenceType: z.string().nullable(),
  notes: z.string().nullable(),
  processedById: z.number(),
  processedAt: z.string(),
})
export type StockMovement = z.infer<typeof StockMovementSchema>

// --- Distribution ---

export const DistributionSlipItemSchema = z.object({
  id: z.number(),
  bookId: z.number(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
})
export type DistributionSlipItem = z.infer<typeof DistributionSlipItemSchema>

export const DistributionSlipSchema = z.object({
  id: z.number(),
  referenceNo: z.string(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  term: z.number(),
  studentId: z.number(),
  studentName: z.string(),
  studentIndexNo: z.string(),
  studentClassName: z.string(),
  studentNationalId: z.string().nullable(),
  parentId: z.number(),
  parentName: z.string(),
  issuedById: z.number(),
  issuedAt: z.string(),
  notes: z.string().nullable(),
  pdfFilePath: z.string().nullable(),
  lifecycleStatus: z.number().optional(),
  finalizedById: z.number().nullable().optional(),
  finalizedAt: z.string().nullable().optional(),
  cancelledById: z.number().nullable().optional(),
  cancelledAt: z.string().nullable().optional(),
  items: z.array(DistributionSlipItemSchema),
})
export type DistributionSlip = z.infer<typeof DistributionSlipSchema>

// --- Returns ---

export const ReturnSlipItemSchema = z.object({
  id: z.number(),
  bookId: z.number(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
  condition: z.number(),
  conditionNotes: z.string().nullable(),
})
export type ReturnSlipItem = z.infer<typeof ReturnSlipItemSchema>

export const ReturnSlipSchema = z.object({
  id: z.number(),
  referenceNo: z.string(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  studentId: z.number(),
  studentName: z.string(),
  studentIndexNo: z.string(),
  studentClassName: z.string(),
  studentNationalId: z.string().nullable(),
  returnedById: z.number(),
  returnedByName: z.string(),
  returnedByNationalId: z.string().nullable().optional(),
  returnedByPhone: z.string().nullable().optional(),
  returnedByRelationship: z.string().nullable().optional(),
  receivedById: z.number(),
  receivedByName: z.string().optional(),
  receivedByDesignation: z.string().nullable().optional(),
  receivedAt: z.string(),
  lifecycleStatus: z.number().optional(),
  finalizedById: z.number().nullable().optional(),
  finalizedAt: z.string().nullable().optional(),
  cancelledById: z.number().nullable().optional(),
  cancelledAt: z.string().nullable().optional(),
  notes: z.string().nullable(),
  pdfFilePath: z.string().nullable(),
  items: z.array(ReturnSlipItemSchema),
})
export type ReturnSlip = z.infer<typeof ReturnSlipSchema>

// --- Teacher Issues ---

export const TeacherIssueItemSchema = z.object({
  id: z.number(),
  bookId: z.number(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
  returnedQuantity: z.number(),
  outstandingQuantity: z.number(),
  returnedAt: z.string().nullable(),
  receivedById: z.number().nullable(),
})
export type TeacherIssueItem = z.infer<typeof TeacherIssueItemSchema>

export const TeacherReturnSlipItemSchema = z.object({
  id: z.number(),
  bookId: z.number(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
})
export type TeacherReturnSlipItem = z.infer<typeof TeacherReturnSlipItemSchema>

export const TeacherReturnSlipSchema = z.object({
  id: z.number(),
  referenceNo: z.string(),
  teacherIssueId: z.number(),
  teacherName: z.string(),
  teacherNationalId: z.string().nullable().optional(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  receivedById: z.number(),
  receivedByName: z.string().optional(),
  receivedByDesignation: z.string().nullable().optional(),
  receivedAt: z.string(),
  lifecycleStatus: z.number().optional(),
  finalizedById: z.number().nullable().optional(),
  finalizedAt: z.string().nullable().optional(),
  cancelledById: z.number().nullable().optional(),
  cancelledAt: z.string().nullable().optional(),
  notes: z.string().nullable(),
  pdfFilePath: z.string().nullable(),
  items: z.array(TeacherReturnSlipItemSchema),
})
export type TeacherReturnSlip = z.infer<typeof TeacherReturnSlipSchema>

export const TeacherIssueSchema = z.object({
  id: z.number(),
  referenceNo: z.string(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  teacherId: z.number(),
  teacherName: z.string(),
  issuedById: z.number(),
  issuedAt: z.string(),
  expectedReturnDate: z.string().nullable(),
  status: z.number(),
  notes: z.string().nullable(),
  pdfFilePath: z.string().nullable(),
  lifecycleStatus: z.number().optional(),
  finalizedById: z.number().nullable().optional(),
  finalizedAt: z.string().nullable().optional(),
  cancelledById: z.number().nullable().optional(),
  cancelledAt: z.string().nullable().optional(),
  items: z.array(TeacherIssueItemSchema),
})
export type TeacherIssue = z.infer<typeof TeacherIssueSchema>

// --- Reports ---

export const StockSummaryReportSchema = z.object({
  bookId: z.number(),
  code: z.string(),
  title: z.string(),
  subjectName: z.string(),
  grade: z.string().nullable(),
  totalStock: z.number(),
  distributed: z.number(),
  withTeachers: z.number(),
  damaged: z.number(),
  lost: z.number(),
  available: z.number(),
})
export type StockSummaryReport = z.infer<typeof StockSummaryReportSchema>

export const DistributionSummaryReportSchema = z.object({
  slipId: z.number(),
  referenceNo: z.string(),
  studentName: z.string(),
  studentIndexNo: z.string(),
  parentName: z.string(),
  issuedAt: z.string(),
  totalBooks: z.number(),
})
export type DistributionSummaryReport = z.infer<typeof DistributionSummaryReportSchema>

export const TeacherOutstandingReportSchema = z.object({
  issueId: z.number(),
  referenceNo: z.string(),
  teacherName: z.string(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
  returnedQuantity: z.number(),
  outstanding: z.number(),
  status: z.number(),
  issuedAt: z.string(),
  expectedReturnDate: z.string().nullable(),
})
export type TeacherOutstandingReport = z.infer<typeof TeacherOutstandingReportSchema>

export const StudentHistoryReportSchema = z.object({
  type: z.string(),
  referenceNo: z.string(),
  date: z.string(),
  bookTitle: z.string(),
  bookCode: z.string(),
  quantity: z.number(),
  condition: z.string().nullable(),
})
export type StudentHistoryReport = z.infer<typeof StudentHistoryReportSchema>

// --- Settings ---

export const ReferenceNumberFormatSchema = z.object({
  id: z.number(),
  slipType: z.number(),
  academicYearId: z.number(),
  academicYearName: z.string(),
  formatTemplate: z.string(),
  paddingWidth: z.number(),
})
export type ReferenceNumberFormat = z.infer<typeof ReferenceNumberFormatSchema>

export const SlipTemplateSettingSchema = z.object({
  id: z.number(),
  category: z.string(),
  key: z.string(),
  value: z.string(),
  sortOrder: z.number(),
})
export type SlipTemplateSetting = z.infer<typeof SlipTemplateSettingSchema>

// --- Users ---

export const UserSchema = z.object({
  id: z.number(),
  userName: z.string(),
  email: z.string(),
  fullName: z.string(),
  nationalId: z.string().nullable(),
  designation: z.string().nullable(),
  isActive: z.boolean(),
  roles: z.array(z.string()),
  createdAt: z.string(),
})
export type User = z.infer<typeof UserSchema>

// --- Audit ---

export const AuditLogSchema = z.object({
  id: z.number(),
  action: z.string(),
  entityType: z.string(),
  entityId: z.string(),
  oldValues: z.string().nullable(),
  newValues: z.string().nullable(),
  userId: z.number().nullable(),
  userName: z.string().nullable(),
  timestamp: z.string(),
})
export type AuditLog = z.infer<typeof AuditLogSchema>
