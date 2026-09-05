<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import type { RankingItemOptionDto, RankingSessionDto, SubmitRankingRoundResult } from '~/types/api'
import { DURATIONS, EASINGS, SPRINGS } from '~/utils/motion'

definePageMeta({ layout: false })

const route = useRoute()
const sessionId = route.params.id as string

const { getRankingSession, startNextRankingRound, submitRankingRound, revealRankingPosition, endRankingGameSession } = useApi()
const quizMotion = useQuizMotion()
const { motionTier } = useResponsiveMotion()
const { pieces: confettiPieces } = useConfettiBurst()

const session = ref<RankingSessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const currentRound = ref<{ roundId: string, teamId: string, teamName: string, listTitle: string } | null>(null)
const pool = ref<RankingItemOptionDto[]>([])
const placed = ref<RankingItemOptionDto[]>([])
const roundResult = ref<SubmitRankingRoundResult | null>(null)

const starting = ref(false)
const submitting = ref(false)
const revealingPosition = ref(false)
const revealedHint = ref<{ position: number, itemLabel: string } | null>(null)

async function loadSession() {
  try {
    session.value = await getRankingSession(sessionId)
    if (session.value.pendingRound && !currentRound.value) {
      const p = session.value.pendingRound
      currentRound.value = { roundId: p.roundId, teamId: p.teamId, teamName: p.teamName, listTitle: p.listTitle }
      pool.value = [...p.items]
      placed.value = []
    }
  } catch {
    errorMessage.value = 'تعذر تحميل اللعبة. تأكد من الرابط.'
  } finally {
    loading.value = false
  }
}

onMounted(loadSession)

async function startRound() {
  starting.value = true
  errorMessage.value = ''
  try {
    const result = await startNextRankingRound(sessionId)
    currentRound.value = { roundId: result.roundId, teamId: result.teamId, teamName: result.teamName, listTitle: result.listTitle }
    pool.value = [...result.items]
    placed.value = []
    roundResult.value = null
    revealedHint.value = null
  } catch {
    errorMessage.value = 'تعذر بدء الجولة.'
  } finally {
    starting.value = false
  }
}

function tapItem(item: RankingItemOptionDto) {
  pool.value = pool.value.filter(i => i.id !== item.id)
  placed.value.push(item)
}

function undoLast() {
  const last = placed.value.pop()
  if (last) pool.value.push(last)
}

function resetOrder() {
  pool.value = [...pool.value, ...placed.value]
  placed.value = []
}

async function submitOrder() {
  if (!currentRound.value) return
  submitting.value = true
  errorMessage.value = ''
  try {
    const result = await submitRankingRound(sessionId, currentRound.value.roundId, placed.value.map(i => i.id))
    roundResult.value = result
    const correctCount = result.correctOrder.filter((item, index) => placed.value[index]?.id === item.id).length
    if (correctCount === result.correctOrder.length) {
      quizMotion.celebrateCorrect(currentRound.value.teamId)
      quizMotion.recordOutcome(currentRound.value.teamId, true)
    } else if (correctCount === 0) {
      quizMotion.shake('round')
    }
  } catch {
    errorMessage.value = 'تعذر إرسال الترتيب.'
  } finally {
    submitting.value = false
  }
}

async function nextRound() {
  currentRound.value = null
  pool.value = []
  placed.value = []
  roundResult.value = null
  revealedHint.value = null
  await loadSession()
}

async function activateRevealPosition() {
  if (!currentRound.value) return
  revealingPosition.value = true
  try {
    const result = await revealRankingPosition(sessionId, currentRound.value.roundId, currentRound.value.teamId)
    revealedHint.value = { position: result.position, itemLabel: result.itemLabel }
    const team = session.value?.teams.find(t => t.id === currentRound.value?.teamId)
    if (team) team.revealPositionAvailable = false
  } catch {
    errorMessage.value = 'تعذر تفعيل التلميح.'
  } finally {
    revealingPosition.value = false
  }
}

const currentRoundTeam = computed(() =>
  currentRound.value ? session.value?.teams.find(t => t.id === currentRound.value?.teamId) : null
)

const winnerResult = computed(() => session.value ? getWinner(session.value.teams) : null)

const leaderTeamId = computed(() => {
  if (!session.value || session.value.teams.length !== 2) return null
  const [a, b] = session.value.teams
  if (!a || !b || a.score === b.score) return null
  return a.score > b.score ? a.id : b.id
})

const progressPercent = computed(() => {
  if (!session.value || session.value.totalRounds === 0) return 0
  return Math.round((session.value.roundsPlayed / session.value.totalRounds) * 100)
})

const showCelebration = ref(false)
watch(() => session.value?.status, (status) => {
  if (status === 'Completed' && motionTier.value === 'full') showCelebration.value = true
})

async function handleEndGame() {
  currentRound.value = null
  pool.value = []
  placed.value = []
  roundResult.value = null
  revealedHint.value = null
  try {
    session.value = await endRankingGameSession(sessionId)
  } catch {
    errorMessage.value = 'تعذر إنهاء اللعبة.'
  }
}
</script>

<template>
  <div class="min-h-screen bg-white dark:bg-gray-950 flex flex-col">
    <GameExitBar
      :show-end-game="session?.status !== 'Completed'"
      @end="handleEndGame"
    />
    <div class="p-3 sm:p-6 flex flex-col gap-6 flex-1">
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
        v-else-if="errorMessage && !session"
        color="error"
        variant="subtle"
        :title="errorMessage"
      />

      <template v-else-if="session">
        <div
          v-if="session.status === 'Completed'"
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
              v-for="team in session.teams"
              :key="team.id"
              class="min-w-40 text-center relative overflow-visible"
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
            </UCard>
          </div>

          <div class="max-w-md w-full mx-auto space-y-1">
            <p class="text-center text-muted">
              الجولة {{ session.roundsPlayed + 1 }} من {{ session.totalRounds }}
            </p>
            <div class="h-1.5 rounded-full bg-primary/10 overflow-hidden">
              <div
                class="h-full rounded-full bg-primary transition-[width] duration-(--motion-duration-slow) ease-decelerate"
                :style="{ width: `${progressPercent}%` }"
              />
            </div>
          </div>

          <UAlert
            v-if="errorMessage"
            color="error"
            variant="subtle"
            :title="errorMessage"
          />

          <div class="flex-1 flex items-center justify-center">
            <MotionScale
              v-if="!currentRound"
              :show="true"
              :duration="DURATIONS.base"
            >
              <UCard class="text-center max-w-md w-full">
                <p class="text-lg mb-4">
                  اضغط لبدء الجولة التالية
                </p>
                <UButton
                  size="xl"
                  class="transition-transform active:scale-95"
                  :loading="starting"
                  @click="startRound"
                >
                  ابدأ الجولة التالية
                </UButton>
              </UCard>
            </MotionScale>

            <UCard
              v-else-if="!roundResult"
              class="max-w-xl w-full"
            >
              <template #header>
                <p class="text-center font-bold text-lg">
                  {{ currentRound.teamName }}
                </p>
                <p class="text-center text-muted">
                  {{ currentRound.listTitle }}
                </p>
              </template>

              <div class="space-y-4">
                <div class="flex justify-center">
                  <UButton
                    v-if="currentRoundTeam?.revealPositionAvailable"
                    color="secondary"
                    variant="soft"
                    size="xs"
                    class="transition-transform active:scale-95"
                    :loading="revealingPosition"
                    @click="activateRevealPosition"
                  >
                    💡 تلميح: اكشف موضع عنصر
                  </UButton>
                  <UBadge
                    v-else-if="!revealedHint"
                    color="neutral"
                    variant="subtle"
                    size="sm"
                  >
                    💡 استُخدم التلميح
                  </UBadge>
                </div>

                <MotionScale
                  :show="!!revealedHint"
                  :duration="DURATIONS.base"
                >
                  <p
                    v-if="revealedHint"
                    class="text-center font-bold text-sm bg-secondary/15 ring-1 ring-secondary/40 rounded-lg py-2 px-3"
                  >
                    💡 العنصر في المركز {{ revealedHint.position }} هو «{{ revealedHint.itemLabel }}»
                  </p>
                </MotionScale>

                <div>
                  <p class="text-sm font-bold text-muted mb-2">
                    البطاقات
                  </p>
                  <div class="flex flex-wrap gap-2">
                    <UButton
                      v-for="item in pool"
                      :key="item.id"
                      color="neutral"
                      variant="outline"
                      size="lg"
                      class="transition-transform active:scale-95"
                      @click="tapItem(item)"
                    >
                      {{ item.label }}
                    </UButton>
                  </div>
                </div>

                <div>
                  <p class="text-sm font-bold text-muted mb-2">
                    ترتيبك
                  </p>
                  <ol class="space-y-1">
                    <AnimatePresence>
                      <motion.li
                        v-for="(item, index) in placed"
                        :key="item.id"
                        layout
                        :initial="{ opacity: 0, y: -10, scale: 0.9 }"
                        :animate="{ opacity: 1, y: 0, scale: 1 }"
                        :exit="{ opacity: 0, scale: 0.9 }"
                        :transition="{ duration: DURATIONS.fast, ease: EASINGS.decelerate }"
                        class="flex items-center gap-2 bg-primary/10 rounded-lg px-3 py-2 font-bold"
                      >
                        <span class="text-primary">{{ index + 1 }}</span>
                        <span>{{ item.label }}</span>
                      </motion.li>
                    </AnimatePresence>
                    <li
                      v-if="placed.length === 0"
                      class="text-muted text-sm px-1"
                    >
                      اضغط على البطاقات أعلاه بالترتيب الصحيح
                    </li>
                  </ol>
                </div>
              </div>

              <template #footer>
                <div class="flex gap-2 justify-center">
                  <UButton
                    color="neutral"
                    variant="ghost"
                    class="transition-transform active:scale-95"
                    :disabled="placed.length === 0"
                    @click="undoLast"
                  >
                    تراجع
                  </UButton>
                  <UButton
                    color="neutral"
                    variant="ghost"
                    class="transition-transform active:scale-95"
                    :disabled="placed.length === 0"
                    @click="resetOrder"
                  >
                    إعادة
                  </UButton>
                  <UButton
                    class="transition-transform active:scale-95"
                    :loading="submitting"
                    :disabled="pool.length > 0"
                    @click="submitOrder"
                  >
                    إرسال الترتيب
                  </UButton>
                </div>
              </template>
            </UCard>

            <UCard
              v-else
              class="max-w-xl w-full text-center"
              :class="{ 'animate-shake': quizMotion.shakingKey.value === 'round' }"
            >
              <p class="text-3xl font-black text-primary mb-2">
                +{{ roundResult.pointsAwarded }} نقطة
              </p>
              <p class="text-sm font-bold text-muted mb-2">
                ملخص الإجابات
              </p>
              <ol class="space-y-1 mb-4">
                <li
                  v-for="(item, index) in roundResult.correctOrder"
                  :key="item.id"
                  class="flex items-center gap-2 rounded-lg px-3 py-2 font-bold"
                  :class="placed[index]?.id === item.id ? 'bg-primary/10' : 'bg-error/10'"
                >
                  <span :class="placed[index]?.id === item.id ? 'text-primary' : 'text-error'">
                    {{ index + 1 }}
                  </span>
                  <span>{{ placed[index]?.id === item.id ? '✅' : '❌' }}</span>
                  <span class="flex-1 text-start">
                    {{ placed[index]?.label ?? '—' }}
                    <span
                      v-if="placed[index]?.id !== item.id"
                      class="block text-xs font-normal text-muted"
                    >
                      الصحيح: {{ item.label }}
                    </span>
                  </span>
                </li>
              </ol>
              <UButton
                size="xl"
                block
                class="transition-transform active:scale-95"
                @click="nextRound"
              >
                التالي
              </UButton>
            </UCard>
          </div>
        </template>
      </template>
    </div>
  </div>
</template>
