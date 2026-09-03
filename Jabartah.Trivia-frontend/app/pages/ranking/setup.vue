<script setup lang="ts">
import type { RankingCategoryDto, TeamSetupInput } from '~/types/api'

const { listRankingCategories, createRankingGameSession } = useApi()

const teams = ref<TeamSetupInput[]>([
  { name: 'فريق ١', color: null, icon: null },
  { name: 'فريق ٢', color: null, icon: null }
])
const categories = ref<RankingCategoryDto[]>([])
const selectedCategoryIds = ref<string[]>([])
const roundsOptions = [2, 4, 6]
const roundsIndex = ref(0)
const roundsPerTeam = computed(() => roundsOptions[roundsIndex.value]!)
const loading = ref(false)
const categoriesLoading = ref(true)
const errorMessage = ref('')

const steps = [
  { title: 'اختر الفئة', description: 'اختر فئة أو أكثر لقوائم الترتيب' },
  { title: 'كوّن فريقك', description: 'سمّ فريقيك واختر لون وأيقونة كل فريق' },
  { title: 'رتب البطاقات', description: 'رتبوا البطاقات المبعثرة قبل انتهاء الجولة' }
]

onMounted(async () => {
  try {
    categories.value = await listRankingCategories()
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
    const result = await createRankingGameSession(teams.value, selectedCategoryIds.value, roundsPerTeam.value)
    await navigateTo(`/ranking/game/${result.rankingGameSessionId}`)
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
        🔢 رتبها
      </h1>
      <p class="text-white/80 mt-1">
        رتب البطاقات بالترتيب الصحيح قبل الوقت
      </p>
    </section>

    <HowToPlaySteps
      title="كيف تلعب رتبها؟"
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
              عدد الجولات لكل فريق
            </p>
            <div class="flex items-center justify-center gap-4">
              <UButton
                icon="i-lucide-minus"
                color="neutral"
                variant="outline"
                :disabled="roundsIndex === 0"
                @click="roundsIndex--"
              />
              <span class="text-2xl font-black text-primary w-10 text-center">{{ roundsPerTeam }}</span>
              <UButton
                icon="i-lucide-plus"
                color="neutral"
                variant="outline"
                :disabled="roundsIndex === roundsOptions.length - 1"
                @click="roundsIndex++"
              />
            </div>
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
