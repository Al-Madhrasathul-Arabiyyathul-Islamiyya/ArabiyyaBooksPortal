import type { UserProfile } from '~/types/entities'
import type { LoginRequest } from '~/types/forms'
import { API } from '~/utils/constants'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null)
  const initialized = ref(false)
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
    const response = await api.post<{ expiresAt: string }>(
      API.auth.login,
      credentials,
    )

    if (response.success) {
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
      else {
        user.value = null
      }
    }
    catch {
      user.value = null
    }
    finally {
      initialized.value = true
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
      initialized.value = true
      navigateTo('/login')
    }
  }

  async function initialize() {
    if (initialized.value) return
    await fetchProfile()
  }

  return {
    user,
    initialized,
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
