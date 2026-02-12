export type ValidationFieldErrors<T extends Record<string, unknown>> = Partial<Record<keyof T, string>>

export interface ValidationIssue {
  field: string
  message: string
}

export interface NormalizedValidationErrors {
  fieldErrors: Record<string, string[]>
  globalErrors: string[]
}
