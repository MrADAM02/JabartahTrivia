<script setup lang="ts">
import { ar } from '@nuxt/ui/locale'

useAuth().restore()

useHead({
  htmlAttrs: {
    lang: 'ar',
    dir: 'rtl'
  },
  link: [
    { rel: 'icon', type: 'image/svg+xml', href: '/logo-icon.svg' },
    { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
    { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
    { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;900&display=swap' }
  ]
})

useSeoMeta({
  title: 'جولة - لعبة مسابقات جماعية',
  description: 'لعبة مسابقات جماعية للعائلة والأصدقاء'
})
</script>

<template>
  <UApp :locale="ar">
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </UApp>
</template>
