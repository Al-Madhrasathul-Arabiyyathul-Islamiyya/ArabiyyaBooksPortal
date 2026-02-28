import { storeToRefs } from 'pinia'

const STEP_LABELS: Record<string, string> = {
  Start: 'Start Setup',
  SuperAdminConfirmed: 'Confirm Super Admin',
  SlipTemplatesReady: 'Confirm Slip Templates',
  HierarchyReady: 'Initialize Master Data Hierarchy',
  ActiveAcademicYearReady: 'Validate Active Academic Year',
  ReferenceFormatsReady: 'Initialize Reference Formats',
  Complete: 'Complete Setup',
}

export function useSetupReadiness() {
  const setupStore = useSetupReadinessStore()
  const { isSuperAdmin } = useAuth()

  const {
    status,
    initialized,
    isLoading,
    isMutating,
    normalizedStatus,
    isCompleted,
    isIncomplete,
    missingSteps,
    hints,
    issues,
    steps,
  } = storeToRefs(setupStore)

  const isOperationBlocked = computed(() => isIncomplete.value)
  const canManageSetup = computed(() => isSuperAdmin.value)
  const blockingMessage = computed(() => {
    if (!isOperationBlocked.value) {
      return ''
    }

    if (missingSteps.value.length > 0) {
      const labels = missingSteps.value.map(step => STEP_LABELS[step] ?? step)
      return `System setup is incomplete. Missing: ${labels.join(', ')}.`
    }

    return 'System setup is incomplete. Complete setup before processing operations.'
  })

  async function ensureLoaded(force = false) {
    return await setupStore.fetchStatus(force)
  }

  async function refresh() {
    return await setupStore.fetchStatus(true)
  }

  return {
    status,
    initialized,
    isLoading,
    isMutating,
    normalizedStatus,
    isCompleted,
    isIncomplete,
    isOperationBlocked,
    canManageSetup,
    missingSteps,
    hints,
    issues,
    steps,
    blockingMessage,
    ensureLoaded,
    refresh,
    mutate: setupStore.mutate,
    clear: setupStore.clear,
  }
}
