import { storeToRefs } from 'pinia'

const STEP_LABELS: Record<string, string> = {
  'super-admin': 'SuperAdmin Account',
  'slip-templates': 'Slip Templates',
  'active-academic-year': 'Academic Year Activation',
  'hierarchy': 'Master Data Hierarchy',
  'reference-formats': 'Reference Number Formats',
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
    requiresBootstrap,
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
    requiresBootstrap,
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
