import type {
  AccountDto,
  ActivateTimerDebuffResult,
  AuthResult,
  AwardPointsResult,
  BoardDto,
  CategoryDto,
  ConsumeRevealTokenResult,
  CreateCustomCategoryResult,
  CreateGameSessionResult,
  CreatePasswordGameSessionResult,
  CreateRankingGameSessionResult,
  CustomQuestionInput,
  IssueRevealTokenResult,
  MyCategoryDetailDto,
  MySessionDto,
  RevealAnswerResult,
  PasswordCategoryDto,
  PasswordSessionDto,
  RankingCategoryDto,
  RankingSessionDto,
  ResolvePasswordRoundResult,
  SelectQuestionResult,
  StartNextPasswordRoundResult,
  StartNextRankingRoundResult,
  StartNextTop100RoundResult,
  SubmitGuessResult,
  SubmitRankingRoundResult,
  TeamSetupInput,
  RevealRankingPositionResult,
  Top100CategoryDto,
  Top100SessionDto,
  CreateTop100GameSessionResult,
  UseExtraTimeResult
} from '~/types/api'

export function useApi() {
  const { public: { apiPort } } = useRuntimeConfig()
  const apiBase = computed(() => `http://${window.location.hostname}:${apiPort}`)
  const { token } = useAuth()

  const api = $fetch.create({
    baseURL: apiBase.value,
    onRequest({ options }) {
      if (token.value) {
        options.headers = new Headers(options.headers)
        options.headers.set('Authorization', `Bearer ${token.value}`)
      }
    }
  })

  // Auth / account

  const register = (name: string, email: string, password: string) =>
    api<AuthResult>('/api/auth/register', { method: 'POST', body: { name, email, password } })

  const login = (email: string, password: string) =>
    api<AuthResult>('/api/auth/login', { method: 'POST', body: { email, password } })

  const getAccount = () => api<AccountDto>('/api/account')

  const deleteAccount = () => api('/api/account', { method: 'DELETE' })

  const getMySessions = () => api<MySessionDto[]>('/api/my-sessions')

  // تصنيفاتي (custom Trivia categories)

  const listMyCategories = () => api<CategoryDto[]>('/api/my-categories')

  const createMyCategory = (name: string, icon: string | null, questions: CustomQuestionInput[]) =>
    api<CreateCustomCategoryResult>('/api/my-categories', { method: 'POST', body: { name, icon, questions } })

  const getMyCategory = (categoryId: string) =>
    api<MyCategoryDetailDto>(`/api/my-categories/${categoryId}`)

  const deleteMyCategory = (categoryId: string) =>
    api(`/api/my-categories/${categoryId}`, { method: 'DELETE' })

  // Trivia board

  const listCategories = () => api<CategoryDto[]>('/api/categories')

  const createGameSession = (teams: TeamSetupInput[], categoryIds: string[]) =>
    api<CreateGameSessionResult>('/api/game-sessions', { method: 'POST', body: { teams, categoryIds } })

  const getBoard = (gameSessionId: string) =>
    api<BoardDto>(`/api/game-sessions/${gameSessionId}/board`)

  const selectQuestion = (
    gameSessionId: string,
    questionId: string,
    activatingTeamId: string | null = null,
    powerUp: 'DoublePoints' | 'TwoAnswers' | null = null
  ) =>
    api<SelectQuestionResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/select`,
      { method: 'POST', body: { activatingTeamId, powerUp } }
    )

  const awardPoints = (gameSessionId: string, questionId: string, winningTeamId: string | null) =>
    api<AwardPointsResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/award`,
      { method: 'POST', body: { winningTeamId } }
    )

  const revealAnswer = (gameSessionId: string, questionId: string) =>
    api<RevealAnswerResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/reveal-answer`,
      { method: 'POST' }
    )

  const activateTimerDebuff = (gameSessionId: string, teamId: string) =>
    api<ActivateTimerDebuffResult>(
      `/api/game-sessions/${gameSessionId}/teams/${teamId}/timer-debuff`,
      { method: 'POST' }
    )

  // Password game (كلمة السر)

  const listPasswordCategories = () => api<PasswordCategoryDto[]>('/api/password-categories')

  const createPasswordGameSession = (teams: TeamSetupInput[], categoryIds: string[], roundsPerTeam: number) =>
    api<CreatePasswordGameSessionResult>('/api/password-sessions', {
      method: 'POST',
      body: { teams, categoryIds, roundsPerTeam }
    })

  const getPasswordSession = (sessionId: string) =>
    api<PasswordSessionDto>(`/api/password-sessions/${sessionId}`)

  const startNextPasswordRound = (sessionId: string) =>
    api<StartNextPasswordRoundResult>(`/api/password-sessions/${sessionId}/rounds/next`, { method: 'POST' })

  const issueRevealToken = (sessionId: string, roundId: string) =>
    api<IssueRevealTokenResult>(`/api/password-sessions/${sessionId}/rounds/${roundId}/reveal-token`, { method: 'POST' })

  const resolvePasswordRound = (sessionId: string, roundId: string, correct: boolean) =>
    api<ResolvePasswordRoundResult>(`/api/password-sessions/${sessionId}/rounds/${roundId}/resolve`, {
      method: 'POST',
      body: { correct }
    })

  const consumeRevealToken = (revealToken: string) =>
    api<ConsumeRevealTokenResult>(`/api/reveal/${revealToken}`, { method: 'POST' })

  const useExtraTime = (sessionId: string, teamId: string) =>
    api<UseExtraTimeResult>(`/api/password-sessions/${sessionId}/teams/${teamId}/extra-time`, { method: 'POST' })

  // Ranking game (رتبها)

  const listRankingCategories = () => api<RankingCategoryDto[]>('/api/ranking-categories')

  const createRankingGameSession = (teams: TeamSetupInput[], categoryIds: string[], roundsPerTeam: number) =>
    api<CreateRankingGameSessionResult>('/api/ranking-sessions', {
      method: 'POST',
      body: { teams, categoryIds, roundsPerTeam }
    })

  const getRankingSession = (sessionId: string) =>
    api<RankingSessionDto>(`/api/ranking-sessions/${sessionId}`)

  const startNextRankingRound = (sessionId: string) =>
    api<StartNextRankingRoundResult>(`/api/ranking-sessions/${sessionId}/rounds/next`, { method: 'POST' })

  const submitRankingRound = (sessionId: string, roundId: string, orderedItemIds: string[]) =>
    api<SubmitRankingRoundResult>(`/api/ranking-sessions/${sessionId}/rounds/${roundId}/submit`, {
      method: 'POST',
      body: { orderedItemIds }
    })

  const revealRankingPosition = (sessionId: string, roundId: string, teamId: string) =>
    api<RevealRankingPositionResult>(`/api/ranking-sessions/${sessionId}/rounds/${roundId}/reveal-position`, {
      method: 'POST',
      body: { teamId }
    })

  // تحدي الـ100

  const listTop100Categories = () => api<Top100CategoryDto[]>('/api/top100-categories')

  const createTop100GameSession = (teams: TeamSetupInput[], categoryIds: string[], guessesPerTeam: number) =>
    api<CreateTop100GameSessionResult>('/api/top100-sessions', {
      method: 'POST',
      body: { teams, categoryIds, guessesPerTeam }
    })

  const getTop100Session = (sessionId: string) =>
    api<Top100SessionDto>(`/api/top100-sessions/${sessionId}`)

  const startNextTop100Round = (sessionId: string) =>
    api<StartNextTop100RoundResult>(`/api/top100-sessions/${sessionId}/rounds/next`, { method: 'POST' })

  const submitGuess = (sessionId: string, roundId: string, guessText: string) =>
    api<SubmitGuessResult>(`/api/top100-sessions/${sessionId}/rounds/${roundId}/guess`, {
      method: 'POST',
      body: { guessText }
    })

  return {
    register,
    login,
    getAccount,
    deleteAccount,
    getMySessions,
    listMyCategories,
    createMyCategory,
    getMyCategory,
    deleteMyCategory,
    listCategories,
    createGameSession,
    getBoard,
    selectQuestion,
    awardPoints,
    revealAnswer,
    activateTimerDebuff,
    listPasswordCategories,
    createPasswordGameSession,
    getPasswordSession,
    startNextPasswordRound,
    issueRevealToken,
    resolvePasswordRound,
    consumeRevealToken,
    useExtraTime,
    listRankingCategories,
    createRankingGameSession,
    getRankingSession,
    startNextRankingRound,
    submitRankingRound,
    revealRankingPosition,
    listTop100Categories,
    createTop100GameSession,
    getTop100Session,
    startNextTop100Round,
    submitGuess
  }
}
