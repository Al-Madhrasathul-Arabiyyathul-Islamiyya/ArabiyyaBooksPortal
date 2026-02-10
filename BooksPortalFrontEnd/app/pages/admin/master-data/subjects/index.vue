<template>
  <div class="flex flex-col gap-4">
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
        <DataTable
          :value="subjects"
          :loading="isLoading"
          data-key="id"
          size="small"
          striped-rows
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
                <Button
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Edit"
                  @click="openEditDialog(data)"
                />
                <Button
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  aria-label="Delete"
                  @click="handleDelete(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="isDialogVisible"
      modal
      :header="isEditing ? 'Edit Subject' : 'Create Subject'"
      :style="{ width: '30rem' }"
    >
      <form
        class="flex flex-col gap-4"
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
import { z } from 'zod/v4'
import type { Subject } from '~/types/entities'
import { CreateSubjectRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    'master-data': 'Master Data',
    subjects: 'Subjects',
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
const formError = ref('')

const form = reactive({
  code: '',
  name: '',
})

const errors = reactive({
  code: '',
  name: '',
})

const FormSchema = z.object({
  code: z.string().min(1, 'Code is required'),
  name: z.string().min(1, 'Name is required'),
})

function clearErrors() {
  errors.code = ''
  errors.name = ''
  formError.value = ''
}

function resetForm() {
  form.code = ''
  form.name = ''
  selectedId.value = null
  clearErrors()
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
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load subjects')
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
  clearErrors()
  form.code = item.code
  form.name = item.name
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

function mapValidationErrors(result: z.SafeParseError<z.infer<typeof FormSchema>>) {
  clearErrors()
  for (const issue of result.error.issues) {
    const field = issue.path[0]
    if (field === 'code') errors.code = issue.message
    if (field === 'name') errors.name = issue.message
  }
}

async function handleSubmit() {
  clearErrors()
  const parsed = FormSchema.safeParse({
    code: form.code,
    name: form.name,
  })

  if (!parsed.success) {
    mapValidationErrors(parsed)
    return
  }

  const requestCheck = CreateSubjectRequestSchema.safeParse(parsed.data)
  if (!requestCheck.success) {
    formError.value = 'Invalid form payload'
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Subject>(
        API.subjects.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Subject updated')
        closeDialog()
        await loadSubjects()
        return
      }
      formError.value = response.message ?? 'Failed to update subject'
      return
    }

    const response = await api.post<Subject>(API.subjects.base, requestCheck.data)
    if (response.success) {
      showSuccess('Subject created')
      closeDialog()
      await loadSubjects()
      return
    }
    formError.value = response.message ?? 'Failed to create subject'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    formError.value = fetchError.data?.message ?? 'Unable to save subject'
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
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to delete subject')
      }
    },
  )
}

onMounted(async () => {
  await loadSubjects()
})
</script>
