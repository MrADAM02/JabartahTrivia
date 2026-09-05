<script setup lang="ts">
import { DURATIONS } from '~/utils/motion'

const { user, isLoggedIn, clearSession } = useAuth()

const navLinks = computed(() => {
  const links = [
    { to: '/', label: 'الرئيسية' },
    { to: '/how-to-play', label: 'كيف تلعب' },
    { to: '/faq', label: 'الأسئلة الشائعة' }
  ]
  if (isLoggedIn.value) {
    links.push({ to: '/my-categories', label: 'تصنيفاتي' })
    links.push({ to: '/previous-games', label: 'ألعابي السابقة' })
  }
  return links
})

const drawerOpen = ref(false)
const accountMenuOpen = ref(false)

function logout() {
  clearSession()
  accountMenuOpen.value = false
  navigateTo('/')
}
</script>

<template>
  <div class="min-h-screen flex flex-col bg-white dark:bg-gray-950">
    <header class="sticky top-0 z-40 bg-green-900 text-white shadow-md">
      <div
        class="max-w-7xl mx-auto px-4 sm:px-6 py-3 flex items-center justify-between gap-4"
      >
        <NuxtLink
          to="/"
          class="font-display text-2xl font-black text-gold-400 shrink-0"
        >
          جولة
        </NuxtLink>

        <nav class="hidden md:flex items-center gap-6 text-sm font-bold">
          <NuxtLink
            v-for="link in navLinks"
            :key="link.to"
            :to="link.to"
            class="text-white/90 hover:text-gold-300 transition-colors"
          >
            {{ link.label }}
          </NuxtLink>
        </nav>

        <div class="hidden md:flex items-center gap-3">
          <UButton
            to="/"
            color="secondary"
            size="md"
            class="font-bold text-green-950 rounded-full"
          >
            ابدأ اللعبة
          </UButton>

          <div
            v-if="isLoggedIn"
            class="relative"
            @mouseenter="accountMenuOpen = true"
            @mouseleave="accountMenuOpen = false"
          >
            <button
              class="flex items-center gap-2 text-sm font-bold text-white/90 hover:text-gold-300 transition-colors"
            >
              <UIcon
                name="i-lucide-user-circle"
                class="size-5"
              />
              {{ user?.name }}
            </button>
            <MotionScale
              :show="accountMenuOpen"
              :duration="DURATIONS.fast"
            >
              <div
                class="absolute inset-s-0 mt-2 w-44 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-white shadow-lg border border-green-100 dark:border-gray-800 overflow-hidden"
              >
                <NuxtLink
                  to="/account"
                  class="block px-4 py-2 text-sm font-bold hover:bg-green-50 dark:hover:bg-gray-800"
                  @click="accountMenuOpen = false"
                >
                  حسابي
                </NuxtLink>
                <button
                  class="w-full text-start px-4 py-2 text-sm font-bold text-error hover:bg-green-50 dark:hover:bg-gray-800"
                  @click="logout"
                >
                  تسجيل الخروج
                </button>
              </div>
            </MotionScale>
          </div>
          <NuxtLink
            v-else
            to="/login"
            class="text-sm font-bold text-white/90 hover:text-gold-300 transition-colors"
          >
            تسجيل الدخول
          </NuxtLink>
        </div>

        <button
          class="md:hidden text-white"
          @click="drawerOpen = true"
        >
          <UIcon
            name="i-lucide-menu"
            class="size-7"
          />
        </button>
      </div>
    </header>

    <SideNavDrawer v-model="drawerOpen" />

    <main class="flex-1">
      <slot />
    </main>

    <footer class="bg-green-900 text-white mt-auto">
      <div
        class="max-w-7xl mx-auto px-4 sm:px-6 py-8 grid grid-cols-1 sm:grid-cols-3 gap-6 text-sm"
      >
        <div>
          <p class="font-display text-xl font-black text-gold-400 mb-2">
            جولة
          </p>
          <p class="text-white/70">
            منصة ألعاب جماعية عربية، للمجالس والسهرات العائلية.
          </p>
        </div>
        <div class="space-y-2">
          <p class="font-bold text-gold-300">
            روابط
          </p>
          <NuxtLink
            to="/about"
            class="block text-white/80 hover:text-gold-300"
          >
            من نحن
          </NuxtLink>
          <NuxtLink
            to="/terms"
            class="block text-white/80 hover:text-gold-300"
          >
            الشروط والأحكام
          </NuxtLink>
          <NuxtLink
            to="/privacy"
            class="block text-white/80 hover:text-gold-300"
          >
            سياسة الخصوصية
          </NuxtLink>
        </div>
        <div class="space-y-2">
          <p class="font-bold text-gold-300">
            المساعدة
          </p>
          <NuxtLink
            to="/how-to-play"
            class="block text-white/80 hover:text-gold-300"
          >
            كيف تلعب
          </NuxtLink>
          <NuxtLink
            to="/faq"
            class="block text-white/80 hover:text-gold-300"
          >
            الأسئلة الشائعة
          </NuxtLink>
          <a
            href="mailto:adamgame340@gmail.com"
            class="block mt-2 text-white/70 hover:text-gold-300"
          >
            تواصل معنا
          </a>
        </div>
      </div>
      <div
        class="border-t border-green-800 text-center text-xs text-white/60 py-3"
      >
        © {{ new Date().getFullYear() }} جولة. جميع الحقوق محفوظة.
      </div>
    </footer>
  </div>
</template>
