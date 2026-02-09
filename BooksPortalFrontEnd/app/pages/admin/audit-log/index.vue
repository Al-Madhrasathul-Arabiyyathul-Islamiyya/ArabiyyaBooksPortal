<template>
  <div>
    <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">Audit Log</h1>
    <p class="mt-2 text-surface-600 dark:text-surface-400">
      Audit log screen will be implemented in Phase 8 under the admin area.
    </p>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    'audit-log': 'Audit Log',
  },
})
</script>
