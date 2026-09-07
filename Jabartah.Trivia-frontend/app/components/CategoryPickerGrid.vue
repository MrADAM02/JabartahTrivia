<script setup lang="ts">
import { categoryColor } from '~/constants/categoryColors'

interface PickerCategory {
  id: string
  name: string
  icon: string | null
}

const props = withDefaults(defineProps<{
  categories: PickerCategory[]
  selectedIds: string[]
  max: number
  emptyText?: string
  loading?: boolean
  variant?: 'grid' | 'list'
  // 'toggle' (default): click adds/removes, other tiles disable at max.
  // 'radio': exactly one selection, clicking any tile always selects it
  // directly (no need to deselect first) -- used by Top100.
  selectionMode?: 'toggle' | 'radio'
}>(), {
  variant: 'grid',
  selectionMode: 'toggle'
})

const emit = defineEmits<{ toggle: [id: string] }>()

function badgeStyle(id: string) {
  const color = categoryColor(id)
  return {
    backgroundColor: `color-mix(in srgb, ${color} 18%, transparent)`,
    color
  }
}
</script>

<template>
  <div
    v-if="props.loading"
    class="grid gap-2 sm:gap-3"
    :class="props.variant === 'list' ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-3 sm:grid-cols-4 md:grid-cols-6'"
  >
    <div
      v-for="i in props.variant === 'list' ? 4 : 6"
      :key="i"
      class="flex flex-col items-center gap-2 rounded-xl ring-1 ring-green-100 dark:ring-gray-800"
      :class="props.variant === 'list' ? 'p-4' : 'p-3'"
    >
      <USkeleton :class="props.variant === 'list' ? 'size-12 rounded-full' : 'size-10 rounded-full'" />
      <USkeleton class="h-3 w-full" />
      <USkeleton
        v-if="props.variant === 'list'"
        class="h-5 w-16 rounded-full"
      />
    </div>
  </div>
  <div
    v-else-if="props.categories.length === 0 && props.emptyText"
    class="text-sm text-muted py-4 text-center"
  >
    {{ props.emptyText }}
  </div>
  <div
    v-else
    class="grid gap-2 sm:gap-3"
    :class="props.variant === 'list' ? 'grid-cols-1 sm:grid-cols-2' : 'grid-cols-3 sm:grid-cols-4 md:grid-cols-6'"
  >
    <button
      v-for="category in props.categories"
      :key="category.id"
      type="button"
      class="relative flex flex-col items-center gap-1 rounded-xl ring-1 transition-[box-shadow,background-color] text-center"
      :class="[
        props.variant === 'list' ? 'p-4 gap-2' : 'p-3',
        props.selectedIds.includes(category.id)
          ? 'ring-2 ring-primary bg-primary/10'
          : 'ring-green-100 dark:ring-gray-800 hover:ring-primary/50'
      ]"
      :disabled="props.selectionMode === 'toggle' && props.selectedIds.length >= props.max && !props.selectedIds.includes(category.id)"
      @click="emit('toggle', category.id)"
    >
      <span
        v-if="props.selectedIds.includes(category.id)"
        class="absolute top-2 inset-e-2 size-5 rounded-full bg-primary text-white ring-2 ring-white dark:ring-gray-900 flex items-center justify-center"
      >
        <UIcon
          name="i-lucide-check"
          class="size-3"
        />
      </span>

      <span
        class="rounded-full flex items-center justify-center shrink-0"
        :class="props.variant === 'list' ? 'size-12 sm:size-14 text-2xl' : 'size-10 sm:size-12 text-xl'"
        :style="badgeStyle(category.id)"
      >
        {{ category.icon ?? '📚' }}
      </span>
      <span
        class="font-bold text-center line-clamp-2"
        :class="props.variant === 'list' ? 'text-sm' : 'text-xs'"
      >
        {{ category.name }}
      </span>

      <slot
        name="extra"
        :category="category"
      />
    </button>
  </div>
</template>
