import type { FetchError } from 'ofetch'
import type { H3Event } from 'h3'
import { appendHeader, createError, getHeader, getMethod, getQuery, readBody, readRawBody, send, setResponseStatus } from 'h3'
import { clearSessionTokens, getExpirySkewSeconds, getSessionTokens, isExpiryExpired, setSessionTokens } from './auth-session'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
const refreshInFlight = new Map<string, Promise<boolean>>()

function deriveBackendErrorMessage(fetchError: FetchError): string {
  const data = fetchError.data as { message?: string, detail?: string, title?: string } | undefined
  return (
    data?.message?.trim()
    || data?.detail?.trim()
    || data?.title?.trim()
    || fetchError.statusMessage?.trim()
    || 'Request to backend failed.'
  )
}

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

  const existing = refreshInFlight.get(refreshToken)
  if (existing) {
    return await existing
  }

  const refreshPromise = (async () => {
    try {
      const response = await $fetch<{
        success: boolean
        data: { accessToken: string, refreshToken: string, expiresAt: string }
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
    }
    catch {
      clearSessionTokens(event)
      return false
    }
    finally {
      refreshInFlight.delete(refreshToken)
    }
  })()

  refreshInFlight.set(refreshToken, refreshPromise)
  return await refreshPromise
}

export async function requireAccessToken(event: H3Event): Promise<string> {
  const session = getSessionTokens(event)
  if (!session.accessToken || !session.refreshToken) {
    throw createError({ statusCode: 401, statusMessage: 'Not authenticated' })
  }

  if (isExpiryExpired(session.expiresAt, getExpirySkewSeconds(event))) {
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
  const path = apiPath.toLowerCase()
  const binary = accepts.includes('application/pdf')
    || accepts.includes('application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
    || accepts.includes('application/octet-stream')
    || path.includes('/print')
    || path.includes('/import-templates/')
    || (path.includes('/bulk/jobs/') && path.endsWith('/report'))
    || path.includes('/reports/export/')
  const hasBody = method === 'POST' || method === 'PUT' || method === 'PATCH'
  const requestContentType = (getHeader(event, 'content-type') ?? '').toLowerCase()
  const hasMultipartBody = requestContentType.includes('multipart/form-data')
  const body = hasBody
    ? (
        hasMultipartBody
          ? await readRawBody(event, false)
          : await readBody(event)
      )
    : undefined

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
  }
  catch (error) {
    const fetchError = error as FetchError
    if (fetchError.statusCode !== 401) {
      const statusCode = Number(fetchError.statusCode ?? 500)
      throw createError({
        statusCode: Number.isFinite(statusCode) ? statusCode : 500,
        statusMessage: deriveBackendErrorMessage(fetchError),
        data: fetchError.data,
      })
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
