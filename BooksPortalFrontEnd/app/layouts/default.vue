<template>
  <div class="flex min-h-screen bg-surface-50 dark:bg-surface-950">
    <!-- Sidebar -->
    <aside
      class="fixed left-0 top-0 z-40 flex h-screen flex-col border-r border-surface-200 bg-surface-0 transition-all duration-300 dark:border-surface-700 dark:bg-surface-900"
      :class="sidebarCollapsed ? 'w-16' : 'w-64'"
    >
      <!-- Logo -->
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
          Books Portal
        </span>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 overflow-y-auto p-2">
        <PanelMenu
          :model="menuItems"
          class="w-full border-none"
        />
      </nav>

      <div
        v-if="isAdmin"
        class="border-t border-surface-200 p-2 dark:border-surface-700"
      >
        <Button
          icon="pi pi-shield"
          :label="sidebarCollapsed ? undefined : 'Admin'"
          text
          severity="secondary"
          class="w-full justify-start"
          @click="navigateTo('/admin')"
        />
      </div>

      <!-- Collapse toggle -->
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

    <!-- Main area -->
    <div
      class="flex min-h-screen flex-1 flex-col transition-all duration-300"
      :class="sidebarCollapsed ? 'ml-16' : 'ml-64'"
    >
      <!-- Header -->
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

      <!-- Page content -->
      <main class="flex-1 p-6">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()
const { user, isAdmin, logout } = useAuth()
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
    command: () => navigateTo(isAdmin.value ? '/admin/settings/profile' : '/'),
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
    label: 'Dashboard',
    icon: 'pi pi-home',
    command: () => navigateTo('/'),
  },
  {
    label: 'Distribution',
    icon: 'pi pi-send',
    command: () => navigateTo('/distribution'),
  },
  {
    label: 'Returns',
    icon: 'pi pi-replay',
    command: () => navigateTo('/returns'),
  },
  {
    label: 'Teacher Issues',
    icon: 'pi pi-users',
    command: () => navigateTo('/teacher-issues'),
  },
]))

// Initialize auth on layout mount
const authStore = useAuthStore()
onMounted(() => {
  authStore.initialize()
})
</script>
