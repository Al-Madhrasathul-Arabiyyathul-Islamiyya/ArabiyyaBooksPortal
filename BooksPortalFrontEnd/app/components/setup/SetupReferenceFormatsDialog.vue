<template>
  <Dialog
    v-model:visible="model"
    modal
    header="Configure Reference Formats"
    :style="{ width: '72rem' }"
  >
    <div class="grid gap-4">
      <div class="flex justify-end">
        <Button
          label="New Format"
          icon="pi pi-plus"
          @click="openCreateDialog"
        />
      </div>

      <CommonAdminDataTable
        :value="formats"
        :loading="isLoading"
        data-key="id"
        size="small"
      >
        <Column
          header="Slip Type"
          style="min-width: 10rem;"
        >
          <template #body="{ data }">
            {{ slipTypeLabels[data.slipType] ?? `Type ${data.slipType}` }}
          </template>
        </Column>
        <Column
          field="academicYearName"
          header="Academic Year"
          style="min-width: 10rem;"
        />
        <Column
          field="formatTemplate"
          header="Template"
          style="min-width: 14rem;"
        />
        <Column
          field="paddingWidth"
          header="Padding"
          style="min-width: 8rem;"
        />
        <Column
          header="Preview"
          style="min-width: 12rem;"
        >
          <template #body="{ data }">
            <code class="rounded bg-surface-100 px-2 py-1 text-xs dark:bg-surface-800">
              {{ previewTemplate(data.formatTemplate, data.paddingWidth) }}
            </code>
          </template>
        </Column>
        <Column
          header="Actions"
          :exportable="false"
          style="width: 12rem;"
        >
          <template #body="{ data }">
            <div class="flex items-center gap-2">
              <CommonIconActionButton
                icon="pi pi-pencil"
                tooltip="Edit format"
                @click="openEditDialog(data)"
              />
              <CommonIconActionButton
                icon="pi pi-trash"
                severity="danger"
                tooltip="Delete format"
                @click="handleDelete(data)"
              />
            </div>
          </template>
        </Column>
      </CommonAdminDataTable>
    </div>

    <Dialog
      v-model:visible="isEditorVisible"
      modal
      :header="isEditing ? 'Edit Reference Format' : 'Create Reference Format'"
      :style="{ width: '38rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <FormsFormField
          label="Slip Type"
          required
          field-id="slipType"
          :error="errors.slipType"
        >
          <Select
            id="slipType"
            v-model="form.slipType"
            :options="slipTypeOptions"
            option-label="label"
            option-value="value"
            fluid
          />
        </FormsFormField>

        <FormsFormField
          label="Academic Year"
          required
          field-id="academicYearId"
          :error="errors.academicYearId"
        >
          <Select
            id="academicYearId"
            v-model="form.academicYearId"
            :options="academicYearOptions"
            option-label="label"
            option-value="value"
            fluid
          />
        </FormsFormField>

        <FormsFormField
          label="Format Template"
          required
          field-id="formatTemplate"
          :error="errors.formatTemplate"
        >
          <InputText
            id="formatTemplate"
            v-model.trim="form.formatTemplate"
            fluid
          />
          <small class="text-xs text-surface-500">
            Must contain <code>{autonum}</code>. Optional token: <code>{year}</code>.
          </small>
        </FormsFormField>

        <FormsFormField
          label="Padding Width"
          required
          field-id="paddingWidth"
          :error="errors.paddingWidth"
        >
          <InputNumber
            id="paddingWidth"
            v-model="form.paddingWidth"
            :min="1"
            :max="12"
            :use-grouping="false"
            fluid
          />
        </FormsFormField>

        <Message
          v-if="formError"
          severity="error"
          :closable="false"
        >
          {{ formError }}
        </Message>

        <div class="flex justify-end gap-2 pt-2">
          <Button
            type="button"
            label="Cancel"
            severity="secondary"
            text
            @click="closeEditor"
          />
          <Button
            type="submit"
            :label="isEditing ? 'Save Changes' : 'Create'"
            :loading="isSubmitting"
          />
        </div>
      </form>
    </Dialog>
  </Dialog>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Lookup, ReferenceNumberFormat } from '~/types/entities'
import { CreateReferenceNumberFormatRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { slipTypeLabels, slipTypeOptions } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const model = defineModel<boolean>('visible', { default: false })
const emit = defineEmits<{ refreshed: [] }>()

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()

const formats = ref<ReferenceNumberFormat[]>([])
const academicYears = ref<Lookup[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const isEditorVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)

const FormSchema = z.object({
  slipType: z.number().int().min(1).nullable(),
  academicYearId: z.number().int().min(1).nullable(),
  formatTemplate: z.string().min(1, 'Format template is required'),
  paddingWidth: z.number().int().min(1).nullable(),
}).superRefine((values, ctx) => {
  if (values.slipType === null) ctx.addIssue({ code: 'custom', message: 'Slip type is required', path: ['slipType'] })
  if (values.academicYearId === null) ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  if (values.paddingWidth === null) ctx.addIssue({ code: 'custom', message: 'Padding width is required', path: ['paddingWidth'] })
  if (!values.formatTemplate.includes('{autonum}')) {
    ctx.addIssue({ code: 'custom', message: 'Template must include {autonum}', path: ['formatTemplate'] })
  }
})

const {
  state: form,
  errors,
  globalError: formError,
  validateWithSchema,
  setGlobalError,
  applyBackendErrors,
  resetForm: resetValidationForm,
} = useAppValidation(
  {
    slipType: null as number | null,
    academicYearId: null as number | null,
    formatTemplate: '',
    paddingWidth: 6 as number | null,
  },
  FormSchema,
)

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({ label: year.name, value: year.id })),
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
  isEditing.value = false
}

function previewTemplate(template: string, paddingWidth: number, sequenceNo = 1) {
  const sequence = String(sequenceNo).padStart(paddingWidth ?? 6, '0')
  return template.replaceAll('{year}', String(new Date().getFullYear())).replaceAll('{autonum}', sequence)
}

async function loadLookups() {
  const response = await api.get<Lookup[]>(API.lookups.academicYears)
  if (response.success) academicYears.value = response.data
}

async function loadReferenceFormats() {
  isLoading.value = true
  try {
    const response = await api.get<ReferenceNumberFormat[]>(API.referenceNumberFormats.base)
    if (response.success) {
      formats.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load reference formats')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load reference formats'))
  }
  finally {
    isLoading.value = false
  }
}

function openCreateDialog() {
  resetForm()
  isEditorVisible.value = true
}

function openEditDialog(item: ReferenceNumberFormat) {
  resetForm()
  isEditing.value = true
  selectedId.value = item.id
  form.slipType = item.slipType
  form.academicYearId = item.academicYearId
  form.formatTemplate = item.formatTemplate
  form.paddingWidth = item.paddingWidth
  isEditorVisible.value = true
}

function closeEditor() {
  isEditorVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (
    parsed.data.slipType === null
    || parsed.data.academicYearId === null
    || parsed.data.paddingWidth === null
  ) {
    setGlobalError('Invalid form payload')
    return
  }

  const requestCheck = CreateReferenceNumberFormatRequestSchema.safeParse({
    slipType: parsed.data.slipType,
    academicYearId: parsed.data.academicYearId,
    formatTemplate: parsed.data.formatTemplate,
    paddingWidth: parsed.data.paddingWidth,
  })
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<ReferenceNumberFormat>(API.referenceNumberFormats.byId(selectedId.value), requestCheck.data)
      if (response.success) {
        showSuccess('Reference format updated')
        closeEditor()
        await loadReferenceFormats()
        emit('refreshed')
        return
      }
      setGlobalError(response.message ?? 'Failed to update reference format')
      return
    }

    const response = await api.post<ReferenceNumberFormat>(API.referenceNumberFormats.base, requestCheck.data)
    if (response.success) {
      showSuccess('Reference format created')
      closeEditor()
      await loadReferenceFormats()
      emit('refreshed')
      return
    }
    setGlobalError(response.message ?? 'Failed to create reference format')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: ReferenceNumberFormat) {
  confirmDelete(
    `Delete format for "${slipTypeLabels[item.slipType] ?? item.slipType}" in "${item.academicYearName}"?`,
    async () => {
      try {
        const response = await api.del<string>(API.referenceNumberFormats.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Reference format deleted')
          await loadReferenceFormats()
          emit('refreshed')
          return
        }
        showError(response.message ?? 'Failed to delete reference format')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete reference format'))
      }
    },
  )
}

watch(model, async (visible) => {
  if (!visible) return
  await loadLookups()
  await loadReferenceFormats()
})
</script>
