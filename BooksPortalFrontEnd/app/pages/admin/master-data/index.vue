<template>
  <PhasePlaceholder
    title="Master Data"
    description="Master data management screens will be implemented in Phase 3 under admin."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', 'master-data': 'Master Data'
  }
})
</script>

