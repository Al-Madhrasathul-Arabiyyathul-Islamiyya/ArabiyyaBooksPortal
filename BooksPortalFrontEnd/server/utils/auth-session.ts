import type { H3Event } from 'h3'
import { getCookie, setCookie, deleteCookie } from 'h3'

const ACCESS_COOKIE = 'bp_access_token'
const REFRESH_COOKIE = 'bp_refresh_token'
const EXPIRY_COOKIE = 'bp_token_expiry'

function toNumber(value: unknown, fallback: number): number {
  const parsed = Number(value)
  if (Number.isNaN(parsed) || parsed <= 0) {
    return fallback
  }
  return parsed
}

function toBoolean(value: unknown, fallback: boolean): boolean {
  if (typeof value === 'boolean') return value
  if (typeof value === 'string') {
    if (value.toLowerCase() === 'true') return true
    if (value.toLowerCase() === 'false') return false
  }
  return fallback
}

function getSessionSettings(event: H3Event) {
  const config = useRuntimeConfig(event)
  const session = config.auth?.session as Record<string, unknown> | undefined

  return {
    accessCookieMaxAgeSeconds: toNumber(session?.accessCookieMaxAgeSeconds, 60 * 60 * 24 * 7),
    refreshCookieMaxAgeSeconds: toNumber(session?.refreshCookieMaxAgeSeconds, 60 * 60 * 24 * 30),
    expiryCookieMaxAgeSeconds: toNumber(session?.expiryCookieMaxAgeSeconds, 60 * 60 * 24 * 7),
    expirySkewSeconds: toNumber(session?.expirySkewSeconds, 30),
    cookieSecure: toBoolean(session?.cookieSecure, !import.meta.dev),
  }
}

function cookieOptions(event: H3Event) {
  const settings = getSessionSettings(event)
  return {
    httpOnly: true,
    sameSite: 'lax' as const,
    secure: settings.cookieSecure,
    path: '/',
  }
}

export function getSessionTokens(event: H3Event) {
  return {
    accessToken: getCookie(event, ACCESS_COOKIE) ?? null,
    refreshToken: getCookie(event, REFRESH_COOKIE) ?? null,
    expiresAt: getCookie(event, EXPIRY_COOKIE) ?? null,
  }
}

export function setSessionTokens(
  event: H3Event,
  accessToken: string,
  refreshToken: string,
  expiresAt: string,
) {
  const options = cookieOptions(event)
  const settings = getSessionSettings(event)
  setCookie(event, ACCESS_COOKIE, accessToken, { ...options, maxAge: settings.accessCookieMaxAgeSeconds })
  setCookie(event, REFRESH_COOKIE, refreshToken, { ...options, maxAge: settings.refreshCookieMaxAgeSeconds })
  setCookie(event, EXPIRY_COOKIE, expiresAt, { ...options, maxAge: settings.expiryCookieMaxAgeSeconds })
}

export function clearSessionTokens(event: H3Event) {
  const options = cookieOptions(event)
  deleteCookie(event, ACCESS_COOKIE, options)
  deleteCookie(event, REFRESH_COOKIE, options)
  deleteCookie(event, EXPIRY_COOKIE, options)
}

export function isExpiryExpired(expiresAt: string | null, skewSeconds = 30): boolean {
  if (!expiresAt) return true
  const expiry = new Date(expiresAt).getTime()
  if (Number.isNaN(expiry)) return true
  return expiry <= Date.now() + skewSeconds * 1000
}

export function getExpirySkewSeconds(event: H3Event): number {
  return getSessionSettings(event).expirySkewSeconds
}
