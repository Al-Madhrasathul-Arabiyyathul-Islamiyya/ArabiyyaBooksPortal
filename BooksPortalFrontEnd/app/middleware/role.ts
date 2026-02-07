export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  const requiredRoles = to.meta.roles as string[] | undefined

  if (!requiredRoles || requiredRoles.length === 0) return

  const hasAccess = requiredRoles.some(role => authStore.hasRole(role))
  if (!hasAccess) {
    return navigateTo('/')
  }
})
