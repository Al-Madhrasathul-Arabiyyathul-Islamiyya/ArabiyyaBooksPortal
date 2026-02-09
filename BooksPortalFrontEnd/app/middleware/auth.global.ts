export default defineNuxtRouteMiddleware(async (to) => {
  if (to.path === '/login') return

  const authStore = useAuthStore()
  await authStore.initialize()

  if (!authStore.isAuthenticated) {
    return navigateTo('/login')
  }
})
