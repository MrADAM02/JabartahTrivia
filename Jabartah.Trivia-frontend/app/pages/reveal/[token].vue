<script setup lang="ts">
const route = useRoute()
const token = route.params.token as string

const { consumeRevealToken } = useApi()

const state = ref<'idle' | 'loading' | 'revealed' | 'expired' | 'consumed' | 'error'>('idle')
const word = ref('')
const categoryName = ref('')

async function reveal() {
  state.value = 'loading'
  try {
    const result = await consumeRevealToken(token)
    if (result.success) {
      word.value = result.word!
      categoryName.value = result.categoryName!
      state.value = 'revealed'
    } else if (result.expired) {
      state.value = 'expired'
    } else if (result.alreadyConsumed) {
      state.value = 'consumed'
    }
  } catch {
    state.value = 'error'
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-linear-to-b from-primary-50 to-white dark:from-gray-950 dark:to-gray-900">
    <UCard class="w-full max-w-sm text-center">
      <template v-if="state === 'idle'">
        <p class="text-lg mb-4">
          هذا الرابط يكشف الكلمة السرية لك فقط. لا تُطلع باقي فريقك عليها.
        </p>
        <UButton
          size="xl"
          block
          @click="reveal"
        >
          اضغط لكشف الكلمة
        </UButton>
      </template>

      <template v-else-if="state === 'loading'">
        <UIcon
          name="i-lucide-loader-circle"
          class="animate-spin size-10 text-primary mx-auto"
        />
      </template>

      <template v-else-if="state === 'revealed'">
        <p class="text-muted mb-2">
          {{ categoryName }}
        </p>
        <p class="text-4xl font-black text-primary">
          {{ word }}
        </p>
      </template>

      <template v-else-if="state === 'expired'">
        <UIcon
          name="i-lucide-clock-alert"
          class="size-10 text-error mx-auto mb-3"
        />
        <p class="text-lg font-bold">
          انتهت صلاحية هذا الرابط
        </p>
        <p class="text-muted mt-1">
          اطلب من مدير اللعبة رمز QR جديد
        </p>
      </template>

      <template v-else-if="state === 'consumed'">
        <UIcon
          name="i-lucide-eye-off"
          class="size-10 text-error mx-auto mb-3"
        />
        <p class="text-lg font-bold">
          تم استخدام هذا الرابط بالفعل
        </p>
        <p class="text-muted mt-1">
          اطلب من مدير اللعبة رمز QR جديد
        </p>
      </template>

      <template v-else-if="state === 'error'">
        <p class="text-lg font-bold text-error">
          حدث خطأ. تأكد من اتصالك بالإنترنت.
        </p>
      </template>
    </UCard>
  </div>
</template>
