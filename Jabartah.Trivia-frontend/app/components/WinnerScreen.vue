<script setup lang="ts">
import type { WinnerResult } from '~/composables/useWinner'
import { DURATIONS } from '~/utils/motion'

interface WinnerScreenTeam {
  id: string
  name: string
  score: number
  color?: string | null
  icon?: string | null
}

const props = defineProps<{
  winner: WinnerResult<WinnerScreenTeam> | null
  teams: WinnerScreenTeam[]
}>()

// Final scoreboard -- without this, a losing team's score simply vanishes
// once the game ends (the announcement above only ever shows the winner).
const rankedTeams = computed(() => [...props.teams].sort((a, b) => b.score - a.score))
function isLeader(team: WinnerScreenTeam) {
  return props.winner ? team.score === props.winner.topScore : false
}

const { motionTier } = useResponsiveMotion()
const { pieces: confettiPieces } = useConfettiBurst()

// Safe as onMounted (not a watcher) -- this component only ever mounts once
// a page's own completion v-if flips true, so "just mounted" already means
// "the game just ended," matching the old per-page watch(...)-based trigger.
const showCelebration = ref(false)
onMounted(() => {
  if (motionTier.value === 'full') showCelebration.value = true
})
</script>

<template>
  <div class="flex-1 p-4 sm:p-6 space-y-6 relative overflow-hidden flex flex-col items-center">
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
      <UCard class="w-full max-w-xl">
        <!-- The wrapper below is the actual fix: without it, the h1/p siblings
             sat directly in MotionScale's single motion.div with no gap of
             their own, letting a font-black text-6xl/7xl heading visually
             collide with the score line beneath it. -->
        <div class="flex flex-col items-center gap-3 text-center">
          <template v-if="winner?.isDraw">
            <p class="text-2xl sm:text-3xl font-bold text-muted">
              🤝 تعادل
            </p>
            <h1 class="text-3xl sm:text-5xl font-black text-primary leading-tight text-balance">
              {{ winner.winners.map(w => w.name).join(' و ') }}
            </h1>
            <p class="text-2xl sm:text-3xl font-bold">
              {{ winner.topScore }} نقطة
            </p>
          </template>
          <template v-else>
            <p class="text-2xl sm:text-3xl font-bold text-muted">
              🎉 الفائز 🎉
            </p>
            <h1
              class="text-4xl sm:text-6xl font-black text-primary leading-tight text-balance"
              :style="{ color: winner?.winners[0]?.color ?? undefined }"
            >
              {{ winner?.winners[0]?.name }}
            </h1>
            <p class="text-2xl sm:text-3xl font-bold">
              {{ winner?.winners[0]?.score }} نقطة
            </p>
          </template>
        </div>
      </UCard>
    </MotionScale>

    <UCard class="w-full max-w-xl">
      <template #header>
        <p class="text-center font-bold text-sm text-muted">
          النتيجة النهائية
        </p>
      </template>
      <ol class="space-y-1.5">
        <li
          v-for="(team, index) in rankedTeams"
          :key="team.id"
          class="flex items-center gap-3 rounded-lg px-3 py-2"
          :class="isLeader(team) ? 'bg-primary/10' : ''"
        >
          <span class="font-black text-muted w-5 text-center shrink-0">{{ index + 1 }}</span>
          <span
            v-if="team.icon"
            class="size-7 rounded-full flex items-center justify-center shrink-0"
            :style="{ backgroundColor: team.color ?? '#123A24' }"
          >
            <UIcon
              :name="team.icon"
              class="size-4 text-white"
            />
          </span>
          <span class="flex-1 font-bold truncate">{{ team.name }}</span>
          <span
            class="font-black"
            :class="isLeader(team) ? 'text-primary' : 'text-muted'"
            :style="{ color: isLeader(team) ? (team.color ?? undefined) : undefined }"
          >
            {{ team.score }}
          </span>
        </li>
      </ol>
    </UCard>

    <slot name="summary" />

    <UButton
      size="xl"
      to="/"
    >
      لعبة جديدة
    </UButton>
  </div>
</template>
