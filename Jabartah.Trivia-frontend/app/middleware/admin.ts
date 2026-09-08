// Assumes the `auth` middleware has already run (always list it first in
// definePageMeta({ middleware: ['auth', 'admin'] })) so a session is already restored here.
export default defineNuxtRouteMiddleware(() => {
  const { user } = useAuth()
  if (user.value?.role !== 'Admin') {
    return navigateTo('/')
  }
})
