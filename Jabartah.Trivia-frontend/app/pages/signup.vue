<script setup lang="ts">
useSeoMeta({ title: 'إنشاء حساب - جولة' })

const { register } = useApi()
const { setSession } = useAuth()

const name = ref('')
const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

const canSubmit = computed(() =>
  name.value.trim().length > 0
  && email.value.trim().length > 0
  && password.value.length >= 6
  && !loading.value
)

async function submit() {
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await register(name.value.trim(), email.value.trim(), password.value)
    setSession(result.token, { id: result.userId, name: result.name, email: result.email })
    await navigateTo('/')
  } catch {
    errorMessage.value = 'تعذر إنشاء الحساب. ربما هذا البريد الإلكتروني مستخدم بالفعل.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-[70vh] flex items-center justify-center p-4">
    <UCard class="w-full max-w-sm">
      <template #header>
        <h1 class="text-2xl font-black text-center text-green-900 dark:text-green-100">
          إنشاء حساب جديد
        </h1>
      </template>

      <form
        class="space-y-4"
        @submit.prevent="submit"
      >
        <UFormField label="الاسم">
          <UInput
            v-model="name"
            size="lg"
            class="w-full"
          />
        </UFormField>
        <UFormField label="البريد الإلكتروني">
          <UInput
            v-model="email"
            type="email"
            size="lg"
            class="w-full"
            placeholder="example@email.com"
          />
        </UFormField>
        <UFormField label="كلمة المرور">
          <UInput
            v-model="password"
            type="password"
            size="lg"
            class="w-full"
          />
          <template #hint>
            <span class="text-xs text-muted">6 أحرف على الأقل</span>
          </template>
        </UFormField>

        <UAlert
          v-if="errorMessage"
          color="error"
          variant="subtle"
          :title="errorMessage"
        />

        <UButton
          type="submit"
          block
          size="lg"
          color="secondary"
          class="font-bold text-green-950"
          :loading="loading"
          :disabled="!canSubmit"
        >
          إنشاء الحساب
        </UButton>
      </form>

      <p class="text-center text-sm text-muted mt-4">
        لديك حساب بالفعل؟
        <NuxtLink
          to="/login"
          class="text-primary font-bold"
        >
          سجّل الدخول
        </NuxtLink>
      </p>
    </UCard>
  </div>
</template>
