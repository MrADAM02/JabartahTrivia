<script setup lang="ts">
definePageMeta({ middleware: 'auth' })
useSeoMeta({ title: 'إنشاء تصنيف جديد - جولة' })

const { createMyCategory } = useApi()
const toast = useToast()

const POINT_TIERS = [100, 200, 300, 400, 500]

const step = ref<1 | 2 | 3>(1)
const name = ref('')
const icon = ref('')
const questions = ref(POINT_TIERS.map(pointValue => ({ pointValue, prompt: '', answer: '' })))
const loading = ref(false)
const errorMessage = ref('')

const step1Valid = computed(() => name.value.trim().length > 0)
const step2Valid = computed(() => questions.value.every(q => q.prompt.trim().length > 0 && q.answer.trim().length > 0))

function goToStep2() {
  if (!step1Valid.value) return
  step.value = 2
}
function goToStep3() {
  if (!step2Valid.value) return
  step.value = 3
}

async function submit() {
  errorMessage.value = ''
  loading.value = true
  try {
    await createMyCategory(name.value.trim(), icon.value.trim() || null, questions.value)
    toast.add({ title: 'تم إنشاء التصنيف', color: 'success' })
    await navigateTo('/my-categories')
  } catch {
    errorMessage.value = 'تعذر إنشاء التصنيف. تأكد من تعبئة جميع الحقول.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="max-w-2xl mx-auto px-4 sm:px-6 py-12 space-y-6">
    <div class="text-center space-y-2">
      <h1 class="text-3xl font-black text-green-900 dark:text-green-100">
        إنشاء تصنيف جديد
      </h1>
      <p class="text-muted">
        أنشئ تصنيفك المخصص بأسئلتك الخاصة
      </p>
    </div>

    <div class="flex items-center justify-center gap-3 text-sm font-bold">
      <span :class="step >= 1 ? 'text-primary' : 'text-muted'">١. معلومات التصنيف</span>
      <UIcon
        name="i-lucide-chevron-left"
        class="text-muted"
      />
      <span :class="step >= 2 ? 'text-primary' : 'text-muted'">٢. إضافة الأسئلة</span>
      <UIcon
        name="i-lucide-chevron-left"
        class="text-muted"
      />
      <span :class="step >= 3 ? 'text-primary' : 'text-muted'">٣. مراجعة وإرسال</span>
    </div>

    <UAlert
      v-if="errorMessage"
      color="error"
      variant="subtle"
      :title="errorMessage"
    />

    <UCard v-if="step === 1">
      <div class="space-y-4">
        <UFormField label="اسم التصنيف">
          <UInput
            v-model="name"
            size="lg"
            class="w-full"
            placeholder="مثال: تاريخ المملكة العربية السعودية"
          />
        </UFormField>
        <UFormField label="أيقونة التصنيف (اختياري)">
          <UInput
            v-model="icon"
            size="lg"
            class="w-full"
            placeholder="🏛️"
          />
        </UFormField>
        <UButton
          block
          size="lg"
          color="secondary"
          class="font-bold text-green-950"
          :disabled="!step1Valid"
          @click="goToStep2"
        >
          التالي
        </UButton>
      </div>
    </UCard>

    <div
      v-else-if="step === 2"
      class="space-y-4"
    >
      <UCard
        v-for="q in questions"
        :key="q.pointValue"
      >
        <template #header>
          <UBadge color="secondary">
            {{ q.pointValue }} نقطة
          </UBadge>
        </template>
        <div class="space-y-3">
          <UFormField label="نص السؤال">
            <UInput
              v-model="q.prompt"
              size="lg"
              class="w-full"
            />
          </UFormField>
          <UFormField label="الإجابة">
            <UInput
              v-model="q.answer"
              size="lg"
              class="w-full"
            />
          </UFormField>
        </div>
      </UCard>

      <div class="flex gap-2">
        <UButton
          variant="outline"
          @click="step = 1"
        >
          السابق
        </UButton>
        <UButton
          block
          color="secondary"
          class="font-bold text-green-950"
          :disabled="!step2Valid"
          @click="goToStep3"
        >
          التالي
        </UButton>
      </div>
    </div>

    <div
      v-else
      class="space-y-4"
    >
      <UCard>
        <div class="flex items-center gap-3 mb-4">
          <span class="text-3xl">{{ icon || '📚' }}</span>
          <p class="text-lg font-bold">
            {{ name }}
          </p>
        </div>
        <div class="space-y-3 divide-y divide-green-100 dark:divide-gray-800">
          <div
            v-for="q in questions"
            :key="q.pointValue"
            class="pt-3 first:pt-0"
          >
            <p class="text-xs font-bold text-primary">
              {{ q.pointValue }} نقطة
            </p>
            <p class="font-bold">
              {{ q.prompt }}
            </p>
            <p class="text-sm text-muted">
              الإجابة: {{ q.answer }}
            </p>
          </div>
        </div>
      </UCard>

      <div class="flex gap-2">
        <UButton
          variant="outline"
          @click="step = 2"
        >
          السابق
        </UButton>
        <UButton
          block
          color="secondary"
          class="font-bold text-green-950"
          :loading="loading"
          icon="i-lucide-send"
          @click="submit"
        >
          إرسال
        </UButton>
      </div>
    </div>
  </div>
</template>
