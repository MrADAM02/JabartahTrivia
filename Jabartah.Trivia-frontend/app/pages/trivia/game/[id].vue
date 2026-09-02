<script setup lang="ts">
import type { BoardDto } from '~/types/api'

definePageMeta({ layout: false })

const route = useRoute()
const gameSessionId = route.params.id as string

const { getBoard, selectQuestion, awardPoints, revealAnswer } = useApi()

const board = ref<BoardDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const modalOpen = ref(false)
const activeQuestion = ref<{ questionId: string, pointValue: number, prompt: string, blockManualReveal: boolean } | null>(null)
const shownAnswer = ref<string | null>(null)
const awarded = ref(false)
const awarding = ref(false)
const revealing = ref(false)
const armedPowerUp = ref<{ teamId: string, type: 'DoublePoints' | 'TwoAnswers' } | null>(null)
const retryNote = ref<string | null>(null)

function togglePowerUp(teamId: string, type: 'DoublePoints' | 'TwoAnswers') {
  if (armedPowerUp.value?.teamId === teamId && armedPowerUp.value.type === type) {
    armedPowerUp.value = null
  } else {
    armedPowerUp.value = { teamId, type }
  }
}

async function loadBoard() {
  try {
    board.value = await getBoard(gameSessionId)
  } catch {
    errorMessage.value = 'تعذر تحميل اللعبة. تأكد من الرابط.'
  } finally {
    loading.value = false
  }
}

onMounted(loadBoard)

const allRevealed = computed(() =>
  !!board.value && board.value.categories.every(c => c.cells.every(cell => cell.isRevealed))
)

const winnerResult = computed(() => board.value ? getWinner(board.value.teams) : null)

async function openQuestion(questionId: string, pointValue: number) {
  const blockManualReveal = armedPowerUp.value?.type === 'TwoAnswers'
  try {
    const result = await selectQuestion(
      gameSessionId,
      questionId,
      armedPowerUp.value?.teamId ?? null,
      armedPowerUp.value?.type ?? null
    )
    armedPowerUp.value = null
    activeQuestion.value = { questionId, pointValue, prompt: result.prompt, blockManualReveal }
    shownAnswer.value = null
    awarded.value = false
    retryNote.value = null
    modalOpen.value = true
  } catch {
    errorMessage.value = 'تعذر فتح السؤال.'
  }
}

async function revealAnswerManually() {
  if (!activeQuestion.value) return
  revealing.value = true
  try {
    const result = await revealAnswer(gameSessionId, activeQuestion.value.questionId)
    shownAnswer.value = result.answer
  } catch {
    errorMessage.value = 'تعذر إظهار الإجابة.'
  } finally {
    revealing.value = false
  }
}

async function award(teamId: string | null) {
  if (!activeQuestion.value) return
  awarding.value = true
  try {
    const result = await awardPoints(gameSessionId, activeQuestion.value.questionId, teamId)
    if (result.canRetry) {
      retryNote.value = `لم يُحسب الجواب — محاولة أخيرة لفريق ${result.retryTeamName}`
    } else {
      shownAnswer.value = result.correctAnswer
      awarded.value = true
    }
    if (board.value) board.value.teams = result.teams
    await loadBoard()
  } catch {
    errorMessage.value = 'تعذر تسجيل النقاط.'
  } finally {
    awarding.value = false
  }
}

function closeModal() {
  modalOpen.value = false
  activeQuestion.value = null
  shownAnswer.value = null
  awarded.value = false
  retryNote.value = null
}
</script>

<template>
  <div class="min-h-screen bg-white dark:bg-gray-950 flex flex-col">
    <GameExitBar />
    <div class="p-3 sm:p-6 flex flex-col gap-4 flex-1">
      <div
        v-if="loading"
        class="flex-1 flex items-center justify-center"
      >
        <UIcon
          name="i-lucide-loader-circle"
          class="animate-spin size-10 text-primary"
        />
      </div>

      <UAlert
        v-else-if="errorMessage && !board"
        color="error"
        variant="subtle"
        :title="errorMessage"
      />

      <template v-else-if="board">
        <div
          v-if="allRevealed"
          class="flex-1 flex flex-col items-center justify-center gap-6 text-center"
        >
          <template v-if="winnerResult?.isDraw">
            <p class="text-2xl sm:text-3xl font-bold text-muted">
              🤝 تعادل
            </p>
            <h1 class="text-4xl sm:text-6xl font-black text-primary">
              {{ winnerResult.winners.map(w => w.name).join(' و ') }}
            </h1>
            <p class="text-3xl sm:text-4xl font-bold">
              {{ winnerResult.topScore }} نقطة
            </p>
          </template>
          <template v-else>
            <p class="text-2xl sm:text-3xl font-bold text-muted">
              🎉 الفائز 🎉
            </p>
            <h1 class="text-5xl sm:text-7xl font-black text-primary">
              {{ winnerResult?.winners[0]?.name }}
            </h1>
            <p class="text-3xl sm:text-4xl font-bold">
              {{ winnerResult?.winners[0]?.score }} نقطة
            </p>
          </template>
          <UButton
            size="xl"
            to="/"
          >
            لعبة جديدة
          </UButton>
        </div>

        <template v-else>
          <div class="flex flex-wrap justify-center gap-3 sm:gap-6">
            <UCard
              v-for="team in board.teams"
              :key="team.id"
              class="min-w-40 text-center"
            >
              <p class="font-bold text-lg truncate">
                {{ team.name }}
              </p>
              <p class="text-3xl sm:text-4xl font-black text-primary">
                {{ team.score }}
              </p>
              <div class="flex gap-1 justify-center mt-2">
                <UButton
                  :color="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'DoublePoints' ? 'primary' : 'neutral'"
                  :variant="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'DoublePoints' ? 'solid' : 'outline'"
                  :disabled="!team.doublePointsAvailable"
                  size="xs"
                  title="مضاعفة النقاط"
                  @click="togglePowerUp(team.id, 'DoublePoints')"
                >
                  💰
                </UButton>
                <UButton
                  :color="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'TwoAnswers' ? 'primary' : 'neutral'"
                  :variant="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'TwoAnswers' ? 'solid' : 'outline'"
                  :disabled="!team.twoAnswersAvailable"
                  size="xs"
                  title="محاولتين"
                  @click="togglePowerUp(team.id, 'TwoAnswers')"
                >
                  🔁
                </UButton>
              </div>
            </UCard>
          </div>

          <div
            class="flex-1 grid gap-2 sm:gap-3"
            :style="{ gridTemplateColumns: `repeat(${board.categories.length}, minmax(0, 1fr))` }"
          >
            <div
              v-for="category in board.categories"
              :key="category.categoryId"
              class="flex flex-col gap-2 sm:gap-3"
            >
              <div class="text-center font-bold text-sm sm:text-lg bg-primary/10 rounded-lg py-2 px-1 truncate">
                <span v-if="category.icon">{{ category.icon }}</span>
                {{ category.name }}
              </div>
              <UButton
                v-for="cell in category.cells"
                :key="cell.questionId"
                :disabled="cell.isRevealed"
                :color="cell.isRevealed ? 'neutral' : 'primary'"
                :variant="cell.isRevealed ? 'subtle' : 'solid'"
                size="xl"
                class="flex-1 justify-center text-lg sm:text-2xl font-black"
                @click="openQuestion(cell.questionId, cell.pointValue)"
              >
                {{ cell.isRevealed ? '✓' : cell.pointValue }}
              </UButton>
            </div>
          </div>
        </template>
      </template>

      <UModal
        v-model:open="modalOpen"
        :dismissible="false"
        :close="false"
      >
        <template #content>
          <UCard v-if="activeQuestion">
            <template #header>
              <p class="text-center font-bold text-primary text-lg">
                {{ activeQuestion.pointValue }} نقطة
              </p>
            </template>

            <p class="text-2xl sm:text-3xl font-bold text-center py-6">
              {{ activeQuestion.prompt }}
            </p>

            <p
              v-if="shownAnswer"
              class="text-xl sm:text-2xl font-bold text-center text-primary bg-primary/10 rounded-lg py-4 mb-4"
            >
              {{ shownAnswer }}
            </p>

            <p
              v-else-if="retryNote"
              class="text-lg font-bold text-center text-warning bg-warning/10 rounded-lg py-3 mb-4"
            >
              {{ retryNote }}
            </p>

            <div
              v-if="!awarded && !shownAnswer"
              class="text-center mb-4"
            >
              <UButton
                color="neutral"
                variant="outline"
                icon="i-lucide-eye"
                :loading="revealing"
                @click="revealAnswerManually"
              >
                إظهار الإجابة
              </UButton>
              <p
                v-if="activeQuestion?.blockManualReveal"
                class="text-xs text-muted mt-1"
              >
                يملك الفريق محاولتين قبل إظهار الإجابة
              </p>
            </div>

            <template #footer>
              <div
                v-if="!awarded"
                class="flex flex-wrap gap-2 justify-center"
              >
                <UButton
                  v-for="team in board?.teams"
                  :key="team.id"
                  :loading="awarding"
                  size="lg"
                  @click="award(team.id)"
                >
                  {{ team.name }} أجاب صح
                </UButton>
                <UButton
                  :loading="awarding"
                  color="neutral"
                  variant="outline"
                  size="lg"
                  @click="award(null)"
                >
                  لا أحد أجاب
                </UButton>
              </div>
              <UButton
                v-else
                block
                size="lg"
                @click="closeModal"
              >
                متابعة
              </UButton>
            </template>
          </UCard>
        </template>
      </UModal>
    </div>
  </div>
</template>
