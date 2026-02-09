<template>
  <PhasePlaceholder
    title="Academic Years"
    description="Academic year CRUD will be implemented in Phase 3."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', 'master-data': 'Master Data', 'academic-years': 'Academic Years'
  }
})
</script>

