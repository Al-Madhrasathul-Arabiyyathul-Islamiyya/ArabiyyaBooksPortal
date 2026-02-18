import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createApp } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import { PiniaColada } from '@pinia/colada'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'
import type { UserProfile } from '~/types/entities'
import { useAuthStore } from '~/stores/auth'

const { navigateToMock, apiGetMock, apiPostMock } = vi.hoisted(() => ({
  navigateToMock: vi.fn(),
  apiGetMock: vi.fn(),
  apiPostMock: vi.fn(),
}))

mockNuxtImport('navigateTo', () => navigateToMock)
mockNuxtImport('useApi', () => () => ({
  get: apiGetMock,
  post: apiPostMock,
}))

const sampleUser: UserProfile = {
  id: 1,
  userName: 'admin',
  email: 'admin@booksportal.local',
  fullName: 'System Administrator',
  nationalId: null,
  designation: 'System Administrator',
  roles: ['SuperAdmin'],
}

let app: ReturnType<typeof createApp>

describe('auth store state transitions', () => {
  beforeEach(() => {
    const pinia = createPinia()
    app = createApp({ render: () => null })
    app.use(pinia)
    app.use(PiniaColada)
    setActivePinia(pinia)
    vi.clearAllMocks()
  })

  it('initialize fetches profile and marks store initialized', async () => {
    apiGetMock.mockResolvedValue({
      success: true,
      data: sampleUser,
    })

    const store = app.runWithContext(() => useAuthStore())
    await store.initialize()

    expect(store.initialized).toBe(true)
    expect(store.isAuthenticated).toBe(true)
    expect(store.user).toEqual(sampleUser)
  })

  it('login success fetches profile and populates user', async () => {
    apiPostMock.mockResolvedValueOnce({
      success: true,
      data: { expiresAt: '2099-12-31T00:00:00Z' },
    })
    apiGetMock.mockResolvedValue({
      success: true,
      data: sampleUser,
    })

    const store = app.runWithContext(() => useAuthStore())
    const result = await store.login({
      email: 'admin@booksportal.local',
      password: 'Admin@123456',
    })

    expect(result.success).toBe(true)
    expect(store.user).toEqual(sampleUser)
    expect(store.isAuthenticated).toBe(true)
  })

  it('login failure does not fetch profile or authenticate user', async () => {
    apiPostMock.mockResolvedValueOnce({
      success: false,
      message: 'Invalid email or password.',
    })

    const store = app.runWithContext(() => useAuthStore())
    const result = await store.login({
      email: 'admin@booksportal.local',
      password: 'wrong',
    })

    expect(result.success).toBe(false)
    expect(apiGetMock).not.toHaveBeenCalled()
    expect(store.isAuthenticated).toBe(false)
    expect(store.user).toBeNull()
  })

  it('logout clears local auth state and redirects even if API fails', async () => {
    apiGetMock.mockResolvedValue({
      success: true,
      data: sampleUser,
    })
    apiPostMock.mockRejectedValueOnce(new Error('network down'))

    const store = app.runWithContext(() => useAuthStore())
    await store.initialize()
    expect(store.isAuthenticated).toBe(true)

    await store.logout()

    expect(store.user).toBeNull()
    expect(store.initialized).toBe(true)
    expect(navigateToMock).toHaveBeenCalledWith('/login')
  })
})
