import type { z } from 'zod/v4'
import { toValidationMessage } from '~/utils/validation/messages'

export function toZodFieldErrors(error: z.ZodError): Record<string, string[]> {
  const mapped: Record<string, string[]> = {}

  for (const issue of error.issues) {
    const field = String(issue.path[0] ?? '')
    if (!field) continue
    if (!mapped[field]) mapped[field] = []
    mapped[field].push(toValidationMessage(issue) ?? 'Invalid value.')
  }

  return mapped
}
