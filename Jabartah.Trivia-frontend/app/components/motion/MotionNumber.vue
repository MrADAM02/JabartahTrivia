<script setup lang="ts">
import { animate } from 'motion-v'
import { DURATIONS, EASINGS } from '~/utils/motion'

const props = withDefaults(defineProps<{
  value: number
  duration?: number
  formatter?: (n: number) => string
}>(), {
  duration: DURATIONS.slow
})

const { prefersReducedMotion } = useReducedMotion()
const displayValue = ref(props.value)
let controls: ReturnType<typeof animate> | null = null

watch(() => props.value, (next) => {
  controls?.stop()
  if (prefersReducedMotion.value) {
    displayValue.value = next
    return
  }
  controls = animate(displayValue.value, next, {
    duration: props.duration,
    ease: EASINGS.decelerate,
    onUpdate: (latest) => { displayValue.value = latest }
  })
})

onUnmounted(() => controls?.stop())

const displayText = computed(() => {
  const rounded = Math.round(displayValue.value)
  return props.formatter ? props.formatter(rounded) : String(rounded)
})
</script>

<template>
  <span>{{ displayText }}</span>
</template>
