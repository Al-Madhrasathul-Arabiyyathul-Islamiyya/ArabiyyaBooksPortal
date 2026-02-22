import type { NormalizedValidationErrors } from '~/types/validation'

interface BackendErrorField {
  field?: string | null
  message?: string | null
}

interface FetchErrorShape {
  data?: {
    message?: string | null
    title?: string | null
    detail?: string | null
    errors?: Record<string, string[] | string> | BackendErrorField[] | null
    status?: number | null
  } | null
  message?: string | null
  status?: number
  statusCode?: number
}

const GENERIC_OPERATION_ERROR
  = 'There was an issue while processing your request. Please try again later. If this continues, contact IT support.'

function isTechnicalServerMessage(message: string): boolean {
  const lowered = message.toLowerCase()
  return (
    lowered.includes('sql')
    || lowered.includes('stack trace')
    || lowered.includes('stacktrace')
    || lowered.includes('exception')
    || lowered.includes('inner exception')
    || lowered.includes('cannot insert duplicate key')
    || lowered.includes('microsoft.data.sqlclient')
    || lowered.includes('entityframeworkcore')
    || lowered.includes('dbupdateexception')
    || lowered.includes(' at ')
  )
}

export function normalizeBackendErrors(error: unknown): NormalizedValidationErrors {
  const fieldErrors: Record<string, string[]> = {}
  const globalErrors: string[] = []
  const fetchError = error as FetchErrorShape
  const data = fetchError.data

  if (Array.isArray(data?.errors)) {
    for (const issue of data.errors) {
      const field = issue.field?.trim()
      const message = issue.message?.trim()
      if (!message) continue

      if (field) {
        if (!fieldErrors[field]) fieldErrors[field] = []
        fieldErrors[field].push(message)
        continue
      }

      globalErrors.push(message)
    }
  }
  else if (data?.errors && typeof data.errors === 'object') {
    for (const [field, value] of Object.entries(data.errors)) {
      const values = Array.isArray(value) ? value : [value]
      for (const raw of values) {
        const message = String(raw ?? '').trim()
        if (!message) continue
        if (field) {
          if (!fieldErrors[field]) fieldErrors[field] = []
          fieldErrors[field].push(message)
        }
        else {
          globalErrors.push(message)
        }
      }
    }
  }

  const statusCode = data?.status ?? fetchError.status ?? fetchError.statusCode ?? null
  const backendMessage = data?.message?.trim() || data?.detail?.trim() || data?.title?.trim() || ''
  const transportMessage = fetchError.message?.trim() || ''
  const message = backendMessage || transportMessage
  const looksLikeTransportMessage = /^\[(GET|POST|PUT|PATCH|DELETE)\]\s*"/i.test(transportMessage)

  if (message) {
    const lowered = message.toLowerCase()
    if (
      lowered.includes('fetch failed')
      || lowered.includes('failed to fetch')
      || lowered.includes('econnrefused')
    ) {
      globalErrors.push('Unable to reach the server. Please ensure the backend is running and try again.')
    }
    else if (lowered.includes('csrf')) {
      globalErrors.push('Security validation failed. Please refresh the page and try again.')
    }
    else if (statusCode === 409 && globalErrors.length === 0) {
      globalErrors.push(backendMessage || 'A conflicting record already exists.')
    }
    else if (statusCode === 401 && globalErrors.length === 0) {
      globalErrors.push('Your session has expired. Please sign in again.')
    }
    else if (statusCode === 403 && globalErrors.length === 0) {
      globalErrors.push('You do not have permission to perform this action.')
    }
    else if (statusCode && statusCode >= 500 && globalErrors.length === 0) {
      globalErrors.push(GENERIC_OPERATION_ERROR)
    }
    else if (isTechnicalServerMessage(message) && globalErrors.length === 0) {
      globalErrors.push(GENERIC_OPERATION_ERROR)
    }
    else if (looksLikeTransportMessage && globalErrors.length === 0) {
      globalErrors.push('The request could not be completed. Please review the form and try again.')
    }
    else if (globalErrors.length === 0) {
      globalErrors.push(message)
    }
  }

  return { fieldErrors, globalErrors }
}

export function getFriendlyErrorMessage(error: unknown, fallback: string): string {
  const normalized = normalizeBackendErrors(error)
  return normalized.globalErrors[0] ?? fallback
}
