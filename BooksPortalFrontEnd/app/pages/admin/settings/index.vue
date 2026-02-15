<template>
  <div class="flex flex-col gap-4">
    <div>
      <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
        Settings
      </h1>
      <p class="text-sm text-surface-600 dark:text-surface-400">
        Manage users, reference formats, templates, and your profile.
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

const settingCards = [
  {
    title: 'Users',
    description: 'Create users, assign roles, and manage account status.',
    to: '/admin/settings/users',
    icon: 'pi pi-users',
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
  {
    title: 'Profile',
    description: 'Review your profile and change your password.',
    to: '/admin/settings/profile',
    icon: 'pi pi-user',
  },
]
</script>
