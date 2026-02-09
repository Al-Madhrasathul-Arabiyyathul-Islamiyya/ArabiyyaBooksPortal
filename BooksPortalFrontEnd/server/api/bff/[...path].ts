import { createError, defineEventHandler } from 'h3'
import { proxyAuthorizedBackendRequest } from '../../utils/backend-api'

export default defineEventHandler(async (event) => {
  const path = event.context.params?.path
  const segments = Array.isArray(path) ? path : path ? [path] : []

  if (segments.length === 0) {
    throw createError({ statusCode: 404, statusMessage: 'Not Found' })
  }

  // Auth endpoints are handled by dedicated files under /api/bff/auth/*
  if (segments[0]?.toLowerCase() === 'auth') {
    throw createError({ statusCode: 404, statusMessage: 'Not Found' })
  }

  const apiPath = `/${segments.join('/')}`
  return await proxyAuthorizedBackendRequest(event, apiPath)
})
