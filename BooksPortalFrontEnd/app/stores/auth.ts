import type { UserProfile } from '~/types/entities'
import type { LoginRequest } from '~/types/forms'
import { API } from '~/utils/constants'
import { STORAGE_KEYS } from '~/utils/constants'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null)
  const isAuthenticated = computed(() => !!user.value)
  const roles = computed(() => user.value?.roles ?? [])

  function hasRole(role: string): boolean {
    return roles.value.includes(role)
  }

  function hasAnyRole(...checkRoles: string[]): boolean {
    return checkRoles.some(r => roles.value.includes(r))
  }

  async function login(credentials: LoginRequest) {
    const api = useApi()
    const response = await api.post<{ accessToken: string; refreshToken: string; expiresAt: string }>(
      API.auth.login,
      credentials,
    )

    if (response.success) {
      api.setTokens(response.data.accessToken, response.data.refreshToken, response.data.expiresAt)
      await fetchProfile()
    }

    return response
  }

  async function fetchProfile() {
    const api = useApi()
    try {
      const response = await api.get<UserProfile>(API.auth.me)
      if (response.success) {
        user.value = response.data
      }
    }
    catch {
      user.value = null
    }
  }

  async function logout() {
    const api = useApi()
    try {
      await api.post(API.auth.logout)
    }
    catch {
      // Logout even if API call fails
    }
    finally {
      user.value = null
      api.clearTokens()
      navigateTo('/login')
    }
  }

  async function initialize() {
    const token = localStorage.getItem(STORAGE_KEYS.accessToken)
    if (token) {
      await fetchProfile()
    }
  }

  return {
    user,
    isAuthenticated,
    roles,
    hasRole,
    hasAnyRole,
    login,
    logout,
    fetchProfile,
    initialize,
  }
})
