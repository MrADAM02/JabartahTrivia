// Coarse gate for the one performance-risky effect (milestone celebration).
// Everything else in the motion system is transform/opacity-only and short
// enough to run at any tier, so this deliberately isn't a fine-grained scale.
export function useResponsiveMotion() {
  const { prefersReducedMotion } = useReducedMotion()
  const isSmallViewport = ref(window.innerWidth < 640)

  const handleResize = () => {
    isSmallViewport.value = window.innerWidth < 640
  }

  onMounted(() => window.addEventListener('resize', handleResize))
  onUnmounted(() => window.removeEventListener('resize', handleResize))

  const motionTier = computed<'full' | 'reduced'>(() =>
    prefersReducedMotion.value || isSmallViewport.value ? 'reduced' : 'full'
  )

  return { motionTier, prefersReducedMotion }
}
