export interface ConfettiPiece {
  id: number
  left: number
  size: number
  shape: 'rect' | 'circle'
  color: string
  duration: number
  delay: number
  drift: number
  spin: number
}

const PALETTE = [
  'var(--color-gold-300)',
  'var(--color-gold-400)',
  'var(--color-gold-500)',
  'var(--color-green-400)',
  'var(--color-green-500)'
]

// Called once per winner-screen mount (not reactive — the burst shouldn't
// re-randomize itself while it's playing). Shared by all 4 modes' winner
// screens so the celebration looks identical everywhere.
export function useConfettiBurst(count = 40) {
  const pieces: ConfettiPiece[] = Array.from({ length: count }, (_, id) => ({
    id,
    left: Math.random() * 100,
    size: 6 + Math.random() * 7,
    shape: Math.random() > 0.5 ? 'circle' : 'rect',
    color: PALETTE[Math.floor(Math.random() * PALETTE.length)]!,
    duration: 1.8 + Math.random() * 1.4,
    delay: Math.random() * 0.6,
    drift: (Math.random() - 0.5) * 160,
    spin: Math.random() > 0.5 ? 1 : -1
  }))

  return { pieces }
}
