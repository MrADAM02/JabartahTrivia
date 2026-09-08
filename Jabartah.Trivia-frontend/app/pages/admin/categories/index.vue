<script setup lang="ts">
import type { AdminCategoryDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })
useSeoMeta({ title: 'الفئات والأسئلة - جولة' })

const { listAdminCategories, createAdminCategory, updateAdminCategory, deleteAdminCategory } = useAdminApi()
const toast = useToast()

const categories = ref<AdminCategoryDto[]>([])
const loading = ref(true)
const errorMessage = ref('')
const deletingId = ref<string | null>(null)

async function load() {
  loading.value = true
  try {
    categories.value = await listAdminCategories()
  } catch {
    errorMessage.value = 'تعذر تحميل الفئات.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// Shared create/edit modal -- editingCategory === null means "creating a new category".
const modalOpen = ref(false)
const editingCategory = ref<AdminCategoryDto | null>(null)
const formName = ref('')
const formIcon = ref('')
const saving = ref(false)

function openCreate() {
  editingCategory.value = null
  formName.value = ''
  formIcon.value = ''
  modalOpen.value = true
}

function openEdit(category: AdminCategoryDto) {
  editingCategory.value = category
  formName.value = category.name
  formIcon.value = category.icon ?? ''
  modalOpen.value = true
}

async function save() {
  if (!formName.value.trim()) return
  saving.value = true
  try {
    const icon = formIcon.value.trim() || null
    if (editingCategory.value) {
      await updateAdminCategory(editingCategory.value.id, formName.value.trim(), icon)
      toast.add({ title: 'تم تحديث الفئة', color: 'success' })
    } else {
      await createAdminCategory(formName.value.trim(), icon)
      toast.add({ title: 'تم إنشاء الفئة', color: 'success' })
    }
    modalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ الفئة', color: 'error' })
  } finally {
    saving.value = false
  }
}

async function remove(category: AdminCategoryDto) {
  deletingId.value = category.id
  try {
    await deleteAdminCategory(category.id)
    categories.value = categories.value.filter(c => c.id !== category.id)
    toast.add({ title: 'تم حذف الفئة', color: 'success' })
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حذف الفئة', color: 'error' })
  } finally {
    deletingId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-black text-green-900 dark:text-green-100">
          الفئات والأسئلة
        </h1>
        <p class="text-muted">
          فئات لعبة التحدي (Trivia) وأسئلتها
        </p>
      </div>
      <UButton
        color="secondary"
        icon="i-lucide-plus"
        class="font-bold text-green-950"
        @click="openCreate"
      >
        فئة جديدة
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
      class="space-y-3"
    >
      <UCard
        v-for="i in 4"
        :key="i"
      >
        <USkeleton class="h-9" />
      </UCard>
    </div>

    <div
      v-else
      class="space-y-2"
    >
      <UCard
        v-for="category in categories"
        :key="category.id"
      >
        <div class="flex items-center justify-between gap-3">
          <NuxtLink
            :to="`/admin/categories/${category.id}`"
            class="flex items-center gap-3 flex-1 min-w-0"
          >
            <span class="text-2xl">{{ category.icon ?? '📚' }}</span>
            <span class="font-bold truncate">{{ category.name }}</span>
            <UBadge
              color="neutral"
              variant="subtle"
            >
              {{ category.questionCount }} سؤال
            </UBadge>
          </NuxtLink>
          <div class="flex items-center gap-1 shrink-0">
            <UButton
              icon="i-lucide-pencil"
              color="neutral"
              variant="ghost"
              @click="openEdit(category)"
            />
            <UButton
              icon="i-lucide-trash-2"
              color="error"
              variant="ghost"
              :loading="deletingId === category.id"
              @click="remove(category)"
            />
          </div>
        </div>
      </UCard>

      <p
        v-if="!categories.length"
        class="text-muted text-center py-8"
      >
        لا توجد فئات بعد.
      </p>
    </div>

    <UModal v-model:open="modalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              {{ editingCategory ? 'تعديل الفئة' : 'فئة جديدة' }}
            </p>
          </template>

          <form
            class="space-y-4"
            @submit.prevent="save"
          >
            <UFormField label="اسم الفئة">
              <UInput
                v-model="formName"
                class="w-full"
                autofocus
              />
            </UFormField>
            <UFormField label="الأيقونة (رمز تعبيري اختياري)">
              <UInput
                v-model="formIcon"
                class="w-full"
                placeholder="🏆"
              />
            </UFormField>
          </form>

          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="modalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="saving"
                :disabled="!formName.trim()"
                @click="save"
              >
                حفظ
              </UButton>
            </div>
          </template>
        </UCard>
      </template>
    </UModal>
  </div>
</template>
