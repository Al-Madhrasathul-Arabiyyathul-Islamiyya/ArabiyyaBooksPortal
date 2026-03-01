import { defineEventHandler } from 'h3'
import { proxyBackendRequest } from '#server/utils/backend-api'

export default defineEventHandler(async (event) => {
  return await proxyBackendRequest(event, '/setup/bootstrap/super-admin')
})
