<script setup lang="ts">
import type { CategoryDto } from '~/types/api'

const props = defineProps<{
  categories: CategoryDto[]
  selectedIds: string[]
  max: number
  emptyText?: string
}>()

const emit = defineEmits<{ toggle: [id: string] }>()
</script>

<template>
  <div
    v-if="props.categories.length === 0 && props.emptyText"
    class="text-sm text-muted py-4 text-center"
  >
    {{ props.emptyText }}
  </div>
  <div
    v-else
    class="flex flex-wrap gap-2"
  >
    <UButton
      v-for="category in props.categories"
      :key="category.id"
      :color="props.selectedIds.includes(category.id) ? 'primary' : 'neutral'"
      :variant="props.selectedIds.includes(category.id) ? 'solid' : 'outline'"
      :disabled="props.selectedIds.length >= props.max && !props.selectedIds.includes(category.id)"
      size="lg"
      @click="emit('toggle', category.id)"
    >
      <span v-if="category.icon">{{ category.icon }}</span>
      {{ category.name }}
    </UButton>
  </div>
</template>
