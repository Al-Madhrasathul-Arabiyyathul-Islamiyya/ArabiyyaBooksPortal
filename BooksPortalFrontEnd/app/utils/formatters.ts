import { TermValue, BookConditionValue, MovementTypeValue, TeacherIssueStatusValue, SlipTypeValue } from '~/types/enums'

// Enum display labels
export const termLabels: Record<number, string> = {
  [TermValue.Term1]: 'Term 1',
  [TermValue.Term2]: 'Term 2',
  [TermValue.Both]: 'Both',
}

export const conditionLabels: Record<number, string> = {
  [BookConditionValue.Good]: 'Good',
  [BookConditionValue.Fair]: 'Fair',
  [BookConditionValue.Poor]: 'Poor',
  [BookConditionValue.Damaged]: 'Damaged',
  [BookConditionValue.Lost]: 'Lost',
}

export const movementTypeLabels: Record<number, string> = {
  [MovementTypeValue.StockEntry]: 'Stock Entry',
  [MovementTypeValue.Distribution]: 'Distribution',
  [MovementTypeValue.Return]: 'Return',
  [MovementTypeValue.TeacherIssue]: 'Teacher Issue',
  [MovementTypeValue.TeacherReturn]: 'Teacher Return',
  [MovementTypeValue.MarkDamaged]: 'Mark Damaged',
  [MovementTypeValue.MarkLost]: 'Mark Lost',
  [MovementTypeValue.Adjustment]: 'Adjustment',
  [MovementTypeValue.WriteOff]: 'Write Off',
}

export const teacherIssueStatusLabels: Record<number, string> = {
  [TeacherIssueStatusValue.Active]: 'Active',
  [TeacherIssueStatusValue.Partial]: 'Partially Returned',
  [TeacherIssueStatusValue.Returned]: 'Fully Returned',
  [TeacherIssueStatusValue.Overdue]: 'Overdue',
}

export const slipTypeLabels: Record<number, string> = {
  [SlipTypeValue.Distribution]: 'Distribution',
  [SlipTypeValue.Return]: 'Return',
  [SlipTypeValue.TeacherIssue]: 'Teacher Issue',
  [SlipTypeValue.TeacherReturn]: 'Teacher Return',
}

// Condition severity colors for PrimeVue Tag/Badge
export const conditionSeverity: Record<number, string> = {
  [BookConditionValue.Good]: 'success',
  [BookConditionValue.Fair]: 'info',
  [BookConditionValue.Poor]: 'warn',
  [BookConditionValue.Damaged]: 'danger',
  [BookConditionValue.Lost]: 'danger',
}

// Teacher issue status severity
export const teacherIssueStatusSeverity: Record<number, string> = {
  [TeacherIssueStatusValue.Active]: 'info',
  [TeacherIssueStatusValue.Partial]: 'warn',
  [TeacherIssueStatusValue.Returned]: 'success',
  [TeacherIssueStatusValue.Overdue]: 'danger',
}

// Backward-compatible alias used by page implementations.
export const teacherIssueStatusSeverities = teacherIssueStatusSeverity

// Dropdown option arrays for PrimeVue Select components
export const termOptions = Object.entries(termLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}))

export const conditionOptions = Object.entries(conditionLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}))

export const slipTypeOptions = Object.entries(slipTypeLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}))

export const teacherIssueStatusOptions = Object.entries(teacherIssueStatusLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}))
