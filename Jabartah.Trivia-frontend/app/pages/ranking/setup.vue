<script setup lang="ts">
import type { RankingCategoryDto } from '~/types/api'

const { listRankingCategories, createRankingGameSession } = useApi()

const teamNames = ref(['فريق ١', 'فريق ٢'])
const categories = ref<RankingCategoryDto[]>([])
const selectedCategoryIds = ref<string[]>([])
const roundsPerTeam = ref(2)
const roundsOptions = [2, 4, 6]
const loading = ref(false)
const errorMessage = ref('')

onMounted(async () => {
  try {
    categories.value = await listRankingCategories()
  } catch {
    errorMessage.value = 'تعذر تحميل الفئات. تأكد من تشغيل الخادم.'
  }
})

function toggleCategory(id: string) {
  const index = selectedCategoryIds.value.indexOf(id)
  if (index === -1) selectedCategoryIds.value.push(id)
  else selectedCategoryIds.value.splice(index, 1)
}

const canStart = computed(
  () =>
    teamNames.value.every(name => name.trim().length > 0)
    && selectedCategoryIds.value.length > 0
    && !loading.value
)

async function startGame() {
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await createRankingGameSession(teamNames.value, selectedCategoryIds.value, roundsPerTeam.value)
    await navigateTo(`/ranking/game/${result.rankingGameSessionId}`)
  } catch {
    errorMessage.value = 'تعذر إنشاء الجلسة. تأكد من اختيار فئات كافية.'
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
          رتبها
        </h1>
        <p class="text-center text-muted mt-1">
          رتب البطاقات بالترتيب الصحيح قبل الوقت
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
          <h2 class="text-lg font-bold">
            اختر الفئات
          </h2>
          <div class="flex flex-wrap gap-2">
            <UButton
              v-for="category in categories"
              :key="category.id"
              :color="selectedCategoryIds.includes(category.id) ? 'primary' : 'neutral'"
              :variant="selectedCategoryIds.includes(category.id) ? 'solid' : 'outline'"
              size="lg"
              @click="toggleCategory(category.id)"
            >
              <span v-if="category.icon">{{ category.icon }}</span>
              {{ category.name }}
            </UButton>
          </div>
        </section>

        <section class="space-y-3">
          <h2 class="text-lg font-bold">
            عدد الجولات لكل فريق
          </h2>
          <div class="flex flex-wrap gap-2">
            <UButton
              v-for="option in roundsOptions"
              :key="option"
              :color="roundsPerTeam === option ? 'primary' : 'neutral'"
              :variant="roundsPerTeam === option ? 'solid' : 'outline'"
              size="lg"
              @click="roundsPerTeam = option"
            >
              {{ option }} لكل فريق
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
