<template>
  <PhasePlaceholder
    title="Distribution Report"
    description="Distribution report UI will be implemented in Phase 7."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', reports: 'Reports', distributions: 'Distributions',
  },
})
</script>
