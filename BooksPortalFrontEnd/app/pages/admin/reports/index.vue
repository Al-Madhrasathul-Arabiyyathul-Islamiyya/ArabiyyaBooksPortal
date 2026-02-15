<template>
  <div class="flex flex-col gap-4">
    <div>
      <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
        Reports
      </h1>
      <p class="text-sm text-surface-600 dark:text-surface-400">
        Review inventory, distributions, teacher outstanding items, and student history.
      </p>
    </div>

    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <Card
        v-for="item in reportCards"
        :key="item.to"
        class="cursor-pointer transition-shadow hover:shadow-md"
        @click="navigateTo(item.to)"
      >
        <template #content>
          <div class="flex items-start justify-between gap-3">
            <div>
              <h2 class="text-lg font-semibold text-surface-900 dark:text-surface-0">
                {{ item.title }}
              </h2>
              <p class="mt-1 text-sm text-surface-600 dark:text-surface-400">
                {{ item.description }}
              </p>
            </div>
            <i
              :class="item.icon"
              class="text-xl text-primary"
            />
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', reports: 'Reports',
  },
})

const reportCards = [
  {
    title: 'Stock Summary',
    description: 'Current stock state across books and subjects.',
    to: '/admin/reports/stock-summary',
    icon: 'pi pi-book',
  },
  {
    title: 'Distributions',
    description: 'Issued distributions by year and date range.',
    to: '/admin/reports/distributions',
    icon: 'pi pi-send',
  },
  {
    title: 'Teacher Outstanding',
    description: 'Outstanding teacher-issued inventory.',
    to: '/admin/reports/teacher-outstanding',
    icon: 'pi pi-user-plus',
  },
  {
    title: 'Student History',
    description: 'Student-level distribution and return history.',
    to: '/admin/reports/student-history',
    icon: 'pi pi-history',
  },
]
</script>
