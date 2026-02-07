<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" :for="fieldId" class="text-sm font-medium text-surface-900 dark:text-surface-0">
      {{ label }}
      <span v-if="required" class="text-red-500">*</span>
    </label>
    <slot />
    <small v-if="error" class="text-red-500">{{ error }}</small>
    <small v-else-if="hint" class="text-surface-500 dark:text-surface-400">{{ hint }}</small>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label?: string
  error?: string
  hint?: string
  required?: boolean
  fieldId?: string
}>()
</script>
