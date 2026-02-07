import type { ApiResponse } from '~/types/api'
import { STORAGE_KEYS, API } from '~/utils/constants'

interface ApiOptions extends Record<string, unknown> {
  headers?: Record<string, string>
}

export function useApi() {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase as string

  function getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.accessToken)
  }

  function getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.refreshToken)
  }

  function setTokens(accessToken: string, refreshToken: string, expiresAt: string) {
    localStorage.setItem(STORAGE_KEYS.accessToken, accessToken)
    localStorage.setItem(STORAGE_KEYS.refreshToken, refreshToken)
    localStorage.setItem(STORAGE_KEYS.tokenExpiry, expiresAt)
  }

  function clearTokens() {
    localStorage.removeItem(STORAGE_KEYS.accessToken)
    localStorage.removeItem(STORAGE_KEYS.refreshToken)
    localStorage.removeItem(STORAGE_KEYS.tokenExpiry)
  }

  function isTokenExpired(): boolean {
    const expiry = localStorage.getItem(STORAGE_KEYS.tokenExpiry)
    if (!expiry) return true
    return new Date(expiry) <= new Date()
  }

  async function refreshAccessToken(): Promise<boolean> {
    const accessToken = getAccessToken()
    const refreshToken = getRefreshToken()
    if (!accessToken || !refreshToken) return false

    try {
      const response = await $fetch<ApiResponse<{ accessToken: string; refreshToken: string; expiresAt: string }>>(
        `${baseURL}${API.auth.refresh}`,
        {
          method: 'POST',
          body: { accessToken, refreshToken },
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
    if (isTokenExpired() && getRefreshToken()) {
      const refreshed = await refreshAccessToken()
      if (!refreshed) {
        clearTokens()
        navigateTo('/login')
        throw new Error('Session expired')
      }
    }

    const token = getAccessToken()
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
      if (fetchError.status === 401 && getRefreshToken()) {
        const refreshed = await refreshAccessToken()
        if (refreshed) {
          const newToken = getAccessToken()
          headers.Authorization = `Bearer ${newToken}`
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
    const token = getAccessToken()
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
