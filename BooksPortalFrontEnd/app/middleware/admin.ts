import { ROLES } from '~/utils/constants'

export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore()

  if (!authStore.hasAnyRole(ROLES.superAdmin, ROLES.admin)) {
    return navigateTo('/')
  }
})
