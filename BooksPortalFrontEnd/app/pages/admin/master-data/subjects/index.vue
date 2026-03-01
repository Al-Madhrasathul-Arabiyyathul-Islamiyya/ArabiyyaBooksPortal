<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Subjects
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage subject definitions used in classes and book cataloging.
        </p>
      </div>
      <Button
        label="New Subject"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <CommonAdminDataTable
          :value="subjects"
          :loading="isLoading"
          data-key="id"
          size="small"
        >
          <Column
            field="code"
            header="Code"
          />
          <Column
            field="name"
            header="Name"
          />
          <Column
            header="Actions"
            :exportable="false"
            style="width: 12rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit subject"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete subject"
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
      :header="isEditing ? 'Edit Subject' : 'Create Subject'"
      :style="{ width: '30rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
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
import type { Subject } from '~/types/entities'
import { CreateSubjectRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'subjects': 'Subjects',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const subjects = ref<Subject[]>([])
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
    code: '',
    name: '',
  },
  CreateSubjectRequestSchema,
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

async function loadSubjects() {
  isLoading.value = true
  try {
    const response = await api.get<Subject[]>(API.subjects.base)
    if (response.success) {
      subjects.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load subjects')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load subjects'))
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

function openEditDialog(item: Subject) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.code = item.code
  form.name = item.name
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Subject>(
        API.subjects.byId(selectedId.value),
        parsed.data,
      )
      if (response.success) {
        showSuccess('Subject updated')
        closeDialog()
        await loadSubjects()
        return
      }
      setGlobalError(response.message ?? 'Failed to update subject')
      return
    }

    const response = await api.post<Subject>(API.subjects.base, parsed.data)
    if (response.success) {
      showSuccess('Subject created')
      closeDialog()
      await loadSubjects()
      return
    }
    setGlobalError(response.message ?? 'Failed to create subject')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Subject) {
  confirmDelete(
    `Delete "${item.name}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.subjects.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Subject deleted')
          await loadSubjects()
          return
        }
        showError(response.message ?? 'Failed to delete subject')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete subject'))
      }
    },
  )
}

onMounted(async () => {
  await loadSubjects()
})
</script>
