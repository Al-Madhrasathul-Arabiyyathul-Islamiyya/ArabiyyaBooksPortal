<template>
  <div class="relative flex min-h-screen items-center justify-center bg-surface-50 dark:bg-surface-950">
    <!-- Color mode toggle -->
    <div class="fixed right-6 top-6">
      <Button
        :icon="colorMode.value === 'dark' ? 'pi pi-sun' : 'pi pi-moon'"
        text
        severity="secondary"
        rounded
        @click="toggleColorMode"
      />
    </div>

    <slot />
  </div>
</template>

<script setup lang="ts">
const colorMode = useColorMode()

function toggleColorMode() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}
</script>
