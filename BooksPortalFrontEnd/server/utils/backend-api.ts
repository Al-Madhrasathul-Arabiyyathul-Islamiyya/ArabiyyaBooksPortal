import type { FetchError } from 'ofetch'
import type { H3Event } from 'h3'
import { appendHeader, createError, getHeader, getMethod, getQuery, readBody, send, setResponseStatus } from 'h3'
import { clearSessionTokens, getSessionTokens, isExpiryExpired, setSessionTokens } from './auth-session'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

function getBackendBaseUrl(event: H3Event): string {
  const config = useRuntimeConfig(event)
  const base = String(config.public.apiBase ?? '').trim()
  if (!base) {
    throw createError({ statusCode: 500, statusMessage: 'Missing runtimeConfig.public.apiBase' })
  }
  return base.replace(/\/+$/, '')
}

async function callBackendRaw(
  event: H3Event,
  path: string,
  options: {
    method: HttpMethod
    body?: unknown
    authToken?: string
    binary?: boolean
  },
) {
  const base = getBackendBaseUrl(event)
  const headers: Record<string, string> = {}
  const requestContentType = getHeader(event, 'content-type')
  if (requestContentType) headers['content-type'] = requestContentType
  if (options.authToken) headers.Authorization = `Bearer ${options.authToken}`

  return await $fetch.raw(`${base}${path}`, {
    method: options.method,
    body: options.body as Record<string, unknown> | undefined,
    query: getQuery(event),
    headers,
    responseType: options.binary ? 'arrayBuffer' : undefined,
  })
}

export async function refreshServerSession(event: H3Event): Promise<boolean> {
  const base = getBackendBaseUrl(event)
  const { accessToken, refreshToken } = getSessionTokens(event)
  if (!accessToken || !refreshToken) return false

  try {
    const response = await $fetch<{
      success: boolean
      data: { accessToken: string; refreshToken: string; expiresAt: string }
    }>(`${base}/auth/refresh`, {
      method: 'POST',
      body: { accessToken, refreshToken },
    })

    if (!response.success) {
      clearSessionTokens(event)
      return false
    }

    setSessionTokens(event, response.data.accessToken, response.data.refreshToken, response.data.expiresAt)
    return true
  } catch {
    clearSessionTokens(event)
    return false
  }
}

export async function requireAccessToken(event: H3Event): Promise<string> {
  const session = getSessionTokens(event)
  if (!session.accessToken || !session.refreshToken) {
    throw createError({ statusCode: 401, statusMessage: 'Not authenticated' })
  }

  if (isExpiryExpired(session.expiresAt)) {
    const refreshed = await refreshServerSession(event)
    if (!refreshed) {
      throw createError({ statusCode: 401, statusMessage: 'Session expired' })
    }
  }

  const latest = getSessionTokens(event).accessToken
  if (!latest) {
    throw createError({ statusCode: 401, statusMessage: 'Not authenticated' })
  }

  return latest
}

export async function proxyAuthorizedBackendRequest(event: H3Event, apiPath: string) {
  const method = getMethod(event).toUpperCase() as HttpMethod
  const accepts = getHeader(event, 'accept') ?? ''
  const binary = accepts.includes('application/pdf') || apiPath.toLowerCase().includes('/print')
  const hasBody = method === 'POST' || method === 'PUT' || method === 'PATCH'
  const body = hasBody ? await readBody(event) : undefined

  let token = await requireAccessToken(event)

  try {
    const response = await callBackendRaw(event, apiPath, { method, body, authToken: token, binary })
    setResponseStatus(event, response.status, response.statusText)

    const contentType = response.headers.get('content-type')
    if (contentType) appendHeader(event, 'content-type', contentType)
    const contentDisposition = response.headers.get('content-disposition')
    if (contentDisposition) appendHeader(event, 'content-disposition', contentDisposition)

    if (binary) {
      return send(event, new Uint8Array(response._data as ArrayBuffer))
    }
    return response._data
  } catch (error) {
    const fetchError = error as FetchError
    if (fetchError.statusCode !== 401) {
      throw error
    }

    const refreshed = await refreshServerSession(event)
    if (!refreshed) {
      throw createError({ statusCode: 401, statusMessage: 'Session expired' })
    }

    token = await requireAccessToken(event)
    const retried = await callBackendRaw(event, apiPath, { method, body, authToken: token, binary })
    setResponseStatus(event, retried.status, retried.statusText)

    const contentType = retried.headers.get('content-type')
    if (contentType) appendHeader(event, 'content-type', contentType)
    const contentDisposition = retried.headers.get('content-disposition')
    if (contentDisposition) appendHeader(event, 'content-disposition', contentDisposition)

    if (binary) {
      return send(event, new Uint8Array(retried._data as ArrayBuffer))
    }
    return retried._data
  }
}
