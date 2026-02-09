import { defineEventHandler } from 'h3'
import { proxyAuthorizedBackendRequest } from '../../../utils/backend-api'

export default defineEventHandler(async (event) => {
  return await proxyAuthorizedBackendRequest(event, '/auth/me')
})
