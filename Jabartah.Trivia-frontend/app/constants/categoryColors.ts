// Kept separate from TEAM_COLORS (teamCustomization.ts) so a category tile's
// color is never visually confused with a team's own chosen color.
export const CATEGORY_COLORS = [
  '#EF4444', // red
  '#F97316', // orange
  '#F59E0B', // amber
  '#84CC16', // lime
  '#10B981', // emerald
  '#14B8A6', // teal
  '#06B6D4', // cyan
  '#3B82F6', // blue
  '#8B5CF6', // violet
  '#EC4899' // pink
]

// Hashes by category id (not array index) so a category keeps the same color
// across reloads/tabs regardless of fetch ordering -- the backend doesn't
// guarantee a stable order for listCategories()/listMyCategories() etc.
export function categoryColor(id: string): string {
  let hash = 0
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) | 0
  }
  return CATEGORY_COLORS[Math.abs(hash) % CATEGORY_COLORS.length]!
}
