<script setup lang="ts">
import type { CategoryDto } from '~/types/api'

const { listCategories, listMyCategories, createGameSession } = useApi()
const { isLoggedIn } = useAuth()

const teamNames = ref(['فريق ١', 'فريق ٢'])
const categories = ref<CategoryDto[]>([])
const myCategories = ref<CategoryDto[]>([])
const selectedCategoryIds = ref<string[]>([])
const activeTab = ref<'shared' | 'mine'>('shared')
const loading = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  try {
    categories.value = await listCategories()
  } catch {
    errorMessage.value = 'تعذر تحميل الفئات. تأكد من تشغيل الخادم.'
  }
  if (isLoggedIn.value) {
    try {
      myCategories.value = await listMyCategories()
    } catch {
      // تصنيفاتي is a bonus tab -- a failure here shouldn't block the shared-categories flow
    }
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
  <div class="min-h-[70vh] flex items-center justify-center p-4 sm:p-8">
    <UCard class="w-full max-w-2xl">
      <template #header>
        <h1 class="text-3xl sm:text-4xl font-black text-center text-green-900 dark:text-green-100">
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

          <div class="flex gap-2 border-b border-green-100 dark:border-gray-800">
            <button
              class="px-3 py-2 text-sm font-bold border-b-2 -mb-px"
              :class="activeTab === 'shared' ? 'border-primary text-primary' : 'border-transparent text-muted'"
              @click="activeTab = 'shared'"
            >
              الفئات
            </button>
            <button
              v-if="isLoggedIn"
              class="px-3 py-2 text-sm font-bold border-b-2 -mb-px"
              :class="activeTab === 'mine' ? 'border-primary text-primary' : 'border-transparent text-muted'"
              @click="activeTab = 'mine'"
            >
              تصنيفاتي
            </button>
          </div>

          <CategoryPickerGrid
            v-if="activeTab === 'shared'"
            :categories="categories"
            :selected-ids="selectedCategoryIds"
            :max="6"
            @toggle="toggleCategory"
          />
          <div v-else>
            <CategoryPickerGrid
              :categories="myCategories"
              :selected-ids="selectedCategoryIds"
              :max="6"
              empty-text="لا توجد تصنيفات بعد — أنشئ تصنيفك الأول من صفحة تصنيفاتي"
              @toggle="toggleCategory"
            />
            <NuxtLink
              to="/my-categories/create"
              class="inline-block mt-2 text-sm text-primary font-bold"
            >
              + إنشاء تصنيف جديد
            </NuxtLink>
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
          color="secondary"
          class="font-bold text-green-950"
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
