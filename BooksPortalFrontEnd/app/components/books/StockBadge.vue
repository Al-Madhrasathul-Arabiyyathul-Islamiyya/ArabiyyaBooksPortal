<template>
  <Tag
    :value="label"
    :severity="severity"
    rounded
  />
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  available: number
  showLabel?: boolean
}>(), {
  showLabel: true,
})

const severity = computed<'success' | 'warn' | 'danger'>(() => {
  if (props.available <= 0) return 'danger'
  if (props.available < 10) return 'warn'
  return 'success'
})

const label = computed(() => {
  if (!props.showLabel) {
    return String(props.available)
  }
  return `Available: ${props.available}`
})
</script>
