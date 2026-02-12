import type { ZodIssue } from 'zod/v4'

const DEFAULT_MESSAGES: Record<string, string> = {
  required: 'This field is required.',
  invalid_email: 'Enter a valid email address.',
  invalid_type: 'Invalid value.',
  too_small_string: 'Value is too short.',
  too_small_number: 'Value is too small.',
  too_big_string: 'Value is too long.',
  too_big_number: 'Value is too large.',
  custom: 'Invalid value.',
  fallback: 'Invalid value.',
}

function getIssueKey(issue: ZodIssue) {
  if (issue.code === 'invalid_type' && (issue as { expected?: string }).expected === 'string') {
    return 'required'
  }

  if (issue.code === 'invalid_format') {
    const format = (issue as { format?: string }).format
    if (format === 'email') return 'invalid_email'
  }

  if (issue.code === 'too_small') {
    return `too_small_${issue.origin}`
  }

  if (issue.code === 'too_big') {
    return `too_big_${issue.origin}`
  }

  return issue.code
}

export function toValidationMessage(issue: ZodIssue) {
  if (issue.message && issue.message !== 'Invalid input') {
    return issue.message
  }

  const key = getIssueKey(issue)
  return DEFAULT_MESSAGES[key] ?? DEFAULT_MESSAGES.fallback
}
