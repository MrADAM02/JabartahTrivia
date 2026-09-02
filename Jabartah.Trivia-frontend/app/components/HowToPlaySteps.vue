<script setup lang="ts">
defineProps<{
  title: string
  steps: { title: string, description: string }[]
}>()
</script>

<template>
  <section class="max-w-4xl mx-auto px-4 sm:px-6 py-10">
    <h2 class="text-xl sm:text-2xl font-black text-center text-green-900 dark:text-green-100 mb-8">
      {{ title }}
    </h2>
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
      <div
        v-for="(step, i) in steps"
        :key="step.title"
        class="text-center space-y-2"
      >
        <div class="mx-auto size-10 rounded-full bg-gold-400 text-green-950 font-black flex items-center justify-center">
          {{ i + 1 }}
        </div>
        <h3 class="font-bold text-green-900 dark:text-green-100">
          {{ step.title }}
        </h3>
        <p class="text-muted text-sm">
          {{ step.description }}
        </p>
      </div>
    </div>
  </section>
</template>
