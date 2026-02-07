export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/login') return

  const token = useCookie('bp_access_token')
  if (!token.value) {
    return navigateTo('/login')
  }
})
