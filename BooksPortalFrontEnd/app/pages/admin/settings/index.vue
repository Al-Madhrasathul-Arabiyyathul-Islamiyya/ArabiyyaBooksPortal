<template>
  <PhasePlaceholder
    title="Settings"
    description="Settings screens will be implemented in Phase 8 under admin."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', settings: 'Settings',
  },
})
</script>
