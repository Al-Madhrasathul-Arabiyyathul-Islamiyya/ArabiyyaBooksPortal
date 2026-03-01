<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div>
      <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
        Settings
      </h1>
      <p class="text-sm text-surface-600 dark:text-surface-400">
        Manage users, reference formats, and template settings.
      </p>
    </div>

    <div class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      <Card
        v-for="item in settingCards"
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
    admin: 'Admin', settings: 'Settings',
  },
})

const { isSuperAdmin } = useAuth()

const settingCards = computed(() => [
  {
    title: 'Users',
    description: 'Create users, assign roles, and manage account status.',
    to: '/admin/settings/users',
    icon: 'pi pi-users',
  },
  {
    title: 'Setup Center',
    description: 'Track setup readiness and complete required bootstrap steps.',
    to: '/admin/settings/setup',
    icon: 'pi pi-cog',
  },
  {
    title: 'Reference Formats',
    description: 'Configure reference number templates per slip type.',
    to: '/admin/settings/reference-formats',
    icon: 'pi pi-hashtag',
  },
  {
    title: 'Slip Templates',
    description: 'Manage print template labels and defaults.',
    to: '/admin/settings/slip-templates',
    icon: 'pi pi-file-edit',
  },
  ...(isSuperAdmin.value
    ? [{
        title: 'Master Data Bulk Upload',
        description: 'Upload academic years, keystages, grades, and classes via JSON template.',
        to: '/admin/settings/master-data-hierarchy-bulk',
        icon: 'pi pi-upload',
      }]
    : []),
].filter(item => isSuperAdmin.value || item.to !== '/admin/settings/users'))
</script>
