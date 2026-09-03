<script setup lang="ts">
import type { AccountDto } from '~/types/api'

definePageMeta({ middleware: 'auth' })
useSeoMeta({ title: 'حسابي - جولة' })

const { getAccount, deleteAccount } = useApi()
const { clearSession } = useAuth()
const toast = useToast()

const account = ref<AccountDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')
const deleteModalOpen = ref(false)
const deleting = ref(false)

onMounted(async () => {
  try {
    account.value = await getAccount()
  } catch {
    errorMessage.value = 'تعذر تحميل بيانات الحساب.'
  } finally {
    loading.value = false
  }
})

function logout() {
  clearSession()
  navigateTo('/')
}

async function confirmDelete() {
  deleting.value = true
  try {
    await deleteAccount()
    clearSession()
    toast.add({ title: 'تم حذف الحساب', color: 'success' })
    await navigateTo('/')
  } catch {
    errorMessage.value = 'تعذر حذف الحساب. حاول مرة أخرى.'
    deleteModalOpen.value = false
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <div class="min-h-[70vh] flex items-center justify-center p-4">
    <div class="w-full max-w-md space-y-4">
      <div class="text-center space-y-2">
        <div class="mx-auto size-16 rounded-full bg-green-900 text-gold-400 flex items-center justify-center">
          <UIcon
            name="i-lucide-user"
            class="size-8"
          />
        </div>
        <h1 class="text-2xl font-black text-green-900 dark:text-green-100">
          حسابي
        </h1>
      </div>

      <UAlert
        v-if="errorMessage"
        color="error"
        variant="subtle"
        :title="errorMessage"
      />

      <UCard
        v-if="account"
        :ui="{ body: 'divide-y divide-green-100 dark:divide-gray-800 p-0' }"
      >
        <div class="flex items-center justify-between px-4 py-3">
          <div>
            <p class="text-xs text-muted">
              الاسم
            </p>
            <p class="font-bold">
              {{ account.name }}
            </p>
          </div>
          <UIcon
            name="i-lucide-user"
            class="text-primary size-5"
          />
        </div>
        <div class="flex items-center justify-between px-4 py-3">
          <div>
            <p class="text-xs text-muted">
              البريد الإلكتروني
            </p>
            <p class="font-bold">
              {{ account.email }}
            </p>
          </div>
          <UIcon
            name="i-lucide-mail"
            class="text-primary size-5"
          />
        </div>
        <NuxtLink
          to="/previous-games"
          class="flex items-center justify-between px-4 py-3 hover:bg-green-50 dark:hover:bg-gray-900"
        >
          <div>
            <p class="text-xs text-muted">
              عدد الألعاب الملعوبة
            </p>
            <p class="font-bold">
              {{ account.gamesPlayedCount }} لعبة — عرض ألعابي السابقة
            </p>
          </div>
          <UIcon
            name="i-lucide-chevron-left"
            class="text-primary size-5"
          />
        </NuxtLink>
      </UCard>

      <UCard
        v-else-if="loading"
        :ui="{ body: 'divide-y divide-green-100 dark:divide-gray-800 p-0' }"
      >
        <div
          v-for="i in 3"
          :key="i"
          class="flex items-center justify-between px-4 py-3"
        >
          <div class="space-y-2">
            <USkeleton class="h-3 w-16" />
            <USkeleton class="h-4 w-32" />
          </div>
          <USkeleton class="size-5 rounded-full" />
        </div>
      </UCard>

      <UButton
        block
        variant="outline"
        color="error"
        icon="i-lucide-log-out"
        @click="logout"
      >
        تسجيل الخروج
      </UButton>
      <UButton
        block
        variant="outline"
        color="error"
        icon="i-lucide-trash-2"
        @click="deleteModalOpen = true"
      >
        حذف الحساب
      </UButton>
    </div>

    <UModal v-model:open="deleteModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold text-error">
              حذف الحساب نهائياً
            </p>
          </template>
          <p class="text-muted">
            سيتم حذف حسابك وتصنيفاتك الخاصة نهائياً، ولا يمكن التراجع عن هذا الإجراء. هل أنت متأكد؟
          </p>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="deleteModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="error"
                :loading="deleting"
                @click="confirmDelete"
              >
                حذف نهائياً
              </UButton>
            </div>
          </template>
        </UCard>
      </template>
    </UModal>
  </div>
</template>
