export interface AuthUser {
  id: string
  name: string
  email: string
}

const STORAGE_KEY = 'jawla:auth'

export function useAuth() {
  const token = useState<string | null>('auth:token', () => null)
  const user = useState<AuthUser | null>('auth:user', () => null)

  function setSession(newToken: string, newUser: AuthUser) {
    token.value = newToken
    user.value = newUser
    if (import.meta.client) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ token: newToken, user: newUser }))
    }
  }

  function clearSession() {
    token.value = null
    user.value = null
    if (import.meta.client) {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  function restore() {
    if (!import.meta.client || token.value) return
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return
    try {
      const parsed = JSON.parse(raw) as { token: string, user: AuthUser }
      token.value = parsed.token
      user.value = parsed.user
    } catch {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  return {
    token,
    user,
    isLoggedIn: computed(() => !!token.value),
    setSession,
    clearSession,
    restore
  }
}
