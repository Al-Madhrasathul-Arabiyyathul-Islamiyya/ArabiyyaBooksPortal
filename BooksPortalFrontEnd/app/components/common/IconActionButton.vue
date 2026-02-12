<template>
  <Button
    :icon="icon"
    text
    rounded
    :severity="severity"
    :aria-label="tooltip"
    :title="tooltip"
    :disabled="disabled"
    @click="(event) => $emit('click', event)"
  />
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  icon: string
  tooltip: string
  severity?: 'secondary' | 'danger' | 'success' | 'info' | 'warn' | 'contrast'
  disabled?: boolean
}>(), {
  severity: 'secondary',
  disabled: false,
})

defineEmits<{
  click: [event: MouseEvent]
}>()
</script>
