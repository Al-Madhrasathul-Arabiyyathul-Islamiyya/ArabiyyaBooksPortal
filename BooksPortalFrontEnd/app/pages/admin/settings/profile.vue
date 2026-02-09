<template>
  <PhasePlaceholder
    title="Profile"
    description="Admin profile and password management will be implemented in Phase 8."
  />
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', settings: 'Settings', profile: 'Profile'
  }
})
</script>

