<script setup lang="ts">
import type { SubmitGuessResult, Top100GuessedItemDto, Top100SessionDto } from '~/types/api'

const route = useRoute()
const sessionId = route.params.id as string

const { getTop100Session, startNextTop100Round, submitGuess } = useApi()

const session = ref<Top100SessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

interface CurrentRound {
  roundId: string
  listTitle: string
  itemCount: number
  maxGuesses: number
  guessesMade: number
  currentTurnTeamId: string
  currentTurnTeamName: string
  guessedItems: Top100GuessedItemDto[]
}

const currentRound = ref<CurrentRound | null>(null)
const roundResult = ref<SubmitGuessResult | null>(null)
const guessText = ref('')
const lastFeedback = ref<{ matched: boolean, label?: string, points?: number } | null>(null)

const starting = ref(false)
const submitting = ref(false)

async function loadSession() {
  try {
    session.value = await getTop100Session(sessionId)
    if (session.value.pendingRound && !currentRound.value) {
      const p = session.value.pendingRound
      currentRound.value = {
        roundId: p.roundId,
        listTitle: p.listTitle,
        itemCount: p.itemCount,
        maxGuesses: p.maxGuesses,
        guessesMade: p.guessesMade,
        currentTurnTeamId: p.currentTurnTeamId,
        currentTurnTeamName: p.currentTurnTeamName,
        guessedItems: p.guessedItems
      }
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
    const result = await startNextTop100Round(sessionId)
    currentRound.value = {
      roundId: result.roundId,
      listTitle: result.listTitle,
      itemCount: result.itemCount,
      maxGuesses: result.maxGuesses,
      guessesMade: 0,
      currentTurnTeamId: result.currentTurnTeamId,
      currentTurnTeamName: result.currentTurnTeamName,
      guessedItems: []
    }
    roundResult.value = null
    lastFeedback.value = null
  } catch {
    errorMessage.value = 'تعذر بدء الجولة.'
  } finally {
    starting.value = false
  }
}

async function submitCurrentGuess() {
  if (!currentRound.value || !guessText.value.trim()) return
  submitting.value = true
  errorMessage.value = ''
  try {
    const result = await submitGuess(sessionId, currentRound.value.roundId, guessText.value.trim())
    guessText.value = ''

    if (result.matched && result.matchedItemId && result.matchedLabel && result.matchedPosition) {
      currentRound.value.guessedItems.push({ id: result.matchedItemId, label: result.matchedLabel, position: result.matchedPosition })
      lastFeedback.value = { matched: true, label: result.matchedLabel, points: result.pointsAwarded }
    } else {
      lastFeedback.value = { matched: false }
    }

    if (result.roundComplete) {
      roundResult.value = result
    } else {
      currentRound.value.guessesMade++
      currentRound.value.currentTurnTeamId = result.nextTurnTeamId
      currentRound.value.currentTurnTeamName = result.nextTurnTeamName
    }

    if (session.value) session.value.teams = result.teams
  } catch {
    errorMessage.value = 'تعذر إرسال التخمين.'
  } finally {
    submitting.value = false
  }
}

async function nextRound() {
  currentRound.value = null
  roundResult.value = null
  lastFeedback.value = null
  await loadSession()
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
            v-if="!currentRound"
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
            v-else-if="!roundResult"
            class="max-w-xl w-full"
          >
            <template #header>
              <p class="text-center text-muted">
                {{ currentRound.listTitle }}
              </p>
              <p class="text-center font-bold text-lg">
                دور فريق: {{ currentRound.currentTurnTeamName }}
              </p>
              <p class="text-center text-sm text-muted">
                محاولة {{ currentRound.guessesMade + 1 }} من {{ currentRound.maxGuesses }}
              </p>
            </template>

            <div class="space-y-4">
              <div class="flex gap-2">
                <UInput
                  v-model="guessText"
                  size="xl"
                  class="flex-1"
                  placeholder="اكتب تخمينك"
                  @keyup.enter="submitCurrentGuess"
                />
                <UButton
                  size="xl"
                  :loading="submitting"
                  :disabled="!guessText.trim()"
                  @click="submitCurrentGuess"
                >
                  إرسال
                </UButton>
              </div>

              <p
                v-if="lastFeedback"
                class="text-center font-bold rounded-lg py-2"
                :class="lastFeedback.matched ? 'text-primary bg-primary/10' : 'text-muted bg-neutral-500/10'"
              >
                <template v-if="lastFeedback.matched">
                  ✅ {{ lastFeedback.label }} — {{ lastFeedback.points }} نقطة
                </template>
                <template v-else>
                  ❌ لا يوجد تطابق
                </template>
              </p>

              <div v-if="currentRound.guessedItems.length > 0">
                <p class="text-sm font-bold text-muted mb-2">
                  ما تم تخمينه
                </p>
                <div class="flex flex-wrap gap-2">
                  <UBadge
                    v-for="item in currentRound.guessedItems"
                    :key="item.id"
                    color="primary"
                    variant="subtle"
                  >
                    {{ item.label }} ({{ item.position }})
                  </UBadge>
                </div>
              </div>
            </div>
          </UCard>

          <UCard
            v-else
            class="max-w-xl w-full text-center"
          >
            <p class="text-sm font-bold text-muted mb-2">
              القائمة كاملة
            </p>
            <ol class="space-y-1 mb-4">
              <li
                v-for="item in roundResult.fullList"
                :key="item.id"
                class="flex items-center gap-2 rounded-lg px-3 py-2 font-bold"
                :class="item.wasGuessed ? 'bg-primary/10' : 'bg-neutral-500/10 text-muted'"
              >
                <span>{{ item.wasGuessed ? '✅' : '⬜' }}</span>
                <span class="flex-1 text-right">{{ item.label }}</span>
                <span>{{ item.position }}</span>
              </li>
            </ol>
            <UButton
              size="xl"
              block
              @click="nextRound"
            >
              التالي
            </UButton>
          </UCard>
        </div>
      </template>
    </template>
  </div>
</template>
