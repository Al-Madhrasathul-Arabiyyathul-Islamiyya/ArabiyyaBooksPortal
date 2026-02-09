<template>
  <PhasePlaceholder
    title="Teachers"
    description="Teacher CRUD and assignment management will be implemented in Phase 3."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'master-data': 'Master Data', 'teachers': 'Teachers',
  },
})
</script>
