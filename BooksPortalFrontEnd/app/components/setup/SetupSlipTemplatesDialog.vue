<template>
  <Dialog
    v-model:visible="model"
    modal
    header="Configure Slip Templates"
    :style="{ width: '70rem' }"
  >
    <div class="grid gap-4">
      <div class="flex justify-end">
        <Button
          label="Load Default Templates"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          :loading="isResetting"
          @click="resetToDefaults"
        />
      </div>

      <Message
        severity="warn"
        :closable="false"
      >
        Changes here directly affect generated slip labels and print output.
      </Message>

      <Message
        v-if="!isLoading && settings.length === 0"
        severity="info"
        :closable="false"
      >
        No slip template records found. Click "Load Default Templates" to initialize baseline values.
      </Message>

      <Card
        v-for="group in groupedSettings"
        :key="group.category"
      >
        <template #title>
          <span class="text-base">{{ group.category }}</span>
        </template>
        <template #content>
          <CommonAdminDataTable
            :value="group.items"
            :loading="isLoading"
            data-key="id"
            size="small"
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
                  @click="saveSetting(data.id)"
                />
              </template>
            </Column>
          </CommonAdminDataTable>
        </template>
      </Card>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import type { SlipTemplateSetting } from '~/types/entities'
import { UpdateSlipTemplateSettingRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const model = defineModel<boolean>('visible', { default: false })

const emit = defineEmits<{
  refreshed: []
}>()

const api = useApi()
const { showError, showSuccess } = useAppToast()

const settings = ref<SlipTemplateSetting[]>([])
const isLoading = ref(false)
const isResetting = ref(false)
const editState = ref<Record<number, { value: string, sortOrder: number }>>({})
const isSavingRow = ref<Record<number, boolean>>({})

const groupedSettings = computed(() => {
  const map = new Map<string, SlipTemplateSetting[]>()
  for (const item of settings.value) {
    if (!map.has(item.category)) map.set(item.category, [])
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
    nextState[item.id] = { value: item.value, sortOrder: item.sortOrder }
    nextSaving[item.id] = false
  }
  editState.value = nextState
  isSavingRow.value = nextSaving
}

function ensureDraft(id: number) {
  if (!editState.value[id]) editState.value[id] = { value: '', sortOrder: 0 }
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

async function saveSetting(id: number) {
  const draft = editState.value[id]
  if (!draft) return

  const parsed = UpdateSlipTemplateSettingRequestSchema.safeParse(draft)
  if (!parsed.success) {
    showError('Invalid slip template value')
    return
  }

  isSavingRow.value[id] = true
  try {
    const response = await api.put<SlipTemplateSetting>(API.slipTemplateSettings.byId(id), parsed.data)
    if (response.success) {
      showSuccess('Template setting saved')
      await loadSettings()
      emit('refreshed')
      return
    }
    showError(response.message ?? 'Failed to save setting')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to save setting'))
  }
  finally {
    isSavingRow.value[id] = false
  }
}

async function resetToDefaults() {
  isResetting.value = true
  try {
    const response = await api.post<string>(API.slipTemplateSettings.reset)
    if (response.success) {
      showSuccess(response.message ?? 'Slip template defaults loaded')
      await loadSettings()
      emit('refreshed')
      return
    }
    showError(response.message ?? 'Failed to load default templates')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load default templates'))
  }
  finally {
    isResetting.value = false
  }
}

watch(model, (visible) => {
  if (visible) void loadSettings()
})
</script>
