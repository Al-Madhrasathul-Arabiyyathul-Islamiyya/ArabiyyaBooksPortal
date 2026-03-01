import { expect, type Page } from '@playwright/test'
import type { ApiEnvelope } from './types'

const backendAuthBase = process.env.PLAYWRIGHT_API_BASE ?? 'http://localhost:5071/api'
const appBaseUrl = process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:3000'

export const e2eCredentials = {
  admin: {
    email: process.env.E2E_ADMIN_EMAIL ?? 'admin@booksportal.local',
    password: process.env.E2E_ADMIN_PASSWORD ?? 'Admin@123456',
  },
  user: {
    email: process.env.E2E_USER_EMAIL ?? 'user@booksportal.local',
    password: process.env.E2E_USER_PASSWORD ?? 'Admin@123456',
  },
}

type LoginTokenData = {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

type SetupStatusResponse = {
  status?: string | number
  issues?: Array<{ key: string }>
}

let setupEnsured = false

async function backendLogin(page: Page, email: string, password: string): Promise<LoginTokenData> {
  const loginRes = await page.request.post(`${backendAuthBase}/auth/login`, {
    data: { email, password },
    headers: {
      'content-type': 'application/json',
    },
  })
  expect(loginRes.ok(), 'Backend auth login should succeed').toBeTruthy()
  const loginBody = await loginRes.json() as ApiEnvelope<LoginTokenData>
  expect(loginBody.success, `Backend auth login success=false (${loginBody.message ?? 'no message'})`).toBeTruthy()
  return loginBody.data
}

async function setupPost(page: Page, accessToken: string, path: string) {
  return page.request.post(`${backendAuthBase}${path}`, {
    headers: {
      authorization: `Bearer ${accessToken}`,
    },
  })
}

async function ensureSetupReadiness(page: Page) {
  if (setupEnsured) return

  const adminSession = await backendLogin(
    page,
    e2eCredentials.admin.email,
    e2eCredentials.admin.password,
  )

  const statusRes = await page.request.get(`${backendAuthBase}/setup/status`, {
    headers: {
      authorization: `Bearer ${adminSession.accessToken}`,
    },
  })

  if (!statusRes.ok()) {
    setupEnsured = true
    return
  }

  const statusBody = await statusRes.json() as ApiEnvelope<SetupStatusResponse>
  const issueKeys = new Set((statusBody.data?.issues ?? []).map(issue => issue.key))
  if (issueKeys.size === 0) {
    setupEnsured = true
    return
  }

  if (issueKeys.has('slip-templates')) {
    await setupPost(page, adminSession.accessToken, '/setup/slip-templates/confirm')
  }
  if (issueKeys.has('hierarchy') || issueKeys.has('active-academic-year')) {
    await setupPost(page, adminSession.accessToken, '/setup/hierarchy/initialize')
  }
  if (issueKeys.has('reference-formats')) {
    await setupPost(page, adminSession.accessToken, '/setup/reference-formats/initialize')
  }

  await setupPost(page, adminSession.accessToken, '/setup/complete')
  setupEnsured = true
}

async function setSessionCookies(page: Page, session: LoginTokenData) {
  const targetUrl = new URL(appBaseUrl)
  await page.context().addCookies([
    {
      name: 'bp_access_token',
      value: session.accessToken,
      domain: targetUrl.hostname,
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'bp_refresh_token',
      value: session.refreshToken,
      domain: targetUrl.hostname,
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'bp_token_expiry',
      value: session.expiresAt,
      domain: targetUrl.hostname,
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
  ])
}

export async function loginAndGetCsrf(page: Page, email: string, password: string) {
  await ensureSetupReadiness(page)
  const session = await backendLogin(page, email, password)
  await setSessionCookies(page, session)

  await page.goto('/distribution')
  await expect(page).toHaveURL(/\/distribution$/, { timeout: 30000 })

  const csrfToken = await page.locator('meta[name="csrf-token"]').getAttribute('content')
  expect(csrfToken, 'CSRF meta tag should be present after page load').toBeTruthy()
  return csrfToken!
}

export async function loginAsAdmin(page: Page) {
  return loginAndGetCsrf(page, e2eCredentials.admin.email, e2eCredentials.admin.password)
}

export async function loginAsUser(page: Page) {
  return loginAndGetCsrf(page, e2eCredentials.user.email, e2eCredentials.user.password)
}
