<script setup lang="ts">
import type { MyCategoryDetailDto, MyCategoryQuestionDto } from '~/types/api'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const categoryId = route.params.id as string

const { getMyCategory, updateMyCategory } = useApi()
const toast = useToast()

const category = ref<MyCategoryDetailDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

useSeoMeta({ title: () => `${category.value?.name ?? 'تصنيف'} - جولة` })

async function load() {
  loading.value = true
  try {
    category.value = await getMyCategory(categoryId)
  } catch {
    errorMessage.value = 'تعذر تحميل التصنيف.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

const sortedQuestions = computed(() => [...(category.value?.questions ?? [])].sort((a, b) => a.pointValue - b.pointValue))

function currentQuestionInputs() {
  return (category.value?.questions ?? []).map(q => ({ questionId: q.id, prompt: q.prompt, answer: q.answer }))
}

// Category details edit (name/icon)
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
    await updateMyCategory(category.value.id, detailsName.value.trim(), detailsIcon.value.trim() || null, currentQuestionInputs())
    toast.add({ title: 'تم تحديث التصنيف', color: 'success' })
    detailsModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ التعديلات', color: 'error' })
  } finally {
    savingDetails.value = false
  }
}

// Question edit -- prompt/answer only, point value is fixed by tier
const questionModalOpen = ref(false)
const editingQuestion = ref<MyCategoryQuestionDto | null>(null)
const formPrompt = ref('')
const formAnswer = ref('')
const savingQuestion = ref(false)

function openQuestionModal(question: MyCategoryQuestionDto) {
  editingQuestion.value = question
  formPrompt.value = question.prompt
  formAnswer.value = question.answer
  questionModalOpen.value = true
}

async function saveQuestion() {
  if (!category.value || !editingQuestion.value || !formPrompt.value.trim() || !formAnswer.value.trim()) return
  savingQuestion.value = true
  try {
    const targetId = editingQuestion.value.id
    const questions = currentQuestionInputs().map(q =>
      q.questionId === targetId ? { ...q, prompt: formPrompt.value.trim(), answer: formAnswer.value.trim() } : q
    )
    await updateMyCategory(category.value.id, category.value.name, category.value.icon, questions)
    toast.add({ title: 'تم حفظ السؤال', color: 'success' })
    questionModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ السؤال', color: 'error' })
  } finally {
    savingQuestion.value = false
  }
}
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 py-12 space-y-6">
    <UButton
      to="/my-categories"
      variant="ghost"
      icon="i-lucide-arrow-right"
      size="sm"
    >
      تصنيفاتي
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

      <div class="space-y-2">
        <p class="text-sm font-bold text-muted">
          الأسئلة
        </p>
        <UCard
          v-for="question in sortedQuestions"
          :key="question.id"
        >
          <div class="flex items-start gap-3">
            <UBadge
              color="secondary"
              size="lg"
              class="font-black tabular-nums shrink-0"
            >
              {{ question.pointValue }}
            </UBadge>
            <div class="flex-1 min-w-0 space-y-1">
              <p class="font-bold">
                {{ question.prompt }}
              </p>
              <p class="text-sm text-muted">
                الإجابة: {{ question.answer }}
              </p>
            </div>
            <UButton
              icon="i-lucide-pencil"
              color="neutral"
              variant="ghost"
              @click="openQuestionModal(question)"
            />
          </div>
        </UCard>
      </div>
    </template>

    <UModal v-model:open="detailsModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              تعديل التصنيف
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveDetails"
          >
            <UFormField label="اسم التصنيف">
              <UInput
                v-model="detailsName"
                class="w-full"
              />
            </UFormField>
            <UFormField label="الأيقونة (اختياري)">
              <UInput
                v-model="detailsIcon"
                class="w-full"
                placeholder="🏛️"
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
              سؤال {{ editingQuestion?.pointValue }} نقطة
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
