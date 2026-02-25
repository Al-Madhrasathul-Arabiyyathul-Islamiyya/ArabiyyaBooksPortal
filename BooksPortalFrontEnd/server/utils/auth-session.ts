import type { H3Event } from 'h3'
import { getCookie, setCookie, deleteCookie } from 'h3'
import { createSign, createVerify } from 'node:crypto'
import { readFileSync } from 'node:fs'

const ACCESS_COOKIE = 'bp_access_token'
const REFRESH_COOKIE = 'bp_refresh_token'
const EXPIRY_COOKIE = 'bp_token_expiry'
const SIGNING_MODE_CERTIFICATE = 'Certificate'

let cachedPrivateKeyPath = ''
let cachedPrivateKeyContent = ''
let cachedPublicCertPath = ''
let cachedPublicCertContent = ''

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
    signingMode: typeof session?.signingMode === 'string' ? session.signingMode : 'None',
    signingPrivateKeyPath: typeof session?.signingPrivateKeyPath === 'string' ? session.signingPrivateKeyPath : '',
    signingPublicCertPath: typeof session?.signingPublicCertPath === 'string' ? session.signingPublicCertPath : '',
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

function shouldUseCertificateSigning(event: H3Event): boolean {
  const settings = getSessionSettings(event)
  return settings.signingMode.toLowerCase() === SIGNING_MODE_CERTIFICATE.toLowerCase()
    && settings.signingPrivateKeyPath.length > 0
    && settings.signingPublicCertPath.length > 0
}

function getPrivateKey(path: string): string {
  if (!cachedPrivateKeyContent || cachedPrivateKeyPath !== path) {
    cachedPrivateKeyContent = readFileSync(path, 'utf8')
    cachedPrivateKeyPath = path
  }
  return cachedPrivateKeyContent
}

function getPublicCert(path: string): string {
  if (!cachedPublicCertContent || cachedPublicCertPath !== path) {
    cachedPublicCertContent = readFileSync(path, 'utf8')
    cachedPublicCertPath = path
  }
  return cachedPublicCertContent
}

function toBase64Url(value: Buffer | string): string {
  return Buffer.from(value)
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '')
}

function fromBase64Url(value: string): Buffer {
  const normalized = value
    .replace(/-/g, '+')
    .replace(/_/g, '/')
  const padding = normalized.length % 4 === 0 ? '' : '='.repeat(4 - (normalized.length % 4))
  return Buffer.from(normalized + padding, 'base64')
}

function signCookieValue(event: H3Event, rawValue: string): string {
  if (!shouldUseCertificateSigning(event)) {
    return rawValue
  }

  const settings = getSessionSettings(event)
  const sign = createSign('RSA-SHA256')
  sign.update(rawValue)
  sign.end()
  const signature = sign.sign(getPrivateKey(settings.signingPrivateKeyPath))
  return `${toBase64Url(rawValue)}.${toBase64Url(signature)}`
}

function verifyCookieValue(event: H3Event, signedValue: string | null): string | null {
  if (!signedValue) {
    return null
  }

  if (!shouldUseCertificateSigning(event)) {
    return signedValue
  }

  const parts = signedValue.split('.')
  if (parts.length !== 2) {
    return null
  }

  const encodedValue = parts[0]
  const encodedSignature = parts[1]
  if (!encodedValue || !encodedSignature) {
    return null
  }
  const rawValue = fromBase64Url(encodedValue).toString('utf8')
  const signature = fromBase64Url(encodedSignature)

  const settings = getSessionSettings(event)
  const verify = createVerify('RSA-SHA256')
  verify.update(rawValue)
  verify.end()
  const isValid = verify.verify(getPublicCert(settings.signingPublicCertPath), signature)
  return isValid ? rawValue : null
}

export function getSessionTokens(event: H3Event) {
  return {
    accessToken: verifyCookieValue(event, getCookie(event, ACCESS_COOKIE) ?? null),
    refreshToken: verifyCookieValue(event, getCookie(event, REFRESH_COOKIE) ?? null),
    expiresAt: verifyCookieValue(event, getCookie(event, EXPIRY_COOKIE) ?? null),
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
  setCookie(event, ACCESS_COOKIE, signCookieValue(event, accessToken), { ...options, maxAge: settings.accessCookieMaxAgeSeconds })
  setCookie(event, REFRESH_COOKIE, signCookieValue(event, refreshToken), { ...options, maxAge: settings.refreshCookieMaxAgeSeconds })
  setCookie(event, EXPIRY_COOKIE, signCookieValue(event, expiresAt), { ...options, maxAge: settings.expiryCookieMaxAgeSeconds })
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
