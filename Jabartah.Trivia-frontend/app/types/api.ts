export interface CategoryDto {
  id: string
  name: string
  icon: string | null
}

export interface TeamDto {
  id: string
  name: string
  score: number
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
  correctAnswer: string
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
