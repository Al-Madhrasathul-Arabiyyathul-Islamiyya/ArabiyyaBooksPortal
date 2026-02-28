import { createError, defineEventHandler } from 'h3'
import { refreshServerSession } from '#server/utils/backend-api'
import { getSessionTokens } from '#server/utils/auth-session'

export default defineEventHandler(async (event) => {
  const refreshed = await refreshServerSession(event)
  if (!refreshed) {
    throw createError({ statusCode: 401, statusMessage: 'Session expired' })
  }

  const session = getSessionTokens(event)
  return {
    success: true,
    data: {
      expiresAt: session.expiresAt,
    },
  }
})
