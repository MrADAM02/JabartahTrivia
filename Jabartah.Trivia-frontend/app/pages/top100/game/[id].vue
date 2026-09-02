<script setup lang="ts">
import type { Top100GuessLogEntryDto, Top100SessionDto, Top100TeamDto } from '~/types/api'

definePageMeta({ layout: false })

const route = useRoute()
const sessionId = route.params.id as string

const { getTop100Session, startNextTop100Round, submitGuess } = useApi()

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
        class="flex-1 p-4 sm:p-6 space-y-8"
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
            class="min-w-40 text-center"
          >
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
              {{ team.score }}
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

        <UCard class="text-center max-w-md w-full">
          <p class="text-lg mb-4">
            اضغط لبدء اللعبة
          </p>
          <UButton
            size="xl"
            :loading="starting"
            @click="startRound"
          >
            ابدأ اللعبة
          </UButton>
        </UCard>
      </div>

      <!-- active round -->
      <template v-else>
        <div class="flex-1 flex flex-col md:flex-row overflow-hidden">
          <aside class="md:w-64 shrink-0 p-3 sm:p-4 flex flex-col gap-3 border-b md:border-b-0 md:border-e border-green-100 dark:border-gray-800 overflow-y-auto">
            <div
              v-for="team in session.teams"
              :key="team.id"
              class="rounded-xl p-3 ring-1 ring-green-100 dark:ring-gray-800"
              :style="{ boxShadow: session.pendingRound.currentTurnTeamId === team.id ? `0 0 0 2px ${team.color}` : 'none' }"
            >
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
              <p
                class="text-2xl font-black text-primary"
                :style="{ color: team.color ?? undefined }"
              >
                {{ team.score }}
              </p>
              <UBadge
                v-if="session.pendingRound.currentTurnTeamId === team.id"
                color="secondary"
                class="mt-1 text-green-950 font-bold"
              >
                دوره الآن ◀
              </UBadge>
            </div>

            <div class="rounded-xl p-3 ring-1 ring-error/30 bg-error/5">
              <div class="flex items-center justify-between mb-1">
                <p class="font-bold text-error text-sm">
                  كومة الأخطاء
                </p>
                <UBadge color="error">
                  {{ mistakes.length }}
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
                <li
                  v-for="g in [...mistakes].reverse()"
                  :key="g.sequenceNumber"
                  class="text-xs text-muted truncate"
                >
                  {{ g.teamName }}: {{ g.guessText }}
                </li>
              </ul>
            </div>
          </aside>

          <main class="flex-1 p-3 sm:p-4 overflow-y-auto">
            <div class="flex items-center justify-between mb-3">
              <p class="font-bold text-green-900 dark:text-green-100">
                {{ session.pendingRound.listTitle }}
              </p>
              <p class="text-sm text-muted">
                {{ session.pendingRound.itemCount }} / {{ discoveredItems.length }}
              </p>
            </div>

            <p
              v-if="discoveredItems.length === 0"
              class="text-muted text-sm text-center py-12"
            >
              لم يتم اكتشاف أي عنصر بعد
            </p>
            <ol class="space-y-1">
              <li
                v-for="item in discoveredItems"
                :key="item.sequenceNumber"
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
              </li>
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
          <p
            v-if="lastFeedback"
            class="text-center font-bold text-sm"
            :class="lastFeedback.matched ? 'text-primary' : 'text-muted'"
          >
            <template v-if="lastFeedback.matched">
              ✅ {{ lastFeedback.label }}
            </template>
            <template v-else>
              ❌ لا يوجد تطابق
            </template>
          </p>
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
              class="font-bold text-green-950"
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
