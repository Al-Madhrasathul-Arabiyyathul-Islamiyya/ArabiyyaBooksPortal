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
const SETUP_INCOMPLETE_ERROR
  = 'System setup is incomplete. Complete the setup steps in Admin > Settings > Setup Center before continuing.'

function getStatusFallbackMessage(statusCode: number): string | null {
  switch (statusCode) {
    case 400:
      return 'The request could not be processed. Please review the data and try again.'
    case 401:
      return 'Your session has expired. Please sign in again.'
    case 403:
      return 'You do not have permission to perform this action.'
    case 404:
      return 'The requested resource was not found.'
    case 409:
      return 'A conflicting record already exists.'
    case 422:
      return 'The submitted data is invalid. Please review the form and try again.'
    case 429:
      return 'Too many requests were made. Please wait and try again.'
    default:
      return statusCode >= 500 ? GENERIC_OPERATION_ERROR : null
  }
}

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

function hasSetupIncompleteSignal(
  fieldErrors: Record<string, string[]>,
  globalErrors: string[],
  data: FetchErrorShape['data'],
): boolean {
  const codeValues = fieldErrors.code ?? []
  if (codeValues.some(value => value.toUpperCase().includes('SETUP_INCOMPLETE'))) {
    return true
  }

  const setupKeys = ['missingsteps', 'hints', 'issues']
  if (data?.errors && typeof data.errors === 'object' && !Array.isArray(data.errors)) {
    const keys = Object.keys(data.errors).map(key => key.toLowerCase())
    if (keys.some(key => setupKeys.includes(key))) {
      return true
    }
  }

  const allMessages = [
    ...globalErrors,
    ...Object.values(fieldErrors).flat(),
    data?.message ?? '',
    data?.detail ?? '',
    data?.title ?? '',
  ]
    .join(' ')
    .toLowerCase()

  return allMessages.includes('setup_incomplete') || allMessages.includes('setup incomplete')
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

  const rawStatusCode = data?.status ?? fetchError.status ?? fetchError.statusCode ?? null
  const statusCode = rawStatusCode === null || rawStatusCode === undefined
    ? null
    : Number(rawStatusCode)
  const backendMessage = data?.message?.trim() || data?.detail?.trim() || data?.title?.trim() || ''
  const transportMessage = fetchError.message?.trim() || ''
  const message = backendMessage || transportMessage
  const looksLikeTransportMessage = /\[(GET|POST|PUT|PATCH|DELETE)\]\s*"/i.test(transportMessage)
  const safeBackendMessage = backendMessage && !isTechnicalServerMessage(backendMessage) ? backendMessage : ''

  if (hasSetupIncompleteSignal(fieldErrors, globalErrors, data)) {
    return { fieldErrors, globalErrors: [SETUP_INCOMPLETE_ERROR] }
  }

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
    else if (statusCode !== null && globalErrors.length === 0) {
      const statusFallback = getStatusFallbackMessage(statusCode)
      if (statusFallback) {
        globalErrors.push(safeBackendMessage || statusFallback)
      }
    }
    else if (isTechnicalServerMessage(message) && globalErrors.length === 0) {
      globalErrors.push(GENERIC_OPERATION_ERROR)
    }
    else if (looksLikeTransportMessage && globalErrors.length === 0) {
      if (statusCode !== null) {
        globalErrors.push(getStatusFallbackMessage(statusCode) ?? 'The request could not be completed. Please review the form and try again.')
      }
      else {
        globalErrors.push('The request could not be completed. Please review the form and try again.')
      }
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
