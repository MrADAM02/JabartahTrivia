<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import { DURATIONS, EASINGS } from '~/utils/motion'

// direction describes the direction of travel as the element appears.
// 'start'/'end' are RTL-logical (this app is RTL-only): 'start' enters from
// the right edge, 'end' enters from the left edge — never hardcode left/right
// directly, translateX's sign is what actually flips between them.
const props = withDefaults(defineProps<{
  show: boolean
  direction?: 'up' | 'down' | 'start' | 'end'
  distance?: number
  duration?: number
  delay?: number
}>(), {
  direction: 'up',
  distance: 24,
  duration: DURATIONS.base,
  delay: 0
})

const { prefersReducedMotion } = useReducedMotion()

const offset = computed(() => {
  switch (props.direction) {
    case 'up': return { y: props.distance }
    case 'down': return { y: -props.distance }
    case 'start': return { x: props.distance }
    case 'end': return { x: -props.distance }
    default: return { y: props.distance }
  }
})

const transition = computed(() => ({
  duration: prefersReducedMotion.value ? DURATIONS.instant : props.duration,
  delay: prefersReducedMotion.value ? 0 : props.delay,
  ease: EASINGS.decelerate
}))
</script>

<template>
  <AnimatePresence>
    <motion.div
      v-if="show"
      :initial="{ ...offset, opacity: 0 }"
      :animate="{ x: 0, y: 0, opacity: 1 }"
      :exit="{ ...offset, opacity: 0 }"
      :transition="transition"
    >
      <slot />
    </motion.div>
  </AnimatePresence>
</template>
