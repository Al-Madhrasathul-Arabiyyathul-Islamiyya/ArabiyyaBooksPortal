import { useMutation, useQuery } from '@pinia/colada'
import type { UserProfile } from '~/types/entities'
import type { LoginRequest } from '~/types/forms'
import { API } from '~/utils/constants'

export const useAuthStore = defineStore('auth', () => {
  const api = useApi()

  const user = ref<UserProfile | null>(null)
  const initialized = ref(false)

  const profileQuery = useQuery({
    key: () => ['auth', 'me'],
    enabled: false,
    query: async () => {
      const response = await api.get<UserProfile>(API.auth.me)
      if (!response.success) {
        throw new Error(response.message ?? 'Failed to fetch user profile')
      }
      return response.data
    },
  })

  const loginMutation = useMutation({
    mutation: (credentials: LoginRequest) =>
      api.post<{ expiresAt: string }>(API.auth.login, credentials),
  })

  const logoutMutation = useMutation({
    mutation: () => api.post(API.auth.logout),
  })

  const isAuthenticated = computed(() => !!user.value)
  const roles = computed(() => user.value?.roles ?? [])
  const isLoading = computed(() =>
    profileQuery.isPending.value
    || loginMutation.isLoading.value
    || logoutMutation.isLoading.value,
  )
  const error = computed(() =>
    profileQuery.error.value
    ?? loginMutation.error.value
    ?? logoutMutation.error.value
    ?? null,
  )

  function hasRole(role: string): boolean {
    return roles.value.includes(role)
  }

  function hasAnyRole(...checkRoles: string[]): boolean {
    return checkRoles.some(role => roles.value.includes(role))
  }

  async function fetchProfile() {
    try {
      const result = await profileQuery.refetch()
      if (result.status === 'success') {
        user.value = result.data
        return
      }
      user.value = null
    }
    catch {
      user.value = null
    }
    finally {
      initialized.value = true
    }
  }

  async function login(credentials: LoginRequest) {
    const response = await loginMutation.mutateAsync(credentials)
    if (response.success) {
      await fetchProfile()
    }
    return response
  }

  async function logout() {
    try {
      await logoutMutation.mutateAsync()
    }
    catch {
      // Always clear local state even if API logout fails.
    }
    finally {
      user.value = null
      initialized.value = true
      navigateTo('/login')
    }
  }

  async function initialize() {
    if (initialized.value) {
      return
    }
    await fetchProfile()
  }

  return {
    user,
    initialized,
    isAuthenticated,
    roles,
    isLoading,
    error,
    hasRole,
    hasAnyRole,
    login,
    logout,
    fetchProfile,
    initialize,
  }
})
