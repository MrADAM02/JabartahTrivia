<script setup lang="ts">
import { AnimatePresence, motion } from 'motion-v'
import type { Top100GuessLogEntryDto, Top100SessionDto, Top100TeamDto } from '~/types/api'
import { DURATIONS, EASINGS, SPRINGS } from '~/utils/motion'

definePageMeta({ layout: false })

const route = useRoute()
const sessionId = route.params.id as string

const { getTop100Session, startNextTop100Round, submitGuess } = useApi()
const quizMotion = useQuizMotion()
const { motionTier } = useResponsiveMotion()
const { pieces: confettiPieces } = useConfettiBurst()

const session = ref<Top100SessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')
const starting = ref(false)
const submitting = ref(false)
const guessText = ref('')
const lastFeedback = ref<{ matched: boolean, label?: string } | null>(null)

async function loadSession() {
  try {
    session.value = await getTop100Session(sessionId)
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
    await startNextTop100Round(sessionId)
    await loadSession()
  } catch {
    errorMessage.value = 'تعذر بدء اللعبة.'
  } finally {
    starting.value = false
  }
}

async function submitCurrentGuess() {
  const pending = session.value?.pendingRound
  const text = guessText.value.trim()
  if (!pending || !text) return

  submitting.value = true
  errorMessage.value = ''
  try {
    const result = await submitGuess(sessionId, pending.roundId, text)
    guessText.value = ''
    lastFeedback.value = result.matched ? { matched: true, label: result.matchedLabel ?? undefined } : { matched: false }

    if (result.matched) {
      quizMotion.celebrateCorrect(result.guessingTeamId)
      quizMotion.recordOutcome(result.guessingTeamId, true)
    } else {
      quizMotion.shake('guess')
    }

    if (result.sessionComplete) {
      await loadSession()
    } else {
      pending.guesses.push({
        sequenceNumber: pending.guesses.length + 1,
        teamId: result.guessingTeamId,
        teamName: result.guessingTeamName,
        guessText: text,
        matched: result.matched,
        matchedLabel: result.matchedLabel,
        matchedPosition: result.matchedPosition
      })
      pending.guessesMade++
      pending.currentTurnTeamId = result.nextTurnTeamId
      pending.currentTurnTeamName = result.nextTurnTeamName
      if (session.value) session.value.teams = result.teams
    }
  } catch {
    errorMessage.value = 'تعذر إرسال التخمين.'
  } finally {
    submitting.value = false
  }
}

function teamById(teamId: string): Top100TeamDto | undefined {
  return session.value?.teams.find(t => t.id === teamId)
}

function guessesForTeam(guesses: Top100GuessLogEntryDto[], teamId: string) {
  return guesses.filter(g => g.teamId === teamId)
}

const discoveredItems = computed(() => session.value?.pendingRound?.guesses.filter(g => g.matched) ?? [])
const mistakes = computed(() => session.value?.pendingRound?.guesses.filter(g => !g.matched) ?? [])

const winnerResult = computed(() => session.value ? getWinner(session.value.teams) : null)

const leaderTeamId = computed(() => {
  if (!session.value || session.value.teams.length !== 2) return null
  const [a, b] = session.value.teams
  if (!a || !b || a.score === b.score) return null
  return a.score > b.score ? a.id : b.id
})

const progressPercent = computed(() => {
  const pending = session.value?.pendingRound
  if (!pending || pending.itemCount === 0) return 0
  return Math.round((discoveredItems.value.length / pending.itemCount) * 100)
})

const showCelebration = ref(false)
watch(() => session.value?.status, (status) => {
  if (status === 'Completed' && motionTier.value === 'full') showCelebration.value = true
})
</script>

<template>
  <div class="min-h-screen bg-white dark:bg-gray-950 flex flex-col">
    <GameExitBar />

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
      class="m-4"
    />

    <template v-else-if="session">
      <!-- session complete: winner + full answers comparison -->
      <div
        v-if="session.status === 'Completed'"
        class="flex-1 p-4 sm:p-6 space-y-8 relative overflow-hidden"
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
          <div class="flex flex-col items-center gap-3 text-center">
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
          </div>
        </MotionScale>

        <UCard
          v-if="session.completedRound"
          class="max-w-3xl mx-auto w-full"
        >
          <template #header>
            <p class="text-center font-bold text-lg text-green-900 dark:text-green-100">
              ملخص الإجابات
            </p>
            <p class="text-center text-sm text-muted">
              {{ session.completedRound.listTitle }}
            </p>
          </template>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div
              v-for="team in session.teams"
              :key="team.id"
            >
              <UBadge
                class="mb-2 font-bold"
                :style="{ backgroundColor: team.color ?? undefined, color: 'white' }"
              >
                {{ team.name }} — {{ guessesForTeam(session.completedRound.guesses, team.id).filter(g => g.matched).length }} صحيحة
              </UBadge>
              <ol class="space-y-1">
                <li
                  v-for="g in guessesForTeam(session.completedRound.guesses, team.id)"
                  :key="g.sequenceNumber"
                  class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm"
                  :class="g.matched ? 'bg-primary/10' : 'bg-error/10'"
                >
                  <span v-if="g.matched">✅</span>
                  <span v-else>❌</span>
                  <span class="flex-1">{{ g.matched ? g.matchedLabel : g.guessText }}</span>
                  <span
                    v-if="g.matched"
                    class="font-bold text-primary"
                  >
                    #{{ g.matchedPosition }}
                  </span>
                </li>
              </ol>
            </div>
          </div>
        </UCard>

        <div class="text-center">
          <UButton
            size="xl"
            to="/"
          >
            لعبة جديدة
          </UButton>
        </div>
      </div>

      <!-- not started yet -->
      <div
        v-else-if="!session.pendingRound"
        class="flex-1 flex flex-col items-center justify-center gap-6 p-4"
      >
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
            <p
              class="text-3xl sm:text-4xl font-black text-primary"
              :style="{ color: team.color ?? undefined }"
            >
              <MotionNumber :value="team.score" />
            </p>
          </UCard>
        </div>

        <p class="text-center text-muted">
          {{ session.guessesPerTeam }} إجابات لكل فريق
        </p>

        <UAlert
          v-if="errorMessage"
          color="error"
          variant="subtle"
          :title="errorMessage"
        />

        <MotionScale
          :show="true"
          :duration="DURATIONS.base"
        >
          <UCard class="text-center max-w-md w-full">
            <p class="text-lg mb-4">
              اضغط لبدء اللعبة
            </p>
            <UButton
              size="xl"
              class="transition-transform active:scale-95"
              :loading="starting"
              @click="startRound"
            >
              ابدأ اللعبة
            </UButton>
          </UCard>
        </MotionScale>
      </div>

      <!-- active round -->
      <template v-else>
        <div class="flex-1 flex flex-col md:flex-row overflow-hidden">
          <aside class="md:w-64 shrink-0 p-3 sm:p-4 flex flex-col gap-3 border-b md:border-b-0 md:border-e border-green-100 dark:border-gray-800 overflow-y-auto">
            <div
              v-for="team in session.teams"
              :key="team.id"
              class="relative overflow-visible rounded-xl p-3 ring-1 ring-green-100 dark:ring-gray-800"
              :style="{ boxShadow: session.pendingRound.currentTurnTeamId === team.id ? `0 0 0 2px ${team.color}` : 'none' }"
            >
              <motion.div
                v-if="leaderTeamId === team.id"
                layout-id="leader-crown"
                :transition="SPRINGS.gentle"
                class="absolute -top-3 left-1/2 -translate-x-1/2 text-xl"
              >
                👑
              </motion.div>
              <div class="flex items-center gap-2 mb-1">
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
                <p class="font-bold truncate flex-1">
                  {{ team.name }}
                </p>
              </div>
              <motion.p
                class="text-2xl font-black text-primary"
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
                  v-if="session.pendingRound.currentTurnTeamId === team.id"
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
            </div>

            <div
              class="rounded-xl p-3 ring-1 ring-error/30 bg-error/5"
              :class="{ 'animate-shake': quizMotion.shakingKey.value === 'guess' }"
            >
              <div class="flex items-center justify-between mb-1">
                <p class="font-bold text-error text-sm">
                  كومة الأخطاء
                </p>
                <UBadge color="error">
                  <MotionNumber :value="mistakes.length" />
                </UBadge>
              </div>
              <p
                v-if="mistakes.length === 0"
                class="text-xs text-muted"
              >
                لا أخطاء بعد
              </p>
              <ul
                v-else
                class="space-y-1 max-h-32 overflow-y-auto"
              >
                <AnimatePresence>
                  <motion.li
                    v-for="g in [...mistakes].reverse()"
                    :key="g.sequenceNumber"
                    layout
                    :initial="{ opacity: 0, x: -12 }"
                    :animate="{ opacity: 1, x: 0 }"
                    :transition="{ duration: DURATIONS.fast, ease: EASINGS.decelerate }"
                    class="text-xs text-muted truncate"
                  >
                    {{ g.teamName }}: {{ g.guessText }}
                  </motion.li>
                </AnimatePresence>
              </ul>
            </div>
          </aside>

          <main class="flex-1 p-3 sm:p-4 overflow-y-auto">
            <div class="flex items-center justify-between mb-1">
              <p class="font-bold text-green-900 dark:text-green-100">
                {{ session.pendingRound.listTitle }}
              </p>
              <p class="text-sm text-muted">
                {{ discoveredItems.length }} / {{ session.pendingRound.itemCount }}
              </p>
            </div>
            <div class="h-1.5 rounded-full bg-primary/10 overflow-hidden mb-3">
              <div
                class="h-full rounded-full bg-primary transition-[width] duration-(--motion-duration-slow) ease-decelerate"
                :style="{ width: `${progressPercent}%` }"
              />
            </div>

            <p
              v-if="discoveredItems.length === 0"
              class="text-muted text-sm text-center py-12"
            >
              لم يتم اكتشاف أي عنصر بعد
            </p>
            <ol class="space-y-1">
              <AnimatePresence>
                <motion.li
                  v-for="item in discoveredItems"
                  :key="item.sequenceNumber"
                  layout
                  :initial="{ opacity: 0, scale: 0.9, y: -8 }"
                  :animate="{ opacity: 1, scale: 1, y: 0 }"
                  :transition="{ duration: DURATIONS.base, ease: EASINGS.decelerate }"
                  class="flex items-center gap-3 rounded-lg px-3 py-2 bg-green-50 dark:bg-gray-900"
                >
                  <span
                    class="size-7 rounded-full flex items-center justify-center text-xs font-black text-white shrink-0"
                    :style="{ backgroundColor: teamById(item.teamId)?.color ?? '#123A24' }"
                  >
                    {{ item.matchedPosition }}
                  </span>
                  <span class="flex-1">{{ item.matchedLabel }}</span>
                  <span class="text-xs text-muted">{{ item.teamName }}</span>
                </motion.li>
              </AnimatePresence>
            </ol>
          </main>
        </div>

        <div class="border-t border-green-100 dark:border-gray-800 p-3 sm:p-4 bg-green-50 dark:bg-gray-900 space-y-2">
          <p
            v-if="errorMessage"
            class="text-error text-sm text-center"
          >
            {{ errorMessage }}
          </p>
          <MotionFade
            :show="!!lastFeedback"
            :duration="DURATIONS.fast"
          >
            <p
              class="text-center font-bold text-sm"
              :class="lastFeedback?.matched ? 'text-primary' : 'text-muted'"
            >
              <template v-if="lastFeedback?.matched">
                ✅ {{ lastFeedback.label }}
              </template>
              <template v-else>
                ❌ لا يوجد تطابق
              </template>
            </p>
          </MotionFade>
          <p class="text-center text-xs text-muted">
            {{ session.pendingRound.guessesMade }} / {{ session.pendingRound.maxGuesses }} إجابات
          </p>
          <div class="flex items-center gap-2 max-w-2xl mx-auto">
            <p class="font-bold shrink-0 text-sm sm:text-base">
              دور {{ session.pendingRound.currentTurnTeamName }}
            </p>
            <UInput
              v-model="guessText"
              size="xl"
              class="flex-1"
              placeholder="اكتب تخمينك"
              @keyup.enter="submitCurrentGuess"
            />
            <UButton
              size="xl"
              color="secondary"
              class="font-bold text-green-950 transition-transform active:scale-95"
              :loading="submitting"
              :disabled="!guessText.trim()"
              @click="submitCurrentGuess"
            >
              تأكيد
            </UButton>
          </div>
        </div>
      </template>
    </template>
  </div>
</template>
