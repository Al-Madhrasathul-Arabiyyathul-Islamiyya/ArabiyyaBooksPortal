import { ROLES } from '~/utils/constants'

export default defineNuxtRouteMiddleware(async (to) => {
  if (to.path === '/login' || to.path === '/setup/bootstrap') return

  const authStore = useAuthStore()
  const setupStore = useSetupReadinessStore()
  await authStore.initialize()

  if (!authStore.isAuthenticated) {
    try {
      await setupStore.fetchStatus(true)
      if (setupStore.requiresBootstrap) {
        return navigateTo('/setup/bootstrap')
      }
    }
    catch {
      // Fall back to login when setup status cannot be resolved.
    }

    return navigateTo('/login')
  }

  try {
    await setupStore.fetchStatus(true)
    const isSuperAdmin = authStore.roles.includes(ROLES.superAdmin)
    const isSetupRoute = to.path === '/admin/settings/setup'
    if (isSuperAdmin && setupStore.isIncomplete && !isSetupRoute) {
      return navigateTo('/admin/settings/setup')
    }
  }
  catch {
    // Keep route navigation available if setup status cannot be resolved.
  }
})
