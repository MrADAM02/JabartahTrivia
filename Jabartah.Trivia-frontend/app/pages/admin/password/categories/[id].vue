<script setup lang="ts">
import type { AdminPasswordCategoryDto, AdminWordDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })

const route = useRoute()
const categoryId = route.params.id as string

const {
  listPasswordAdminCategories,
  updatePasswordAdminCategory,
  listAdminWords,
  createAdminWord,
  updateAdminWord,
  deleteAdminWord
} = useAdminApi()
const toast = useToast()

const category = ref<AdminPasswordCategoryDto | null>(null)
const words = ref<AdminWordDto[]>([])
const loading = ref(true)
const errorMessage = ref('')

useSeoMeta({ title: () => `${category.value?.name ?? 'فئة'} - جولة` })

async function load() {
  loading.value = true
  try {
    const [categories, wordList] = await Promise.all([
      listPasswordAdminCategories(),
      listAdminWords(categoryId)
    ])
    category.value = categories.find(c => c.id === categoryId) ?? null
    words.value = wordList
  } catch {
    errorMessage.value = 'تعذر تحميل بيانات الفئة.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

// Category details edit
const detailsModalOpen = ref(false)
const detailsName = ref('')
const detailsIcon = ref('')
const savingDetails = ref(false)

function openDetailsEdit() {
  if (!category.value) return
  detailsName.value = category.value.name
  detailsIcon.value = category.value.icon ?? ''
  detailsModalOpen.value = true
}

async function saveDetails() {
  if (!category.value || !detailsName.value.trim()) return
  savingDetails.value = true
  try {
    await updatePasswordAdminCategory(category.value.id, detailsName.value.trim(), detailsIcon.value.trim() || null)
    toast.add({ title: 'تم تحديث الفئة', color: 'success' })
    detailsModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ التعديلات', color: 'error' })
  } finally {
    savingDetails.value = false
  }
}

// Word create/edit modal
const wordModalOpen = ref(false)
const editingWord = ref<AdminWordDto | null>(null)
const formWord = ref('')
const savingWord = ref(false)
const deletingWordId = ref<string | null>(null)

function openWordModal(existing: AdminWordDto | null = null) {
  editingWord.value = existing
  formWord.value = existing?.word ?? ''
  wordModalOpen.value = true
}

async function saveWord() {
  if (!formWord.value.trim()) return
  savingWord.value = true
  try {
    if (editingWord.value) {
      await updateAdminWord(editingWord.value.id, formWord.value.trim())
    } else {
      await createAdminWord(categoryId, formWord.value.trim())
    }
    toast.add({ title: 'تم حفظ الكلمة', color: 'success' })
    wordModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ الكلمة', color: 'error' })
  } finally {
    savingWord.value = false
  }
}

async function removeWord(word: AdminWordDto) {
  deletingWordId.value = word.id
  try {
    await deleteAdminWord(word.id)
    toast.add({ title: 'تم حذف الكلمة', color: 'success' })
    await load()
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حذف الكلمة', color: 'error' })
  } finally {
    deletingWordId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <UButton
      to="/admin/password/categories"
      variant="ghost"
      icon="i-lucide-arrow-right"
      size="sm"
    >
      الفئات
    </UButton>

    <UAlert
      v-if="errorMessage"
      color="error"
      variant="subtle"
      :title="errorMessage"
    />

    <template v-if="loading">
      <UCard>
        <USkeleton class="h-9" />
      </UCard>
    </template>

    <template v-else-if="category">
      <div class="flex items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <span class="text-4xl">{{ category.icon ?? '🔑' }}</span>
          <h1 class="text-2xl font-black text-green-900 dark:text-green-100">
            {{ category.name }}
          </h1>
        </div>
        <UButton
          icon="i-lucide-pencil"
          color="neutral"
          variant="ghost"
          @click="openDetailsEdit"
        >
          تعديل الاسم
        </UButton>
      </div>

      <div class="space-y-2">
        <div class="flex items-center justify-between">
          <p class="text-sm font-bold text-muted">
            الكلمات ({{ words.length }})
          </p>
          <UButton
            icon="i-lucide-plus"
            size="sm"
            color="secondary"
            class="font-bold text-green-950"
            @click="openWordModal()"
          >
            كلمة جديدة
          </UButton>
        </div>

        <UCard
          v-for="word in words"
          :key="word.id"
        >
          <div class="flex items-center gap-3">
            <p class="flex-1 min-w-0 truncate font-bold">
              {{ word.word }}
            </p>
            <div class="flex items-center gap-1 shrink-0">
              <UButton
                icon="i-lucide-pencil"
                color="neutral"
                variant="ghost"
                @click="openWordModal(word)"
              />
              <UButton
                icon="i-lucide-trash-2"
                color="error"
                variant="ghost"
                :loading="deletingWordId === word.id"
                @click="removeWord(word)"
              />
            </div>
          </div>
        </UCard>

        <p
          v-if="!words.length"
          class="text-muted italic text-sm px-1"
        >
          لا توجد كلمات بعد.
        </p>
      </div>
    </template>

    <UModal v-model:open="detailsModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              تعديل الفئة
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveDetails"
          >
            <UFormField label="اسم الفئة">
              <UInput
                v-model="detailsName"
                class="w-full"
              />
            </UFormField>
            <UFormField label="الأيقونة (رمز تعبيري اختياري)">
              <UInput
                v-model="detailsIcon"
                class="w-full"
                placeholder="🔑"
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="detailsModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingDetails"
                :disabled="!detailsName.trim()"
                @click="saveDetails"
              >
                حفظ
              </UButton>
            </div>
          </template>
        </UCard>
      </template>
    </UModal>

    <UModal v-model:open="wordModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              {{ editingWord ? 'تعديل الكلمة' : 'كلمة جديدة' }}
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveWord"
          >
            <UFormField label="الكلمة">
              <UInput
                v-model="formWord"
                class="w-full"
                autofocus
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="wordModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingWord"
                :disabled="!formWord.trim()"
                @click="saveWord"
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
