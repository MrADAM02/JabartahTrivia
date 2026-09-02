<script setup lang="ts">
import type { TeamSetupInput } from '~/types/api'
import { TEAM_COLORS, TEAM_ICONS } from '~/constants/teamCustomization'

const props = defineProps<{
  modelValue: TeamSetupInput
  label: string
  defaultIndex: number
}>()

const emit = defineEmits<{ 'update:modelValue': [TeamSetupInput] }>()

onMounted(() => {
  if (!props.modelValue.color || !props.modelValue.icon) {
    emit('update:modelValue', {
      ...props.modelValue,
      color: props.modelValue.color ?? TEAM_COLORS[props.defaultIndex % TEAM_COLORS.length]!,
      icon: props.modelValue.icon ?? TEAM_ICONS[props.defaultIndex % TEAM_ICONS.length]!
    })
  }
})

function updateName(name: string | number) {
  emit('update:modelValue', { ...props.modelValue, name: String(name) })
}
function selectColor(color: string) {
  emit('update:modelValue', { ...props.modelValue, color })
}
function selectIcon(icon: string) {
  emit('update:modelValue', { ...props.modelValue, icon })
}
function swatchStyle(color: string) {
  return {
    backgroundColor: color,
    boxShadow: props.modelValue.color === color ? `0 0 0 2px white, 0 0 0 4px ${color}` : 'none'
  }
}
</script>

<template>
  <div class="space-y-3 rounded-xl ring-1 ring-green-100 dark:ring-gray-800 p-4">
    <UInput
      :model-value="modelValue.name"
      size="lg"
      class="w-full"
      :placeholder="label"
      @update:model-value="updateName"
    />

    <div>
      <p class="text-xs font-bold text-muted mb-2">
        اللون
      </p>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="color in TEAM_COLORS"
          :key="color"
          type="button"
          class="size-8 rounded-full flex items-center justify-center"
          :style="swatchStyle(color)"
          @click="selectColor(color)"
        >
          <UIcon
            v-if="modelValue.color === color"
            name="i-lucide-check"
            class="size-4 text-white"
          />
        </button>
      </div>
    </div>

    <div>
      <p class="text-xs font-bold text-muted mb-2">
        الأيقونة
      </p>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="icon in TEAM_ICONS"
          :key="icon"
          type="button"
          class="size-9 rounded-lg flex items-center justify-center ring-1 transition-colors"
          :class="modelValue.icon === icon ? 'ring-2 ring-primary bg-primary/10 text-primary' : 'ring-green-100 dark:ring-gray-800 text-muted'"
          @click="selectIcon(icon)"
        >
          <UIcon
            :name="icon"
            class="size-5"
          />
        </button>
      </div>
    </div>
  </div>
</template>
