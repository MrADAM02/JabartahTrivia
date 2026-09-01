export interface WinnerResult<T extends { score: number }> {
  isDraw: boolean
  winners: T[] // 1 team normally, 2+ on a tie
  topScore: number
}

export function getWinner<T extends { score: number }>(teams: T[]): WinnerResult<T> | null {
  if (teams.length === 0) return null
  const topScore = Math.max(...teams.map(t => t.score))
  const winners = teams.filter(t => t.score === topScore)
  return { isDraw: winners.length > 1, winners, topScore }
}
