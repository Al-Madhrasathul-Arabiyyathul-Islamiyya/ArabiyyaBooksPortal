<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Setup Center
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Complete required bootstrap steps before operations can be processed.
        </p>
      </div>
      <div class="flex items-center gap-2">
        <Tag
          :value="statusLabel"
          :severity="statusSeverity"
        />
        <Button
          label="Refresh"
          icon="pi pi-refresh"
          severity="secondary"
          text
          :loading="isLoading"
          @click="refreshStatus"
        />
      </div>
    </div>

    <Message
      v-if="blockingMessage"
      severity="warn"
      :closable="false"
    >
      {{ blockingMessage }}
    </Message>

    <Card>
      <template #content>
        <div class="grid gap-3">
          <div
            v-for="step in steps"
            :key="step.key"
            class="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-surface-200 p-3 dark:border-surface-700"
          >
            <div class="grid gap-1">
              <div class="flex items-center gap-2">
                <i
                  class="pi text-sm"
                  :class="step.isComplete ? 'pi-check-circle text-green-500' : 'pi-clock text-amber-500'"
                />
                <span class="font-semibold">{{ step.title }}</span>
              </div>
              <div class="text-sm text-surface-600 dark:text-surface-300">
                {{ step.hint || 'No additional instructions.' }}
              </div>
              <div
                v-if="step.completedAtUtc"
                class="text-xs text-surface-500"
              >
                Completed: {{ new Date(step.completedAtUtc).toLocaleString() }}
              </div>
            </div>

            <div
              v-if="canManageSetup && !isCompleted"
              class="flex gap-2"
            >
              <Button
                v-if="actionLabel(step.key)"
                :label="actionLabel(step.key)!"
                size="small"
                :loading="isMutating"
                :disabled="step.isComplete || !isActionEnabled(step.key)"
                @click="runStep(step.key)"
              />
            </div>
          </div>

          <div
            v-if="canManageSetup && !isCompleted"
            class="flex justify-end"
          >
            <Button
              label="Complete Setup"
              icon="pi pi-check"
              :loading="isMutating"
              :disabled="missingSteps.length > 0"
              @click="runStep('complete')"
            />
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    settings: 'Settings',
    setup: 'Setup Center',
  },
})

const { showError, showSuccess } = useAppToast()
const {
  normalizedStatus,
  isCompleted,
  canManageSetup,
  isLoading,
  isMutating,
  steps,
  blockingMessage,
  ensureLoaded,
  refresh,
  mutate,
  missingSteps,
} = useSetupReadiness()

const statusLabel = computed(() => {
  if (normalizedStatus.value === 'Completed') return 'Completed'
  if (normalizedStatus.value === 'InProgress') return 'In Progress'
  return 'Not Started'
})

const statusSeverity = computed(() => {
  if (normalizedStatus.value === 'Completed') return 'success'
  if (normalizedStatus.value === 'InProgress') return 'warn'
  return 'secondary'
})

function actionLabel(stepKey: string): string | null {
  switch (stepKey) {
    case 'super-admin': return 'Confirm'
    case 'slip-templates': return 'Confirm'
    case 'hierarchy': return 'Initialize'
    case 'active-academic-year': return 'Initialize'
    case 'reference-formats': return 'Initialize'
    default: return null
  }
}

function isActionEnabled(stepKey: string): boolean {
  if (stepKey === 'super-admin') return false
  return true
}

async function runStep(stepKey: string) {
  try {
    switch (stepKey) {
      case 'super-admin':
        await mutate('confirmSuperAdmin')
        break
      case 'slip-templates':
        await mutate('confirmSlipTemplates')
        break
      case 'hierarchy':
      case 'active-academic-year':
        await mutate('initializeHierarchy')
        break
      case 'reference-formats':
        await mutate('initializeReferenceFormats')
        break
      case 'complete':
        await mutate('complete')
        break
      default:
        return
    }
    showSuccess('Setup step completed.')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to update setup state'))
  }
}

async function refreshStatus() {
  try {
    await refresh()
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load setup status'))
  }
}

onMounted(() => {
  void ensureLoaded()
})
</script>
