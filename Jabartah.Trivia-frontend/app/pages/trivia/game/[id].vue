<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import type { BoardDto } from '~/types/api'
import { DURATIONS, EASINGS, SPRINGS } from '~/utils/motion'

definePageMeta({ layout: false })

const ANSWER_SECONDS = 20
const COUNTDOWN_EMPHASIS_THRESHOLD = 5

const route = useRoute()
const gameSessionId = route.params.id as string

const { getBoard, selectQuestion, awardPoints, revealAnswer, activateTimerDebuff } = useApi()
const quizMotion = useQuizMotion()
const { motionTier } = useResponsiveMotion()

const board = ref<BoardDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const modalOpen = ref(false)
const activeQuestion = ref<{
  questionId: string
  pointValue: number
  prompt: string
  blockManualReveal: boolean
  turnTeamId: string
  turnTeamName: string
  timerDebuffed: boolean
} | null>(null)
const shownAnswer = ref<string | null>(null)
const awarded = ref(false)
const awarding = ref(false)
const revealing = ref(false)
const armedPowerUp = ref<{ teamId: string, type: 'DoublePoints' | 'TwoAnswers' } | null>(null)
const retryNote = ref<string | null>(null)

const secondsLeft = ref<number | null>(null)
let answerTimer: ReturnType<typeof setInterval> | undefined

function stopAnswerTimer() {
  clearInterval(answerTimer)
  secondsLeft.value = null
}

// Purely a visual cue -- the timer never decides the outcome on its own. It
// stops counting at 0 and stays red/emphasized, but the host still has to
// click a button (someone may answer verbally right as it hits zero).
function startAnswerTimer(seconds = ANSWER_SECONDS) {
  clearInterval(answerTimer)
  secondsLeft.value = seconds
  answerTimer = setInterval(() => {
    if (secondsLeft.value === null) return
    if (secondsLeft.value <= 0) {
      clearInterval(answerTimer)
      return
    }
    secondsLeft.value--
  }, 1000)
}

function togglePowerUp(teamId: string, type: 'DoublePoints' | 'TwoAnswers') {
  if (armedPowerUp.value?.teamId === teamId && armedPowerUp.value.type === type) {
    armedPowerUp.value = null
  } else {
    armedPowerUp.value = { teamId, type }
  }
}

const POWER_UP_LABELS: Record<'DoublePoints' | 'TwoAnswers', string> = {
  DoublePoints: '💰 مضاعفة النقاط',
  TwoAnswers: '🔁 محاولتين'
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

const leaderTeamId = computed(() => {
  if (!board.value || board.value.teams.length !== 2) return null
  const [a, b] = board.value.teams
  if (!a || !b || a.score === b.score) return null
  return a.score > b.score ? a.id : b.id
})

const revealedCount = computed(() =>
  board.value ? board.value.categories.reduce((sum, c) => sum + c.cells.filter(cell => cell.isRevealed).length, 0) : 0
)
const totalCellCount = computed(() =>
  board.value ? board.value.categories.reduce((sum, c) => sum + c.cells.length, 0) : 0
)
const progressPercent = computed(() =>
  totalCellCount.value === 0 ? 0 : Math.round((revealedCount.value / totalCellCount.value) * 100)
)

const armedPowerUpTeamName = computed(() =>
  board.value?.teams.find(t => t.id === armedPowerUp.value?.teamId)?.name ?? ''
)

const debuffedTeamName = computed(() =>
  board.value?.teams.find(t => t.id === board.value?.pendingTimerDebuffTeamId)?.name ?? ''
)

const activatingTimerDebuff = ref(false)
async function activateTimerDebuffPowerUp(teamId: string) {
  activatingTimerDebuff.value = true
  try {
    await activateTimerDebuff(gameSessionId, teamId)
    await loadBoard()
  } catch {
    errorMessage.value = 'تعذر تفعيل خصم الوقت.'
  } finally {
    activatingTimerDebuff.value = false
  }
}

const showCelebration = ref(false)
const { pieces: confettiPieces } = useConfettiBurst()
watch(allRevealed, (isDone) => {
  if (isDone && motionTier.value === 'full') showCelebration.value = true
})

async function openQuestion(questionId: string, pointValue: number) {
  if (!board.value) return
  const blockManualReveal = armedPowerUp.value?.type === 'TwoAnswers'
  try {
    const result = await selectQuestion(
      gameSessionId,
      questionId,
      armedPowerUp.value?.teamId ?? null,
      armedPowerUp.value?.type ?? null
    )
    armedPowerUp.value = null
    const timerDebuffed = board.value.pendingTimerDebuffTeamId === board.value.currentTurnTeamId
    activeQuestion.value = {
      questionId,
      pointValue,
      prompt: result.prompt,
      blockManualReveal,
      turnTeamId: board.value.currentTurnTeamId,
      turnTeamName: board.value.currentTurnTeamName,
      timerDebuffed
    }
    shownAnswer.value = null
    awarded.value = false
    retryNote.value = null
    modalOpen.value = true
    startAnswerTimer(timerDebuffed ? Math.round(ANSWER_SECONDS / 2) : ANSWER_SECONDS)
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
  stopAnswerTimer()
  awarding.value = true
  try {
    const result = await awardPoints(gameSessionId, activeQuestion.value.questionId, teamId)
    if (result.canRetry) {
      retryNote.value = `لم يُحسب الجواب — محاولة أخيرة لفريق ${result.retryTeamName}`
      quizMotion.shake('question')
      startAnswerTimer(activeQuestion.value.timerDebuffed ? Math.round(ANSWER_SECONDS / 2) : ANSWER_SECONDS)
    } else {
      shownAnswer.value = result.correctAnswer
      awarded.value = true
      if (teamId) {
        quizMotion.celebrateCorrect(teamId)
        quizMotion.recordOutcome(teamId, true)
      } else {
        quizMotion.shake('question')
      }
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
  stopAnswerTimer()
  modalOpen.value = false
  activeQuestion.value = null
  shownAnswer.value = null
  awarded.value = false
  retryNote.value = null
}

onBeforeUnmount(stopAnswerTimer)
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
          class="flex-1 flex flex-col items-center justify-center gap-6 text-center relative overflow-hidden"
        >
          <span
            v-if="showCelebration"
            class="pointer-events-none absolute inset-0"
            aria-hidden="true"
          >
            <span
              v-for="piece in confettiPieces"
              :key="piece.id"
              class="confetti-piece"
              :style="{
                'left': `${piece.left}%`,
                'width': `${piece.size}px`,
                'height': `${piece.shape === 'circle' ? piece.size : piece.size * 1.6}px`,
                'borderRadius': piece.shape === 'circle' ? '50%' : '2px',
                'backgroundColor': piece.color,
                'animationDuration': `${piece.duration}s`,
                'animationDelay': `${piece.delay}s`,
                '--drift': `${piece.drift}px`,
                '--spin': piece.spin
              }"
            />
          </span>

          <MotionScale
            :show="true"
            :duration="DURATIONS.slow"
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
              <h1
                class="text-5xl sm:text-7xl font-black text-primary"
                :style="{ color: winnerResult?.winners[0]?.color ?? undefined }"
              >
                {{ winnerResult?.winners[0]?.name }}
              </h1>
              <p class="text-3xl sm:text-4xl font-bold">
                {{ winnerResult?.winners[0]?.score }} نقطة
              </p>
            </template>
          </MotionScale>
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
              class="w-full sm:w-72 text-center relative overflow-visible"
            >
              <motion.div
                v-if="leaderTeamId === team.id"
                layout-id="leader-crown"
                :transition="SPRINGS.gentle"
                class="absolute -top-3 left-1/2 -translate-x-1/2 text-2xl"
              >
                👑
              </motion.div>
              <div class="flex items-center justify-center gap-2">
                <span
                  v-if="team.icon"
                  class="size-6 rounded-full flex items-center justify-center shrink-0"
                  :style="{ backgroundColor: team.color ?? '#123A24' }"
                >
                  <UIcon
                    :name="team.icon"
                    class="size-3.5 text-white"
                  />
                </span>
                <p class="font-bold text-lg truncate">
                  {{ team.name }}
                </p>
              </div>
              <motion.p
                class="text-3xl sm:text-4xl font-black text-primary"
                :style="{ color: team.color ?? undefined }"
                :animate="{ scale: quizMotion.celebratingTeamId.value === team.id ? [1, 1.15, 1] : 1 }"
                :transition="{ duration: DURATIONS.slow, ease: EASINGS.standard }"
              >
                <MotionNumber :value="team.score" />
              </motion.p>
              <MotionScale
                :show="quizMotion.streakFor(team.id) >= 2"
                :duration="DURATIONS.fast"
              >
                <span class="text-xs font-bold text-gold-600">🔥×{{ quizMotion.streakFor(team.id) }}</span>
              </MotionScale>
              <AnimatePresence>
                <motion.div
                  v-if="board.currentTurnTeamId === team.id"
                  layout-id="turn-badge"
                  :initial="{ opacity: 0, scale: 0.8 }"
                  :animate="{ opacity: 1, scale: 1 }"
                  :exit="{ opacity: 0, scale: 0.8 }"
                  :transition="SPRINGS.snappy"
                  class="mt-1"
                >
                  <UBadge
                    color="secondary"
                    class="text-green-950 font-bold"
                  >
                    دوره الآن ◀
                  </UBadge>
                </motion.div>
              </AnimatePresence>
              <div class="flex gap-1.5 justify-center flex-wrap mt-2">
                <UButton
                  v-if="team.doublePointsAvailable"
                  :color="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'DoublePoints' ? 'primary' : 'neutral'"
                  :variant="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'DoublePoints' ? 'solid' : 'outline'"
                  size="xs"
                  class="transition-transform active:scale-95"
                  :disabled="board.currentTurnTeamId !== team.id"
                  @click="togglePowerUp(team.id, 'DoublePoints')"
                >
                  💰 مضاعفة
                </UButton>
                <UBadge
                  v-else
                  color="neutral"
                  variant="subtle"
                  size="sm"
                >
                  💰 استُخدمت
                </UBadge>
                <UButton
                  v-if="team.twoAnswersAvailable"
                  :color="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'TwoAnswers' ? 'primary' : 'neutral'"
                  :variant="armedPowerUp?.teamId === team.id && armedPowerUp.type === 'TwoAnswers' ? 'solid' : 'outline'"
                  size="xs"
                  class="transition-transform active:scale-95"
                  :disabled="board.currentTurnTeamId !== team.id"
                  @click="togglePowerUp(team.id, 'TwoAnswers')"
                >
                  🔁 محاولتين
                </UButton>
                <UBadge
                  v-else
                  color="neutral"
                  variant="subtle"
                  size="sm"
                >
                  🔁 استُخدمت
                </UBadge>
                <UButton
                  v-if="team.halfOpponentTimerAvailable"
                  color="neutral"
                  variant="outline"
                  size="xs"
                  class="transition-transform active:scale-95"
                  :loading="activatingTimerDebuff"
                  :disabled="board.currentTurnTeamId !== team.id || !!board.pendingTimerDebuffTeamId"
                  @click="activateTimerDebuffPowerUp(team.id)"
                >
                  ⏱️ خصم وقت الخصم
                </UButton>
                <UBadge
                  v-else
                  color="neutral"
                  variant="subtle"
                  size="sm"
                >
                  ⏱️ استُخدمت
                </UBadge>
              </div>
            </UCard>
          </div>

          <MotionScale
            :show="!!board.pendingTimerDebuffTeamId"
            :duration="DURATIONS.fast"
          >
            <div class="flex items-center justify-center gap-3 flex-wrap rounded-xl bg-error/10 ring-1 ring-error/40 px-4 py-2 text-center">
              <p class="font-bold text-sm sm:text-base">
                ⏱️ فريق <span class="text-error">{{ debuffedTeamName }}</span> سيحصل على وقت إجابة مخفّض في دوره القادم
              </p>
            </div>
          </MotionScale>

          <MotionScale
            :show="!!armedPowerUp"
            :duration="DURATIONS.fast"
          >
            <div class="flex items-center justify-center gap-3 flex-wrap rounded-xl bg-secondary/15 ring-1 ring-secondary/40 px-4 py-2 text-center">
              <p class="font-bold text-sm sm:text-base">
                {{ armedPowerUp ? POWER_UP_LABELS[armedPowerUp.type] : '' }} جاهزة لفريق
                <span class="text-primary">{{ armedPowerUpTeamName }}</span>
                — اختر أي سؤال لتفعيلها
              </p>
              <UButton
                size="xs"
                color="neutral"
                variant="ghost"
                class="transition-transform active:scale-95"
                @click="armedPowerUp = null"
              >
                إلغاء
              </UButton>
            </div>
          </MotionScale>

          <div class="h-1.5 rounded-full bg-primary/10 overflow-hidden">
            <div
              class="h-full rounded-full bg-primary transition-[width] duration-(--motion-duration-slow) ease-decelerate"
              :style="{ width: `${progressPercent}%` }"
            />
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
                class="flex-1 justify-center text-lg sm:text-2xl font-black transition-transform active:scale-95"
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
          <UCard
            v-if="activeQuestion"
            :class="{ 'animate-shake': quizMotion.shakingKey.value === 'question' }"
          >
            <template #header>
              <div class="text-center space-y-1">
                <p class="font-bold text-primary text-lg">
                  {{ activeQuestion.pointValue }} نقطة — دور {{ activeQuestion.turnTeamName }}
                </p>
                <p
                  v-if="secondsLeft !== null"
                  class="text-2xl font-black transition-colors"
                  :class="secondsLeft <= COUNTDOWN_EMPHASIS_THRESHOLD ? 'text-error animate-pulse-emphasis' : 'text-primary'"
                >
                  {{ secondsLeft }} ث
                </p>
                <p
                  v-if="activeQuestion.timerDebuffed"
                  class="text-xs font-bold text-error"
                >
                  ⏱️ الوقت مخفّض!
                </p>
              </div>
            </template>

            <p class="text-2xl sm:text-3xl font-bold text-center py-6">
              {{ activeQuestion.prompt }}
            </p>

            <MotionScale
              :show="!!shownAnswer"
              :duration="DURATIONS.base"
            >
              <p class="text-xl sm:text-2xl font-bold text-center text-primary bg-primary/10 rounded-lg py-4 mb-4">
                {{ shownAnswer }}
              </p>
            </MotionScale>

            <MotionScale
              :show="!shownAnswer && !!retryNote"
              :duration="DURATIONS.base"
            >
              <p class="text-lg font-bold text-center text-warning bg-warning/10 rounded-lg py-3 mb-4">
                {{ retryNote }}
              </p>
            </MotionScale>

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
                  :loading="awarding"
                  size="lg"
                  class="transition-transform active:scale-95"
                  @click="award(activeQuestion.turnTeamId)"
                >
                  {{ activeQuestion.turnTeamName }} أجاب صح
                </UButton>
                <UButton
                  :loading="awarding"
                  color="neutral"
                  variant="outline"
                  size="lg"
                  class="transition-transform active:scale-95"
                  @click="award(null)"
                >
                  لم يُجب
                </UButton>
              </div>
              <UButton
                v-else
                block
                size="lg"
                class="transition-transform active:scale-95"
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
