<script setup lang="ts">
const props = defineProps<{ modelValue: boolean }>()
const emit = defineEmits<{ 'update:modelValue': [boolean] }>()

const { user, isLoggedIn, clearSession } = useAuth()
const route = useRoute()

const navItems = computed(() => {
  const items = [
    { to: '/', label: 'الرئيسية', icon: 'i-lucide-home' },
    { to: '/', label: 'العب', icon: 'i-lucide-gamepad-2' },
    { to: '/how-to-play', label: 'كيف تلعب', icon: 'i-lucide-circle-help' },
    { to: '/faq', label: 'الأسئلة الشائعة', icon: 'i-lucide-message-circle-question' }
  ]
  if (isLoggedIn.value) {
    items.push({ to: '/previous-games', label: 'ألعابي السابقة', icon: 'i-lucide-history' })
    items.push({ to: '/my-categories', label: 'تصنيفاتي', icon: 'i-lucide-layers' })
    items.push({ to: '/account', label: 'حسابي', icon: 'i-lucide-user' })
  }
  return items
})

// "الرئيسية" and "العب" both point at "/" -- only the first match gets the active
// indicator, so landing on "/" doesn't light up both rows at once.
const activeIndex = computed(() => navItems.value.findIndex(item => item.to === route.path))

function close() {
  emit('update:modelValue', false)
}

function logout() {
  clearSession()
  close()
  navigateTo('/')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="drawer-fade">
      <div
        v-if="props.modelValue"
        class="fixed inset-0 z-50 bg-black/50"
        @click="close"
      />
    </Transition>
    <Transition name="drawer-slide">
      <aside
        v-if="props.modelValue"
        class="fixed inset-y-0 inset-s-0 z-50 w-[85%] max-w-xs bg-white dark:bg-gray-950 shadow-2xl flex flex-col"
      >
        <div class="flex items-center justify-between px-4 py-4 border-b border-green-100 dark:border-gray-800 shrink-0">
          <button
            class="text-gray-500 dark:text-gray-400"
            @click="close"
          >
            <UIcon
              name="i-lucide-x"
              class="size-6"
            />
          </button>
          <span class="font-display text-xl font-black text-green-900 dark:text-green-100">
            جولة
          </span>
        </div>

        <div class="p-4 shrink-0">
          <div
            v-if="isLoggedIn"
            class="flex items-center gap-3 bg-green-50 dark:bg-gray-900 rounded-xl px-4 py-3"
          >
            <div class="flex-1 text-end">
              <p class="font-bold">
                {{ user?.name }}
              </p>
              <p class="text-xs text-muted">
                {{ user?.email }}
              </p>
            </div>
            <div class="size-10 shrink-0 rounded-full bg-green-900 text-gold-400 flex items-center justify-center">
              <UIcon
                name="i-lucide-user"
                class="size-5"
              />
            </div>
          </div>
          <NuxtLink
            v-else
            to="/login"
            class="flex items-center justify-center gap-2 bg-green-50 dark:bg-gray-900 rounded-xl px-4 py-3 font-bold text-primary"
            @click="close"
          >
            <UIcon
              name="i-lucide-user"
              class="size-5"
            />
            تسجيل الدخول
          </NuxtLink>
        </div>

        <nav class="flex-1 overflow-y-auto px-2 space-y-1">
          <NuxtLink
            v-for="(item, index) in navItems"
            :key="item.label"
            :to="item.to"
            class="flex items-center gap-3 px-4 py-3 rounded-lg font-bold transition-colors hover:bg-green-50 dark:hover:bg-gray-900"
            :class="index === activeIndex ? 'text-primary' : 'text-gray-700 dark:text-gray-300'"
            @click="close"
          >
            <span
              v-if="index === activeIndex"
              class="size-1.5 rounded-full bg-gold-500 shrink-0"
            />
            <span class="flex-1 text-end">{{ item.label }}</span>
            <UIcon
              :name="item.icon"
              class="size-5 shrink-0"
            />
          </NuxtLink>
        </nav>

        <div class="p-4 border-t border-green-100 dark:border-gray-800 space-y-2 shrink-0">
          <UButton
            to="/"
            block
            color="secondary"
            class="font-bold text-green-950 rounded-full"
            @click="close"
          >
            ابدأ اللعبة
          </UButton>
          <button
            v-if="isLoggedIn"
            class="w-full flex items-center justify-center gap-1 py-2 text-sm font-bold text-error"
            @click="logout"
          >
            <UIcon
              name="i-lucide-log-out"
              class="size-4"
            />
            تسجيل الخروج
          </button>
        </div>
      </aside>
    </Transition>
  </Teleport>
</template>

<style scoped>
.drawer-slide-enter-active,
.drawer-slide-leave-active {
  transition: transform var(--motion-duration-base) var(--ease-standard);
}
.drawer-slide-enter-from,
.drawer-slide-leave-to {
  transform: translateX(100%);
}

.drawer-fade-enter-active,
.drawer-fade-leave-active {
  transition: opacity var(--motion-duration-base) var(--ease-standard);
}
.drawer-fade-enter-from,
.drawer-fade-leave-to {
  opacity: 0;
}
</style>
