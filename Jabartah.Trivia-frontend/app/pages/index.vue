<script setup lang="ts">
const modes = [
  { to: '/trivia/setup', icon: '🎯', title: 'لعبة الأسئلة', description: 'اختر فئة ونقطة، ومن يجيب أولاً يفوز بالنقاط' },
  { to: '/password/setup', icon: '🤫', title: 'كلمة السر', description: 'لمّح لفريقك بكلمة واحدة ليخمنوا الكلمة السرية' },
  { to: '/ranking/setup', icon: '🔢', title: 'رتبها', description: 'رتب البطاقات بالترتيب الصحيح قبل الوقت' },
  { to: '/top100/setup', icon: '💯', title: 'تحدي الـ100', description: 'خمّنوا عناصر القائمة، والعنصر الأصعب يستحق أكثر' }
]

const values = [
  { icon: 'i-lucide-users', title: 'فريقان وإثارة حقيقية', description: 'نافس بروح المجالس والسهرات — مناسب لجميع الأعمار' },
  { icon: 'i-lucide-gift', title: 'لعبة مجانية بالكامل', description: 'العب بلا حدود — بدون اشتراك وبدون رسوم' },
  { icon: 'i-lucide-monitor-smartphone', title: 'شاشة واحدة، والكل يلعب', description: 'اجمعوا حول الشاشة وابدأوا اللعب فوراً' }
]

const steps = [
  { title: 'اختر لعبتك', description: 'من بين 4 ألعاب مختلفة تناسب كل مجلس' },
  { title: 'اجمع فريقك', description: 'قسّموا أنفسكم فريقين وابدأوا التحدي' },
  { title: 'العب وافوز', description: 'نقاط وضحك ومنافسة حتى النهاية' }
]
</script>

<template>
  <div>
    <section class="bg-green-900 text-white">
      <div class="max-w-5xl mx-auto px-4 sm:px-6 py-16 sm:py-24 text-center space-y-6">
        <h1 class="text-4xl sm:text-6xl font-black text-gold-400">
          جولة
        </h1>
        <p class="text-lg sm:text-2xl font-bold text-white/90">
          منصة الألعاب الجماعية العربية
        </p>
        <p class="text-white/70 max-w-xl mx-auto">
          اختر لعبتك وابدأ التحدي مع أصدقائك وعائلتك — أربع ألعاب مختلفة، شاشة واحدة، ومنافسة لا تُنسى.
        </p>
      </div>
    </section>

    <section class="max-w-6xl mx-auto px-4 sm:px-6 -mt-10 sm:-mt-14 pb-16">
      <h2 class="sr-only">
        اختر لعبتك
      </h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <NuxtLink
          v-for="mode in modes"
          :key="mode.to"
          :to="mode.to"
          class="block"
        >
          <UCard
            class="h-full text-center ring-1 ring-green-100 dark:ring-gray-800 hover:ring-2 hover:ring-secondary transition-all cursor-pointer shadow-lg"
          >
            <div class="text-5xl mb-3">
              {{ mode.icon }}
            </div>
            <h3 class="text-xl font-bold mb-2 text-green-900 dark:text-green-100">
              {{ mode.title }}
            </h3>
            <p class="text-muted text-sm">
              {{ mode.description }}
            </p>
          </UCard>
        </NuxtLink>
      </div>
    </section>

    <section class="bg-green-50 dark:bg-gray-900/40">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 py-14">
        <h2 class="text-2xl sm:text-3xl font-black text-center text-green-900 dark:text-green-100 mb-8">
          ليش جولة؟
        </h2>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
          <div
            v-for="value in values"
            :key="value.title"
            class="text-center space-y-2"
          >
            <div class="mx-auto size-12 rounded-full bg-green-900 text-gold-400 flex items-center justify-center">
              <UIcon
                :name="value.icon"
                class="size-6"
              />
            </div>
            <h3 class="font-bold text-green-900 dark:text-green-100">
              {{ value.title }}
            </h3>
            <p class="text-muted text-sm">
              {{ value.description }}
            </p>
          </div>
        </div>
      </div>
    </section>

    <section class="max-w-4xl mx-auto px-4 sm:px-6 py-14">
      <h2 class="text-2xl sm:text-3xl font-black text-center text-green-900 dark:text-green-100 mb-8">
        كيف تلعبون جولة؟
      </h2>
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
        <div
          v-for="(step, i) in steps"
          :key="step.title"
          class="text-center space-y-2"
        >
          <div class="mx-auto size-10 rounded-full bg-gold-400 text-green-950 font-black flex items-center justify-center">
            {{ i + 1 }}
          </div>
          <h3 class="font-bold text-green-900 dark:text-green-100">
            {{ step.title }}
          </h3>
          <p class="text-muted text-sm">
            {{ step.description }}
          </p>
        </div>
      </div>
      <div class="text-center mt-8">
        <UButton
          to="#"
          color="secondary"
          size="xl"
          class="font-bold text-green-950"
          @click.prevent="$router.push(modes[0]!.to)"
        >
          ابدأ اللعبة الآن
        </UButton>
      </div>
    </section>
  </div>
</template>
