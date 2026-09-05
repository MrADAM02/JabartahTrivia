<script setup lang="ts">
useSeoMeta({ title: 'كيف تلعب - جولة' })

const modes = [
  {
    icon: '🎯',
    title: 'لعبة الأسئلة',
    to: '/trivia/setup',
    facts: ['🗂️ 6 فئات بالضبط', '⏱️ 20 ث لكل دور', '⚡ 3 قوى خاصة'],
    steps: [
      'اكتب اسمي الفريقين',
      'اختر بالضبط 6 فئات أسئلة',
      'يتناوب الفريقان على اختيار فئة ونقطة من الشبكة',
      'الفريق صاحب الدور يجيب خلال 20 ثانية، ثم يقرر المشرف إن كانت إجابته صحيحة',
      'لكل فريق 3 قوى خاصة تُستخدم مرة واحدة طوال اللعبة: 💰 مضاعفة النقاط، 🔁 محاولتين، ⏱️ خصم وقت الخصم'
    ]
  },
  {
    icon: '🤫',
    title: 'كلمة السر',
    to: '/password/setup',
    facts: ['🔁 3 · 5 · 7 جولات', '⏱️ 60 ث لكل جولة', '⚡ وقت إضافي +15 ث'],
    steps: [
      'اختر فئات الكلمات وعدد الجولات (3 أو 5 أو 7 لكل فريق)',
      'في كل جولة، يضغط المُلمّح على "عرض رمز QR" ويمسحه بجواله الخاص فقط',
      'يرى المُلمّح الكلمة السرية بمفرده، ويعطي فريقه تلميحاً واحداً خلال 60 ثانية',
      'يقرر مدير اللعبة هل خمّن الفريق الكلمة صح أو لا'
    ]
  },
  {
    icon: '🔢',
    title: 'رتبها',
    to: '/ranking/setup',
    facts: ['🔁 2 · 4 · 6 جولات', '💡 اكشف موضع عنصر'],
    steps: [
      'اختر الفئات وعدد الجولات (2 أو 4 أو 6 لكل فريق)',
      'في كل جولة تظهر بطاقات مبعثرة',
      'رتبوها بالترتيب الصحيح',
      'تُحسب النقاط حسب عدد البطاقات في مكانها الصحيح، مع مكافأة إضافية للترتيب الكامل'
    ]
  },
  {
    icon: '💯',
    title: 'تحدي الـ100',
    to: '/top100/setup',
    facts: ['🎯 3–10 إجابات لكل فريق', '📋 قائمة واحدة'],
    steps: [
      'اختر الفئة وعدد الإجابات المتاحة لكل فريق (من 3 إلى 10)',
      'يتناوب الفريقان على تخمين عناصر القائمة، عنصراً واحداً في كل مرة',
      'كل تخمين صحيح يمنحكم نقاطاً بعدد ترتيب ذلك العنصر في القائمة',
      'في نهاية اللعبة تُكشف القائمة كاملة'
    ]
  }
]
</script>

<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 py-12 space-y-8">
    <div class="text-center space-y-2">
      <h1 class="text-3xl sm:text-4xl font-black text-green-900 dark:text-green-100">
        كيف تلعبون جولة؟
      </h1>
      <p class="text-muted">
        أربع ألعاب مختلفة، كل واحدة بأسلوبها الخاص — اختر ما يناسب مجلسكم
      </p>
    </div>

    <UCard
      v-for="mode in modes"
      :key="mode.title"
      class="game-card-glow"
    >
      <div class="flex items-center gap-3 mb-3">
        <span class="text-4xl">{{ mode.icon }}</span>
        <h2 class="text-2xl font-black text-green-900 dark:text-green-100">
          {{ mode.title }}
        </h2>
      </div>

      <div class="flex flex-wrap gap-2 mb-5">
        <UBadge
          v-for="fact in mode.facts"
          :key="fact"
          color="neutral"
          variant="subtle"
          class="font-bold"
        >
          {{ fact }}
        </UBadge>
      </div>

      <ol class="space-y-3 mb-5">
        <li
          v-for="(step, i) in mode.steps"
          :key="step"
          class="flex items-start gap-3"
        >
          <span class="shrink-0 size-7 rounded-full bg-green-900 text-gold-400 font-black text-sm flex items-center justify-center">
            {{ i + 1 }}
          </span>
          <p class="text-sm text-muted pt-0.5">
            {{ step }}
          </p>
        </li>
      </ol>

      <UButton
        :to="mode.to"
        color="secondary"
        class="font-bold text-green-950"
      >
        العب {{ mode.title }}
      </UButton>
    </UCard>

    <UCard class="text-center bg-green-900 text-white">
      <p class="font-bold mb-1">
        🚪 الخروج أثناء اللعب
      </p>
      <p class="text-white/70 text-sm">
        في أي لحظة تقدر تضغط "إنهاء اللعبة" لإنهائها فوراً وحفظ النتيجة الحالية في سجلك، أو "الخروج" لمغادرة اللعبة بدون حفظ أي نتيجة.
      </p>
    </UCard>
  </div>
</template>
