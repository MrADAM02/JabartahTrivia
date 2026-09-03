<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import { DURATIONS, EASINGS } from '~/utils/motion'

const props = withDefaults(defineProps<{
  show: boolean
  duration?: number
  delay?: number
  fromScale?: number
}>(), {
  duration: DURATIONS.base,
  delay: 0,
  fromScale: 0.9
})

const { prefersReducedMotion } = useReducedMotion()

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
      :initial="{ opacity: 0, scale: fromScale }"
      :animate="{ opacity: 1, scale: 1 }"
      :exit="{ opacity: 0, scale: fromScale }"
      :transition="transition"
    >
      <slot />
    </motion.div>
  </AnimatePresence>
</template>
