import type {
  AwardPointsResult,
  BoardDto,
  CategoryDto,
  CreateGameSessionResult,
  SelectQuestionResult
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

  return { listCategories, createGameSession, getBoard, selectQuestion, awardPoints }
}
