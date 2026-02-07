import { STORAGE_KEYS } from '~/utils/constants'

export default defineNuxtRouteMiddleware((to) => {
  if (to.path === '/login') return

  const token = localStorage.getItem(STORAGE_KEYS.accessToken)
  if (!token) {
    return navigateTo('/login')
  }
})
