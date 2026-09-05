<script setup lang="ts">
useSeoMeta({ title: 'الأسئلة الشائعة - جولة' })

const groups = [
  {
    title: 'عام',
    items: [
      { q: 'ما هي جولة؟', a: 'جولة منصة ألعاب جماعية عربية مجانية بالكامل، مصممة لتُلعب على شاشة واحدة (جوال أو تابلت أو تلفزيون) بين فريقين، مناسبة للمجالس والسهرات العائلية.' },
      { q: 'هل جولة مجانية؟', a: 'نعم، جولة مجانية بالكامل ولا تحتاج اشتراكاً أو أي رسوم.' },
      { q: 'كم شخص يقدر يلعب جولة؟', a: 'اللعبة مصممة لفريقين يتنافسان، وكل فريق يقدر يضم أكثر من شخص يتشاورون مع بعض قبل الإجابة.' },
      { q: 'هل جولة تحتاج إنترنت؟', a: 'نعم، الشاشة المشتركة تتواصل مع الخادم طوال اللعبة، فتحتاج اتصالاً بالإنترنت.' }
    ]
  },
  {
    title: 'طريقة اللعب',
    items: [
      { q: 'كم لعبة متوفرة في جولة؟', a: 'أربع ألعاب: لعبة الأسئلة، كلمة السر، رتبها، وتحدي الـ100 — لكل لعبة أسلوبها الخاص.' },
      { q: 'كيف تعمل لعبة الأسئلة؟', a: 'تختار بالضبط 6 فئات، ثم يتناوب الفريقان على اختيار فئة ونقطة من الشبكة. الفريق صاحب الدور يجيب خلال 20 ثانية، ثم يقرر المشرف إن كانت إجابته صحيحة. لكل فريق ثلاث قوى خاصة تُستخدم مرة واحدة طوال اللعبة: 💰 مضاعفة النقاط، 🔁 محاولتين، و⏱️ خصم وقت الخصم.' },
      { q: 'كيف تعمل كلمة السر؟', a: 'يتناوب الفريقان، ويمسح المُلمّح رمز QR بجواله الخاص ليرى الكلمة السرية بمفرده، ثم يعطي فريقه تلميحاً واحداً فقط خلال 60 ثانية. يملك كل فريق قوة "⏱️ وقت إضافي" تضيف 15 ثانية، تُستخدم مرة واحدة طوال اللعبة.' },
      { q: 'كيف تعمل رتبها؟', a: 'تظهر بطاقات مبعثرة، ومهمتكم ترتيبها بالترتيب الصحيح — تُحسب النقاط حسب عدد البطاقات في مكانها الصحيح، مع مكافأة إضافية عند الترتيب الكامل. يملك كل فريق قوة "💡 اكشف موضع عنصر" تُستخدم مرة واحدة طوال اللعبة.' },
      { q: 'كيف يعمل تحدي الـ100؟', a: 'يتناوب الفريقان على تخمين عناصر قائمة مرتبة، عنصراً واحداً في كل مرة. كل تخمين صحيح يمنحكم نقاطاً بعدد ترتيب ذلك العنصر، فكلما كان العنصر أصعب استحق نقاطاً أكثر.' },
      { q: 'هل ممكن يصير تعادل بين الفريقين؟', a: 'نعم، إذا تساوى الفريقان في النقاط تظهر شاشة "تعادل" توضح ذلك، بدل اختيار فائز عشوائي.' }
    ]
  },
  {
    title: 'الحساب',
    items: [
      { q: 'هل أحتاج حساباً علشان ألعب؟', a: 'لا، تقدر تلعب أي لعبة من الأربع مباشرة بدون تسجيل دخول.' },
      { q: 'إيش يضيفه الحساب؟', a: 'تسجيل الدخول يفتح لك صفحة "حسابي"، وسجل "ألعابي السابقة"، وإمكانية إنشاء تصنيفات أسئلة خاصة بك في "تصنيفاتي" تظهر لك أنت فقط.' },
      { q: 'كيف أحذف حسابي؟', a: 'من صفحة "حسابي" تقدر تحذف حسابك نهائياً، ويشمل الحذف تصنيفاتك الخاصة.' }
    ]
  }
]

const openItem = ref<string | null>(null)
function toggle(key: string) {
  openItem.value = openItem.value === key ? null : key
}
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 py-12 space-y-10">
    <div class="text-center space-y-2">
      <h1 class="text-3xl sm:text-4xl font-black text-green-900 dark:text-green-100">
        الأسئلة الشائعة
      </h1>
      <p class="text-muted">
        إجابات على أكثر الأسئلة شيوعاً حول جولة
      </p>
    </div>

    <section
      v-for="group in groups"
      :key="group.title"
      class="space-y-3"
    >
      <UBadge
        color="secondary"
        variant="solid"
        class="text-green-950 font-bold"
      >
        {{ group.title }}
      </UBadge>

      <div class="rounded-xl ring-1 ring-green-100 dark:ring-gray-800 divide-y divide-green-100 dark:divide-gray-800 overflow-hidden">
        <div
          v-for="item in group.items"
          :key="item.q"
        >
          <button
            class="w-full flex items-center justify-between gap-3 px-4 py-3 text-start font-bold text-green-900 dark:text-green-100 hover:bg-green-50 dark:hover:bg-gray-900 transition-colors"
            @click="toggle(item.q)"
          >
            <span>{{ item.q }}</span>
            <UIcon
              :name="openItem === item.q ? 'i-lucide-chevron-up' : 'i-lucide-chevron-down'"
              class="size-5 shrink-0 text-muted"
            />
          </button>
          <p
            v-if="openItem === item.q"
            class="px-4 pb-4 text-muted"
          >
            {{ item.a }}
          </p>
        </div>
      </div>
    </section>

    <UCard class="text-center bg-green-900 text-white">
      <p class="font-bold mb-1">
        لم تجد إجابة سؤالك؟
      </p>
      <p class="text-white/70 text-sm">
        راجع صفحة "كيف تلعب" لتفاصيل أكثر عن كل لعبة.
      </p>
    </UCard>
  </div>
</template>
