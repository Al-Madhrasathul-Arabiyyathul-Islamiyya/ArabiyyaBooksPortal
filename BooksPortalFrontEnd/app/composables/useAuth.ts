import { ROLES } from '~/utils/constants'

export function useAuth() {
  const authStore = useAuthStore()

  const isSuperAdmin = computed(() => authStore.hasRole(ROLES.superAdmin))
  const isAdmin = computed(() => authStore.hasAnyRole(ROLES.superAdmin, ROLES.admin))
  const isStaff = computed(() => authStore.hasAnyRole(ROLES.superAdmin, ROLES.admin, ROLES.staff))

  return {
    user: computed(() => authStore.user),
    isAuthenticated: computed(() => authStore.isAuthenticated),
    roles: computed(() => authStore.roles),
    isSuperAdmin,
    isAdmin,
    isStaff,
    hasRole: authStore.hasRole,
    hasAnyRole: authStore.hasAnyRole,
    login: authStore.login,
    logout: authStore.logout,
  }
}
