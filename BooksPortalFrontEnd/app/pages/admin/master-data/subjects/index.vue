<template>
  <PhasePlaceholder
    title="Subjects"
    description="Subject CRUD will be implemented in Phase 3."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'master-data': 'Master Data', 'subjects': 'Subjects',
  },
})
</script>
