import type { H3Event } from 'h3'
import { getCookie, setCookie, deleteCookie } from 'h3'

const ACCESS_COOKIE = 'bp_access_token'
const REFRESH_COOKIE = 'bp_refresh_token'
const EXPIRY_COOKIE = 'bp_token_expiry'

function cookieOptions() {
  return {
    httpOnly: true,
    sameSite: 'lax' as const,
    secure: !import.meta.dev,
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
  const options = cookieOptions()
  setCookie(event, ACCESS_COOKIE, accessToken, { ...options, maxAge: 60 * 60 * 24 * 7 })
  setCookie(event, REFRESH_COOKIE, refreshToken, { ...options, maxAge: 60 * 60 * 24 * 30 })
  setCookie(event, EXPIRY_COOKIE, expiresAt, { ...options, maxAge: 60 * 60 * 24 * 7 })
}

export function clearSessionTokens(event: H3Event) {
  const options = cookieOptions()
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
