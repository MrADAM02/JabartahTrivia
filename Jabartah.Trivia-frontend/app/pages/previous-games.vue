<script setup lang="ts">
import type { MySessionDto } from '~/types/api'

definePageMeta({ middleware: 'auth' })
useSeoMeta({ title: 'ألعابي السابقة - جولة' })

const { getMySessions } = useApi()

const sessions = ref<MySessionDto[]>([])
const loading = ref(true)
const errorMessage = ref('')

const modeMeta: Record<string, { icon: string, name: string }> = {
  Trivia: { icon: '🎯', name: 'لعبة الأسئلة' },
  Password: { icon: '🤫', name: 'كلمة السر' },
  Ranking: { icon: '🔢', name: 'رتبها' },
  Top100: { icon: '💯', name: 'تحدي الـ100' }
}

onMounted(async () => {
  try {
    sessions.value = await getMySessions()
  } catch {
    errorMessage.value = 'تعذر تحميل ألعابك السابقة.'
  } finally {
    loading.value = false
  }
})

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('ar-SA', { year: 'numeric', month: 'long', day: 'numeric' })
}
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 py-12 space-y-6">
    <div class="text-center space-y-2">
      <h1 class="text-3xl sm:text-4xl font-black text-green-900 dark:text-green-100">
        ألعابي السابقة
      </h1>
      <p class="text-muted">
        سجل جميع الجلسات التي لعبتها
      </p>
    </div>

    <UAlert
      v-if="errorMessage"
      color="error"
      variant="subtle"
      :title="errorMessage"
    />

    <div
      v-if="loading"
      class="text-center text-muted"
    >
      جارِ التحميل...
    </div>

    <div
      v-else-if="sessions.length === 0"
      class="text-center space-y-4 py-12"
    >
      <UIcon
        name="i-lucide-gamepad-2"
        class="size-14 text-muted mx-auto"
      />
      <p class="text-lg font-bold">
        لا توجد ألعاب سابقة
      </p>
      <p class="text-muted">
        ابدأ لعبتك الأولى لتظهر هنا
      </p>
      <UButton
        to="/"
        color="secondary"
        class="font-bold text-green-950"
      >
        ابدأ لعبة جديدة
      </UButton>
    </div>

    <div
      v-else
      class="space-y-3"
    >
      <UCard
        v-for="session in sessions"
        :key="session.id"
      >
        <div class="flex items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <span class="text-3xl">{{ modeMeta[session.mode]?.icon ?? '🎮' }}</span>
            <div>
              <p class="font-bold">
                {{ modeMeta[session.mode]?.name ?? session.mode }}
              </p>
              <p class="text-xs text-muted">
                {{ formatDate(session.createdAt) }}
              </p>
            </div>
          </div>
          <div class="text-end">
            <p
              v-if="session.isDraw"
              class="font-bold text-muted"
            >
              🤝 تعادل
            </p>
            <p
              v-else-if="session.winnerTeamNames.length > 0"
              class="font-bold text-primary"
            >
              🏆 {{ session.winnerTeamNames[0] }}
            </p>
            <p class="text-xs text-muted">
              {{ session.teams.map(t => `${t.name}: ${t.score}`).join(' — ') }}
            </p>
          </div>
        </div>
      </UCard>
    </div>
  </div>
</template>
