import type {
  AdminCategoryDto,
  AdminPasswordCategoryDto,
  AdminQuestionDto,
  AdminRankingCategoryDto,
  AdminRankingListDetailDto,
  AdminRankingListDto,
  AdminTop100CategoryDto,
  AdminTop100ListDetailDto,
  AdminTop100ListDto,
  AdminWordDto,
  DashboardStatsDto
} from '~/types/api'

// Separate from useApi.ts purely because of the endpoint count this section adds
// (categories + questions/words/lists CRUD across all 4 modes) -- same $fetch.create +
// bearer-token pattern either way.
export function useAdminApi() {
  const { public: { apiPort, apiBase: configuredApiBase } } = useRuntimeConfig()
  const apiBase = computed(() =>
    configuredApiBase || `http://${window.location.hostname}:${apiPort}`
  )
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

  const getDashboardStats = () => api<DashboardStatsDto>('/api/admin/stats')

  // Trivia: categories + questions

  const listAdminCategories = () => api<AdminCategoryDto[]>('/api/admin/categories')

  const createAdminCategory = (name: string, icon: string | null) =>
    api<{ categoryId: string }>('/api/admin/categories', { method: 'POST', body: { name, icon } })

  const updateAdminCategory = (categoryId: string, name: string, icon: string | null) =>
    api(`/api/admin/categories/${categoryId}`, { method: 'PUT', body: { name, icon } })

  const deleteAdminCategory = (categoryId: string) =>
    api(`/api/admin/categories/${categoryId}`, { method: 'DELETE' })

  const listAdminQuestions = (categoryId: string) =>
    api<AdminQuestionDto[]>(`/api/admin/categories/${categoryId}/questions`)

  const createAdminQuestion = (
    categoryId: string,
    pointValue: number,
    prompt: string,
    answer: string,
    mediaUrl: string | null
  ) =>
    api<{ questionId: string }>(`/api/admin/categories/${categoryId}/questions`, {
      method: 'POST',
      body: { pointValue, prompt, answer, mediaUrl }
    })

  const updateAdminQuestion = (
    questionId: string,
    pointValue: number,
    prompt: string,
    answer: string,
    mediaUrl: string | null
  ) =>
    api(`/api/admin/questions/${questionId}`, {
      method: 'PUT',
      body: { pointValue, prompt, answer, mediaUrl }
    })

  const deleteAdminQuestion = (questionId: string) =>
    api(`/api/admin/questions/${questionId}`, { method: 'DELETE' })

  // Password (كلمة السر): categories + words

  const listPasswordAdminCategories = () => api<AdminPasswordCategoryDto[]>('/api/admin/password/categories')

  const createPasswordAdminCategory = (name: string, icon: string | null) =>
    api<{ categoryId: string }>('/api/admin/password/categories', { method: 'POST', body: { name, icon } })

  const updatePasswordAdminCategory = (categoryId: string, name: string, icon: string | null) =>
    api(`/api/admin/password/categories/${categoryId}`, { method: 'PUT', body: { name, icon } })

  const deletePasswordAdminCategory = (categoryId: string) =>
    api(`/api/admin/password/categories/${categoryId}`, { method: 'DELETE' })

  const listAdminWords = (categoryId: string) =>
    api<AdminWordDto[]>(`/api/admin/password/categories/${categoryId}/words`)

  const createAdminWord = (categoryId: string, word: string) =>
    api<{ wordId: string }>(`/api/admin/password/categories/${categoryId}/words`, { method: 'POST', body: { word } })

  const updateAdminWord = (wordId: string, word: string) =>
    api(`/api/admin/password/words/${wordId}`, { method: 'PUT', body: { word } })

  const deleteAdminWord = (wordId: string) =>
    api(`/api/admin/password/words/${wordId}`, { method: 'DELETE' })

  // Ranking (رتبها): categories + lists + items

  const listRankingAdminCategories = () => api<AdminRankingCategoryDto[]>('/api/admin/ranking/categories')

  const createRankingAdminCategory = (name: string, icon: string | null) =>
    api<{ categoryId: string }>('/api/admin/ranking/categories', { method: 'POST', body: { name, icon } })

  const updateRankingAdminCategory = (categoryId: string, name: string, icon: string | null) =>
    api(`/api/admin/ranking/categories/${categoryId}`, { method: 'PUT', body: { name, icon } })

  const deleteRankingAdminCategory = (categoryId: string) =>
    api(`/api/admin/ranking/categories/${categoryId}`, { method: 'DELETE' })

  const listRankingAdminLists = (categoryId: string) =>
    api<AdminRankingListDto[]>(`/api/admin/ranking/categories/${categoryId}/lists`)

  const createRankingAdminList = (categoryId: string, title: string) =>
    api<{ listId: string }>(`/api/admin/ranking/categories/${categoryId}/lists`, { method: 'POST', body: { title } })

  const getRankingAdminList = (listId: string) =>
    api<AdminRankingListDetailDto>(`/api/admin/ranking/lists/${listId}`)

  const updateRankingAdminList = (listId: string, title: string) =>
    api(`/api/admin/ranking/lists/${listId}`, { method: 'PUT', body: { title } })

  const deleteRankingAdminList = (listId: string) =>
    api(`/api/admin/ranking/lists/${listId}`, { method: 'DELETE' })

  const createRankingAdminItem = (listId: string, label: string, correctPosition: number) =>
    api<{ itemId: string }>(`/api/admin/ranking/lists/${listId}/items`, {
      method: 'POST',
      body: { label, correctPosition }
    })

  const updateRankingAdminItem = (itemId: string, label: string, correctPosition: number) =>
    api(`/api/admin/ranking/items/${itemId}`, { method: 'PUT', body: { label, correctPosition } })

  const deleteRankingAdminItem = (itemId: string) =>
    api(`/api/admin/ranking/items/${itemId}`, { method: 'DELETE' })

  // تحدي الـ100: categories + lists + items

  const listTop100AdminCategories = () => api<AdminTop100CategoryDto[]>('/api/admin/top100/categories')

  const createTop100AdminCategory = (name: string, icon: string | null, description: string | null) =>
    api<{ categoryId: string }>('/api/admin/top100/categories', { method: 'POST', body: { name, icon, description } })

  const updateTop100AdminCategory = (categoryId: string, name: string, icon: string | null, description: string | null) =>
    api(`/api/admin/top100/categories/${categoryId}`, { method: 'PUT', body: { name, icon, description } })

  const deleteTop100AdminCategory = (categoryId: string) =>
    api(`/api/admin/top100/categories/${categoryId}`, { method: 'DELETE' })

  const listTop100AdminLists = (categoryId: string) =>
    api<AdminTop100ListDto[]>(`/api/admin/top100/categories/${categoryId}/lists`)

  const createTop100AdminList = (categoryId: string, title: string) =>
    api<{ listId: string }>(`/api/admin/top100/categories/${categoryId}/lists`, { method: 'POST', body: { title } })

  const getTop100AdminList = (listId: string) =>
    api<AdminTop100ListDetailDto>(`/api/admin/top100/lists/${listId}`)

  const updateTop100AdminList = (listId: string, title: string) =>
    api(`/api/admin/top100/lists/${listId}`, { method: 'PUT', body: { title } })

  const deleteTop100AdminList = (listId: string) =>
    api(`/api/admin/top100/lists/${listId}`, { method: 'DELETE' })

  const createTop100AdminItem = (listId: string, label: string, position: number, alternateSpellings: string[]) =>
    api<{ itemId: string }>(`/api/admin/top100/lists/${listId}/items`, {
      method: 'POST',
      body: { label, position, alternateSpellings }
    })

  const updateTop100AdminItem = (itemId: string, label: string, position: number, alternateSpellings: string[]) =>
    api(`/api/admin/top100/items/${itemId}`, { method: 'PUT', body: { label, position, alternateSpellings } })

  const deleteTop100AdminItem = (itemId: string) =>
    api(`/api/admin/top100/items/${itemId}`, { method: 'DELETE' })

  return {
    getDashboardStats,
    listAdminCategories,
    createAdminCategory,
    updateAdminCategory,
    deleteAdminCategory,
    listAdminQuestions,
    createAdminQuestion,
    updateAdminQuestion,
    deleteAdminQuestion,
    listPasswordAdminCategories,
    createPasswordAdminCategory,
    updatePasswordAdminCategory,
    deletePasswordAdminCategory,
    listAdminWords,
    createAdminWord,
    updateAdminWord,
    deleteAdminWord,
    listRankingAdminCategories,
    createRankingAdminCategory,
    updateRankingAdminCategory,
    deleteRankingAdminCategory,
    listRankingAdminLists,
    createRankingAdminList,
    getRankingAdminList,
    updateRankingAdminList,
    deleteRankingAdminList,
    createRankingAdminItem,
    updateRankingAdminItem,
    deleteRankingAdminItem,
    listTop100AdminCategories,
    createTop100AdminCategory,
    updateTop100AdminCategory,
    deleteTop100AdminCategory,
    listTop100AdminLists,
    createTop100AdminList,
    getTop100AdminList,
    updateTop100AdminList,
    deleteTop100AdminList,
    createTop100AdminItem,
    updateTop100AdminItem,
    deleteTop100AdminItem
  }
}
