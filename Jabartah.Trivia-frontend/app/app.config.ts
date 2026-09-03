export default defineAppConfig({
  ui: {
    colors: {
      primary: 'green',
      secondary: 'gold',
      neutral: 'slate'
    },
    button: {
      slots: {
        // Bare `transition` (not `transition-colors`) so it also covers the
        // active:scale press feedback below, applied to every UButton.
        base: 'transition active:scale-95'
      }
    }
  }
})
