export type SetupStatusValue = 'NotStarted' | 'InProgress' | 'Completed' | string | number

export interface SetupIssue {
  stepKey: string
  title: string
  description: string
  blocking: boolean
  hint?: string | null
}

export interface SetupStep {
  key: string
  title: string
  description: string
  completed: boolean
  completedAtUtc?: string | null
}

export interface SetupStatusResponse {
  status: SetupStatusValue
  issues: SetupIssue[]
  missingSteps: string[]
  hints: string[]
  steps: SetupStep[]
  lastEvaluatedAtUtc?: string | null
}
