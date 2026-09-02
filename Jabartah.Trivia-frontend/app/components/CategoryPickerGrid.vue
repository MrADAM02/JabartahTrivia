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
    class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-6 gap-2 sm:gap-3"
  >
    <button
      v-for="category in props.categories"
      :key="category.id"
      type="button"
      class="flex flex-col items-center gap-1 rounded-xl p-3 ring-1 transition-all"
      :class="props.selectedIds.includes(category.id)
        ? 'ring-2 ring-primary bg-primary/10'
        : 'ring-green-100 dark:ring-gray-800 hover:ring-primary/50'"
      :disabled="props.selectedIds.length >= props.max && !props.selectedIds.includes(category.id)"
      @click="emit('toggle', category.id)"
    >
      <span class="text-3xl">{{ category.icon ?? '📚' }}</span>
      <span class="text-xs font-bold text-center line-clamp-2">{{ category.name }}</span>
    </button>
  </div>
</template>
