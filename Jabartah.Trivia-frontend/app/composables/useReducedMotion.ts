export function useReducedMotion() {
  const query = window.matchMedia('(prefers-reduced-motion: reduce)')
  const prefersReducedMotion = ref(query.matches)

  const handleChange = (event: MediaQueryListEvent) => {
    prefersReducedMotion.value = event.matches
  }

  onMounted(() => query.addEventListener('change', handleChange))
  onUnmounted(() => query.removeEventListener('change', handleChange))

  return { prefersReducedMotion }
}
