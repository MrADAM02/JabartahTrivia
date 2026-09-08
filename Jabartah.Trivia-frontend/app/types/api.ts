// Auth / account

export interface AuthResult {
  token: string
  userId: string
  name: string
  email: string
  role: 'User' | 'Admin'
}

export interface AccountDto {
  name: string
  email: string
  role: 'User' | 'Admin'
  gamesPlayedCount: number
}

export interface MyTeamDto {
  name: string
  score: number
}

export interface MySessionDto {
  id: string
  mode: string
  createdAt: string
  completedAt: string | null
  teams: MyTeamDto[]
  winnerTeamNames: string[]
  isDraw: boolean
}

export interface CategoryDto {
  id: string
  name: string
  icon: string | null
}

// Team setup (name + color + badge, chosen at إعداد اللعبة)

export interface TeamSetupInput {
  name: string
  color: string | null
  icon: string | null
}

// تصنيفاتي (custom Trivia categories)

export interface CustomQuestionInput {
  pointValue: number
  prompt: string
  answer: string
}

export interface CreateCustomCategoryResult {
  categoryId: string
}

export interface MyCategoryQuestionDto {
  id: string
  pointValue: number
  prompt: string
  answer: string
}

export interface MyCategoryDetailDto {
  id: string
  name: string
  icon: string | null
  questions: MyCategoryQuestionDto[]
}

export interface TeamDto {
  id: string
  name: string
  score: number
  doublePointsAvailable: boolean
  twoAnswersAvailable: boolean
  color: string | null
  icon: string | null
  halfOpponentTimerAvailable: boolean
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
  currentTurnTeamId: string
  currentTurnTeamName: string
  pendingTimerDebuffTeamId: string | null
}

export interface ActivateTimerDebuffResult {
  debuffedTeamId: string
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

export interface RevealAnswerResult {
  answer: string
}

export interface AwardPointsResult {
  teams: TeamDto[]
  correctAnswer: string | null
}

export interface EndGameSessionResult {
  teams: TeamDto[]
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
  color: string | null
  icon: string | null
  extraTimeAvailable: boolean
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

export interface UseExtraTimeResult {
  extraTimeAvailable: boolean
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
  color: string | null
  icon: string | null
  revealPositionAvailable: boolean
}

export interface RevealRankingPositionResult {
  position: number
  itemLabel: string
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
  description: string | null
  itemCount: number
}

export interface Top100TeamDto {
  id: string
  name: string
  score: number
  color: string | null
  icon: string | null
}

export interface CreateTop100GameSessionResult {
  top100GameSessionId: string
  teams: Top100TeamDto[]
}

// One entry per guess attempt, correct or not -- filtered client-side into the
// discovered-items list (matched) and the shared mistakes pile (not matched).
export interface Top100GuessLogEntryDto {
  sequenceNumber: number
  teamId: string
  teamName: string
  guessText: string
  matched: boolean
  matchedLabel: string | null
  matchedPosition: number | null
}

export interface Top100PendingRoundDto {
  roundId: string
  listTitle: string
  itemCount: number
  maxGuesses: number
  guessesMade: number
  currentTurnTeamId: string
  currentTurnTeamName: string
  guesses: Top100GuessLogEntryDto[]
}

export interface Top100CompletedRoundSummaryDto {
  listTitle: string
  guesses: Top100GuessLogEntryDto[]
}

export interface Top100SessionDto {
  id: string
  status: string
  guessesPerTeam: number
  teams: Top100TeamDto[]
  pendingRound: Top100PendingRoundDto | null
  completedRound: Top100CompletedRoundSummaryDto | null
}

export interface StartNextTop100RoundResult {
  roundId: string
  currentTurnTeamId: string
  currentTurnTeamName: string
  listTitle: string
  itemCount: number
  maxGuesses: number
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
  sessionComplete: boolean
  teams: Top100TeamDto[]
}

// Admin panel

export interface ModeStatsDto {
  total: number
  completed: number
  inProgress: number
}

export interface TopCategoryDto {
  categoryId: string
  name: string
  icon: string | null
  timesPlayed: number
}

export interface DashboardStatsDto {
  totalUsers: number
  totalGames: number
  completedGames: number
  trivia: ModeStatsDto
  password: ModeStatsDto
  ranking: ModeStatsDto
  top100: ModeStatsDto
  topCategories: TopCategoryDto[]
}

export interface AdminCategoryDto {
  id: string
  name: string
  icon: string | null
  questionCount: number
}

export interface AdminQuestionDto {
  id: string
  pointValue: number
  prompt: string
  answer: string
  mediaUrl: string | null
}

// Admin: كلمة السر content

export interface AdminPasswordCategoryDto {
  id: string
  name: string
  icon: string | null
  wordCount: number
}

export interface AdminWordDto {
  id: string
  word: string
}

// Admin: رتبها content

export interface AdminRankingCategoryDto {
  id: string
  name: string
  icon: string | null
  listCount: number
}

export interface AdminRankingListDto {
  id: string
  title: string
  itemCount: number
}

export interface AdminRankingListItemDto {
  id: string
  label: string
  correctPosition: number
}

export interface AdminRankingListDetailDto {
  id: string
  title: string
  categoryId: string
  items: AdminRankingListItemDto[]
}

// Admin: تحدي الـ100 content

export interface AdminTop100CategoryDto {
  id: string
  name: string
  icon: string | null
  description: string | null
  listCount: number
}

export interface AdminTop100ListDto {
  id: string
  title: string
  itemCount: number
}

export interface AdminTop100ListItemDto {
  id: string
  label: string
  position: number
  alternateSpellings: string[]
}

export interface AdminTop100ListDetailDto {
  id: string
  title: string
  categoryId: string
  items: AdminTop100ListItemDto[]
}
