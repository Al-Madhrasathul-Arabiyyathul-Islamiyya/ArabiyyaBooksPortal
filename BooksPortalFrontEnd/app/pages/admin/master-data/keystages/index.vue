<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Keystages
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage keystage definitions used by class sections.
        </p>
      </div>
      <Button
        label="New Keystage"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <DataTable
          :value="keystages"
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
      :header="isEditing ? 'Edit Keystage' : 'Create Keystage'"
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
import type { Keystage } from '~/types/entities'
import { CreateKeystageRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    'master-data': 'Master Data',
    keystages: 'Keystages',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const keystages = ref<Keystage[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)
const formError = ref('')

const form = reactive({
  code: '',
  name: '',
  sortOrder: null as number | null,
})

const errors = reactive({
  code: '',
  name: '',
  sortOrder: '',
})

const FormSchema = z.object({
  code: z.string().min(1, 'Code is required'),
  name: z.string().min(1, 'Name is required'),
  sortOrder: z.number().int().min(0, 'Sort order must be 0 or greater'),
})

function clearErrors() {
  errors.code = ''
  errors.name = ''
  errors.sortOrder = ''
  formError.value = ''
}

function resetForm() {
  form.code = ''
  form.name = ''
  form.sortOrder = null
  selectedId.value = null
  clearErrors()
}

async function loadKeystages() {
  isLoading.value = true
  try {
    const response = await api.get<Keystage[]>(API.keystages.base)
    if (response.success) {
      keystages.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load keystages')
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load keystages')
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

function openEditDialog(item: Keystage) {
  isEditing.value = true
  selectedId.value = item.id
  clearErrors()
  form.code = item.code
  form.name = item.name
  form.sortOrder = item.sortOrder
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
    if (field === 'sortOrder') errors.sortOrder = issue.message
  }
}

async function handleSubmit() {
  clearErrors()
  const parsed = FormSchema.safeParse({
    code: form.code,
    name: form.name,
    sortOrder: form.sortOrder,
  })

  if (!parsed.success) {
    mapValidationErrors(parsed)
    return
  }

  const requestCheck = CreateKeystageRequestSchema.safeParse(parsed.data)
  if (!requestCheck.success) {
    formError.value = 'Invalid form payload'
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Keystage>(
        API.keystages.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Keystage updated')
        closeDialog()
        await loadKeystages()
        return
      }
      formError.value = response.message ?? 'Failed to update keystage'
      return
    }

    const response = await api.post<Keystage>(API.keystages.base, requestCheck.data)
    if (response.success) {
      showSuccess('Keystage created')
      closeDialog()
      await loadKeystages()
      return
    }
    formError.value = response.message ?? 'Failed to create keystage'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    formError.value = fetchError.data?.message ?? 'Unable to save keystage'
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Keystage) {
  confirmDelete(
    `Delete "${item.name}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.keystages.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Keystage deleted')
          await loadKeystages()
          return
        }
        showError(response.message ?? 'Failed to delete keystage')
      }
      catch (error: unknown) {
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to delete keystage')
      }
    },
  )
}

onMounted(async () => {
  await loadKeystages()
})
</script>
