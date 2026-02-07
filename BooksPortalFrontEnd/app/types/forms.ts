import { z } from 'zod/v4'

// --- Auth ---

export const LoginRequestSchema = z.object({
  email: z.email(),
  password: z.string().min(1, 'Password is required'),
})
export type LoginRequest = z.infer<typeof LoginRequestSchema>

export const RefreshTokenRequestSchema = z.object({
  accessToken: z.string(),
  refreshToken: z.string(),
})
export type RefreshTokenRequest = z.infer<typeof RefreshTokenRequestSchema>

export const ChangePasswordRequestSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(6, 'Password must be at least 6 characters'),
})
export type ChangePasswordRequest = z.infer<typeof ChangePasswordRequestSchema>

// --- Master Data ---

export const CreateAcademicYearRequestSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  year: z.number().int().min(2000).max(2100),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().min(1, 'End date is required'),
})
export type CreateAcademicYearRequest = z.infer<typeof CreateAcademicYearRequestSchema>

export const UpdateAcademicYearRequestSchema = CreateAcademicYearRequestSchema
export type UpdateAcademicYearRequest = z.infer<typeof UpdateAcademicYearRequestSchema>

export const CreateKeystageRequestSchema = z.object({
  code: z.string().min(1, 'Code is required'),
  name: z.string().min(1, 'Name is required'),
  sortOrder: z.number().int().min(0),
})
export type CreateKeystageRequest = z.infer<typeof CreateKeystageRequestSchema>

export const CreateSubjectRequestSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required'),
})
export type CreateSubjectRequest = z.infer<typeof CreateSubjectRequestSchema>

export const CreateClassSectionRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  keystageId: z.number().int().min(1, 'Keystage is required'),
  grade: z.string().min(1, 'Grade is required'),
  section: z.string().min(1, 'Section is required'),
})
export type CreateClassSectionRequest = z.infer<typeof CreateClassSectionRequestSchema>

export const StudentParentRequestSchema = z.object({
  parentId: z.number().int(),
  isPrimary: z.boolean(),
})
export type StudentParentRequest = z.infer<typeof StudentParentRequestSchema>

export const CreateStudentRequestSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  indexNo: z.string().min(1, 'Index number is required'),
  nationalId: z.string().nullable().optional(),
  classSectionId: z.number().int().min(1, 'Class section is required'),
  parents: z.array(StudentParentRequestSchema).optional(),
})
export type CreateStudentRequest = z.infer<typeof CreateStudentRequestSchema>

export const UpdateStudentRequestSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().nullable().optional(),
  classSectionId: z.number().int().min(1, 'Class section is required'),
  parents: z.array(StudentParentRequestSchema).optional(),
})
export type UpdateStudentRequest = z.infer<typeof UpdateStudentRequestSchema>

export const CreateParentRequestSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().min(1, 'National ID is required'),
  phone: z.string().nullable().optional(),
  relationship: z.string().nullable().optional(),
})
export type CreateParentRequest = z.infer<typeof CreateParentRequestSchema>

export const CreateTeacherRequestSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().min(1, 'National ID is required'),
  email: z.string().nullable().optional(),
  phone: z.string().nullable().optional(),
})
export type CreateTeacherRequest = z.infer<typeof CreateTeacherRequestSchema>

export const CreateTeacherAssignmentRequestSchema = z.object({
  subjectId: z.number().int().min(1, 'Subject is required'),
  classSectionId: z.number().int().min(1, 'Class section is required'),
})
export type CreateTeacherAssignmentRequest = z.infer<typeof CreateTeacherAssignmentRequestSchema>

// --- Books ---

export const CreateBookRequestSchema = z.object({
  isbn: z.string().nullable().optional(),
  code: z.string().min(1, 'Code is required'),
  title: z.string().min(1, 'Title is required'),
  author: z.string().nullable().optional(),
  edition: z.string().nullable().optional(),
  publisher: z.string().nullable().optional(),
  publishedYear: z.number().int().nullable().optional(),
  subjectId: z.number().int().min(1, 'Subject is required'),
  grade: z.string().nullable().optional(),
})
export type CreateBookRequest = z.infer<typeof CreateBookRequestSchema>

export const AddStockRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  quantity: z.number().int().min(1, 'Quantity must be at least 1'),
  source: z.string().nullable().optional(),
  notes: z.string().nullable().optional(),
})
export type AddStockRequest = z.infer<typeof AddStockRequestSchema>

export const AdjustStockRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  movementType: z.number().int(),
  quantity: z.number().int().min(1, 'Quantity must be at least 1'),
  notes: z.string().nullable().optional(),
})
export type AdjustStockRequest = z.infer<typeof AdjustStockRequestSchema>

// --- Distribution ---

export const CreateDistributionSlipItemRequestSchema = z.object({
  bookId: z.number().int(),
  quantity: z.number().int().min(1),
})
export type CreateDistributionSlipItemRequest = z.infer<typeof CreateDistributionSlipItemRequestSchema>

export const CreateDistributionSlipRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  term: z.number().int(),
  studentId: z.number().int().min(1, 'Student is required'),
  parentId: z.number().int().min(1, 'Parent is required'),
  notes: z.string().nullable().optional(),
  items: z.array(CreateDistributionSlipItemRequestSchema).min(1, 'At least one book is required'),
})
export type CreateDistributionSlipRequest = z.infer<typeof CreateDistributionSlipRequestSchema>

// --- Returns ---

export const CreateReturnSlipItemRequestSchema = z.object({
  bookId: z.number().int(),
  quantity: z.number().int().min(1),
  condition: z.number().int(),
  conditionNotes: z.string().nullable().optional(),
})
export type CreateReturnSlipItemRequest = z.infer<typeof CreateReturnSlipItemRequestSchema>

export const CreateReturnSlipRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  studentId: z.number().int().min(1, 'Student is required'),
  returnedById: z.number().int().min(1, 'Returned by is required'),
  notes: z.string().nullable().optional(),
  items: z.array(CreateReturnSlipItemRequestSchema).min(1, 'At least one book is required'),
})
export type CreateReturnSlipRequest = z.infer<typeof CreateReturnSlipRequestSchema>

// --- Teacher Issues ---

export const CreateTeacherIssueItemRequestSchema = z.object({
  bookId: z.number().int(),
  quantity: z.number().int().min(1),
})
export type CreateTeacherIssueItemRequest = z.infer<typeof CreateTeacherIssueItemRequestSchema>

export const CreateTeacherIssueRequestSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  teacherId: z.number().int().min(1, 'Teacher is required'),
  expectedReturnDate: z.string().nullable().optional(),
  notes: z.string().nullable().optional(),
  items: z.array(CreateTeacherIssueItemRequestSchema).min(1, 'At least one book is required'),
})
export type CreateTeacherIssueRequest = z.infer<typeof CreateTeacherIssueRequestSchema>

export const TeacherReturnItemRequestSchema = z.object({
  teacherIssueItemId: z.number().int(),
  quantity: z.number().int().min(1),
})
export type TeacherReturnItemRequest = z.infer<typeof TeacherReturnItemRequestSchema>

export const ProcessTeacherReturnRequestSchema = z.object({
  items: z.array(TeacherReturnItemRequestSchema).min(1, 'At least one item is required'),
  notes: z.string().nullable().optional(),
})
export type ProcessTeacherReturnRequest = z.infer<typeof ProcessTeacherReturnRequestSchema>

// --- Settings ---

export const CreateReferenceNumberFormatRequestSchema = z.object({
  slipType: z.number().int(),
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  formatTemplate: z.string().min(1, 'Format template is required'),
  paddingWidth: z.number().int().min(1).default(6),
})
export type CreateReferenceNumberFormatRequest = z.infer<typeof CreateReferenceNumberFormatRequestSchema>

export const UpdateSlipTemplateSettingRequestSchema = z.object({
  value: z.string().min(1, 'Value is required'),
  sortOrder: z.number().int().min(0),
})
export type UpdateSlipTemplateSettingRequest = z.infer<typeof UpdateSlipTemplateSettingRequestSchema>

// --- Users ---

export const CreateUserRequestSchema = z.object({
  userName: z.string().min(1, 'Username is required'),
  email: z.email(),
  password: z.string().min(6, 'Password must be at least 6 characters'),
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().nullable().optional(),
  designation: z.string().nullable().optional(),
  roles: z.array(z.string()).min(1, 'At least one role is required'),
})
export type CreateUserRequest = z.infer<typeof CreateUserRequestSchema>

export const UpdateUserRequestSchema = z.object({
  email: z.email(),
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().nullable().optional(),
  designation: z.string().nullable().optional(),
  isActive: z.boolean(),
})
export type UpdateUserRequest = z.infer<typeof UpdateUserRequestSchema>
