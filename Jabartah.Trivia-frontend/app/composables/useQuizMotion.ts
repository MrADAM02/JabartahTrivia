import { DURATIONS } from '~/utils/motion'

// Bridges backend state changes to short-lived animation triggers. Instantiate
// fresh per game page (not a global singleton) so state never leaks across
// sessions. Score tweening itself lives in <MotionNumber> (bound directly to
// team.score) — this composable only orchestrates one-shot feedback and the
// client-side-only streak counter.
export function useQuizMotion() {
  const celebratingTeamId = ref<string | null>(null)
  const shakingKey = ref<string | null>(null)
  const streaks = reactive(new Map<string, number>())

  let celebrateTimer: ReturnType<typeof setTimeout> | undefined
  let shakeTimer: ReturnType<typeof setTimeout> | undefined

  function celebrateCorrect(teamId: string) {
    celebratingTeamId.value = teamId
    clearTimeout(celebrateTimer)
    celebrateTimer = setTimeout(() => {
      celebratingTeamId.value = null
    }, DURATIONS.slow * 1000)
  }

  function shake(key: string) {
    shakingKey.value = key
    clearTimeout(shakeTimer)
    shakeTimer = setTimeout(() => {
      shakingKey.value = null
    }, DURATIONS.slow * 1000)
  }

  // Consecutive-correct counter, session-only (not persisted) — resets to 0
  // on a wrong answer or a page refresh.
  function recordOutcome(teamId: string, correct: boolean) {
    const next = correct ? (streaks.get(teamId) ?? 0) + 1 : 0
    streaks.set(teamId, next)
    return next
  }

  function streakFor(teamId: string) {
    return streaks.get(teamId) ?? 0
  }

  onUnmounted(() => {
    clearTimeout(celebrateTimer)
    clearTimeout(shakeTimer)
  })

  return { celebratingTeamId, shakingKey, celebrateCorrect, shake, recordOutcome, streakFor }
}
