import type { ApiResponse } from '~/types/api'

type ApiMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
type QueryValue = string | number | boolean | null | undefined
type QueryParams = Record<string, QueryValue>
type JsonBody = Record<string, unknown>
type ApiBody = BodyInit | JsonBody | null

interface ApiOptions {
  method?: ApiMethod
  body?: ApiBody
  query?: QueryParams
  headers?: HeadersInit
}

export function useApi() {
  const baseURL = '/api/bff'
  const { $csrfFetch } = useNuxtApp()
  const csrfFetch = $csrfFetch ?? $fetch

  async function apiFetch<T>(
    url: string,
    options: ApiOptions = {},
  ): Promise<ApiResponse<T>> {
    try {
      return await csrfFetch<ApiResponse<T>>(`${baseURL}${url}`, {
        ...options,
      })
    }
    catch (error: unknown) {
      const fetchError = error as { status?: number }

      if (fetchError.status === 401) {
        try {
          await csrfFetch<ApiResponse<{ expiresAt: string }>>(`${baseURL}/auth/refresh`, {
            method: 'POST',
          })

          return await csrfFetch<ApiResponse<T>>(`${baseURL}${url}`, {
            ...options,
          })
        }
        catch {
          await navigateTo('/login')
        }
      }

      throw error
    }
  }

  function get<T>(url: string, query?: QueryParams) {
    return apiFetch<T>(url, { method: 'GET', query })
  }

  function post<T>(url: string, body?: ApiBody) {
    return apiFetch<T>(url, { method: 'POST', body })
  }

  function put<T>(url: string, body?: ApiBody) {
    return apiFetch<T>(url, { method: 'PUT', body })
  }

  function del<T>(url: string) {
    return apiFetch<T>(url, { method: 'DELETE' })
  }

  async function downloadBlob(url: string, filename: string, openInNewTab = false) {
    if (!import.meta.client) return

    const response = await $fetch.raw(`${baseURL}${url}`, { method: 'GET' })
    const blob = new Blob([response._data as BlobPart], {
      type: response.headers.get('content-type') ?? 'application/octet-stream',
    })

    const objectUrl = URL.createObjectURL(blob)
    if (openInNewTab) {
      window.open(objectUrl, '_blank')
      setTimeout(() => URL.revokeObjectURL(objectUrl), 1000)
      return
    }

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
  }
}
