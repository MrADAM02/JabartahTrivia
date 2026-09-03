<script setup lang="ts">
import type { CategoryDto } from '~/types/api'

definePageMeta({ middleware: 'auth' })
useSeoMeta({ title: 'تصنيفاتي - جولة' })

const { listMyCategories, deleteMyCategory } = useApi()
const toast = useToast()

const categories = ref<CategoryDto[]>([])
const loading = ref(true)
const errorMessage = ref('')
const deletingId = ref<string | null>(null)

async function load() {
  loading.value = true
  try {
    categories.value = await listMyCategories()
  } catch {
    errorMessage.value = 'تعذر تحميل تصنيفاتك.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function remove(id: string) {
  deletingId.value = id
  try {
    await deleteMyCategory(id)
    categories.value = categories.value.filter(c => c.id !== id)
    toast.add({ title: 'تم حذف التصنيف', color: 'success' })
  } catch {
    errorMessage.value = 'تعذر حذف التصنيف.'
  } finally {
    deletingId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 py-12 space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-black text-green-900 dark:text-green-100">
          تصنيفاتي
        </h1>
        <p class="text-muted">
          تصنيفاتك الخاصة بك فقط، لن تظهر لأي مستخدم آخر
        </p>
      </div>
      <UButton
        to="/my-categories/create"
        color="secondary"
        icon="i-lucide-plus"
        class="font-bold text-green-950"
      >
        إنشاء تصنيف جديد
      </UButton>
    </div>

    <UAlert
      v-if="errorMessage"
      color="error"
      variant="subtle"
      :title="errorMessage"
    />

    <div
      v-if="loading"
      class="grid grid-cols-1 sm:grid-cols-2 gap-3"
    >
      <UCard
        v-for="i in 4"
        :key="i"
      >
        <div class="flex items-center gap-3">
          <USkeleton class="size-9 rounded-full" />
          <USkeleton class="h-5 flex-1" />
        </div>
      </UCard>
    </div>

    <div
      v-else-if="categories.length === 0"
      class="text-center space-y-4 py-12"
    >
      <UIcon
        name="i-lucide-folder"
        class="size-14 text-muted mx-auto"
      />
      <p class="text-lg font-bold">
        لم تقم بإنشاء أي تصنيف بعد
      </p>
      <p class="text-muted">
        أنشئ تصنيفك الخاص وأسئلتك المخصصة واستخدمها في ألعابك
      </p>
      <UButton
        to="/my-categories/create"
        color="secondary"
        class="font-bold text-green-950"
      >
        إنشاء تصنيفي الأول
      </UButton>
    </div>

    <div
      v-else
      class="grid grid-cols-1 sm:grid-cols-2 gap-3"
    >
      <UCard
        v-for="category in categories"
        :key="category.id"
        class="hover:ring-1 hover:ring-primary/40 transition-all"
      >
        <div class="flex items-center justify-between gap-3">
          <div class="flex items-center gap-3">
            <span class="text-3xl">{{ category.icon ?? '📚' }}</span>
            <p class="font-bold">
              {{ category.name }}
            </p>
          </div>
          <UButton
            icon="i-lucide-trash-2"
            color="error"
            variant="ghost"
            :loading="deletingId === category.id"
            @click="remove(category.id)"
          />
        </div>
      </UCard>
    </div>
  </div>
</template>
