<template>
  <PhasePlaceholder
    title="Reports"
    description="Report hub will be implemented in Phase 7 under admin."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', reports: 'Reports'
  }
})
</script>

