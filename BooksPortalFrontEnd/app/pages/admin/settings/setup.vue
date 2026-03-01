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
          <SetupStepCard
            v-for="step in steps"
            :key="step.key"
            :step="step"
          >
            <template #actions>
              <Button
                v-if="canConfigure(step.key) && canManageSetup && !isCompleted"
                :label="configureLabel(step.key)"
                icon="pi pi-cog"
                outlined
                size="small"
                @click="openConfigure(step.key)"
              />
              <Button
                v-if="canManageSetup && !isCompleted && actionLabel(step.key)"
                :label="actionLabel(step.key)!"
                size="small"
                :loading="isMutating"
                :disabled="step.isComplete || !isActionEnabled(step.key)"
                @click="runStep(step.key)"
              />
            </template>
          </SetupStepCard>

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

    <SetupSlipTemplatesDialog
      v-model:visible="isSlipTemplatesDialogVisible"
      @refreshed="refreshStatus"
    />
    <SetupReferenceFormatsDialog
      v-model:visible="isReferenceFormatsDialogVisible"
      @refreshed="refreshStatus"
    />
    <SetupHierarchyDialog
      v-model:visible="isHierarchyDialogVisible"
      @refreshed="refreshStatus"
    />
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

const isSlipTemplatesDialogVisible = ref(false)
const isReferenceFormatsDialogVisible = ref(false)
const isHierarchyDialogVisible = ref(false)

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
    case 'hierarchy': return 'Confirm'
    case 'active-academic-year': return 'Confirm'
    case 'reference-formats': return 'Confirm'
    default: return null
  }
}

function configureLabel(stepKey: string): string {
  switch (stepKey) {
    case 'slip-templates': return 'Edit Templates'
    case 'reference-formats': return 'Edit Formats'
    default: return 'Configure'
  }
}

function canConfigure(stepKey: string) {
  return stepKey === 'slip-templates' || stepKey === 'hierarchy' || stepKey === 'active-academic-year' || stepKey === 'reference-formats'
}

function openConfigure(stepKey: string) {
  if (stepKey === 'slip-templates') {
    isSlipTemplatesDialogVisible.value = true
    return
  }
  if (stepKey === 'reference-formats') {
    isReferenceFormatsDialogVisible.value = true
    return
  }
  if (stepKey === 'hierarchy' || stepKey === 'active-academic-year') {
    isHierarchyDialogVisible.value = true
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
