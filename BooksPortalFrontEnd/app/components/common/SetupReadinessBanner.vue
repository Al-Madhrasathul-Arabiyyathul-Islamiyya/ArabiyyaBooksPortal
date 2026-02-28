<template>
  <Message
    v-if="shouldShow"
    severity="warn"
    :closable="false"
  >
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div class="grid gap-1">
        <div class="font-semibold">
          System Setup Incomplete
        </div>
        <div class="text-sm">
          {{ blockingMessage }}
        </div>
        <div
          v-if="missingSteps.length > 0"
          class="text-xs text-surface-700 dark:text-surface-200"
        >
          Missing steps: {{ missingSteps.join(', ') }}
        </div>
      </div>

      <Button
        label="Open Setup Center"
        icon="pi pi-cog"
        size="small"
        severity="warning"
        outlined
        @click="navigateTo('/admin/settings/setup')"
      />
    </div>
  </Message>
</template>

<script setup lang="ts">
const { isIncomplete, blockingMessage, missingSteps, ensureLoaded } = useSetupReadiness()
const { isAuthenticated, isAdmin } = useAuth()

const shouldShow = computed(() => isAuthenticated.value && isAdmin.value && isIncomplete.value)

onMounted(() => {
  void ensureLoaded()
})
</script>
