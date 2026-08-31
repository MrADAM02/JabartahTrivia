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
  SubmitRankingRoundResult
} from '~/types/api'

export function useApi() {
  const { public: { apiBase } } = useRuntimeConfig()

  const listCategories = () => $fetch<CategoryDto[]>('/api/categories', { baseURL: apiBase })

  const createGameSession = (teamNames: string[], categoryIds: string[]) =>
    $fetch<CreateGameSessionResult>('/api/game-sessions', {
      baseURL: apiBase,
      method: 'POST',
      body: { teamNames, categoryIds }
    })

  const getBoard = (gameSessionId: string) =>
    $fetch<BoardDto>(`/api/game-sessions/${gameSessionId}/board`, { baseURL: apiBase })

  const selectQuestion = (gameSessionId: string, questionId: string) =>
    $fetch<SelectQuestionResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/select`,
      { baseURL: apiBase, method: 'POST' }
    )

  const awardPoints = (gameSessionId: string, questionId: string, winningTeamId: string | null) =>
    $fetch<AwardPointsResult>(
      `/api/game-sessions/${gameSessionId}/questions/${questionId}/award`,
      { baseURL: apiBase, method: 'POST', body: { winningTeamId } }
    )

  // Password game (كلمة السر)

  const listPasswordCategories = () => $fetch<PasswordCategoryDto[]>('/api/password-categories', { baseURL: apiBase })

  const createPasswordGameSession = (teamNames: string[], categoryIds: string[]) =>
    $fetch<CreatePasswordGameSessionResult>('/api/password-sessions', {
      baseURL: apiBase,
      method: 'POST',
      body: { teamNames, categoryIds }
    })

  const getPasswordSession = (sessionId: string) =>
    $fetch<PasswordSessionDto>(`/api/password-sessions/${sessionId}`, { baseURL: apiBase })

  const startNextPasswordRound = (sessionId: string) =>
    $fetch<StartNextPasswordRoundResult>(
      `/api/password-sessions/${sessionId}/rounds/next`,
      { baseURL: apiBase, method: 'POST' }
    )

  const issueRevealToken = (sessionId: string, roundId: string) =>
    $fetch<IssueRevealTokenResult>(
      `/api/password-sessions/${sessionId}/rounds/${roundId}/reveal-token`,
      { baseURL: apiBase, method: 'POST' }
    )

  const resolvePasswordRound = (sessionId: string, roundId: string, correct: boolean) =>
    $fetch<ResolvePasswordRoundResult>(
      `/api/password-sessions/${sessionId}/rounds/${roundId}/resolve`,
      { baseURL: apiBase, method: 'POST', body: { correct } }
    )

  const consumeRevealToken = (token: string) =>
    $fetch<ConsumeRevealTokenResult>(`/api/reveal/${token}`, { baseURL: apiBase, method: 'POST' })

  // Ranking game (رتبها)

  const listRankingCategories = () => $fetch<RankingCategoryDto[]>('/api/ranking-categories', { baseURL: apiBase })

  const createRankingGameSession = (teamNames: string[], categoryIds: string[]) =>
    $fetch<CreateRankingGameSessionResult>('/api/ranking-sessions', {
      baseURL: apiBase,
      method: 'POST',
      body: { teamNames, categoryIds }
    })

  const getRankingSession = (sessionId: string) =>
    $fetch<RankingSessionDto>(`/api/ranking-sessions/${sessionId}`, { baseURL: apiBase })

  const startNextRankingRound = (sessionId: string) =>
    $fetch<StartNextRankingRoundResult>(
      `/api/ranking-sessions/${sessionId}/rounds/next`,
      { baseURL: apiBase, method: 'POST' }
    )

  const submitRankingRound = (sessionId: string, roundId: string, orderedItemIds: string[]) =>
    $fetch<SubmitRankingRoundResult>(
      `/api/ranking-sessions/${sessionId}/rounds/${roundId}/submit`,
      { baseURL: apiBase, method: 'POST', body: { orderedItemIds } }
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
    submitRankingRound
  }
}
