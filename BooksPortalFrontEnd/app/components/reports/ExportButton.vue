<template>
  <Button
    :label="label"
    :icon="icon"
    severity="secondary"
    outlined
    :loading="loading"
    :disabled="disabled"
    @click="emit('export')"
  />
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  label?: string
  icon?: string
  loading?: boolean
  disabled?: boolean
}>(), {
  label: 'Export',
  icon: 'pi pi-download',
  loading: false,
  disabled: false,
})

const emit = defineEmits<{
  export: []
}>()
</script>
