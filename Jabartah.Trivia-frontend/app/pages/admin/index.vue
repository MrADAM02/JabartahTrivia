<script setup lang="ts">
import type { DashboardStatsDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })
useSeoMeta({ title: 'لوحة التحكم - جولة' })

const { getDashboardStats } = useAdminApi()

const stats = ref<DashboardStatsDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

onMounted(async () => {
  try {
    stats.value = await getDashboardStats()
  } catch {
    errorMessage.value = 'تعذر تحميل الإحصائيات.'
  } finally {
    loading.value = false
  }
})

const completionRate = computed(() => {
  if (!stats.value || stats.value.totalGames === 0) return 0
  return Math.round((stats.value.completedGames / stats.value.totalGames) * 100)
})

const modeBars = computed(() => {
  if (!stats.value) return []
  const modes = [
    { label: 'التحدي (Trivia)', value: stats.value.trivia.total },
    { label: 'كلمة السر', value: stats.value.password.total },
    { label: 'رتبها', value: stats.value.ranking.total },
    { label: 'تحدي الـ100', value: stats.value.top100.total }
  ]
  const max = Math.max(1, ...modes.map(m => m.value))
  return modes.map(m => ({ ...m, percent: Math.round((m.value / max) * 100) }))
})
</script>

<template>
  <div class="space-y-8 max-w-5xl">
    <div>
      <h1 class="text-2xl font-black text-green-900 dark:text-green-100">
        الإحصائيات
      </h1>
      <p class="text-muted">
        نظرة عامة على استخدام التطبيق
      </p>
    </div>

    <UAlert
      v-if="errorMessage"
      color="error"
      variant="subtle"
      :title="errorMessage"
    />

    <div
      v-if="loading"
      class="grid grid-cols-1 sm:grid-cols-3 gap-4"
    >
      <UCard
        v-for="i in 3"
        :key="i"
      >
        <USkeleton class="h-16" />
      </UCard>
    </div>

    <template v-else-if="stats">
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <UCard>
          <p class="text-4xl font-black text-green-900 dark:text-green-100 tabular-nums">
            {{ stats.totalGames }}
          </p>
          <p class="text-muted font-bold">
            إجمالي الألعاب
          </p>
        </UCard>
        <UCard>
          <p class="text-4xl font-black text-green-900 dark:text-green-100 tabular-nums">
            {{ stats.totalUsers }}
          </p>
          <p class="text-muted font-bold">
            مستخدم مسجل
          </p>
        </UCard>
        <UCard>
          <p class="text-4xl font-black text-green-900 dark:text-green-100 tabular-nums">
            {{ completionRate }}٪
          </p>
          <p class="text-muted font-bold">
            نسبة إتمام الألعاب
          </p>
        </UCard>
      </div>

      <UCard>
        <template #header>
          <p class="font-bold">
            الألعاب حسب النوع
          </p>
        </template>
        <div class="space-y-3">
          <div
            v-for="mode in modeBars"
            :key="mode.label"
            class="flex items-center gap-3"
          >
            <span class="w-32 shrink-0 text-sm font-bold">{{ mode.label }}</span>
            <div class="flex-1 h-3 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
              <div
                class="h-full rounded-full bg-secondary"
                :style="{ width: `${mode.percent}%` }"
              />
            </div>
            <span class="w-8 text-end text-sm font-bold tabular-nums">{{ mode.value }}</span>
          </div>
        </div>
      </UCard>

      <UCard>
        <template #header>
          <p class="font-bold">
            الفئات الأكثر لعباً
          </p>
        </template>
        <ul
          v-if="stats.topCategories.length"
          class="divide-y divide-gray-100 dark:divide-gray-800"
        >
          <li
            v-for="(category, index) in stats.topCategories"
            :key="category.categoryId"
            class="flex items-center gap-3 py-2.5"
          >
            <span class="w-6 text-muted font-bold tabular-nums">{{ index + 1 }}</span>
            <span class="text-xl">{{ category.icon ?? '📚' }}</span>
            <span class="flex-1 font-bold">{{ category.name }}</span>
            <span class="text-muted text-sm">{{ category.timesPlayed }} لعبة</span>
          </li>
        </ul>
        <p
          v-else
          class="text-muted text-sm"
        >
          لا توجد بيانات كافية بعد.
        </p>
      </UCard>
    </template>
  </div>
</template>
