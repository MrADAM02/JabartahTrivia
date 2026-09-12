<script setup lang="ts">
const colorMode = useColorMode()

function toggle() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}
</script>

<template>
  <button
    type="button"
    :aria-label="colorMode.value === 'dark' ? 'تفعيل الوضع الفاتح' : 'تفعيل الوضع الداكن'"
    @click="toggle"
  >
    <UIcon
      :name="colorMode.value === 'dark' ? 'i-lucide-sun' : 'i-lucide-moon'"
      class="size-5"
    />
  </button>
</template>
