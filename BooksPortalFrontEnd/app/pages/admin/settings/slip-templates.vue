<template>
  <PhasePlaceholder
    title="Slip Templates"
    description="Slip template settings UI will be implemented in Phase 8."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', settings: 'Settings', 'slip-templates': 'Slip Templates'
  }
})
</script>

