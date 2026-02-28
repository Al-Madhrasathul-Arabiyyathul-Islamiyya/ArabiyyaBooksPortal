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
          Missing steps: {{ missingStepLabels }}
        </div>
        <div
          v-if="!canManageSetup"
          class="text-xs text-surface-700 dark:text-surface-200"
        >
          Setup updates require a SuperAdmin account.
        </div>
      </div>

      <Button
        v-if="canManageSetup"
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
const { isIncomplete, canManageSetup, blockingMessage, missingSteps, ensureLoaded } = useSetupReadiness()
const { isAuthenticated } = useAuth()

const STEP_LABELS: Record<string, string> = {
  'super-admin': 'SuperAdmin Account',
  'slip-templates': 'Slip Templates',
  'active-academic-year': 'Active Academic Year',
  'hierarchy': 'Master Data Hierarchy',
  'reference-formats': 'Reference Number Formats',
}

const shouldShow = computed(() => isAuthenticated.value && isIncomplete.value)
const missingStepLabels = computed(() =>
  missingSteps.value.map(step => STEP_LABELS[step] ?? step).join(', '),
)

onMounted(() => {
  void ensureLoaded()
})
</script>
