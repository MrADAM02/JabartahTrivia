<script setup lang="ts">
import { motion } from 'motion-v'

// Purely decorative background layer for a bg-hero-atmosphere section --
// large blurred glow circles (+ a couple of glyph accents on the big hero
// only). 'compact' is for the 4 setup pages' thin hero bands: fewer shapes,
// no float, so a slim band doesn't feel busy.
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
    : [
        { class: 'w-[420px] h-[420px] -top-40 start-[6%]', delay: 0 },
        { class: 'w-80 h-80 top-1/3 end-[4%]', delay: 0.6 },
        { class: 'w-64 h-64 -bottom-16 start-1/3', delay: 1.1 }
      ]
)

const glyphs = computed(() =>
  props.size === 'compact'
    ? []
    : [
        { symbol: '✦', class: 'top-10 start-[18%] text-2xl', delay: 0.2 },
        { symbol: '✦', class: 'top-16 end-[16%] text-xl', delay: 0.9 }
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
    <motion.span
      v-for="(glyph, i) in glyphs"
      :key="`glyph-${i}`"
      class="absolute text-gold-400/25"
      :class="glyph.class"
      :animate="floatAnimate"
      :transition="floatTransition(glyph.delay)"
    >
      {{ glyph.symbol }}
    </motion.span>
  </div>
</template>
