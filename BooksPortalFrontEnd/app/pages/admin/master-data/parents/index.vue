<template>
  <PhasePlaceholder
    title="Parents"
    description="Parent CRUD will be implemented in Phase 3."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', 'master-data': 'Master Data', parents: 'Parents'
  }
})
</script>

