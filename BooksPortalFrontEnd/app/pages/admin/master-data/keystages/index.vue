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
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit keystage"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete keystage"
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
import type { Keystage } from '~/types/entities'
import { CreateKeystageRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'keystages': 'Keystages',
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
    sortOrder: null as number | null,
  },
  CreateKeystageRequestSchema.extend({
    sortOrder: CreateKeystageRequestSchema.shape.sortOrder.nullable(),
  }).superRefine((values, ctx) => {
    if (values.sortOrder === null) {
      ctx.addIssue({
        code: 'custom',
        message: 'Sort order is required',
        path: ['sortOrder'],
      })
    }
  }),
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
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
  setGlobalError('')
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
  if (parsed.data.sortOrder === null) {
    setGlobalError('Invalid form payload')
    return
  }

  const requestCheck = CreateKeystageRequestSchema.safeParse({
    ...parsed.data,
    sortOrder: parsed.data.sortOrder,
  })
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
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
      setGlobalError(response.message ?? 'Failed to update keystage')
      return
    }

    const response = await api.post<Keystage>(API.keystages.base, requestCheck.data)
    if (response.success) {
      showSuccess('Keystage created')
      closeDialog()
      await loadKeystages()
      return
    }
    setGlobalError(response.message ?? 'Failed to create keystage')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
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
