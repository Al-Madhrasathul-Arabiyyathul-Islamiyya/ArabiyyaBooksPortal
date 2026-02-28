import type { SetupStatusResponse, SetupStatusValue } from '~/types/setup'
import { API } from '~/utils/constants'

type SetupMutationAction
  = 'start'
    | 'confirmSuperAdmin'
    | 'confirmSlipTemplates'
    | 'initializeHierarchy'
    | 'initializeReferenceFormats'
    | 'complete'

function normalizeSetupStatus(value: SetupStatusValue | null | undefined): 'NotStarted' | 'InProgress' | 'Completed' {
  if (typeof value === 'string') {
    const normalized = value.trim().toLowerCase()
    if (normalized === 'completed') return 'Completed'
    if (normalized === 'inprogress' || normalized === 'in_progress') return 'InProgress'
    return 'NotStarted'
  }

  if (typeof value === 'number') {
    if (value === 2) return 'Completed'
    if (value === 1) return 'InProgress'
    return 'NotStarted'
  }

  return 'NotStarted'
}

export const useSetupReadinessStore = defineStore('setup-readiness', () => {
  const api = useApi()
  const authStore = useAuthStore()

  const status = ref<SetupStatusResponse | null>(null)
  const initialized = ref(false)
  const isLoading = ref(false)
  const isMutating = ref(false)

  const normalizedStatus = computed(() => normalizeSetupStatus(status.value?.status))
  const isCompleted = computed(() => normalizedStatus.value === 'Completed')
  const isIncomplete = computed(() => !isCompleted.value)
  const missingSteps = computed(() => status.value?.missingSteps ?? [])
  const hints = computed(() => status.value?.hints ?? [])
  const issues = computed(() => status.value?.issues ?? [])
  const steps = computed(() => status.value?.steps ?? [])

  async function fetchStatus(force = false) {
    if (!authStore.isAuthenticated && !force) {
      return null
    }

    if (initialized.value && !force) {
      return status.value
    }

    isLoading.value = true
    try {
      const response = await api.get<SetupStatusResponse>(API.setup.status)
      if (response.success) {
        status.value = response.data
        initialized.value = true
      }
      return status.value
    }
    finally {
      isLoading.value = false
    }
  }

  function clear() {
    status.value = null
    initialized.value = false
  }

  async function mutate(action: SetupMutationAction) {
    isMutating.value = true
    try {
      switch (action) {
        case 'start':
          await api.post(API.setup.start)
          break
        case 'confirmSuperAdmin':
          await api.post(API.setup.superAdmin)
          break
        case 'confirmSlipTemplates':
          await api.post(API.setup.slipTemplatesConfirm)
          break
        case 'initializeHierarchy':
          await api.post(API.setup.hierarchyInitialize)
          break
        case 'initializeReferenceFormats':
          await api.post(API.setup.referenceFormatsInitialize)
          break
        case 'complete':
          await api.post(API.setup.complete)
          break
      }

      await fetchStatus(true)
    }
    finally {
      isMutating.value = false
    }
  }

  return {
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
    fetchStatus,
    clear,
    mutate,
  }
})
