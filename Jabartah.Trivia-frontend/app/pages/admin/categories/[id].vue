<script setup lang="ts">
import type { AdminCategoryDto, AdminQuestionDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })

const route = useRoute()
const categoryId = route.params.id as string

const {
  listAdminCategories,
  updateAdminCategory,
  listAdminQuestions,
  createAdminQuestion,
  updateAdminQuestion,
  deleteAdminQuestion
} = useAdminApi()
const toast = useToast()

const POINT_TIERS = [100, 200, 300, 400, 500]

const category = ref<AdminCategoryDto | null>(null)
const questions = ref<AdminQuestionDto[]>([])
const loading = ref(true)
const errorMessage = ref('')

useSeoMeta({ title: () => `${category.value?.name ?? 'فئة'} - جولة` })

async function load() {
  loading.value = true
  try {
    const [categories, questionList] = await Promise.all([
      listAdminCategories(),
      listAdminQuestions(categoryId)
    ])
    category.value = categories.find(c => c.id === categoryId) ?? null
    questions.value = questionList
  } catch {
    errorMessage.value = 'تعذر تحميل بيانات الفئة.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

function questionsFor(pointValue: number) {
  return questions.value.filter(q => q.pointValue === pointValue)
}

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
    await updateAdminCategory(category.value.id, detailsName.value.trim(), detailsIcon.value.trim() || null)
    toast.add({ title: 'تم تحديث الفئة', color: 'success' })
    detailsModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ التعديلات', color: 'error' })
  } finally {
    savingDetails.value = false
  }
}

// Question create/edit modal
const questionModalOpen = ref(false)
const editingQuestion = ref<AdminQuestionDto | null>(null)
const activePointValue = ref(100)
const formPrompt = ref('')
const formAnswer = ref('')
const formMediaUrl = ref('')
const savingQuestion = ref(false)
const deletingQuestionId = ref<string | null>(null)

function openQuestionModal(pointValue: number, existing: AdminQuestionDto | null = null) {
  editingQuestion.value = existing
  activePointValue.value = pointValue
  formPrompt.value = existing?.prompt ?? ''
  formAnswer.value = existing?.answer ?? ''
  formMediaUrl.value = existing?.mediaUrl ?? ''
  questionModalOpen.value = true
}

async function saveQuestion() {
  if (!formPrompt.value.trim() || !formAnswer.value.trim()) return
  savingQuestion.value = true
  try {
    const mediaUrl = formMediaUrl.value.trim() || null
    if (editingQuestion.value) {
      await updateAdminQuestion(editingQuestion.value.id, activePointValue.value, formPrompt.value.trim(), formAnswer.value.trim(), mediaUrl)
    } else {
      await createAdminQuestion(categoryId, activePointValue.value, formPrompt.value.trim(), formAnswer.value.trim(), mediaUrl)
    }
    toast.add({ title: 'تم حفظ السؤال', color: 'success' })
    questionModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ السؤال', color: 'error' })
  } finally {
    savingQuestion.value = false
  }
}

async function removeQuestion(question: AdminQuestionDto) {
  deletingQuestionId.value = question.id
  try {
    await deleteAdminQuestion(question.id)
    toast.add({ title: 'تم حذف السؤال', color: 'success' })
    await load()
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حذف السؤال', color: 'error' })
  } finally {
    deletingQuestionId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <UButton
      to="/admin/categories"
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
          <span class="text-4xl">{{ category.icon ?? '📚' }}</span>
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

      <div class="space-y-6">
        <div
          v-for="pointValue in POINT_TIERS"
          :key="pointValue"
          class="space-y-2"
        >
          <div class="flex items-center justify-between gap-3">
            <div class="flex items-center gap-2">
              <UBadge
                color="secondary"
                size="lg"
                class="font-black tabular-nums"
              >
                {{ pointValue }}
              </UBadge>
              <span class="text-sm text-muted font-bold">
                {{ questionsFor(pointValue).length }} سؤال
              </span>
            </div>
            <UButton
              icon="i-lucide-plus"
              size="sm"
              color="neutral"
              variant="ghost"
              @click="openQuestionModal(pointValue)"
            >
              إضافة سؤال
            </UButton>
          </div>

          <UCard
            v-for="question in questionsFor(pointValue)"
            :key="question.id"
          >
            <div class="flex items-center gap-3">
              <p class="flex-1 min-w-0 truncate">
                {{ question.prompt }}
              </p>
              <div class="flex items-center gap-1 shrink-0">
                <UButton
                  icon="i-lucide-pencil"
                  color="neutral"
                  variant="ghost"
                  @click="openQuestionModal(pointValue, question)"
                />
                <UButton
                  icon="i-lucide-trash-2"
                  color="error"
                  variant="ghost"
                  :loading="deletingQuestionId === question.id"
                  @click="removeQuestion(question)"
                />
              </div>
            </div>
          </UCard>

          <p
            v-if="!questionsFor(pointValue).length"
            class="text-muted italic text-sm px-1"
          >
            لا توجد أسئلة بعد
          </p>
        </div>
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
                placeholder="🏆"
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

    <UModal v-model:open="questionModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              سؤال {{ activePointValue }} نقطة
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveQuestion"
          >
            <UFormField label="نص السؤال">
              <UTextarea
                v-model="formPrompt"
                class="w-full"
                :rows="3"
              />
            </UFormField>
            <UFormField label="الإجابة">
              <UInput
                v-model="formAnswer"
                class="w-full"
              />
            </UFormField>
            <UFormField label="رابط وسائط (اختياري)">
              <UInput
                v-model="formMediaUrl"
                class="w-full"
                placeholder="https://..."
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="questionModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingQuestion"
                :disabled="!formPrompt.trim() || !formAnswer.trim()"
                @click="saveQuestion"
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
