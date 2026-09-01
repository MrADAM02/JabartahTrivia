import type {
  AwardPointsResult,
  BoardDto,
  CategoryDto,
  ConsumeRevealTokenResult,
  CreateGameSessionResult,
  CreatePasswordGameSessionResult,
  CreateRankingGameSessionResult,
  IssueRevealTokenResult,
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
  Top100CategoryDto,
  Top100SessionDto,
  CreateTop100GameSessionResult
} from '~/types/api'

export function useApi() {
  const { public: { apiPort } } = useRuntimeConfig()
  const apiBase = computed(() => `http://${window.location.hostname}:${apiPort}`)

  const listCategories = () => $fetch<CategoryDto[]>('/api/categories', { baseURL: apiBase.value })

  const createGameSession = (teamNames: string[], categoryIds: string[]) =>
    $fetch<CreateGameSessionResult>('/api/game-sessions', {
      baseURL: apiBase.value,
      method: 'POST',
      body: { teamNames, categoryIds }
    })

  const getBoard = (gameSessionId: string) =>
    $fetch<BoardDto>(`/api/game-sessions/${gameSessionId}/board`, { baseURL: apiBase.value })

  const selectQuestion = (
    gameSessionId: string,
    questionId: string,
    activatingTeamId: string | null = null,
    powerUp: 'DoublePoints' | 'TwoAnswers' | null = null
  ) =>
    $fetch<SelectQuestionResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/select`,
      { baseURL: apiBase.value, method: 'POST', body: { activatingTeamId, powerUp } }
    )

  const awardPoints = (gameSessionId: string, questionId: string, winningTeamId: string | null) =>
    $fetch<AwardPointsResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/award`,
      { baseURL: apiBase.value, method: 'POST', body: { winningTeamId } }
    )

  // Password game (كلمة السر)

  const listPasswordCategories = () => $fetch<PasswordCategoryDto[]>('/api/password-categories', { baseURL: apiBase.value })

  const createPasswordGameSession = (teamNames: string[], categoryIds: string[], roundsPerTeam: number) =>
    $fetch<CreatePasswordGameSessionResult>('/api/password-sessions', {
      baseURL: apiBase.value,
      method: 'POST',
      body: { teamNames, categoryIds, roundsPerTeam }
    })

  const getPasswordSession = (sessionId: string) =>
    $fetch<PasswordSessionDto>(`/api/password-sessions/${sessionId}`, { baseURL: apiBase.value })

  const startNextPasswordRound = (sessionId: string) =>
    $fetch<StartNextPasswordRoundResult>(
      `/api/password-sessions/${sessionId}/rounds/next`,
      { baseURL: apiBase.value, method: 'POST' }
    )

  const issueRevealToken = (sessionId: string, roundId: string) =>
    $fetch<IssueRevealTokenResult>(
      `/api/password-sessions/${sessionId}/rounds/${roundId}/reveal-token`,
      { baseURL: apiBase.value, method: 'POST' }
    )

  const resolvePasswordRound = (sessionId: string, roundId: string, correct: boolean) =>
    $fetch<ResolvePasswordRoundResult>(
      `/api/password-sessions/${sessionId}/rounds/${roundId}/resolve`,
      { baseURL: apiBase.value, method: 'POST', body: { correct } }
    )

  const consumeRevealToken = (token: string) =>
    $fetch<ConsumeRevealTokenResult>(`/api/reveal/${token}`, { baseURL: apiBase.value, method: 'POST' })

  // Ranking game (رتبها)

  const listRankingCategories = () => $fetch<RankingCategoryDto[]>('/api/ranking-categories', { baseURL: apiBase.value })

  const createRankingGameSession = (teamNames: string[], categoryIds: string[], roundsPerTeam: number) =>
    $fetch<CreateRankingGameSessionResult>('/api/ranking-sessions', {
      baseURL: apiBase.value,
      method: 'POST',
      body: { teamNames, categoryIds, roundsPerTeam }
    })

  const getRankingSession = (sessionId: string) =>
    $fetch<RankingSessionDto>(`/api/ranking-sessions/${sessionId}`, { baseURL: apiBase.value })

  const startNextRankingRound = (sessionId: string) =>
    $fetch<StartNextRankingRoundResult>(
      `/api/ranking-sessions/${sessionId}/rounds/next`,
      { baseURL: apiBase.value, method: 'POST' }
    )

  const submitRankingRound = (sessionId: string, roundId: string, orderedItemIds: string[]) =>
    $fetch<SubmitRankingRoundResult>(
      `/api/ranking-sessions/${sessionId}/rounds/${roundId}/submit`,
      { baseURL: apiBase.value, method: 'POST', body: { orderedItemIds } }
    )

  // تحدي الـ100

  const listTop100Categories = () => $fetch<Top100CategoryDto[]>('/api/top100-categories', { baseURL: apiBase.value })

  const createTop100GameSession = (teamNames: string[], categoryIds: string[], roundsPerTeam: number) =>
    $fetch<CreateTop100GameSessionResult>('/api/top100-sessions', {
      baseURL: apiBase.value,
      method: 'POST',
      body: { teamNames, categoryIds, roundsPerTeam }
    })

  const getTop100Session = (sessionId: string) =>
    $fetch<Top100SessionDto>(`/api/top100-sessions/${sessionId}`, { baseURL: apiBase.value })

  const startNextTop100Round = (sessionId: string) =>
    $fetch<StartNextTop100RoundResult>(
      `/api/top100-sessions/${sessionId}/rounds/next`,
      { baseURL: apiBase.value, method: 'POST' }
    )

  const submitGuess = (sessionId: string, roundId: string, guessText: string) =>
    $fetch<SubmitGuessResult>(
      `/api/top100-sessions/${sessionId}/rounds/${roundId}/guess`,
      { baseURL: apiBase.value, method: 'POST', body: { guessText } }
    )

  return {
    listCategories,
    createGameSession,
    getBoard,
    selectQuestion,
    awardPoints,
    listPasswordCategories,
    createPasswordGameSession,
    getPasswordSession,
    startNextPasswordRound,
    issueRevealToken,
    resolvePasswordRound,
    consumeRevealToken,
    listRankingCategories,
    createRankingGameSession,
    getRankingSession,
    startNextRankingRound,
    submitRankingRound,
    listTop100Categories,
    createTop100GameSession,
    getTop100Session,
    startNextTop100Round,
    submitGuess
  }
}
