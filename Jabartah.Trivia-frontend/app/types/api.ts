export interface CategoryDto {
  id: string
  name: string
  icon: string | null
}

export interface TeamDto {
  id: string
  name: string
  score: number
  doublePointsAvailable: boolean
  twoAnswersAvailable: boolean
}

export interface BoardCellDto {
  questionId: string
  pointValue: number
  isRevealed: boolean
  wonByTeamId: string | null
}

export interface CategoryColumnDto {
  categoryId: string
  name: string
  icon: string | null
  cells: BoardCellDto[]
}

export interface BoardDto {
  gameSessionId: string
  teams: TeamDto[]
  categories: CategoryColumnDto[]
}

export interface CreateGameSessionResult {
  gameSessionId: string
  teams: TeamDto[]
}

export interface SelectQuestionResult {
  questionId: string
  pointValue: number
  prompt: string
  mediaUrl: string | null
}

export interface AwardPointsResult {
  teams: TeamDto[]
  correctAnswer: string | null
  canRetry: boolean
  retryTeamId: string | null
  retryTeamName: string | null
}

// Password game (كلمة السر)

export interface PasswordCategoryDto {
  id: string
  name: string
  icon: string | null
}

export interface PasswordTeamDto {
  id: string
  name: string
  score: number
}

export interface CreatePasswordGameSessionResult {
  passwordGameSessionId: string
  teams: PasswordTeamDto[]
}

export interface PasswordPendingRoundDto {
  roundId: string
  teamId: string
  teamName: string
  roundNumber: number
}

export interface PasswordSessionDto {
  id: string
  status: string
  teams: PasswordTeamDto[]
  roundsPlayed: number
  totalRounds: number
  pendingRound: PasswordPendingRoundDto | null
}

export interface StartNextPasswordRoundResult {
  roundId: string
  teamId: string
  teamName: string
  roundNumber: number
  totalRounds: number
}

export interface IssueRevealTokenResult {
  token: string
  expiresAt: string
}

export interface ResolvePasswordRoundResult {
  teams: PasswordTeamDto[]
  isSessionComplete: boolean
}

export interface ConsumeRevealTokenResult {
  success: boolean
  expired: boolean
  alreadyConsumed: boolean
  word: string | null
  categoryName: string | null
}

// Ranking game (رتبها)

export interface RankingCategoryDto {
  id: string
  name: string
  icon: string | null
}

export interface RankingTeamDto {
  id: string
  name: string
  score: number
}

export interface RankingItemOptionDto {
  id: string
  label: string
}

export interface CreateRankingGameSessionResult {
  rankingGameSessionId: string
  teams: RankingTeamDto[]
}

export interface RankingPendingRoundDto {
  roundId: string
  teamId: string
  teamName: string
  roundNumber: number
  listTitle: string
  items: RankingItemOptionDto[]
}

export interface RankingSessionDto {
  id: string
  status: string
  teams: RankingTeamDto[]
  roundsPlayed: number
  totalRounds: number
  pendingRound: RankingPendingRoundDto | null
}

export interface StartNextRankingRoundResult {
  roundId: string
  teamId: string
  teamName: string
  roundNumber: number
  totalRounds: number
  listTitle: string
  items: RankingItemOptionDto[]
}

export interface RankingItemResultDto {
  id: string
  label: string
}

export interface SubmitRankingRoundResult {
  pointsAwarded: number
  correctOrder: RankingItemResultDto[]
  teams: RankingTeamDto[]
  isSessionComplete: boolean
}

// تحدي الـ100

export interface Top100CategoryDto {
  id: string
  name: string
  icon: string | null
}

export interface Top100TeamDto {
  id: string
  name: string
  score: number
}

export interface CreateTop100GameSessionResult {
  top100GameSessionId: string
  teams: Top100TeamDto[]
}

export interface Top100GuessedItemDto {
  id: string
  label: string
  position: number
}

export interface Top100PendingRoundDto {
  roundId: string
  listTitle: string
  itemCount: number
  maxGuesses: number
  guessesMade: number
  currentTurnTeamId: string
  currentTurnTeamName: string
  guessedItems: Top100GuessedItemDto[]
}

export interface Top100SessionDto {
  id: string
  status: string
  roundsPerTeam: number
  teams: Top100TeamDto[]
  roundsPlayed: number
  totalRounds: number
  pendingRound: Top100PendingRoundDto | null
}

export interface StartNextTop100RoundResult {
  roundId: string
  currentTurnTeamId: string
  currentTurnTeamName: string
  roundNumber: number
  totalRounds: number
  listTitle: string
  itemCount: number
  maxGuesses: number
}

export interface Top100RevealedItemDto {
  id: string
  label: string
  position: number
  wasGuessed: boolean
}

export interface SubmitGuessResult {
  matched: boolean
  matchedItemId: string | null
  matchedLabel: string | null
  matchedPosition: number | null
  pointsAwarded: number
  guessingTeamId: string
  guessingTeamName: string
  nextTurnTeamId: string
  nextTurnTeamName: string
  roundComplete: boolean
  fullList: Top100RevealedItemDto[] | null
  teams: Top100TeamDto[]
  isSessionComplete: boolean
}
