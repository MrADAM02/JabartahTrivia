<script setup lang="ts">
import type { Top100CategoryDto, TeamSetupInput } from '~/types/api'

const { listTop100Categories, createTop100GameSession } = useApi()

const teams = ref<TeamSetupInput[]>([
  { name: 'فريق ١', color: null, icon: null },
  { name: 'فريق ٢', color: null, icon: null }
])
const categories = ref<Top100CategoryDto[]>([])
const selectedCategoryId = ref<string | null>(null)
const guessesOptions = [3, 4, 5, 6, 7, 8, 9, 10]
const guessesIndex = ref(2) // defaults to 5
const guessesPerTeam = computed(() => guessesOptions[guessesIndex.value]!)
const loading = ref(false)
const categoriesLoading = ref(true)
const errorMessage = ref('')

const infoModalOpen = ref(false)
const infoCategory = ref<Top100CategoryDto | null>(null)
function openInfo(category: Top100CategoryDto) {
  infoCategory.value = category
  infoModalOpen.value = true
}

// Radio semantics: clicking any card always selects it directly, no need
// to deselect the current one first (see CategoryPickerGrid's selectionMode).
function selectCategory(id: string) {
  selectedCategoryId.value = id
}

const steps = [
  { title: 'اختر القائمة', description: 'اختر قائمة واحدة من قوائم التحدي' },
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

const canStart = computed(
  () =>
    teams.value.every(t => t.name.trim().length > 0)
    && selectedCategoryId.value !== null
    && !loading.value
)

async function startGame() {
  if (!selectedCategoryId.value) return
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await createTop100GameSession(teams.value, [selectedCategoryId.value], guessesPerTeam.value)
    await navigateTo(`/top100/game/${result.top100GameSessionId}`)
  } catch {
    errorMessage.value = 'تعذر إنشاء الجلسة.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <section class="bg-hero-atmosphere text-white text-center py-10 px-4 relative overflow-hidden">
      <HeroDecoration size="compact" />
      <div class="relative">
        <h1 class="text-3xl sm:text-4xl font-black">
          💯 تحدي الـ100
        </h1>
        <p class="text-white/80 mt-1">
          تناوبوا في تخمين عناصر القائمة، وكل عنصر يستحق نقاط بحسب ترتيبه
        </p>
      </div>
    </section>

    <HowToPlaySteps
      title="كيف تلعب تحدي الـ100؟"
      :steps="steps"
    />

    <div class="max-w-3xl mx-auto px-4 sm:px-6 pb-14 space-y-8">
      <section class="space-y-3">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-bold text-green-900 dark:text-green-100">
            اختر القائمة
          </h2>
          <CategoryCountBadge
            :selected="selectedCategoryId ? 1 : 0"
            :max="1"
          />
        </div>

        <CategoryPickerGrid
          :categories="categories"
          :selected-ids="selectedCategoryId ? [selectedCategoryId] : []"
          :max="1"
          variant="list"
          selection-mode="radio"
          :loading="categoriesLoading"
          @toggle="selectCategory"
        >
          <template #extra="{ category }">
            <UBadge
              color="neutral"
              variant="subtle"
              class="font-bold"
            >
              {{ (category as Top100CategoryDto).itemCount }} عنصر
            </UBadge>
            <button
              type="button"
              class="absolute top-2 inset-s-2 size-7 rounded-full bg-black/5 dark:bg-white/10 flex items-center justify-center text-muted hover:text-primary transition-colors"
              aria-label="معلومات القائمة"
              @click.stop="openInfo(category as Top100CategoryDto)"
            >
              <UIcon
                name="i-lucide-info"
                class="size-4"
              />
            </button>
          </template>
        </CategoryPickerGrid>
      </section>

      <UCard>
        <template #header>
          <h2 class="text-lg font-bold text-center text-green-900 dark:text-green-100">
            إعداد اللعبة
          </h2>
        </template>

        <div class="space-y-6">
          <CategorySelectionPills
            v-if="selectedCategoryId"
            :items="categories.filter(c => c.id === selectedCategoryId)"
            @remove="selectedCategoryId = null"
          />

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

    <UModal v-model:open="infoModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold text-green-900 dark:text-green-100">
              {{ infoCategory?.icon }} {{ infoCategory?.name }}
            </p>
          </template>
          <p class="text-muted">
            {{ infoCategory?.description }}
          </p>
          <template #footer>
            <UButton
              block
              color="neutral"
              variant="outline"
              @click="infoModalOpen = false"
            >
              حسناً
            </UButton>
          </template>
        </UCard>
      </template>
    </UModal>
  </div>
</template>
