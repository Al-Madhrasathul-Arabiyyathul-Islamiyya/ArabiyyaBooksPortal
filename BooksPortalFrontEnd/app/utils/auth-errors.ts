export function toFriendlyLoginError(message?: string | null) {
  if (!message) {
    return 'Unable to sign in right now. Please try again.'
  }

  const normalized = message.toLowerCase()
  if (
    normalized.includes('fetch failed')
    || normalized.includes('econnrefused')
    || normalized.includes('failed to fetch')
  ) {
    return 'Unable to reach the server. Please ensure the backend is running and try again.'
  }

  if (normalized.includes('csrf')) {
    return 'Security validation failed. Please refresh the page and try again.'
  }

  return message
}
