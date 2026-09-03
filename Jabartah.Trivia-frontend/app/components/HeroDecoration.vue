<script setup lang="ts">
import { motion } from 'motion-v'

// Purely decorative background layer for a bg-hero-atmosphere section.
// 'large' (index.vue's hero only) gets one very subtle dashed-circle
// "game route" motif plus a few star/dot accents. 'compact' is for the 4
// setup pages' thin hero bands: smaller floating glow circles, unchanged.
const props = withDefaults(defineProps<{
  size?: 'large' | 'compact'
}>(), {
  size: 'large'
})

const { prefersReducedMotion } = useReducedMotion()

const circles = computed(() =>
  props.size === 'compact'
    ? [
        { class: 'w-56 h-56 -top-24 -start-12', delay: 0 },
        { class: 'w-44 h-44 -bottom-20 -end-10', delay: 0.4 }
      ]
    : []
)

// Small static-position accents around the route motif -- only these get the
// gentle float, the big circle itself stays still so it never draws the eye.
const accents = computed(() =>
  props.size === 'compact'
    ? []
    : [
        { symbol: '✦', class: 'top-[18%] start-[20%] text-xl', opacity: 0.04, delay: 0 },
        { symbol: '✦', class: 'top-[22%] end-[18%] text-lg', opacity: 0.045, delay: 0.8 },
        { symbol: '●', class: 'bottom-[20%] start-[30%] text-xs', opacity: 0.035, delay: 1.4 }
      ]
)

const floatAnimate = computed(() => (prefersReducedMotion.value ? {} : { y: [0, -8, 0] }))

function floatTransition(delay: number) {
  return {
    duration: 5,
    delay,
    repeat: prefersReducedMotion.value ? 0 : Infinity,
    repeatType: 'mirror' as const,
    ease: 'easeInOut'
  }
}
</script>

<template>
  <div
    class="absolute inset-0 overflow-hidden pointer-events-none"
    aria-hidden="true"
  >
    <motion.div
      v-for="(circle, i) in circles"
      :key="`circle-${i}`"
      class="absolute rounded-full blur-2xl"
      :class="circle.class"
      style="background: radial-gradient(circle, color-mix(in srgb, var(--color-gold-400) 10%, transparent), transparent 70%)"
      :animate="floatAnimate"
      :transition="floatTransition(circle.delay)"
    />

    <svg
      v-if="size === 'large'"
      class="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 text-gold-400"
      style="width: min(65vw, 620px); height: min(65vw, 620px)"
      viewBox="0 0 200 200"
      fill="none"
    >
      <circle
        cx="100"
        cy="100"
        r="94"
        stroke="currentColor"
        stroke-width="1"
        stroke-dasharray="2 9"
        opacity="0.03"
      />
    </svg>

    <motion.span
      v-for="(accent, i) in accents"
      :key="`accent-${i}`"
      class="absolute text-gold-400"
      :class="accent.class"
      :style="{ opacity: accent.opacity }"
      :animate="floatAnimate"
      :transition="floatTransition(accent.delay)"
    >
      {{ accent.symbol }}
    </motion.span>
  </div>
</template>
