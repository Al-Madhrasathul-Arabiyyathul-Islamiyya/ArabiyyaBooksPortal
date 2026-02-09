import { defineEventHandler } from 'h3'
import { clearSessionTokens, getSessionTokens } from '../../../utils/auth-session'

type LogoutResponse = {
  success: boolean
  message?: string
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)
  const base = String(config.public.apiBase).replace(/\/+$/, '')
  const { accessToken, refreshToken } = getSessionTokens(event)

  try {
    if (accessToken && refreshToken) {
      await $fetch<LogoutResponse>(`${base}/auth/logout`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      })
    }
  } catch {
    // clear local session even when backend logout fails
  } finally {
    clearSessionTokens(event)
  }

  return {
    success: true,
    message: 'Logged out',
  }
})
