export type SetupStatusValue = 'NotStarted' | 'InProgress' | 'Completed' | string | number

export interface SetupIssue {
  key: string
  message: string
  hint?: string | null
}

export interface SetupStep {
  key: string
  title: string
  isComplete: boolean
  isBlocking?: boolean
  hint?: string | null
  completedAtUtc?: string | null
}

export interface SetupStatusResponse {
  status: SetupStatusValue
  issues: SetupIssue[]
  steps: SetupStep[]
  isReady?: boolean
  startedAtUtc?: string | null
  lastEvaluatedAtUtc?: string | null
  completedAtUtc?: string | null
}

export interface BootstrapSuperAdminRequest {
  userName: string
  email: string
  password: string
  fullName: string
  nationalId?: string | null
  designation?: string | null
}
