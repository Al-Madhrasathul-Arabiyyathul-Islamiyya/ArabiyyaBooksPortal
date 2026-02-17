import { describe, expect, it } from 'vitest'
import { toFriendlyLoginError } from '~/utils/auth-errors'

describe('toFriendlyLoginError', () => {
  it('returns a fallback message when no detail exists', () => {
    expect(toFriendlyLoginError()).toBe('Unable to sign in right now. Please try again.')
  })

  it('maps backend-unreachable errors to a user-friendly message', () => {
    expect(toFriendlyLoginError('fetch failed')).toBe(
      'Unable to reach the server. Please ensure the backend is running and try again.',
    )
    expect(toFriendlyLoginError('ECONNREFUSED 127.0.0.1')).toBe(
      'Unable to reach the server. Please ensure the backend is running and try again.',
    )
  })

  it('maps csrf errors to a refresh guidance message', () => {
    expect(toFriendlyLoginError('CSRF Token mismatch')).toBe(
      'Security validation failed. Please refresh the page and try again.',
    )
  })

  it('preserves unknown backend messages', () => {
    expect(toFriendlyLoginError('Invalid username/password')).toBe('Invalid username/password')
  })
})
