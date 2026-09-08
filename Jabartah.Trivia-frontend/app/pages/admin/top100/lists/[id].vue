<script setup lang="ts">
import type { AdminTop100ListDetailDto, AdminTop100ListItemDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })

const route = useRoute()
const listId = route.params.id as string

const { getTop100AdminList, updateTop100AdminList, createTop100AdminItem, updateTop100AdminItem, deleteTop100AdminItem } = useAdminApi()
const toast = useToast()

const list = ref<AdminTop100ListDetailDto | null>(null)
const loading = ref(true)
const errorMessage = ref('')

useSeoMeta({ title: () => `${list.value?.title ?? 'قائمة'} - جولة` })

async function load() {
  loading.value = true
  try {
    list.value = await getTop100AdminList(listId)
  } catch {
    errorMessage.value = 'تعذر تحميل القائمة.'
  } finally {
    loading.value = false
  }
}

onMounted(load)

const sortedItems = computed(() => [...(list.value?.items ?? [])].sort((a, b) => a.position - b.position))
const nextPosition = computed(() => (list.value?.items.length ?? 0) + 1)

// Title edit
const titleModalOpen = ref(false)
const formTitle = ref('')
const savingTitle = ref(false)

function openTitleEdit() {
  if (!list.value) return
  formTitle.value = list.value.title
  titleModalOpen.value = true
}

async function saveTitle() {
  if (!list.value || !formTitle.value.trim()) return
  savingTitle.value = true
  try {
    await updateTop100AdminList(list.value.id, formTitle.value.trim())
    toast.add({ title: 'تم تحديث القائمة', color: 'success' })
    titleModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ التعديلات', color: 'error' })
  } finally {
    savingTitle.value = false
  }
}

// Item create/edit
const itemModalOpen = ref(false)
const editingItem = ref<AdminTop100ListItemDto | null>(null)
const formLabel = ref('')
const formPosition = ref(1)
const formAlternateSpellings = ref('')
const savingItem = ref(false)
const deletingItemId = ref<string | null>(null)

function openItemModal(existing: AdminTop100ListItemDto | null = null) {
  editingItem.value = existing
  formLabel.value = existing?.label ?? ''
  formPosition.value = existing?.position ?? nextPosition.value
  formAlternateSpellings.value = existing?.alternateSpellings.join('، ') ?? ''
  itemModalOpen.value = true
}

async function saveItem() {
  if (!formLabel.value.trim() || !list.value) return
  savingItem.value = true
  try {
    const alternateSpellings = formAlternateSpellings.value
      .split(/[،,]/)
      .map(s => s.trim())
      .filter(Boolean)

    if (editingItem.value) {
      await updateTop100AdminItem(editingItem.value.id, formLabel.value.trim(), formPosition.value, alternateSpellings)
    } else {
      await createTop100AdminItem(list.value.id, formLabel.value.trim(), formPosition.value, alternateSpellings)
    }
    toast.add({ title: 'تم حفظ العنصر', color: 'success' })
    itemModalOpen.value = false
    await load()
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حفظ العنصر', color: 'error' })
  } finally {
    savingItem.value = false
  }
}

async function removeItem(item: AdminTop100ListItemDto) {
  deletingItemId.value = item.id
  try {
    await deleteTop100AdminItem(item.id)
    toast.add({ title: 'تم حذف العنصر', color: 'success' })
    await load()
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حذف العنصر', color: 'error' })
  } finally {
    deletingItemId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <UButton
      :to="list ? `/admin/top100/categories/${list.categoryId}` : '/admin/top100/categories'"
      variant="ghost"
      icon="i-lucide-arrow-right"
      size="sm"
    >
      رجوع
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

    <template v-else-if="list">
      <div class="flex items-center justify-between gap-3">
        <h1 class="text-xl font-black text-green-900 dark:text-green-100">
          {{ list.title }}
        </h1>
        <UButton
          icon="i-lucide-pencil"
          color="neutral"
          variant="ghost"
          @click="openTitleEdit"
        >
          تعديل العنوان
        </UButton>
      </div>

      <div class="space-y-2">
        <div class="flex items-center justify-between">
          <p class="text-sm font-bold text-muted">
            العناصر بترتيب الأهمية ({{ list.items.length }})
          </p>
          <UButton
            icon="i-lucide-plus"
            size="sm"
            color="secondary"
            class="font-bold text-green-950"
            @click="openItemModal()"
          >
            عنصر جديد
          </UButton>
        </div>

        <UCard
          v-for="item in sortedItems"
          :key="item.id"
        >
          <div class="flex items-center gap-3">
            <UBadge
              color="secondary"
              size="lg"
              class="font-black tabular-nums shrink-0"
            >
              {{ item.position }}
            </UBadge>
            <div class="flex-1 min-w-0">
              <p class="truncate">
                {{ item.label }}
              </p>
              <p
                v-if="item.alternateSpellings.length"
                class="text-xs text-muted truncate"
              >
                بدائل: {{ item.alternateSpellings.join('، ') }}
              </p>
            </div>
            <div class="flex items-center gap-1 shrink-0">
              <UButton
                icon="i-lucide-pencil"
                color="neutral"
                variant="ghost"
                @click="openItemModal(item)"
              />
              <UButton
                icon="i-lucide-trash-2"
                color="error"
                variant="ghost"
                :loading="deletingItemId === item.id"
                @click="removeItem(item)"
              />
            </div>
          </div>
        </UCard>

        <p
          v-if="!list.items.length"
          class="text-muted italic text-sm px-1"
        >
          لا توجد عناصر بعد.
        </p>
      </div>
    </template>

    <UModal v-model:open="titleModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              تعديل عنوان القائمة
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveTitle"
          >
            <UFormField label="العنوان">
              <UInput
                v-model="formTitle"
                class="w-full"
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="titleModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingTitle"
                :disabled="!formTitle.trim()"
                @click="saveTitle"
              >
                حفظ
              </UButton>
            </div>
          </template>
        </UCard>
      </template>
    </UModal>

    <UModal v-model:open="itemModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              {{ editingItem ? 'تعديل العنصر' : 'عنصر جديد' }}
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveItem"
          >
            <UFormField label="النص">
              <UInput
                v-model="formLabel"
                class="w-full"
                autofocus
              />
            </UFormField>
            <UFormField label="الترتيب (وقيمة النقاط)">
              <UInput
                v-model.number="formPosition"
                type="number"
                :min="1"
                class="w-full"
              />
            </UFormField>
            <UFormField label="إجابات بديلة مقبولة (افصل بفاصلة، اختياري)">
              <UInput
                v-model="formAlternateSpellings"
                class="w-full"
                placeholder="مثال: أمريكا، الولايات المتحدة"
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="itemModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingItem"
                :disabled="!formLabel.trim()"
                @click="saveItem"
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
