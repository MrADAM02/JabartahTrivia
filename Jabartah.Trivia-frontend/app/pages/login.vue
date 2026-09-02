<script setup lang="ts">
useSeoMeta({ title: 'تسجيل الدخول - جولة' })

const route = useRoute()
const { login } = useApi()
const { setSession } = useAuth()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

const canSubmit = computed(() => email.value.trim().length > 0 && password.value.length > 0 && !loading.value)

async function submit() {
  errorMessage.value = ''
  loading.value = true
  try {
    const result = await login(email.value.trim(), password.value)
    setSession(result.token, { id: result.userId, name: result.name, email: result.email })
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await navigateTo(redirect)
  } catch {
    errorMessage.value = 'البريد الإلكتروني أو كلمة المرور غير صحيحة.'
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
          تسجيل الدخول
        </h1>
      </template>

      <form
        class="space-y-4"
        @submit.prevent="submit"
      >
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
          دخول
        </UButton>
      </form>

      <p class="text-center text-sm text-muted mt-4">
        ليس لديك حساب؟
        <NuxtLink
          to="/signup"
          class="text-primary font-bold"
        >
          أنشئ حساباً
        </NuxtLink>
      </p>
    </UCard>
  </div>
</template>
