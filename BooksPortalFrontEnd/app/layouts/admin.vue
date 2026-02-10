<template>
  <div class="flex min-h-screen bg-surface-50 dark:bg-surface-950">
    <aside
      class="fixed left-0 top-0 z-40 flex h-screen w-64 flex-col border-r border-surface-200 bg-surface-0 transition-all duration-300 dark:border-surface-700 dark:bg-surface-900"
    >
      <div class="flex h-16 items-center gap-3 border-b border-surface-200 px-4 dark:border-surface-700">
        <NuxtImg
          src="/logo.png"
          alt="Logo"
          class="h-8 w-8 shrink-0"
        />
        <span class="text-lg font-semibold text-surface-900 dark:text-surface-0">
          Admin
        </span>
      </div>

      <nav class="flex-1 overflow-y-auto p-2">
        <PanelMenu
          v-model:expandedKeys="expandedKeys"
          :model="menuItems"
          class="w-full border-none"
        />
      </nav>

      <div class="border-t border-surface-200 p-2 dark:border-surface-700">
        <Button
          icon="pi pi-arrow-left"
          label="Back to Operations"
          text
          severity="secondary"
          class="w-full justify-start"
          @click="navigateTo('/')"
        />
      </div>
    </aside>

    <div
      class="ml-64 flex min-h-screen flex-1 flex-col transition-all duration-300"
    >
      <header class="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-surface-200 bg-surface-0 px-6 dark:border-surface-700 dark:bg-surface-900">
        <h1 class="text-base font-semibold text-surface-900 dark:text-surface-0">
          Arabiyya Books Portal
        </h1>
        <div class="flex items-center gap-3">
          <Button
            :icon="colorMode.value === 'dark' ? 'pi pi-sun' : 'pi pi-moon'"
            text
            severity="secondary"
            rounded
            @click="toggleColorMode"
          />
          <Button
            :label="user?.fullName"
            icon="pi pi-user"
            text
            severity="secondary"
            @click="toggleUserMenu"
          />
          <Menu
            ref="userMenuRef"
            :model="userMenuItems"
            :popup="true"
          />
        </div>
      </header>

      <main class="flex-1 p-6">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()
const { user, logout } = useAuth()
const route = useRoute()
const userMenuRef = ref()
const expandedKeys = ref<Record<string, boolean>>({})

function toggleColorMode() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

function toggleUserMenu(event: Event) {
  userMenuRef.value.toggle(event)
}

const userMenuItems = ref([
  {
    label: 'Profile',
    icon: 'pi pi-user',
    command: () => navigateTo('/admin/settings/profile'),
  },
  { separator: true },
  {
    label: 'Logout',
    icon: 'pi pi-sign-out',
    command: () => logout(),
  },
])

function isActive(path: string) {
  return route.path === path || route.path.startsWith(`${path}/`)
}

const menuItems = computed(() => ([
  {
    key: 'admin-dashboard',
    label: 'Admin Dashboard',
    icon: 'pi pi-home',
    command: () => navigateTo('/admin'),
    class: route.path === '/admin' ? 'app-menu-item-active' : undefined,
  },
  {
    key: 'master-data',
    label: 'Master Data',
    icon: 'pi pi-database',
    items: [
      {
        key: 'master-data-academic-years',
        label: 'AcademicYear',
        command: () => navigateTo('/admin/master-data/academic-years'),
        class: isActive('/admin/master-data/academic-years') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-subjects',
        label: 'Subjects',
        command: () => navigateTo('/admin/master-data/subjects'),
        class: isActive('/admin/master-data/subjects') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-keystages',
        label: 'Keystages',
        command: () => navigateTo('/admin/master-data/keystages'),
        class: isActive('/admin/master-data/keystages') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-class-sections',
        label: 'Classes',
        command: () => navigateTo('/admin/master-data/class-sections'),
        class: isActive('/admin/master-data/class-sections') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-students',
        label: 'Students',
        command: () => navigateTo('/admin/master-data/students'),
        class: isActive('/admin/master-data/students') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-parents',
        label: 'Parents',
        command: () => navigateTo('/admin/master-data/parents'),
        class: isActive('/admin/master-data/parents') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'master-data-teachers',
        label: 'Teachers',
        command: () => navigateTo('/admin/master-data/teachers'),
        class: isActive('/admin/master-data/teachers') ? 'app-menu-item-active' : undefined,
      },
    ],
  },
  {
    key: 'books',
    label: 'Books',
    icon: 'pi pi-book',
    command: () => navigateTo('/admin/books'),
    class: isActive('/admin/books') ? 'app-menu-item-active' : undefined,
  },
  {
    key: 'reports',
    label: 'Reports',
    icon: 'pi pi-chart-bar',
    items: [
      {
        key: 'reports-stock-summary',
        label: 'Stock Summary',
        command: () => navigateTo('/admin/reports/stock-summary'),
        class: isActive('/admin/reports/stock-summary') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'reports-distributions',
        label: 'Distributions',
        command: () => navigateTo('/admin/reports/distributions'),
        class: isActive('/admin/reports/distributions') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'reports-teacher-outstanding',
        label: 'Teacher Outstanding',
        command: () => navigateTo('/admin/reports/teacher-outstanding'),
        class: isActive('/admin/reports/teacher-outstanding') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'reports-student-history',
        label: 'Student History',
        command: () => navigateTo('/admin/reports/student-history'),
        class: isActive('/admin/reports/student-history') ? 'app-menu-item-active' : undefined,
      },
    ],
  },
  {
    key: 'settings',
    label: 'Settings',
    icon: 'pi pi-cog',
    items: [
      {
        key: 'settings-users',
        label: 'Users',
        command: () => navigateTo('/admin/settings/users'),
        class: isActive('/admin/settings/users') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'settings-reference-formats',
        label: 'Reference Formats',
        command: () => navigateTo('/admin/settings/reference-formats'),
        class: isActive('/admin/settings/reference-formats') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'settings-slip-templates',
        label: 'Slip Templates',
        command: () => navigateTo('/admin/settings/slip-templates'),
        class: isActive('/admin/settings/slip-templates') ? 'app-menu-item-active' : undefined,
      },
      {
        key: 'settings-profile',
        label: 'Profile',
        command: () => navigateTo('/admin/settings/profile'),
        class: isActive('/admin/settings/profile') ? 'app-menu-item-active' : undefined,
      },
    ],
  },
  {
    key: 'audit-log',
    label: 'Audit Log',
    icon: 'pi pi-history',
    command: () => navigateTo('/admin/audit-log'),
    class: isActive('/admin/audit-log') ? 'app-menu-item-active' : undefined,
  },
]))

function syncExpandedKeys(path: string) {
  expandedKeys.value = {
    'master-data': path.startsWith('/admin/master-data'),
    reports: path.startsWith('/admin/reports'),
    settings: path.startsWith('/admin/settings'),
  }
}

watch(() => route.path, syncExpandedKeys, { immediate: true })
</script>

<style scoped>
:deep(.app-menu-item-active > .p-panelmenu-header > .p-panelmenu-header-content),
:deep(.app-menu-item-active > .p-menuitem-content) {
  background: color-mix(in srgb, var(--p-primary-color) 18%, transparent);
  border-radius: 0.5rem;
}

:deep(.app-menu-item-active .p-panelmenu-header-label),
:deep(.app-menu-item-active .p-menuitem-text) {
  color: var(--p-primary-color);
  font-weight: 600;
}
</style>
