<template>
  <div>
    <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">Admin Dashboard</h1>
    <p class="mt-2 text-surface-600 dark:text-surface-400">
      Administrative area for master data, reports, settings, and audit.
    </p>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
  },
})
</script>
