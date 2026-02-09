<template>
  <PhasePlaceholder
    title="Stock Summary Report"
    description="Stock summary report UI will be implemented in Phase 7."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', reports: 'Reports', 'stock-summary': 'Stock Summary'
  }
})
</script>

