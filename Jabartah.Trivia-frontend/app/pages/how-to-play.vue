<script setup lang="ts">
useSeoMeta({ title: 'كيف تلعب - جولة' })

const modes = [
  {
    icon: '🎯',
    title: 'لعبة الأسئلة',
    to: '/trivia/setup',
    steps: [
      'اكتب اسمي الفريقين',
      'اختر بالضبط 6 فئات أسئلة',
      'يتناوب الفريقان على اختيار فئة ونقطة من الشبكة',
      'اقرأ السؤال على الشاشة — من يجيب صح أولاً يفوز بالنقاط',
      'استخدم "مضاعفة النقاط" أو "محاولتين" مرة واحدة لكل فريق طوال اللعبة'
    ]
  },
  {
    icon: '🤫',
    title: 'كلمة السر',
    to: '/password/setup',
    steps: [
      'اختر فئات الكلمات وعدد الجولات (5 أو 7 أو 10 لكل فريق)',
      'في كل جولة، يضغط المُلمّح على "عرض رمز QR" ويمسحه بجواله الخاص فقط',
      'يرى المُلمّح الكلمة السرية بمفرده، ويعطي فريقه تلميحاً واحداً فقط',
      'يقرر مدير اللعبة هل خمّن الفريق الكلمة صح أو لا'
    ]
  },
  {
    icon: '🔢',
    title: 'رتبها',
    to: '/ranking/setup',
    steps: [
      'اختر الفئات وعدد الجولات (2 أو 4 أو 6 لكل فريق)',
      'في كل جولة تظهر بطاقات مبعثرة',
      'رتبوها بالترتيب الصحيح قبل انتهاء الجولة',
      'تُحسب النقاط حسب عدد البطاقات في مكانها الصحيح، مع مكافأة إضافية للترتيب الكامل'
    ]
  },
  {
    icon: '💯',
    title: 'تحدي الـ100',
    to: '/top100/setup',
    steps: [
      'اختر الفئات وعدد الجولات (1 أو 2 أو 3 لكل فريق)',
      'يتناوب الفريقان على تخمين عناصر القائمة، عنصراً واحداً في كل مرة',
      'كل تخمين صحيح يمنحكم نقاطاً بعدد ترتيب ذلك العنصر في القائمة',
      'في نهاية الجولة تُكشف القائمة كاملة'
    ]
  }
]
</script>

<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 py-12 space-y-12">
    <div class="text-center space-y-2">
      <h1 class="text-3xl sm:text-4xl font-black text-green-900 dark:text-green-100">
        كيف تلعبون جولة؟
      </h1>
      <p class="text-muted">
        أربع ألعاب مختلفة، كل واحدة بأسلوبها الخاص — اختر ما يناسب مجلسكم
      </p>
    </div>

    <section
      v-for="mode in modes"
      :key="mode.title"
      class="space-y-4"
    >
      <div class="flex items-center gap-3">
        <span class="text-4xl">{{ mode.icon }}</span>
        <h2 class="text-2xl font-black text-green-900 dark:text-green-100">
          {{ mode.title }}
        </h2>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div
          v-for="(step, i) in mode.steps"
          :key="step"
          class="flex items-start gap-3 rounded-xl ring-1 ring-green-100 dark:ring-gray-800 p-4"
        >
          <span class="shrink-0 size-7 rounded-full bg-green-900 text-gold-400 font-black text-sm flex items-center justify-center">
            {{ i + 1 }}
          </span>
          <p class="text-sm text-muted">
            {{ step }}
          </p>
        </div>
      </div>

      <UButton
        :to="mode.to"
        color="secondary"
        class="font-bold text-green-950"
      >
        العب {{ mode.title }}
      </UButton>
    </section>
  </div>
</template>
