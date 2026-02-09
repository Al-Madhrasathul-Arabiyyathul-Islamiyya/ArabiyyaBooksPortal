<template>
  <div class="flex min-h-screen bg-surface-50 dark:bg-surface-950">
    <aside
      class="fixed left-0 top-0 z-40 flex h-screen flex-col border-r border-surface-200 bg-surface-0 transition-all duration-300 dark:border-surface-700 dark:bg-surface-900"
      :class="sidebarCollapsed ? 'w-16' : 'w-64'"
    >
      <div class="flex h-16 items-center gap-3 border-b border-surface-200 px-4 dark:border-surface-700">
        <NuxtImg
          src="/logo.png"
          alt="Logo"
          class="h-8 w-8 shrink-0"
        />
        <span
          v-if="!sidebarCollapsed"
          class="text-lg font-semibold text-surface-900 dark:text-surface-0"
        >
          Admin
        </span>
      </div>

      <nav class="flex-1 overflow-y-auto p-2">
        <PanelMenu
          :model="menuItems"
          class="w-full border-none"
          :class="{ 'sidebar-panel-collapsed': sidebarCollapsed }"
        />
      </nav>

      <div class="border-t border-surface-200 p-2 dark:border-surface-700">
        <Button
          icon="pi pi-arrow-left"
          :label="sidebarCollapsed ? undefined : 'Back to Operations'"
          text
          severity="secondary"
          class="w-full justify-start"
          @click="navigateTo('/')"
        />
      </div>

      <div class="border-t border-surface-200 p-2 dark:border-surface-700">
        <Button
          :icon="sidebarCollapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"
          text
          severity="secondary"
          class="w-full"
          @click="appStore.toggleSidebar()"
        />
      </div>
    </aside>

    <div
      class="flex min-h-screen flex-1 flex-col transition-all duration-300"
      :class="sidebarCollapsed ? 'ml-16' : 'ml-64'"
    >
      <header class="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-surface-200 bg-surface-0 px-6 dark:border-surface-700 dark:bg-surface-900">
        <CommonAppBreadcrumb />
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
const appStore = useAppStore()

const sidebarCollapsed = computed(() => appStore.sidebarCollapsed)
const userMenuRef = ref()

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

const menuItems = computed(() => ([
  {
    label: 'Admin Dashboard',
    icon: 'pi pi-home',
    command: () => navigateTo('/admin'),
  },
  {
    label: 'Master Data',
    icon: 'pi pi-database',
    items: [
      { label: 'Academic Years', command: () => navigateTo('/admin/master-data/academic-years') },
      { label: 'Keystages', command: () => navigateTo('/admin/master-data/keystages') },
      { label: 'Subjects', command: () => navigateTo('/admin/master-data/subjects') },
      { label: 'Class Sections', command: () => navigateTo('/admin/master-data/class-sections') },
      { label: 'Students', command: () => navigateTo('/admin/master-data/students') },
      { label: 'Parents', command: () => navigateTo('/admin/master-data/parents') },
      { label: 'Teachers', command: () => navigateTo('/admin/master-data/teachers') },
    ],
  },
  {
    label: 'Books',
    icon: 'pi pi-book',
    command: () => navigateTo('/admin/books'),
  },
  {
    label: 'Reports',
    icon: 'pi pi-chart-bar',
    items: [
      { label: 'Stock Summary', command: () => navigateTo('/admin/reports/stock-summary') },
      { label: 'Distributions', command: () => navigateTo('/admin/reports/distributions') },
      { label: 'Teacher Outstanding', command: () => navigateTo('/admin/reports/teacher-outstanding') },
      { label: 'Student History', command: () => navigateTo('/admin/reports/student-history') },
    ],
  },
  {
    label: 'Settings',
    icon: 'pi pi-cog',
    items: [
      { label: 'Users', command: () => navigateTo('/admin/settings/users') },
      { label: 'Reference Formats', command: () => navigateTo('/admin/settings/reference-formats') },
      { label: 'Slip Templates', command: () => navigateTo('/admin/settings/slip-templates') },
      { label: 'Profile', command: () => navigateTo('/admin/settings/profile') },
    ],
  },
  {
    label: 'Audit Log',
    icon: 'pi pi-history',
    command: () => navigateTo('/admin/audit-log'),
  },
]))
</script>

<style scoped>
.sidebar-panel-collapsed :deep(.p-panelmenu-content),
.sidebar-panel-collapsed :deep(.p-panelmenu-panel),
.sidebar-panel-collapsed :deep(.p-panelmenu-header-content),
.sidebar-panel-collapsed :deep(.p-menuitem-link) {
  border: none;
}

.sidebar-panel-collapsed :deep(.p-menuitem-link) {
  justify-content: center;
  gap: 0;
  padding-inline: 0.5rem;
}

.sidebar-panel-collapsed :deep(.p-menuitem-text),
.sidebar-panel-collapsed :deep(.p-submenu-icon) {
  display: none;
}
</style>
