<script setup lang="ts">
import QRCode from 'qrcode'
import type { PasswordSessionDto } from '~/types/api'

const ROUND_SECONDS = 60

const route = useRoute()
const sessionId = route.params.id as string

const { getPasswordSession, startNextPasswordRound, issueRevealToken, resolvePasswordRound } = useApi()

const session = ref<PasswordSessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const qrDataUrl = ref<string | null>(null)
const secondsLeft = ref<number | null>(null)
const starting = ref(false)
const revealing = ref(false)
const resolving = ref(false)
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
  try {
    await resolvePasswordRound(sessionId, session.value.pendingRound.roundId, correct)
    resetRoundUi()
    await loadSession()
  } catch {
    errorMessage.value = 'تعذر تسجيل النتيجة.'
  } finally {
    resolving.value = false
  }
}

const winnerResult = computed(() => session.value ? getWinner(session.value.teams) : null)
</script>

<template>
  <div class="min-h-screen p-3 sm:p-6 flex flex-col gap-6">
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
            v-for="team in session.teams"
            :key="team.id"
            class="min-w-40 text-center"
          >
            <p class="font-bold text-lg truncate">
              {{ team.name }}
            </p>
            <p class="text-3xl sm:text-4xl font-black text-primary">
              {{ team.score }}
            </p>
          </UCard>
        </div>

        <p class="text-center text-muted">
          الجولة {{ session.roundsPlayed + 1 }} من {{ session.totalRounds }}
        </p>

        <UAlert
          v-if="errorMessage"
          color="error"
          variant="subtle"
          :title="errorMessage"
        />

        <div class="flex-1 flex items-center justify-center">
          <UCard
            v-if="!session.pendingRound"
            class="text-center max-w-md w-full"
          >
            <p class="text-lg mb-4">
              اضغط لبدء الجولة التالية
            </p>
            <UButton
              size="xl"
              :loading="starting"
              @click="startRound"
            >
              ابدأ الجولة التالية
            </UButton>
          </UCard>

          <UCard
            v-else
            class="text-center max-w-md w-full"
          >
            <p class="text-xl font-bold mb-1">
              دور فريق: {{ session.pendingRound.teamName }}
            </p>
            <p class="text-muted mb-4">
              يقوم أحد أفراد الفريق بمسح الرمز بجواله ليرى الكلمة السرية بمفرده
            </p>

            <div v-if="!qrDataUrl">
              <UButton
                size="xl"
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
              <p
                v-if="secondsLeft !== null"
                class="text-2xl font-black text-primary"
              >
                {{ secondsLeft }} ثانية
              </p>
              <div class="flex flex-wrap gap-2 justify-center">
                <UButton
                  color="success"
                  :loading="resolving"
                  @click="resolve(true)"
                >
                  إجابة صحيحة
                </UButton>
                <UButton
                  color="neutral"
                  variant="outline"
                  :loading="resolving"
                  @click="resolve(false)"
                >
                  تخطي
                </UButton>
              </div>
            </div>
          </UCard>
        </div>
      </template>
    </template>
  </div>
</template>
