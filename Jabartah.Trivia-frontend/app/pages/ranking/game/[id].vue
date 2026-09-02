<script setup lang="ts">
import type { RankingItemOptionDto, RankingSessionDto, SubmitRankingRoundResult } from '~/types/api'

definePageMeta({ layout: false })

const route = useRoute()
const sessionId = route.params.id as string

const { getRankingSession, startNextRankingRound, submitRankingRound } = useApi()

const session = ref<RankingSessionDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

const currentRound = ref<{ roundId: string, teamName: string, listTitle: string } | null>(null)
const pool = ref<RankingItemOptionDto[]>([])
const placed = ref<RankingItemOptionDto[]>([])
const roundResult = ref<SubmitRankingRoundResult | null>(null)

const starting = ref(false)
const submitting = ref(false)

async function loadSession() {
  try {
    session.value = await getRankingSession(sessionId)
    if (session.value.pendingRound && !currentRound.value) {
      const p = session.value.pendingRound
      currentRound.value = { roundId: p.roundId, teamName: p.teamName, listTitle: p.listTitle }
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
    currentRound.value = { roundId: result.roundId, teamName: result.teamName, listTitle: result.listTitle }
    pool.value = [...result.items]
    placed.value = []
    roundResult.value = null
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
    roundResult.value = await submitRankingRound(sessionId, currentRound.value.roundId, placed.value.map(i => i.id))
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
  await loadSession()
}

const winnerResult = computed(() => session.value ? getWinner(session.value.teams) : null)
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
                <p class="text-center font-bold text-lg">
                  {{ currentRound.teamName }}
                </p>
                <p class="text-center text-muted">
                  {{ currentRound.listTitle }}
                </p>
              </template>

              <div class="space-y-4">
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
                    <li
                      v-for="(item, index) in placed"
                      :key="item.id"
                      class="flex items-center gap-2 bg-primary/10 rounded-lg px-3 py-2 font-bold"
                    >
                      <span class="text-primary">{{ index + 1 }}</span>
                      <span>{{ item.label }}</span>
                    </li>
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
                    :disabled="placed.length === 0"
                    @click="undoLast"
                  >
                    تراجع
                  </UButton>
                  <UButton
                    color="neutral"
                    variant="ghost"
                    :disabled="placed.length === 0"
                    @click="resetOrder"
                  >
                    إعادة
                  </UButton>
                  <UButton
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
