<script setup lang="ts">
const confirmOpen = ref(false)

function askExit() {
  confirmOpen.value = true
}

function confirmExit() {
  confirmOpen.value = false
  navigateTo('/')
}
</script>

<template>
  <div class="sticky top-0 z-40 bg-green-900 text-white shadow-md">
    <div class="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
      <span
        class="text-xl font-black text-gold-400 cursor-pointer"
        @click="askExit"
      >
        جولة
      </span>
      <UButton
        color="error"
        variant="outline"
        icon="i-lucide-log-out"
        size="sm"
        @click="askExit"
      >
        الخروج
      </UButton>
    </div>
  </div>

  <UModal v-model:open="confirmOpen">
    <template #content>
      <UCard>
        <template #header>
          <p class="font-bold text-error">
            الخروج من اللعبة
          </p>
        </template>
        <p class="text-muted">
          هل أنت متأكد من الخروج؟ لن تتمكن من العودة إلى هذه الشاشة مرة أخرى.
        </p>
        <template #footer>
          <div class="flex gap-2 justify-end">
            <UButton
              variant="ghost"
              @click="confirmOpen = false"
            >
              إلغاء
            </UButton>
            <UButton
              color="error"
              @click="confirmExit"
            >
              الخروج
            </UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>
