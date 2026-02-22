<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Slip Templates
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage key/value template settings for printed slips.
        </p>
      </div>
      <Button
        v-if="isSuperAdmin"
        label="Reset to Defaults"
        icon="pi pi-refresh"
        severity="danger"
        outlined
        :loading="isResetting"
        @click="handleResetToDefaults"
      />
    </div>

    <div
      v-for="group in groupedSettings"
      :key="group.category"
    >
      <Card>
        <template #title>
          <span class="text-base">{{ group.category }}</span>
        </template>
        <template #content>
          <CommonAdminDataTable
            :value="group.items"
            :loading="isLoading"
            data-key="id"
            size="small"
            responsive-layout="scroll"
          >
            <Column
              field="key"
              header="Key"
              style="min-width: 12rem;"
            />
            <Column
              header="Value"
              style="min-width: 18rem;"
            >
              <template #body="{ data }">
                <InputText
                  :model-value="getDraftValue(data.id)"
                  fluid
                  @update:model-value="setDraftValue(data.id, $event)"
                />
                <small
                  v-if="rowErrors[data.id]?.value"
                  class="text-red-500"
                >
                  {{ rowErrors[data.id]?.value }}
                </small>
              </template>
            </Column>
            <Column
              header="Sort Order"
              style="min-width: 10rem;"
            >
              <template #body="{ data }">
                <InputNumber
                  :model-value="getDraftSortOrder(data.id)"
                  :min="0"
                  :use-grouping="false"
                  fluid
                  @update:model-value="setDraftSortOrder(data.id, $event)"
                />
                <small
                  v-if="rowErrors[data.id]?.sortOrder"
                  class="text-red-500"
                >
                  {{ rowErrors[data.id]?.sortOrder }}
                </small>
              </template>
            </Column>
            <Column
              header="Actions"
              :exportable="false"
              style="width: 8rem;"
            >
              <template #body="{ data }">
                <CommonIconActionButton
                  icon="pi pi-save"
                  severity="success"
                  tooltip="Save setting"
                  :disabled="isSavingRow[data.id]"
                  @click="requestSaveSetting(data.id)"
                />
              </template>
            </Column>
          </CommonAdminDataTable>
        </template>
      </Card>
    </div>

    <Dialog
      v-model:visible="isSaveConfirmVisible"
      modal
      header="Confirm Slip Template Change"
      :style="{ width: '36rem' }"
      :closable="!isConfirmingSave"
    >
      <div class="flex h-full min-h-0 flex-col gap-4">
        <Message
          severity="warn"
          :closable="false"
        >
          <div class="font-semibold">
            Warning: Template changes directly affect generated slip labels and print output.
          </div>
          <div class="mt-1">
            Saving incorrect values can break printed slip formatting for operations.
          </div>
        </Message>

        <div
          v-if="pendingSaveContext"
          class="rounded-lg border border-surface-200 p-3 text-sm dark:border-surface-700"
        >
          <div><span class="font-semibold">Category:</span> {{ pendingSaveContext.category }}</div>
          <div><span class="font-semibold">Key:</span> {{ pendingSaveContext.key }}</div>
          <div><span class="font-semibold">New Value:</span> {{ pendingSaveContext.value }}</div>
          <div><span class="font-semibold">Sort Order:</span> {{ pendingSaveContext.sortOrder }}</div>
        </div>

        <div class="flex items-start gap-2">
          <Checkbox
            v-model="hasAcknowledgedSaveRisk"
            input-id="acknowledgeTemplateSaveRisk"
            binary
          />
          <label
            for="acknowledgeTemplateSaveRisk"
            class="text-sm text-surface-700 dark:text-surface-300"
          >
            I understand this may break slip labels and printed templates if configured incorrectly.
          </label>
        </div>

        <div class="flex justify-end gap-2">
          <Button
            type="button"
            label="Cancel"
            severity="secondary"
            text
            :disabled="isConfirmingSave"
            @click="closeSaveConfirmDialog"
          />
          <Button
            type="button"
            label="Confirm Save"
            severity="warn"
            :disabled="!hasAcknowledgedSaveRisk"
            :loading="isConfirmingSave"
            @click="performConfirmedSave"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { SlipTemplateSetting } from '~/types/entities'
import { UpdateSlipTemplateSettingRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'
import { toZodFieldErrors } from '~/utils/validation/zod-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'settings': 'Settings',
    'slip-templates': 'Slip Templates',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const settings = ref<SlipTemplateSetting[]>([])
const isLoading = ref(false)
const isResetting = ref(false)
const editState = ref<Record<number, { value: string, sortOrder: number }>>({})
const isSavingRow = ref<Record<number, boolean>>({})
const rowErrors = ref<Record<number, { value?: string, sortOrder?: string }>>({})
const isSaveConfirmVisible = ref(false)
const hasAcknowledgedSaveRisk = ref(false)
const isConfirmingSave = ref(false)
const pendingSaveRequest = ref<{
  id: number
  payload: {
    value: string
    sortOrder: number
  }
} | null>(null)

const RowSchema = z.object({
  value: z.string().min(1, 'Value is required'),
  sortOrder: z.number().int().min(0, 'Sort order must be 0 or higher'),
})

const groupedSettings = computed(() => {
  const map = new Map<string, SlipTemplateSetting[]>()
  for (const item of settings.value) {
    if (!map.has(item.category)) {
      map.set(item.category, [])
    }
    map.get(item.category)!.push(item)
  }
  return Array.from(map.entries()).map(([category, items]) => ({
    category,
    items: [...items].sort((a, b) => a.sortOrder - b.sortOrder),
  }))
})

function initializeEditState() {
  const nextState: Record<number, { value: string, sortOrder: number }> = {}
  const nextSaving: Record<number, boolean> = {}
  for (const item of settings.value) {
    nextState[item.id] = {
      value: item.value,
      sortOrder: item.sortOrder,
    }
    nextSaving[item.id] = false
  }
  editState.value = nextState
  isSavingRow.value = nextSaving
  rowErrors.value = {}
}

function ensureDraft(id: number) {
  if (!editState.value[id]) {
    editState.value[id] = { value: '', sortOrder: 0 }
  }
  return editState.value[id]
}

function getDraftValue(id: number) {
  return ensureDraft(id).value
}

function setDraftValue(id: number, value: string | undefined) {
  ensureDraft(id).value = value ?? ''
}

function getDraftSortOrder(id: number) {
  return ensureDraft(id).sortOrder
}

function setDraftSortOrder(id: number, value: number | null) {
  ensureDraft(id).sortOrder = value ?? 0
}

const pendingSaveContext = computed(() => {
  if (!pendingSaveRequest.value) return null
  const item = settings.value.find(entry => entry.id === pendingSaveRequest.value!.id)
  if (!item) return null
  return {
    category: item.category,
    key: item.key,
    value: pendingSaveRequest.value.payload.value,
    sortOrder: pendingSaveRequest.value.payload.sortOrder,
  }
})

async function loadSettings() {
  isLoading.value = true
  try {
    const response = await api.get<SlipTemplateSetting[]>(API.slipTemplateSettings.base)
    if (response.success) {
      settings.value = response.data
      initializeEditState()
      return
    }
    showError(response.message ?? 'Failed to load slip template settings')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load slip template settings'))
  }
  finally {
    isLoading.value = false
  }
}

function closeSaveConfirmDialog() {
  isSaveConfirmVisible.value = false
  hasAcknowledgedSaveRisk.value = false
  pendingSaveRequest.value = null
}

function requestSaveSetting(id: number) {
  const current = editState.value[id]
  if (!current) return

  rowErrors.value[id] = {}
  const parsed = RowSchema.safeParse(current)
  if (!parsed.success) {
    const fieldErrors = toZodFieldErrors(parsed.error)
    rowErrors.value[id] = {
      value: fieldErrors.value?.[0],
      sortOrder: fieldErrors.sortOrder?.[0],
    }
    return
  }

  const requestCheck = UpdateSlipTemplateSettingRequestSchema.safeParse(parsed.data)
  if (!requestCheck.success) {
    showError('Invalid payload for slip template setting')
    return
  }

  pendingSaveRequest.value = {
    id,
    payload: requestCheck.data,
  }
  hasAcknowledgedSaveRisk.value = false
  isSaveConfirmVisible.value = true
}

async function performConfirmedSave() {
  if (!pendingSaveRequest.value || !hasAcknowledgedSaveRisk.value) return

  const { id, payload } = pendingSaveRequest.value

  isConfirmingSave.value = true
  isSavingRow.value[id] = true
  try {
    const response = await api.put<SlipTemplateSetting>(
      API.slipTemplateSettings.byId(id),
      payload,
    )
    if (response.success) {
      showSuccess('Setting saved')
      const idx = settings.value.findIndex(item => item.id === id)
      if (idx >= 0) {
        settings.value[idx] = response.data
      }
      initializeEditState()
      closeSaveConfirmDialog()
      return
    }
    showError(response.message ?? 'Failed to save setting')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to save setting'))
  }
  finally {
    isConfirmingSave.value = false
    isSavingRow.value[id] = false
  }
}

function handleResetToDefaults() {
  confirmAction(
    'Reset all slip template settings to defaults?',
    async () => {
      isResetting.value = true
      try {
        const response = await api.post<string>(API.slipTemplateSettings.reset)
        if (response.success) {
          showSuccess(response.message ?? 'Slip template settings reset')
          await loadSettings()
          return
        }
        showError(response.message ?? 'Failed to reset slip template settings')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to reset slip template settings'))
      }
      finally {
        isResetting.value = false
      }
    },
    'Reset Slip Templates',
    'Reset',
    'Cancel',
  )
}

onMounted(async () => {
  await loadSettings()
})
</script>
