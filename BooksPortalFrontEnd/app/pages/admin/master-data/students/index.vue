<template>
  <PhasePlaceholder
    title="Students"
    description="Student CRUD and parent linking will be implemented in Phase 3."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', 'master-data': 'Master Data', students: 'Students'
  }
})
</script>

