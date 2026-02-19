import { expect, type Page } from '@playwright/test'
import type { ApiEnvelope, QueryParams } from './types'

export async function bffGet<T>(page: Page, path: string, query?: QueryParams) {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query ?? {})) {
    if (value === undefined) continue
    params.set(key, String(value))
  }
  const suffix = params.size > 0 ? `?${params.toString()}` : ''
  const res = await page.request.get(`/api/bff${path}${suffix}`)
  expect(res.ok(), `GET ${path} should succeed`).toBeTruthy()
  const body = await res.json() as ApiEnvelope<T>
  expect(body.success, `GET ${path} should return success=true (${body.message ?? 'no message'})`).toBeTruthy()
  return body.data
}

export async function bffPost<T>(page: Page, path: string, payload: unknown, csrfToken: string) {
  const res = await page.request.post(`/api/bff${path}`, {
    data: payload,
    headers: {
      'content-type': 'application/json',
      'csrf-token': csrfToken,
    },
  })
  if (!res.ok()) {
    const errorText = await res.text()
    throw new Error(`POST ${path} failed with HTTP ${res.status()}: ${errorText}`)
  }
  const body = await res.json() as ApiEnvelope<T>
  expect(body.success, `POST ${path} should return success=true (${body.message ?? 'no message'})`).toBeTruthy()
  return body.data
}

export async function bffDelete<T>(page: Page, path: string, csrfToken: string) {
  const res = await page.request.delete(`/api/bff${path}`, {
    headers: {
      'csrf-token': csrfToken,
    },
  })
  if (!res.ok()) {
    const errorText = await res.text()
    throw new Error(`DELETE ${path} failed with HTTP ${res.status()}: ${errorText}`)
  }
  const body = await res.json() as ApiEnvelope<T>
  expect(body.success, `DELETE ${path} should return success=true (${body.message ?? 'no message'})`).toBeTruthy()
  return body.data
}
