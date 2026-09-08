<script setup lang="ts">
import type { AdminTop100CategoryDto, AdminTop100ListDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })

const route = useRoute()
const categoryId = route.params.id as string

const {
  listTop100AdminCategories,
  updateTop100AdminCategory,
  listTop100AdminLists,
  createTop100AdminList,
  deleteTop100AdminList
} = useAdminApi()
const toast = useToast()

const category = ref<AdminTop100CategoryDto | null>(null)
const lists = ref<AdminTop100ListDto[]>([])
const loading = ref(true)
const errorMessage = ref('')

useSeoMeta({ title: () => `${category.value?.name ?? 'فئة'} - جولة` })

async function load() {
  loading.value = true
  try {
    const [categories, listOfLists] = await Promise.all([
      listTop100AdminCategories(),
      listTop100AdminLists(categoryId)
    ])
    category.value = categories.find(c => c.id === categoryId) ?? null
    lists.value = listOfLists
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
const detailsDescription = ref('')
const savingDetails = ref(false)

function openDetailsEdit() {
  if (!category.value) return
  detailsName.value = category.value.name
  detailsIcon.value = category.value.icon ?? ''
  detailsDescription.value = category.value.description ?? ''
  detailsModalOpen.value = true
}

async function saveDetails() {
  if (!category.value || !detailsName.value.trim()) return
  savingDetails.value = true
  try {
    await updateTop100AdminCategory(
      category.value.id,
      detailsName.value.trim(),
      detailsIcon.value.trim() || null,
      detailsDescription.value.trim() || null
    )
    toast.add({ title: 'تم تحديث الفئة', color: 'success' })
    detailsModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر حفظ التعديلات', color: 'error' })
  } finally {
    savingDetails.value = false
  }
}

// New list modal
const listModalOpen = ref(false)
const formTitle = ref('')
const savingList = ref(false)
const deletingListId = ref<string | null>(null)

function openListModal() {
  formTitle.value = ''
  listModalOpen.value = true
}

async function saveList() {
  if (!formTitle.value.trim()) return
  savingList.value = true
  try {
    await createTop100AdminList(categoryId, formTitle.value.trim())
    toast.add({ title: 'تم إنشاء القائمة', color: 'success' })
    listModalOpen.value = false
    await load()
  } catch {
    toast.add({ title: 'تعذر إنشاء القائمة', color: 'error' })
  } finally {
    savingList.value = false
  }
}

async function removeList(list: AdminTop100ListDto) {
  deletingListId.value = list.id
  try {
    await deleteTop100AdminList(list.id)
    lists.value = lists.value.filter(l => l.id !== list.id)
    toast.add({ title: 'تم حذف القائمة', color: 'success' })
  } catch (err: unknown) {
    const message = (err as { data?: { detail?: string } })?.data?.detail
    toast.add({ title: message ?? 'تعذر حذف القائمة', color: 'error' })
  } finally {
    deletingListId.value = null
  }
}
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <UButton
      to="/admin/top100/categories"
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
          <span class="text-4xl">{{ category.icon ?? '💯' }}</span>
          <div>
            <h1 class="text-2xl font-black text-green-900 dark:text-green-100">
              {{ category.name }}
            </h1>
            <p
              v-if="category.description"
              class="text-sm text-muted"
            >
              {{ category.description }}
            </p>
          </div>
        </div>
        <UButton
          icon="i-lucide-pencil"
          color="neutral"
          variant="ghost"
          @click="openDetailsEdit"
        >
          تعديل
        </UButton>
      </div>

      <div class="space-y-2">
        <div class="flex items-center justify-between">
          <p class="text-sm font-bold text-muted">
            القوائم ({{ lists.length }})
          </p>
          <UButton
            icon="i-lucide-plus"
            size="sm"
            color="secondary"
            class="font-bold text-green-950"
            @click="openListModal"
          >
            قائمة جديدة
          </UButton>
        </div>

        <UCard
          v-for="list in lists"
          :key="list.id"
        >
          <div class="flex items-center justify-between gap-3">
            <NuxtLink
              :to="`/admin/top100/lists/${list.id}`"
              class="flex items-center gap-3 flex-1 min-w-0"
            >
              <span class="font-bold truncate">{{ list.title }}</span>
              <UBadge
                color="neutral"
                variant="subtle"
              >
                {{ list.itemCount }} عنصر
              </UBadge>
            </NuxtLink>
            <UButton
              icon="i-lucide-trash-2"
              color="error"
              variant="ghost"
              :loading="deletingListId === list.id"
              @click="removeList(list)"
            />
          </div>
        </UCard>

        <p
          v-if="!lists.length"
          class="text-muted italic text-sm px-1"
        >
          لا توجد قوائم بعد.
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
                placeholder="💯"
              />
            </UFormField>
            <UFormField label="الوصف (اختياري)">
              <UTextarea
                v-model="detailsDescription"
                class="w-full"
                :rows="2"
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

    <UModal v-model:open="listModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <p class="font-bold">
              قائمة جديدة
            </p>
          </template>
          <form
            class="space-y-4"
            @submit.prevent="saveList"
          >
            <UFormField label="عنوان القائمة">
              <UInput
                v-model="formTitle"
                class="w-full"
                autofocus
              />
            </UFormField>
          </form>
          <template #footer>
            <div class="flex gap-2 justify-end">
              <UButton
                variant="ghost"
                @click="listModalOpen = false"
              >
                إلغاء
              </UButton>
              <UButton
                color="secondary"
                class="font-bold text-green-950"
                :loading="savingList"
                :disabled="!formTitle.trim()"
                @click="saveList"
              >
                إنشاء
              </UButton>
            </div>
          </template>
        </UCard>
      </template>
    </UModal>
  </div>
</template>
