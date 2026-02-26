<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Grades
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage grade definitions by keystage.
        </p>
      </div>
      <Button
        v-if="isSuperAdmin"
        label="New Grade"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <CommonAdminDataTable
          :value="grades"
          :loading="isLoading"
          data-key="id"
          size="small"
        >
          <Column
            field="keystageName"
            header="Keystage"
          />
          <Column
            field="code"
            header="Code"
          />
          <Column
            field="name"
            header="Name"
          />
          <Column
            field="sortOrder"
            header="Sort Order"
          />
          <Column
            header="Actions"
            :exportable="false"
            style="width: 12rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-pencil"
                  tooltip="Edit grade"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete grade"
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
      :header="isEditing ? 'Edit Grade' : 'Create Grade'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <FormsFormField
          label="Keystage"
          required
          field-id="keystageId"
          :error="errors.keystageId"
        >
          <Select
            id="keystageId"
            v-model="form.keystageId"
            :options="keystageOptions"
            option-label="label"
            option-value="value"
            fluid
            :invalid="!!errors.keystageId"
            @blur="touchField('keystageId')"
          />
        </FormsFormField>

        <FormsFormField
          label="Code"
          required
          field-id="code"
          :error="errors.code"
        >
          <InputText
            id="code"
            v-model.trim="form.code"
            fluid
            :invalid="!!errors.code"
            @blur="touchField('code')"
          />
        </FormsFormField>

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
          label="Sort Order"
          required
          field-id="sortOrder"
          :error="errors.sortOrder"
        >
          <InputNumber
            id="sortOrder"
            v-model="form.sortOrder"
            :min="0"
            fluid
            :invalid="!!errors.sortOrder"
            @blur="touchField('sortOrder')"
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
import type { Grade, Keystage } from '~/types/entities'
import { CreateGradeRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'grades': 'Grades',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const grades = ref<Grade[]>([])
const keystages = ref<Keystage[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)

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
    keystageId: null as number | null,
    code: '',
    name: '',
    sortOrder: null as number | null,
  },
  CreateGradeRequestSchema.extend({
    keystageId: CreateGradeRequestSchema.shape.keystageId.nullable(),
    sortOrder: CreateGradeRequestSchema.shape.sortOrder.nullable(),
  }).superRefine((values, ctx) => {
    if (values.keystageId === null) {
      ctx.addIssue({ code: 'custom', message: 'Keystage is required', path: ['keystageId'] })
    }
    if (values.sortOrder === null) {
      ctx.addIssue({ code: 'custom', message: 'Sort order is required', path: ['sortOrder'] })
    }
  }),
)

const keystageOptions = computed(() =>
  keystages.value.map(keystage => ({
    label: keystage.name,
    value: keystage.id,
  })),
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

async function loadLookupData() {
  const response = await api.get<Keystage[]>(API.keystages.base)
  if (response.success) {
    keystages.value = response.data
    return
  }
  showError(response.message ?? 'Failed to load keystages')
}

async function loadGrades() {
  isLoading.value = true
  try {
    const response = await api.get<Grade[]>(API.grades.base)
    if (response.success) {
      grades.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load grades')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load grades'))
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

function openEditDialog(item: Grade) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.keystageId = item.keystageId
  form.code = item.code
  form.name = item.name
  form.sortOrder = item.sortOrder
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (parsed.data.keystageId === null || parsed.data.sortOrder === null) {
    setGlobalError('Invalid form payload')
    return
  }

  const payload = {
    keystageId: parsed.data.keystageId,
    code: parsed.data.code,
    name: parsed.data.name,
    sortOrder: parsed.data.sortOrder,
  }
  const requestCheck = CreateGradeRequestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Grade>(
        API.grades.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Grade updated')
        closeDialog()
        await loadGrades()
        return
      }
      setGlobalError(response.message ?? 'Failed to update grade')
      return
    }

    const response = await api.post<Grade>(API.grades.base, requestCheck.data)
    if (response.success) {
      showSuccess('Grade created')
      closeDialog()
      await loadGrades()
      return
    }
    setGlobalError(response.message ?? 'Failed to create grade')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Grade) {
  confirmDelete(
    `Delete "${item.name}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.grades.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Grade deleted')
          await loadGrades()
          return
        }
        showError(response.message ?? 'Failed to delete grade')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete grade'))
      }
    },
  )
}

onMounted(async () => {
  await Promise.all([loadLookupData(), loadGrades()])
})
</script>
