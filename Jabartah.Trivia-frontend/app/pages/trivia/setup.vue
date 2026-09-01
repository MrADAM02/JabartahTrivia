<script setup lang="ts">
import type { CategoryDto } from '~/types/api'

const { listCategories, createGameSession } = useApi()

const teamNames = ref(['فريق ١', 'فريق ٢'])
const categories = ref<CategoryDto[]>([])
const selectedCategoryIds = ref<string[]>([])
const loading = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  try {
    categories.value = await listCategories()
  } catch {
    errorMessage.value = 'تعذر تحميل الفئات. تأكد من تشغيل الخادم.'
  }
})

function toggleCategory(id: string) {
  const index = selectedCategoryIds.value.indexOf(id)
  if (index !== -1) {
    selectedCategoryIds.value.splice(index, 1)
    return
  }
  if (selectedCategoryIds.value.length >= 6) return
  selectedCategoryIds.value.push(id)
}

const canStart = computed(
  () =>
    teamNames.value.every(name => name.trim().length > 0)
    && selectedCategoryIds.value.length === 6
    && !loading.value
)

async function startGame() {
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await createGameSession(
      teamNames.value,
      selectedCategoryIds.value
    )
    await navigateTo(`/trivia/game/${result.gameSessionId}`)
  } catch {
    errorMessage.value = 'تعذر إنشاء الجلسة. حاول مرة أخرى.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div
    class="min-h-screen flex items-center justify-center p-4 sm:p-8 bg-linear-to-b from-primary-50 to-white dark:from-gray-950 dark:to-gray-900"
  >
    <UCard class="w-full max-w-2xl">
      <template #header>
        <h1 class="text-3xl sm:text-4xl font-black text-center text-primary">
          لعبة الأسئلة
        </h1>
        <p class="text-center text-muted mt-1">
          اختر فئة ونقطة، ومن يجيب أولاً يفوز بالنقاط
        </p>
      </template>

      <div class="space-y-8">
        <section class="space-y-3">
          <h2 class="text-lg font-bold">
            الفرق
          </h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <UInput
              v-for="(_, i) in teamNames"
              :key="i"
              v-model="teamNames[i]"
              size="xl"
              :placeholder="`اسم الفريق ${i + 1}`"
            />
          </div>
        </section>

        <section class="space-y-3">
          <div class="flex items-center justify-between">
            <h2 class="text-lg font-bold">
              اختر الفئات
            </h2>
            <p class="text-sm text-muted">
              {{ selectedCategoryIds.length }} من 6
            </p>
          </div>
          <div class="flex flex-wrap gap-2">
            <UButton
              v-for="category in categories"
              :key="category.id"
              :color="
                selectedCategoryIds.includes(category.id)
                  ? 'primary'
                  : 'neutral'
              "
              :variant="
                selectedCategoryIds.includes(category.id) ? 'solid' : 'outline'
              "
              :disabled="selectedCategoryIds.length >= 6 && !selectedCategoryIds.includes(category.id)"
              size="lg"
              @click="toggleCategory(category.id)"
            >
              <span v-if="category.icon">{{ category.icon }}</span>
              {{ category.name }}
            </UButton>
          </div>
        </section>

        <UAlert
          v-if="errorMessage"
          color="error"
          variant="subtle"
          :title="errorMessage"
        />

        <UButton
          block
          size="xl"
          :loading="loading"
          :disabled="!canStart"
          @click="startGame"
        >
          ابدأ اللعبة
        </UButton>
      </div>
    </UCard>
  </div>
</template>
