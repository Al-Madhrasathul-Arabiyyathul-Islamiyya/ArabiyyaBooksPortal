<template>
  <PhasePlaceholder
    title="Teacher Outstanding Report"
    description="Teacher outstanding report UI will be implemented in Phase 7."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'reports': 'Reports', 'teacher-outstanding': 'Teacher Outstanding',
  },
})
</script>
