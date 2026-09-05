<script setup lang="ts">
withDefaults(defineProps<{ showEndGame?: boolean }>(), { showEndGame: true })
const emit = defineEmits<{ end: [] }>()

const confirmOpen = ref(false)
const confirmEndOpen = ref(false)

function askExit() {
  confirmOpen.value = true
}

function confirmExit() {
  confirmOpen.value = false
  navigateTo('/')
}

function askEndGame() {
  confirmEndOpen.value = true
}

function confirmEndGame() {
  confirmEndOpen.value = false
  emit('end')
}
</script>

<template>
  <div class="sticky top-0 z-40 bg-green-900 text-white shadow-md">
    <div class="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between gap-2">
      <span
        class="font-display text-xl font-black text-gold-400 cursor-pointer"
        @click="askExit"
      >
        جولة
      </span>
      <div class="flex items-center gap-2">
        <UButton
          v-if="showEndGame"
          color="success"
          variant="outline"
          icon="i-lucide-flag"
          size="sm"
          @click="askEndGame"
        >
          إنهاء اللعبة
        </UButton>
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

  <UModal v-model:open="confirmEndOpen">
    <template #content>
      <UCard>
        <template #header>
          <p class="font-bold text-success">
            إنهاء اللعبة الآن
          </p>
        </template>
        <p class="text-muted">
          سيتم حفظ النتيجة الحالية وعرض الفوز مباشرة.
        </p>
        <template #footer>
          <div class="flex gap-2 justify-end">
            <UButton
              variant="ghost"
              @click="confirmEndOpen = false"
            >
              إلغاء
            </UButton>
            <UButton
              color="success"
              @click="confirmEndGame"
            >
              إنهاء اللعبة
            </UButton>
          </div>
        </template>
      </UCard>
    </template>
  </UModal>
</template>
