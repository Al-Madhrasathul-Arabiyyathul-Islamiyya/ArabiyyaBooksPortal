<template>
  <PhasePlaceholder
    title="Users"
    description="User management UI will be implemented in Phase 8."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', settings: 'Settings', users: 'Users',
  },
})
</script>
