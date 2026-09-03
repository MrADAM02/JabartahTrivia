<script setup lang="ts">
import type { Top100CategoryDto, TeamSetupInput } from '~/types/api'

const { listTop100Categories, createTop100GameSession } = useApi()

const teams = ref<TeamSetupInput[]>([
  { name: 'فريق ١', color: null, icon: null },
  { name: 'فريق ٢', color: null, icon: null }
])
const categories = ref<Top100CategoryDto[]>([])
const selectedCategoryIds = ref<string[]>([])
const guessesOptions = [3, 4, 5, 6, 7, 8, 9, 10]
const guessesIndex = ref(2) // defaults to 5
const guessesPerTeam = computed(() => guessesOptions[guessesIndex.value]!)
const loading = ref(false)
const categoriesLoading = ref(true)
const errorMessage = ref('')

const steps = [
  { title: 'اختر القائمة', description: 'اختر فئة أو أكثر لقوائم التحدي' },
  { title: 'كوّن فريقك', description: 'سمّ فريقيك واختر لون وأيقونة كل فريق' },
  { title: 'خمّن واكسب', description: 'كل فريق يحصل على عدد محدد من الإجابات، والعنصر الصحيح يمنحك نقاطاً بعدد ترتيبه' }
]

onMounted(async () => {
  try {
    categories.value = await listTop100Categories()
  } catch {
    errorMessage.value = 'تعذر تحميل الفئات. تأكد من تشغيل الخادم.'
  } finally {
    categoriesLoading.value = false
  }
})

function toggleCategory(id: string) {
  const index = selectedCategoryIds.value.indexOf(id)
  if (index === -1) selectedCategoryIds.value.push(id)
  else selectedCategoryIds.value.splice(index, 1)
}

const canStart = computed(
  () =>
    teams.value.every(t => t.name.trim().length > 0)
    && selectedCategoryIds.value.length > 0
    && !loading.value
)

async function startGame() {
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await createTop100GameSession(teams.value, selectedCategoryIds.value, guessesPerTeam.value)
    await navigateTo(`/top100/game/${result.top100GameSessionId}`)
  } catch {
    errorMessage.value = 'تعذر إنشاء الجلسة. تأكد من اختيار فئات كافية.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <section class="bg-green-900 text-white text-center py-10 px-4">
      <h1 class="text-3xl sm:text-4xl font-black">
        💯 تحدي الـ100
      </h1>
      <p class="text-white/80 mt-1">
        تناوبوا في تخمين عناصر القائمة، وكل عنصر يستحق نقاط بحسب ترتيبه
      </p>
    </section>

    <HowToPlaySteps
      title="كيف تلعب تحدي الـ100؟"
      :steps="steps"
    />

    <div class="max-w-3xl mx-auto px-4 sm:px-6 pb-14 space-y-8">
      <section class="space-y-3">
        <h2 class="text-lg font-bold text-green-900 dark:text-green-100">
          اختر الفئات
        </h2>
        <CategoryPickerGrid
          :categories="categories"
          :selected-ids="selectedCategoryIds"
          :max="Infinity"
          :loading="categoriesLoading"
          @toggle="toggleCategory"
        />
      </section>

      <UCard>
        <template #header>
          <h2 class="text-lg font-bold text-center text-green-900 dark:text-green-100">
            إعداد اللعبة
          </h2>
        </template>

        <div class="space-y-6">
          <section class="text-center">
            <p class="text-sm font-bold text-muted mb-2">
              عدد الإجابات لكل فريق
            </p>
            <div class="flex items-center justify-center gap-4">
              <UButton
                icon="i-lucide-minus"
                color="neutral"
                variant="outline"
                :disabled="guessesIndex === 0"
                @click="guessesIndex--"
              />
              <span class="text-2xl font-black text-primary w-10 text-center">{{ guessesPerTeam }}</span>
              <UButton
                icon="i-lucide-plus"
                color="neutral"
                variant="outline"
                :disabled="guessesIndex === guessesOptions.length - 1"
                @click="guessesIndex++"
              />
            </div>
            <p class="text-xs text-muted mt-1">
              من 3 إلى 10 إجابات لكل فريق
            </p>
          </section>

          <TeamSetupCard
            v-model="teams[0]!"
            label="اسم الفريق الأول"
            :default-index="0"
          />
          <TeamSetupCard
            v-model="teams[1]!"
            label="اسم الفريق الثاني"
            :default-index="1"
          />

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
  </div>
</template>
