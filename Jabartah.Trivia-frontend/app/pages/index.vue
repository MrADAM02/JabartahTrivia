<script setup lang="ts">
const modes = [
  { to: '/trivia/setup', icon: '🎯', title: 'لعبة الأسئلة', description: 'اختر فئة ونقطة، ومن يجيب أولاً يفوز بالنقاط' },
  { to: '/password/setup', icon: '🤫', title: 'كلمة السر', description: 'لمّح لفريقك بكلمة واحدة ليخمنوا الكلمة السرية' },
  { to: '/ranking/setup', icon: '🔢', title: 'رتبها', description: 'رتب البطاقات بالترتيب الصحيح قبل الوقت' }
]
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 sm:p-8 bg-linear-to-b from-primary-50 to-white dark:from-gray-950 dark:to-gray-900">
    <div class="w-full max-w-4xl space-y-8">
      <div class="text-center">
        <h1 class="text-4xl sm:text-5xl font-black text-primary">
          جولة
        </h1>
        <p class="text-muted mt-2">
          لعبة مسابقات جماعية للعائلة والأصدقاء
        </p>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <NuxtLink
          v-for="mode in modes"
          :key="mode.to"
          :to="mode.to"
          class="block"
        >
          <UCard class="h-full text-center hover:ring-2 hover:ring-primary transition-all cursor-pointer">
            <div class="text-5xl mb-3">
              {{ mode.icon }}
            </div>
            <h2 class="text-xl font-bold mb-2">
              {{ mode.title }}
            </h2>
            <p class="text-muted text-sm">
              {{ mode.description }}
            </p>
          </UCard>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>
