// Shared motion tokens for motion-v-driven animation. Durations/easings are
// kept in sync with the --motion-* / --ease-* custom properties in
// app/assets/css/main.css, which cover plain Tailwind transition-* usage.

export const DURATIONS = {
  instant: 0.1,
  fast: 0.15,
  base: 0.25,
  slow: 0.35
} as const

export const EASINGS = {
  standard: [0.4, 0, 0.2, 1],
  decelerate: [0, 0, 0.2, 1],
  accelerate: [0.4, 0, 1, 1]
} as const

export const SPRINGS = {
  snappy: { type: 'spring', stiffness: 500, damping: 30 },
  gentle: { type: 'spring', stiffness: 300, damping: 25 },
  bouncy: { type: 'spring', stiffness: 400, damping: 15 }
} as const

export const fadeVariants = {
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: { duration: DURATIONS.base, ease: EASINGS.standard } },
  exit: { opacity: 0, transition: { duration: DURATIONS.fast, ease: EASINGS.accelerate } }
}

export const scaleInVariants = {
  initial: { opacity: 0, scale: 0.9 },
  animate: { opacity: 1, scale: 1, transition: { duration: DURATIONS.base, ease: EASINGS.decelerate } },
  exit: { opacity: 0, scale: 0.9, transition: { duration: DURATIONS.fast, ease: EASINGS.accelerate } }
}

// offset is in the element's own logical-start/end or up/down axis; callers
// pick the sign so RTL start/end always maps to the visually correct side.
export function slideVariants(axis: 'x' | 'y', offset: number) {
  const hidden = axis === 'x' ? { x: offset } : { y: offset }
  const shown = axis === 'x' ? { x: 0 } : { y: 0 }
  return {
    initial: { ...hidden, opacity: 0 },
    animate: { ...shown, opacity: 1, transition: { duration: DURATIONS.base, ease: EASINGS.decelerate } },
    exit: { ...hidden, opacity: 0, transition: { duration: DURATIONS.fast, ease: EASINGS.accelerate } }
  }
}

export const successPulseVariants = {
  initial: { scale: 1 },
  animate: {
    scale: [1, 1.12, 1],
    transition: { duration: DURATIONS.slow, ease: EASINGS.standard }
  }
}
