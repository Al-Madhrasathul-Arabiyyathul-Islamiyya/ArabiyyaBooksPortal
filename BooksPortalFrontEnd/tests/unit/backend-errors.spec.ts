import { describe, expect, it } from 'vitest'
import { getFriendlyErrorMessage, normalizeBackendErrors } from '~/utils/validation/backend-errors'

describe('normalizeBackendErrors', () => {
  it('maps object-style field errors', () => {
    const result = normalizeBackendErrors({
      data: {
        errors: {
          email: ['Invalid email'],
          password: 'Required',
        },
      },
    })

    expect(result.fieldErrors.email).toEqual(['Invalid email'])
    expect(result.fieldErrors.password).toEqual(['Required'])
  })

  it('maps array-style field/global errors', () => {
    const result = normalizeBackendErrors({
      data: {
        errors: [
          { field: 'code', message: 'Already exists' },
          { field: null, message: 'General validation failed' },
        ],
      },
    })

    expect(result.fieldErrors.code).toEqual(['Already exists'])
    expect(result.globalErrors).toContain('General validation failed')
  })

  it('normalizes common transport and status errors', () => {
    const offline = normalizeBackendErrors({ message: 'fetch failed' })
    expect(offline.globalErrors[0]).toContain('Unable to reach the server')

    const unauthorized = normalizeBackendErrors({ status: 401, message: 'Unauthorized' })
    expect(unauthorized.globalErrors[0]).toContain('session has expired')

    const conflict = normalizeBackendErrors({
      data: { status: 409, message: 'Duplicate key' },
    })
    expect(conflict.globalErrors[0]).toBe('Duplicate key')

    const serverError = normalizeBackendErrors({
      data: { status: 500, message: 'Unhandled exception with stack trace' },
    })
    expect(serverError.globalErrors[0]).toContain('There was an issue while processing your request')
  })

  it('hides technical server internals from users', () => {
    const result = normalizeBackendErrors({
      data: {
        message: 'Microsoft.Data.SqlClient.SqlException: Cannot insert duplicate key row',
      },
    })

    expect(result.globalErrors[0]).toContain('There was an issue while processing your request')
  })

  it('returns friendly fallback message helper', () => {
    const message = getFriendlyErrorMessage(
      { status: 403, message: 'Forbidden' },
      'Fallback message',
    )

    expect(message).toContain('do not have permission')
  })
})
