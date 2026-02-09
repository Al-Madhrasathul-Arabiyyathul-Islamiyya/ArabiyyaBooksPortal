<template>
  <div class="rounded-xl border border-surface-200 bg-surface-0 p-6 dark:border-surface-700 dark:bg-surface-900">
    <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
      {{ title }}
    </h1>
    <p class="mt-2 text-surface-600 dark:text-surface-400">
      {{ description }}
    </p>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  description: string
}>()
</script>
