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
