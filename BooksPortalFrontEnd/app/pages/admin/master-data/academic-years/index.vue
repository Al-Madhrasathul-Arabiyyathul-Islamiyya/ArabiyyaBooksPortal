<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Academic Years
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage active and historical academic years.
        </p>
      </div>
      <Button
        label="New Academic Year"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <CommonAdminDataTable
          :value="academicYears"
          :loading="isLoading"
          data-key="id"
          size="small"
        >
          <Column
            field="name"
            header="Name"
          />
          <Column
            field="year"
            header="Year"
          />
          <Column header="Start Date">
            <template #body="{ data }">
              {{ formatDate(data.startDate) }}
            </template>
          </Column>
          <Column header="End Date">
            <template #body="{ data }">
              {{ formatDate(data.endDate) }}
            </template>
          </Column>
          <Column header="Status">
            <template #body="{ data }">
              <Tag
                :value="data.isActive ? 'Active' : 'Inactive'"
                :severity="data.isActive ? 'success' : 'secondary'"
              />
            </template>
          </Column>
          <Column
            header="Actions"
            :exportable="false"
            style="width: 16rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit academic year"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="!data.isActive"
                  icon="pi pi-check-circle"
                  severity="success"
                  tooltip="Activate year"
                  @click="handleActivate(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete academic year"
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
      :header="isEditing ? 'Edit Academic Year' : 'Create Academic Year'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <FormsFormField
          label="Name"
          required
          field-id="name"
          :error="errors.name"
        >
          <InputText
            id="name"
            v-model.trim="form.name"
            fluid
            :invalid="!!errors.name"
            @blur="touchField('name')"
          />
        </FormsFormField>

        <FormsFormField
          label="Year"
          required
          field-id="year"
          :error="errors.year"
        >
          <InputNumber
            id="year"
            v-model="form.year"
            :min="2000"
            :max="2100"
            fluid
            disabled
            :invalid="!!errors.year"
            @blur="touchField('year')"
          />
        </FormsFormField>

        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
          <FormsFormField
            label="Start Date"
            required
            field-id="startDate"
            :error="errors.startDate"
          >
            <DatePicker
              id="startDate"
              v-model="form.startDate"
              show-icon
              fluid
              :invalid="!!errors.startDate"
              @blur="touchField('startDate')"
            />
          </FormsFormField>

          <FormsFormField
            label="End Date"
            required
            field-id="endDate"
            :error="errors.endDate"
          >
            <DatePicker
              id="endDate"
              v-model="form.endDate"
              show-icon
              fluid
              :invalid="!!errors.endDate"
              @blur="touchField('endDate')"
            />
          </FormsFormField>
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
import type { AcademicYear } from '~/types/entities'
import {
  CreateAcademicYearRequestSchema,
  UpdateAcademicYearRequestSchema,
} from '~/types/forms'
import { API } from '~/utils/constants'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'academic-years': 'Academic Years',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction, confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const academicYears = ref<AcademicYear[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)

const FormSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  year: z.number().int().min(2000).max(2100).nullable(),
  startDate: z.date().nullable(),
  endDate: z.date().nullable(),
}).superRefine((values, ctx) => {
  if (values.year === null) {
    ctx.addIssue({ code: 'custom', message: 'Year is required', path: ['year'] })
  }
  if (values.startDate === null) {
    ctx.addIssue({ code: 'custom', message: 'Start date is required', path: ['startDate'] })
  }
  if (values.endDate === null) {
    ctx.addIssue({ code: 'custom', message: 'End date is required', path: ['endDate'] })
  }

  if (!values.startDate || !values.endDate) return

  const startAt = new Date(values.startDate.getFullYear(), values.startDate.getMonth(), values.startDate.getDate()).getTime()
  const endAt = new Date(values.endDate.getFullYear(), values.endDate.getMonth(), values.endDate.getDate()).getTime()

  if (endAt <= startAt) {
    ctx.addIssue({
      code: 'custom',
      message: 'End date must be after start date',
      path: ['endDate'],
    })
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
    name: '',
    year: null as number | null,
    startDate: null as Date | null,
    endDate: null as Date | null,
  },
  FormSchema,
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

function toDate(value: string | null | undefined) {
  if (!value) return null
  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime()) ? null : parsed
}

function formatDate(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleDateString()
}

async function loadAcademicYears() {
  isLoading.value = true
  try {
    const response = await api.get<AcademicYear[]>(API.academicYears.base)
    if (response.success) {
      academicYears.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load academic years')
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load academic years')
  }
  finally {
    isLoading.value = false
  }
}

function openCreateDialog() {
  isEditing.value = false
  resetForm()
  isDialogVisible.value = true
}

function openEditDialog(item: AcademicYear) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.name = item.name
  form.year = item.year
  form.startDate = toDate(item.startDate)
  form.endDate = toDate(item.endDate)
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (!parsed.data.startDate || !parsed.data.endDate || parsed.data.year === null) {
    setGlobalError('Invalid form payload')
    return
  }

  const payload = {
    name: parsed.data.name,
    year: parsed.data.year,
    startDate: parsed.data.startDate.toISOString(),
    endDate: parsed.data.endDate.toISOString(),
  }

  const requestSchema = isEditing.value
    ? UpdateAcademicYearRequestSchema
    : CreateAcademicYearRequestSchema

  const requestCheck = requestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<AcademicYear>(
        API.academicYears.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Academic year updated')
        closeDialog()
        await loadAcademicYears()
        return
      }
      setGlobalError(response.message ?? 'Failed to update academic year')
      return
    }

    const response = await api.post<AcademicYear>(
      API.academicYears.base,
      requestCheck.data,
    )
    if (response.success) {
      showSuccess('Academic year created')
      closeDialog()
      await loadAcademicYears()
      return
    }
    setGlobalError(response.message ?? 'Failed to create academic year')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleActivate(item: AcademicYear) {
  confirmAction(
    `Set "${item.name}" as the active academic year?`,
    async () => {
      try {
        const response = await api.post<string>(API.academicYears.activate(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Academic year activated')
          await loadAcademicYears()
          return
        }
        showError(response.message ?? 'Failed to activate academic year')
      }
      catch (error: unknown) {
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to activate academic year')
      }
    },
    'Activate Academic Year',
    'Activate',
    'Cancel',
  )
}

function handleDelete(item: AcademicYear) {
  confirmDelete(
    `Delete "${item.name}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.academicYears.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Academic year deleted')
          await loadAcademicYears()
          return
        }
        showError(response.message ?? 'Failed to delete academic year')
      }
      catch (error: unknown) {
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to delete academic year')
      }
    },
  )
}

onMounted(async () => {
  await loadAcademicYears()
})

watch(
  () => form.startDate,
  (startDate) => {
    if (startDate) {
      form.year = startDate.getFullYear()
    }
  },
)
</script>
