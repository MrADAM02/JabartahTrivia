export default defineNuxtRouteMiddleware((to) => {
  const { isLoggedIn, restore } = useAuth()
  restore()
  if (!isLoggedIn.value) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
