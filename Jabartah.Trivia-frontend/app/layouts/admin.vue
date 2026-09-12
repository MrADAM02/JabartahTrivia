<script setup lang="ts">
const { user, clearSession } = useAuth()
const colorMode = useColorMode()

function toggleColorMode() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

const navLinks = [
  { to: '/admin', label: 'الإحصائيات', icon: 'i-lucide-layout-dashboard' },
  { to: '/admin/categories', label: 'فئات التحدي', icon: 'i-lucide-grid-3x3' },
  { to: '/admin/password/categories', label: 'فئات كلمة السر', icon: 'i-lucide-key-round' },
  { to: '/admin/ranking/categories', label: 'فئات رتبها', icon: 'i-lucide-list-ordered' },
  { to: '/admin/top100/categories', label: 'فئات تحدي الـ100', icon: 'i-lucide-trophy' }
]

const drawerOpen = ref(false)

function logout() {
  clearSession()
  navigateTo('/')
}
</script>

<template>
  <div class="min-h-screen flex flex-col lg:flex-row bg-gray-50 dark:bg-gray-950">
    <!-- Mobile top bar: the sidebar below is desktop-only, this replaces it on small screens -->
    <header class="lg:hidden sticky top-0 z-30 flex items-center justify-between gap-2 bg-green-950 text-white px-3 py-3 shadow-md">
      <button
        class="p-1"
        @click="drawerOpen = true"
      >
        <UIcon
          name="i-lucide-menu"
          class="size-6"
        />
      </button>
      <NuxtLink
        to="/admin"
        class="font-display text-lg font-black text-gold-400"
      >
        لوحة التحكم
      </NuxtLink>
      <div class="flex items-center gap-3">
        <DarkModeToggle class="text-white/80" />
        <NuxtLink
          to="/"
          class="flex items-center gap-1 text-xs font-bold text-white/80"
        >
          <UIcon
            name="i-lucide-arrow-right"
            class="size-4"
          />
          الموقع
        </NuxtLink>
      </div>
    </header>

    <!-- Desktop sidebar -->
    <aside class="hidden lg:flex lg:w-60 lg:shrink-0 bg-green-950 text-white flex-col">
      <NuxtLink
        to="/admin"
        class="px-5 py-5 border-b border-white/10"
      >
        <p class="font-display text-xl font-black text-gold-400">
          جولة
        </p>
        <p class="text-xs text-white/60 font-bold">
          لوحة التحكم
        </p>
      </NuxtLink>

      <NuxtLink
        to="/"
        class="flex items-center gap-3 px-5 py-3 text-sm font-bold text-white/70 hover:bg-white/10 hover:text-white transition-colors border-b border-white/10"
      >
        <UIcon
          name="i-lucide-arrow-right"
          class="size-4"
        />
        العودة إلى الموقع
      </NuxtLink>

      <nav class="flex-1 px-3 py-4 space-y-1">
        <NuxtLink
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-bold text-white/80 hover:bg-white/10 hover:text-white transition-colors"
          active-class="!bg-white/15 !text-white"
        >
          <UIcon
            :name="link.icon"
            class="size-5"
          />
          {{ link.label }}
        </NuxtLink>
      </nav>

      <div class="px-3 py-4 border-t border-white/10 space-y-2">
        <button
          type="button"
          class="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-bold text-white/70 hover:bg-white/10 hover:text-white transition-colors"
          @click="toggleColorMode"
        >
          <UIcon
            :name="colorMode.value === 'dark' ? 'i-lucide-sun' : 'i-lucide-moon'"
            class="size-4"
          />
          {{ colorMode.value === 'dark' ? 'الوضع الفاتح' : 'الوضع الداكن' }}
        </button>
        <div class="flex items-center justify-between px-3 py-2 text-xs text-white/60">
          <span class="truncate">{{ user?.name }}</span>
          <button
            class="font-bold text-error hover:text-red-400 shrink-0"
            @click="logout"
          >
            خروج
          </button>
        </div>
      </div>
    </aside>

    <!-- Mobile drawer -->
    <Teleport to="body">
      <Transition name="drawer-fade">
        <div
          v-if="drawerOpen"
          class="fixed inset-0 z-50 bg-black/50 lg:hidden"
          @click="drawerOpen = false"
        />
      </Transition>
      <Transition name="drawer-slide">
        <aside
          v-if="drawerOpen"
          class="fixed inset-y-0 inset-s-0 z-50 w-[80%] max-w-xs bg-green-950 text-white shadow-2xl flex flex-col lg:hidden"
        >
          <div class="flex items-center justify-between px-4 py-4 border-b border-white/10 shrink-0">
            <button @click="drawerOpen = false">
              <UIcon
                name="i-lucide-x"
                class="size-6"
              />
            </button>
            <span class="font-display text-lg font-black text-gold-400">لوحة التحكم</span>
          </div>

          <NuxtLink
            to="/"
            class="flex items-center gap-3 px-5 py-3 text-sm font-bold text-white/80 border-b border-white/10 shrink-0"
            @click="drawerOpen = false"
          >
            <UIcon
              name="i-lucide-arrow-right"
              class="size-4"
            />
            العودة إلى الموقع
          </NuxtLink>

          <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
            <NuxtLink
              v-for="link in navLinks"
              :key="link.to"
              :to="link.to"
              class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-bold text-white/80 hover:bg-white/10 hover:text-white transition-colors"
              active-class="!bg-white/15 !text-white"
              @click="drawerOpen = false"
            >
              <UIcon
                :name="link.icon"
                class="size-5"
              />
              {{ link.label }}
            </NuxtLink>
          </nav>

          <div class="px-3 py-4 border-t border-white/10 shrink-0">
            <div class="flex items-center justify-between px-3 py-2 text-xs text-white/60">
              <span class="truncate">{{ user?.name }}</span>
              <button
                class="font-bold text-error hover:text-red-400 shrink-0"
                @click="logout"
              >
                خروج
              </button>
            </div>
          </div>
        </aside>
      </Transition>
    </Teleport>

    <main class="flex-1 min-w-0 p-4 sm:p-6 lg:p-8">
      <slot />
    </main>
  </div>
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
