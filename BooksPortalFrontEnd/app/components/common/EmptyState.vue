<template>
  <div class="flex flex-col items-center justify-center py-12 text-center">
    <Icon
      v-if="icon"
      :name="icon"
      class="mb-4 text-6xl text-surface-400 dark:text-surface-600"
    />
    <h3 class="mb-2 text-xl font-semibold text-surface-900 dark:text-surface-0">
      {{ title }}
    </h3>
    <p
      v-if="subtitle"
      class="mb-6 text-surface-600 dark:text-surface-400"
    >
      {{ subtitle }}
    </p>
    <Button
      v-if="actionLabel"
      :label="actionLabel"
      :icon="actionIcon"
      @click="$emit('action')"
    />
  </div>
</template>

<script setup lang="ts">
defineProps<{
  icon?: string
  title: string
  subtitle?: string
  actionLabel?: string
  actionIcon?: string
}>()

defineEmits<{
  action: []
}>()
</script>
