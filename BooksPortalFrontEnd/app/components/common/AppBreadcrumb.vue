<template>
  <Breadcrumb :model="items" class="border-none bg-transparent px-0">
    <template #item="{ item }">
      <NuxtLink v-if="item.to" :to="item.to" class="text-surface-700 hover:text-primary dark:text-surface-300">
        {{ item.label }}
      </NuxtLink>
      <span v-else class="text-surface-500 dark:text-surface-400">{{ item.label }}</span>
    </template>
  </Breadcrumb>
</template>

<script setup lang="ts">
const route = useRoute()

const items = computed(() => {
  const pathSegments = route.path.split('/').filter(Boolean)
  const breadcrumbs = [{ label: 'Home', to: '/' }]
  const breadcrumbMeta = (route.meta.breadcrumb ?? {}) as Record<string, string>

  let currentPath = ''
  for (const segment of pathSegments) {
    currentPath += `/${segment}`
    const label = breadcrumbMeta[segment] ?? segment.replace(/-/g, ' ').replace(/^\w/, c => c.toUpperCase())
    breadcrumbs.push({
      label,
      to: currentPath === route.path ? undefined : currentPath,
    })
  }

  return breadcrumbs
})
</script>
