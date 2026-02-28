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
})
