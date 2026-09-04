<script setup lang="ts">
import { motion } from 'motion-v'
import QRCode from 'qrcode'
import type { PasswordSessionDto } from '~/types/api'
import { DURATIONS, EASINGS, SPRINGS } from '~/utils/motion'

definePageMeta({ layout: false })

const ROUND_SECONDS = 60
const COUNTDOWN_EMPHASIS_THRESHOLD = 10

const route = useRoute()
const sessionId = route.params.id as string

const EXTRA_TIME_SECONDS = 15

const { getPasswordSession, startNextPasswordRound, issueRevealToken, resolvePasswordRound, useExtraTime } = useApi()
const quizMotion = useQuizMotion()
const { motionTier } = useResponsiveMotion()
const { pieces: confettiPieces } = useConfettiBurst()

const session = ref<PasswordSessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const qrDataUrl = ref<string | null>(null)
const secondsLeft = ref<number | null>(null)
const starting = ref(false)
const revealing = ref(false)
const resolving = ref(false)
const usingExtraTime = ref(false)
let countdownTimer: ReturnType<typeof setInterval> | undefined

async function loadSession() {
  try {
    session.value = await getPasswordSession(sessionId)
  } catch {
    errorMessage.value = 'تعذر تحميل اللعبة. تأكد من الرابط.'
  } finally {
    loading.value = false
  }
}

onMounted(loadSession)
onBeforeUnmount(() => clearInterval(countdownTimer))

function resetRoundUi() {
  qrDataUrl.value = null
  secondsLeft.value = null
  clearInterval(countdownTimer)
}

async function startRound() {
  starting.value = true
  errorMessage.value = ''
  try {
    await startNextPasswordRound(sessionId)
    resetRoundUi()
    await loadSession()
  } catch {
    errorMessage.value = 'تعذر بدء الجولة.'
  } finally {
    starting.value = false
  }
}

async function showQr() {
  if (!session.value?.pendingRound) return
  revealing.value = true
  try {
    const { token } = await issueRevealToken(sessionId, session.value.pendingRound.roundId)
    const url = `${window.location.origin}/reveal/${token}`
    qrDataUrl.value = await QRCode.toDataURL(url, { width: 320 })

    secondsLeft.value = ROUND_SECONDS
    clearInterval(countdownTimer)
    countdownTimer = setInterval(() => {
      if (secondsLeft.value !== null && secondsLeft.value > 0) secondsLeft.value--
      else clearInterval(countdownTimer)
    }, 1000)
  } catch {
    errorMessage.value = 'تعذر إنشاء رمز QR.'
  } finally {
    revealing.value = false
  }
}

async function resolve(correct: boolean) {
  if (!session.value?.pendingRound) return
  resolving.value = true
  const { roundId, teamId } = session.value.pendingRound
  try {
    await resolvePasswordRound(sessionId, roundId, correct)
    if (correct) {
      quizMotion.celebrateCorrect(teamId)
      quizMotion.recordOutcome(teamId, true)
    } else {
      quizMotion.shake('round')
    }
    resetRoundUi()
    await loadSession()
  } catch {
    errorMessage.value = 'تعذر تسجيل النتيجة.'
  } finally {
    resolving.value = false
  }
}

async function activateExtraTime() {
  const pending = session.value?.pendingRound
  if (!pending) return
  usingExtraTime.value = true
  try {
    await useExtraTime(sessionId, pending.teamId)
    const team = session.value?.teams.find(t => t.id === pending.teamId)
    if (team) team.extraTimeAvailable = false
    if (secondsLeft.value !== null) secondsLeft.value += EXTRA_TIME_SECONDS
  } catch {
    errorMessage.value = 'تعذر تفعيل الوقت الإضافي.'
  } finally {
    usingExtraTime.value = false
  }
}

const pendingTeam = computed(() =>
  session.value?.pendingRound ? session.value.teams.find(t => t.id === session.value!.pendingRound!.teamId) : null
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
</script>

<template>
  <div class="min-h-screen bg-white dark:bg-gray-950 flex flex-col">
    <GameExitBar />
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
              v-if="!session.pendingRound"
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

            <MotionScale
              v-else
              :show="true"
              :duration="DURATIONS.base"
            >
              <UCard
                class="text-center max-w-md w-full"
                :class="{ 'animate-shake': quizMotion.shakingKey.value === 'round' }"
              >
                <p class="text-xl font-bold mb-1">
                  دور فريق: {{ session.pendingRound.teamName }}
                </p>
                <p class="text-muted mb-4">
                  يقوم أحد أفراد الفريق بمسح الرمز بجواله ليرى الكلمة السرية بمفرده
                </p>

                <p
                  v-if="secondsLeft !== null"
                  class="text-3xl font-black mb-4 transition-colors"
                  :class="secondsLeft <= COUNTDOWN_EMPHASIS_THRESHOLD ? 'text-error animate-pulse-emphasis' : 'text-primary'"
                >
                  {{ secondsLeft }} ثانية
                </p>

                <div v-if="!qrDataUrl">
                  <UButton
                    size="xl"
                    class="transition-transform active:scale-95"
                    :loading="revealing"
                    @click="showQr"
                  >
                    عرض رمز QR
                  </UButton>
                </div>

                <div
                  v-else
                  class="space-y-4"
                >
                  <img
                    :src="qrDataUrl"
                    alt="QR"
                    class="mx-auto rounded-lg border border-default"
                  >

                  <div class="flex justify-center">
                    <UButton
                      v-if="pendingTeam?.extraTimeAvailable"
                      color="secondary"
                      variant="soft"
                      size="xs"
                      class="transition-transform active:scale-95"
                      :loading="usingExtraTime"
                      @click="activateExtraTime"
                    >
                      ⏱️ وقت إضافي (+{{ EXTRA_TIME_SECONDS }} ث)
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

                  <div class="flex flex-wrap gap-2 justify-center">
                    <UButton
                      color="success"
                      class="transition-transform active:scale-95"
                      :loading="resolving"
                      @click="resolve(true)"
                    >
                      إجابة صحيحة
                    </UButton>
                    <UButton
                      color="neutral"
                      variant="outline"
                      class="transition-transform active:scale-95"
                      :loading="resolving"
                      @click="resolve(false)"
                    >
                      تخطي
                    </UButton>
                  </div>
                </div>
              </UCard>
            </MotionScale>
          </div>
        </template>
      </template>
    </div>
  </div>
</template>
