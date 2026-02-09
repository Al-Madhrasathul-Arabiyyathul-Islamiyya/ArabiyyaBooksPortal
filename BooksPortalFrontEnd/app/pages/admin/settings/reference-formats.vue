<template>
  <PhasePlaceholder
    title="Reference Formats"
    description="Reference number format management will be implemented in Phase 8."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'settings': 'Settings', 'reference-formats': 'Reference Formats',
  },
})
</script>
