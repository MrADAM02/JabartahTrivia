<script setup lang="ts">
const props = defineProps<{
  selected: number
  max: number | null // null = uncapped (Password/Ranking)
}>()

const text = computed(() =>
  props.max === null ? `${props.selected} مختارة` : `${props.selected} من ${props.max}`
)
</script>

<template>
  <UBadge
    color="primary"
    variant="subtle"
    class="font-bold"
  >
    {{ text }}
  </UBadge>
</template>
