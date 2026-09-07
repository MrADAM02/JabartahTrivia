<script setup lang="ts">
import { DURATIONS } from '~/utils/motion'

defineProps<{
  items: { id: string, name: string, icon: string | null }[]
}>()

const emit = defineEmits<{ remove: [id: string] }>()
</script>

<template>
  <MotionFade
    :show="items.length > 0"
    :duration="DURATIONS.fast"
  >
    <div class="flex flex-wrap gap-2">
      <UBadge
        v-for="item in items"
        :key="item.id"
        color="primary"
        variant="subtle"
        class="gap-1"
      >
        <template #default>
          <span v-if="item.icon">{{ item.icon }}</span>
          {{ item.name }}
        </template>
        <template #trailing>
          <button
            type="button"
            aria-label="إزالة"
            class="rounded-full hover:bg-black/10 dark:hover:bg-white/10 transition-colors"
            @click="emit('remove', item.id)"
          >
            <UIcon
              name="i-lucide-x"
              class="size-3"
            />
          </button>
        </template>
      </UBadge>
    </div>
  </MotionFade>
</template>
