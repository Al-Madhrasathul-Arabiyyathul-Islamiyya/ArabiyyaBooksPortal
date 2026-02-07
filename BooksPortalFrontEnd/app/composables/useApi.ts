import type { ApiResponse } from '~/types/api'
import { API } from '~/utils/constants'

const TOKEN_COOKIE = 'bp_access_token'
const REFRESH_COOKIE = 'bp_refresh_token'
const EXPIRY_COOKIE = 'bp_token_expiry'

interface ApiOptions extends Record<string, unknown> {
  headers?: Record<string, string>
}

export function useApi() {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase as string

  const accessToken = useCookie(TOKEN_COOKIE, { maxAge: 60 * 60 * 24 * 7 })
  const refreshToken = useCookie(REFRESH_COOKIE, { maxAge: 60 * 60 * 24 * 30 })
  const tokenExpiry = useCookie(EXPIRY_COOKIE, { maxAge: 60 * 60 * 24 * 7 })

  function getAccessToken(): string | null {
    return accessToken.value ?? null
  }

  function setTokens(access: string, refresh: string, expiresAt: string) {
    accessToken.value = access
    refreshToken.value = refresh
    tokenExpiry.value = expiresAt
  }

  function clearTokens() {
    accessToken.value = null
    refreshToken.value = null
    tokenExpiry.value = null
  }

  function isTokenExpired(): boolean {
    if (!tokenExpiry.value) return true
    return new Date(tokenExpiry.value) <= new Date()
  }

  async function refreshAccessToken(): Promise<boolean> {
    const access = accessToken.value
    const refresh = refreshToken.value
    if (!access || !refresh) return false

    try {
      const response = await $fetch<ApiResponse<{ accessToken: string; refreshToken: string; expiresAt: string }>>(
        `${baseURL}${API.auth.refresh}`,
        {
          method: 'POST',
          body: { accessToken: access, refreshToken: refresh },
        },
      )

      if (response.success) {
        setTokens(response.data.accessToken, response.data.refreshToken, response.data.expiresAt)
        return true
      }
      return false
    }
    catch {
      clearTokens()
      return false
    }
  }

  async function apiFetch<T>(
    url: string,
    options: ApiOptions & { method?: string; body?: unknown; query?: Record<string, unknown> } = {},
  ): Promise<ApiResponse<T>> {
    // Try refreshing if token is expired
    if (isTokenExpired() && refreshToken.value) {
      const refreshed = await refreshAccessToken()
      if (!refreshed) {
        clearTokens()
        navigateTo('/login')
        throw new Error('Session expired')
      }
    }

    const token = accessToken.value
    const headers: Record<string, string> = {
      ...options.headers,
    }
    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    try {
      return await $fetch<ApiResponse<T>>(`${baseURL}${url}`, {
        ...options,
        headers,
      })
    }
    catch (error: unknown) {
      const fetchError = error as { status?: number; data?: ApiResponse<unknown> }

      // On 401, try refresh once
      if (fetchError.status === 401 && refreshToken.value) {
        const refreshed = await refreshAccessToken()
        if (refreshed) {
          headers.Authorization = `Bearer ${accessToken.value}`
          return await $fetch<ApiResponse<T>>(`${baseURL}${url}`, {
            ...options,
            headers,
          })
        }
        clearTokens()
        navigateTo('/login')
      }

      throw error
    }
  }

  function get<T>(url: string, query?: Record<string, unknown>) {
    return apiFetch<T>(url, { method: 'GET', query })
  }

  function post<T>(url: string, body?: unknown) {
    return apiFetch<T>(url, { method: 'POST', body })
  }

  function put<T>(url: string, body?: unknown) {
    return apiFetch<T>(url, { method: 'PUT', body })
  }

  function del<T>(url: string) {
    return apiFetch<T>(url, { method: 'DELETE' })
  }

  async function downloadBlob(url: string, filename: string) {
    const token = accessToken.value
    const headers: Record<string, string> = {}
    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    const blob = await $fetch<Blob>(`${baseURL}${url}`, {
      headers,
      responseType: 'blob',
    })

    const objectUrl = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = objectUrl
    link.download = filename
    link.click()
    URL.revokeObjectURL(objectUrl)
  }

  return {
    get,
    post,
    put,
    del,
    downloadBlob,
    apiFetch,
    getAccessToken,
    setTokens,
    clearTokens,
    isTokenExpired,
  }
}
