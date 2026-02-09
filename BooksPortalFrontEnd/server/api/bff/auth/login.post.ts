import { createError, defineEventHandler, readBody } from 'h3'
import { clearSessionTokens, setSessionTokens } from '../../../utils/auth-session'

type LoginRequest = {
  email: string
  password: string
}

type TokenResponse = {
  success: boolean
  data: {
    accessToken: string
    refreshToken: string
    expiresAt: string
  }
  message?: string
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)
  const base = String(config.public.apiBase).replace(/\/+$/, '')
  const body = await readBody<LoginRequest>(event)

  if (!body?.email || !body?.password) {
    throw createError({ statusCode: 400, statusMessage: 'Email and password are required' })
  }

  try {
    const response = await $fetch<TokenResponse>(`${base}/auth/login`, {
      method: 'POST',
      body,
    })

    if (!response.success) {
      clearSessionTokens(event)
      return response
    }

    setSessionTokens(event, response.data.accessToken, response.data.refreshToken, response.data.expiresAt)
    return response
  }
  catch (error) {
    clearSessionTokens(event)
    throw error
  }
})
