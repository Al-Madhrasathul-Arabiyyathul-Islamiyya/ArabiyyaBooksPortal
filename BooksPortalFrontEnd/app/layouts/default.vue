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
          src="/logo.svg"
          alt="Logo"
          class="h-8 w-8 shrink-0"
        />
      </div>

      <!-- Navigation -->
      <nav class="flex-1 overflow-y-auto p-2">
        <div
          v-if="sidebarCollapsed"
          class="grid gap-1"
        >
          <Button
            v-for="item in collapsedNavItems"
            :key="item.label"
            :icon="item.icon"
            text
            severity="secondary"
            class="w-full justify-center"
            :class="{ 'app-link-active': item.active }"
            @click="item.command"
          />
        </div>

        <div
          v-else
          class="grid gap-3"
        >
          <div class="rounded-lg border border-surface-200 p-2 dark:border-surface-700">
            <div class="px-2 py-1 text-xs font-semibold uppercase tracking-wide text-surface-500">
              Students
            </div>
            <div class="grid gap-1">
              <Button
                label="Distribution"
                icon="pi pi-send"
                text
                severity="secondary"
                class="w-full justify-start"
                :class="{ 'app-link-active': isRouteActive('/distribution') }"
                @click="navigateTo('/distribution')"
              />
              <Button
                label="Returns"
                icon="pi pi-replay"
                text
                severity="secondary"
                class="w-full justify-start"
                :class="{ 'app-link-active': isRouteActive('/returns') }"
                @click="navigateTo('/returns')"
              />
            </div>
          </div>

          <div class="rounded-lg border border-surface-200 p-2 dark:border-surface-700">
            <div class="px-2 py-1 text-xs font-semibold uppercase tracking-wide text-surface-500">
              Teachers
            </div>
            <div class="grid gap-1">
              <Button
                label="Issue Books"
                icon="pi pi-users"
                text
                severity="secondary"
                class="w-full justify-start"
                :class="{ 'app-link-active': isRouteActive('/teacher-issues') }"
                @click="navigateTo('/teacher-issues')"
              />
              <Button
                label="Returns"
                icon="pi pi-replay"
                text
                severity="secondary"
                class="w-full justify-start"
                :class="{ 'app-link-active': isRouteActive('/teacher-returns') }"
                @click="navigateTo('/teacher-returns')"
              />
            </div>
          </div>
        </div>
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
          :class="[
            'w-full',
            sidebarCollapsed ? 'justify-center' : 'justify-start',
          ]"
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
        <h1 class="text-base font-semibold text-surface-900 dark:text-surface-0">
          {{ appTitle }}
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
const route = useRoute()
const { public: { appTitle } } = useRuntimeConfig()

const sidebarCollapsed = computed(() => appStore.sidebarCollapsed)
const userMenuRef = ref()

function toggleColorMode() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}

function toggleUserMenu(event: Event) {
  userMenuRef.value.toggle(event)
}

function isRouteActive(path: string) {
  return route.path === path || route.path.startsWith(`${path}/`)
}

const userMenuItems = ref([
  {
    label: 'Profile',
    icon: 'pi pi-user',
    command: () => navigateTo(isAdmin.value ? '/admin/settings/profile' : '/distribution'),
  },
  { separator: true },
  {
    label: 'Logout',
    icon: 'pi pi-sign-out',
    command: () => logout(),
  },
])

const collapsedNavItems = computed(() => ([
  {
    label: 'Distribution',
    icon: 'pi pi-send',
    command: () => navigateTo('/distribution'),
    active: isRouteActive('/distribution'),
  },
  {
    label: 'Returns',
    icon: 'pi pi-replay',
    command: () => navigateTo('/returns'),
    active: isRouteActive('/returns'),
  },
  {
    label: 'Teacher Issues',
    icon: 'pi pi-users',
    command: () => navigateTo('/teacher-issues'),
    active: isRouteActive('/teacher-issues'),
  },
  {
    label: 'Teacher Returns',
    icon: 'pi pi-replay',
    command: () => navigateTo('/teacher-returns'),
    active: isRouteActive('/teacher-returns'),
  },
]))

// Initialize auth on layout mount
const authStore = useAuthStore()
onMounted(() => {
  authStore.initialize()
})
</script>

<style scoped>
.app-link-active {
  background: color-mix(in srgb, var(--p-primary-color) 18%, transparent);
  border-radius: 0.5rem;
}

.app-link-active :deep(.p-button-label),
.app-link-active :deep(.p-button-icon) {
  color: var(--p-primary-color);
  font-weight: 600;
}
</style>
