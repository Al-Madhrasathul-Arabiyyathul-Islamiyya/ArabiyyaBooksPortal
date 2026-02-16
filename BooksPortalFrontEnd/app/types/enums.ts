import { z } from 'zod/v4'

export const Term = z.enum(['Term1', 'Term2', 'Both'])
export type Term = z.infer<typeof Term>

export const TermValue = {
  Term1: 1,
  Term2: 2,
  Both: 3,
} as const

export const BookCondition = z.enum(['Good', 'Fair', 'Poor', 'Damaged', 'Lost'])
export type BookCondition = z.infer<typeof BookCondition>

export const BookConditionValue = {
  Good: 1,
  Fair: 2,
  Poor: 3,
  Damaged: 4,
  Lost: 5,
} as const

export const MovementType = z.enum([
  'StockEntry',
  'Distribution',
  'Return',
  'TeacherIssue',
  'TeacherReturn',
  'MarkDamaged',
  'MarkLost',
  'Adjustment',
  'WriteOff',
])
export type MovementType = z.infer<typeof MovementType>

export const MovementTypeValue = {
  StockEntry: 1,
  Distribution: 2,
  Return: 3,
  TeacherIssue: 4,
  TeacherReturn: 5,
  MarkDamaged: 6,
  MarkLost: 7,
  Adjustment: 8,
  WriteOff: 9,
} as const

export const SlipType = z.enum(['Distribution', 'Return', 'TeacherIssue', 'TeacherReturn'])
export type SlipType = z.infer<typeof SlipType>

export const SlipTypeValue = {
  Distribution: 1,
  Return: 2,
  TeacherIssue: 3,
  TeacherReturn: 4,
} as const

export const SlipLifecycleStatus = z.enum(['Processing', 'Finalized', 'Cancelled'])
export type SlipLifecycleStatus = z.infer<typeof SlipLifecycleStatus>

export const SlipLifecycleStatusValue = {
  Processing: 0,
  Finalized: 1,
  Cancelled: 2,
} as const

export const TeacherIssueStatus = z.enum(['Active', 'Partial', 'Returned', 'Overdue'])
export type TeacherIssueStatus = z.infer<typeof TeacherIssueStatus>

export const TeacherIssueStatusValue = {
  Active: 1,
  Partial: 2,
  Returned: 3,
  Overdue: 4,
} as const

export const UserRole = z.enum(['SuperAdmin', 'Admin', 'User'])
export type UserRole = z.infer<typeof UserRole>
