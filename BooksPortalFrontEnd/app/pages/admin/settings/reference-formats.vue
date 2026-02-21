<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Reference Formats
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Configure slip numbering templates by slip type and academic year.
        </p>
      </div>
      <Button
        label="New Format"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <div class="grid gap-3 md:grid-cols-3">
          <Select
            v-model="filters.slipType"
            :options="slipTypeOptions"
            option-label="label"
            option-value="value"
            placeholder="Filter by slip type"
            show-clear
            fluid
            @change="void loadReferenceFormats()"
          />
          <Select
            v-model="filters.academicYearId"
            :options="academicYearFilterOptions"
            option-label="label"
            option-value="value"
            placeholder="Filter by academic year"
            fluid
            @change="void loadReferenceFormats()"
          >
            <template #option="{ option }">
              <div class="flex items-center gap-2">
                <span>{{ option.label }}</span>
                <Tag
                  v-if="option.isActive"
                  value="Active"
                  severity="success"
                />
              </div>
            </template>
          </Select>
          <div class="flex items-center justify-end">
            <Button
              label="Reset"
              icon="pi pi-refresh"
              severity="secondary"
              outlined
              @click="resetFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
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
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete format"
                  @click="handleDelete(data)"
                />
              </div>
            </template>
          </Column>
        </CommonAdminDataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="isDialogVisible"
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
            :invalid="!!errors.slipType"
            @blur="touchField('slipType')"
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
            :invalid="!!errors.academicYearId"
            @blur="touchField('academicYearId')"
          >
            <template #option="{ option }">
              <div class="flex items-center gap-2">
                <span>{{ option.label }}</span>
                <Tag
                  v-if="option.isActive"
                  value="Active"
                  severity="success"
                />
              </div>
            </template>
          </Select>
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
            :invalid="!!errors.formatTemplate"
            @blur="touchField('formatTemplate')"
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
            :invalid="!!errors.paddingWidth"
            @blur="touchField('paddingWidth')"
          />
        </FormsFormField>

        <div class="rounded-lg border border-surface-200 bg-surface-50 p-3 dark:border-surface-700 dark:bg-surface-900">
          <div class="mb-2 text-sm font-semibold text-surface-900 dark:text-surface-0">
            Live Format Preview
          </div>
          <div class="grid gap-2 text-sm">
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Template</span>
              <code class="rounded bg-surface-100 px-2 py-1 text-xs dark:bg-surface-800">
                {{ form.formatTemplate || '{year}-{autonum}' }}
              </code>
            </div>
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Academic Year</span>
              <span class="font-medium text-surface-900 dark:text-surface-0">{{ previewYear }}</span>
            </div>
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Slip Type</span>
              <span class="font-medium text-surface-900 dark:text-surface-0">{{ previewSlipType }}</span>
            </div>
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Example (first issued)</span>
              <code class="rounded bg-emerald-50 px-2 py-1 text-xs text-emerald-700 dark:bg-emerald-950/40 dark:text-emerald-300">
                {{ previewTemplate(form.formatTemplate || '{year}-{autonum}', form.paddingWidth ?? 6, 1) }}
              </code>
            </div>
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Example (mid sequence)</span>
              <code class="rounded bg-surface-100 px-2 py-1 text-xs dark:bg-surface-800">
                {{ previewTemplate(form.formatTemplate || '{year}-{autonum}', form.paddingWidth ?? 6, 128) }}
              </code>
            </div>
            <div class="flex items-center justify-between gap-3">
              <span class="text-surface-600 dark:text-surface-400">Example (higher sequence)</span>
              <code class="rounded bg-surface-100 px-2 py-1 text-xs dark:bg-surface-800">
                {{ previewTemplate(form.formatTemplate || '{year}-{autonum}', form.paddingWidth ?? 6, 1024) }}
              </code>
            </div>
          </div>
        </div>

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
            @click="closeDialog"
          />
          <Button
            type="submit"
            :label="isEditing ? 'Save Changes' : 'Create'"
            :loading="isSubmitting"
          />
        </div>
      </form>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Lookup, ReferenceNumberFormat } from '~/types/entities'
import { CreateReferenceNumberFormatRequestSchema } from '~/types/forms'
import { slipTypeLabels, slipTypeOptions } from '~/utils/formatters'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'settings': 'Settings',
    'reference-formats': 'Reference Formats',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const formats = ref<ReferenceNumberFormat[]>([])
const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)

const filters = reactive({
  slipType: null as number | null,
  academicYearId: null as number | null,
})

const FormSchema = z.object({
  slipType: z.number().int().min(1).nullable(),
  academicYearId: z.number().int().min(1).nullable(),
  formatTemplate: z.string().min(1, 'Format template is required'),
  paddingWidth: z.number().int().min(1, 'Padding width must be at least 1').nullable(),
}).superRefine((values, ctx) => {
  if (values.slipType === null) {
    ctx.addIssue({ code: 'custom', message: 'Slip type is required', path: ['slipType'] })
  }
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.paddingWidth === null) {
    ctx.addIssue({ code: 'custom', message: 'Padding width is required', path: ['paddingWidth'] })
  }
  if (!values.formatTemplate.includes('{autonum}')) {
    ctx.addIssue({ code: 'custom', message: 'Template must include {autonum}', path: ['formatTemplate'] })
  }
})

const {
  state: form,
  errors,
  globalError: formError,
  touchField,
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
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
    isActive: year.id === activeAcademicYearId.value,
  })),
)

const academicYearFilterOptions = computed(() => [
  { label: 'All Academic Years', value: null as number | null, isActive: false },
  ...academicYearOptions.value,
])

const slipTypeNameMap: Record<string, number> = {
  distribution: 1,
  return: 2,
  teacherissue: 3,
  teacherreturn: 4,
}

function normalizeSlipType(value: unknown): number | null {
  if (typeof value === 'number' && Number.isInteger(value)) {
    return value
  }
  if (typeof value === 'string') {
    const trimmed = value.trim()
    if (!trimmed) return null
    const parsed = Number(trimmed)
    if (Number.isInteger(parsed)) return parsed
    return slipTypeNameMap[trimmed.toLowerCase()] ?? null
  }
  return null
}

function previewTemplate(template: string, paddingWidth: number, sequenceNo = 1) {
  const sequence = String(sequenceNo).padStart(paddingWidth ?? 6, '0')
  return template
    .replaceAll('{year}', previewYear.value)
    .replaceAll('{autonum}', sequence)
}

const previewYear = computed(() => {
  const selected = academicYears.value.find(year => year.id === form.academicYearId)
  if (selected) return selected.name
  const active = academicYears.value.find(year => year.id === activeAcademicYearId.value)
  return active?.name ?? '2026'
})

const previewSlipType = computed(() => {
  if (form.slipType === null) return 'Not selected'
  return slipTypeLabels[form.slipType] ?? `Type ${form.slipType}`
})

function resetForm() {
  resetValidationForm()
  selectedId.value = null
  isEditing.value = false
}

async function loadAcademicYears() {
  try {
    const [yearsResponse, activeResponse] = await Promise.all([
      api.get<Lookup[]>(API.lookups.academicYears),
      api.get<{ id: number }>(API.academicYears.active),
    ])

    if (yearsResponse.success) {
      academicYears.value = yearsResponse.data
    }
    else {
      showError(yearsResponse.message ?? 'Failed to load academic years')
    }

    if (activeResponse.success) {
      activeAcademicYearId.value = activeResponse.data.id
      if (!isEditing.value && form.academicYearId === null) {
        form.academicYearId = activeResponse.data.id
      }
      if (filters.academicYearId === null) {
        filters.academicYearId = activeResponse.data.id
      }
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load academic years'))
  }
}

async function loadReferenceFormats() {
  isLoading.value = true
  try {
    const response = await api.get<ReferenceNumberFormat[]>(API.referenceNumberFormats.base, {
      slipType: filters.slipType ?? undefined,
      academicYearId: filters.academicYearId ?? undefined,
    })
    if (response.success) {
      formats.value = response.data.map(item => ({
        ...item,
        slipType: normalizeSlipType(item.slipType) ?? item.slipType,
      }))
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
  form.academicYearId = activeAcademicYearId.value
  isDialogVisible.value = true
}

function openEditDialog(item: ReferenceNumberFormat) {
  resetForm()
  isEditing.value = true
  selectedId.value = item.id
  form.slipType = normalizeSlipType(item.slipType)
  form.academicYearId = item.academicYearId
  form.formatTemplate = item.formatTemplate
  form.paddingWidth = item.paddingWidth
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
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

  const payload = {
    slipType: parsed.data.slipType,
    academicYearId: parsed.data.academicYearId,
    formatTemplate: parsed.data.formatTemplate,
    paddingWidth: parsed.data.paddingWidth,
  }
  const requestCheck = CreateReferenceNumberFormatRequestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<ReferenceNumberFormat>(
        API.referenceNumberFormats.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Reference format updated')
        closeDialog()
        await loadReferenceFormats()
        return
      }
      setGlobalError(response.message ?? 'Failed to update reference format')
      return
    }

    const response = await api.post<ReferenceNumberFormat>(
      API.referenceNumberFormats.base,
      requestCheck.data,
    )
    if (response.success) {
      showSuccess('Reference format created')
      closeDialog()
      await loadReferenceFormats()
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

async function resetFilters() {
  filters.slipType = null
  filters.academicYearId = null
  await loadReferenceFormats()
}

onMounted(async () => {
  await loadAcademicYears()
  await loadReferenceFormats()
})
</script>
