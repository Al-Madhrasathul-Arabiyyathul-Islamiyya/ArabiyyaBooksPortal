<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Parents
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage parent/guardian records used in student distribution flows.
        </p>
      </div>
      <Button
        label="New Parent"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <div class="mb-4 max-w-lg">
          <FormsSearchInput
            id="parents-search"
            v-model="searchTerm"
            name="parents-search"
            persist-key="bp.search.admin.parents"
            placeholder="Search by name or national ID"
            @search="handleSearch"
          />
        </div>

        <CommonAdminDataTable
          :value="parents"
          :loading="isLoading"
          data-key="id"
          paginator
          lazy
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          size="small"
          @page="onPageAndLoad"
        >
          <Column
            field="fullName"
            header="Full Name"
          />
          <Column
            field="nationalId"
            header="National ID"
          />
          <Column
            field="phone"
            header="Phone"
          />
          <Column
            field="relationship"
            header="Relationship"
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
                  tooltip="Edit parent"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete parent"
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
      :header="isEditing ? 'Edit Parent' : 'Create Parent'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <FormsFormField
          label="Full Name"
          required
          field-id="fullName"
          :error="errors.fullName"
        >
          <InputText
            id="fullName"
            v-model.trim="form.fullName"
            fluid
            :invalid="!!errors.fullName"
            @blur="touchField('fullName')"
          />
        </FormsFormField>

        <FormsFormField
          label="National ID"
          required
          field-id="nationalId"
          :error="errors.nationalId"
        >
          <InputText
            id="nationalId"
            v-model.trim="form.nationalId"
            fluid
            :invalid="!!errors.nationalId"
            @blur="touchField('nationalId')"
          />
        </FormsFormField>

        <FormsFormField
          label="Phone"
          field-id="phone"
          :error="errors.phone"
        >
          <InputText
            id="phone"
            v-model.trim="form.phone"
            fluid
            :invalid="!!errors.phone"
            @blur="touchField('phone')"
          />
        </FormsFormField>

        <FormsFormField
          label="Relationship"
          field-id="relationship"
          :error="errors.relationship"
        >
          <InputText
            id="relationship"
            v-model.trim="form.relationship"
            fluid
            :invalid="!!errors.relationship"
            @blur="touchField('relationship')"
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
import type { PaginatedList } from '~/types/api'
import type { Parent } from '~/types/entities'
import { CreateParentRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'parents': 'Parents',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const parents = ref<Parent[]>([])
const searchTerm = ref('')

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
    fullName: '',
    nationalId: '',
    phone: '',
    relationship: '',
  },
  CreateParentRequestSchema.extend({
    phone: z.string().optional(),
    relationship: z.string().optional(),
  }),
)

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

function normalizeNullable(value: string) {
  const trimmed = value.trim()
  return trimmed.length ? trimmed : null
}

async function loadParents() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<Parent>>(API.parents.base, {
      ...queryParams.value,
      search: searchTerm.value || undefined,
    })
    if (response.success) {
      parents.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load parents')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load parents'))
  }
  finally {
    isLoading.value = false
  }
}

async function onPageAndLoad(event: { page: number, rows: number }) {
  onPage(event)
  await loadParents()
}

async function handleSearch() {
  reset()
  await loadParents()
}

function openCreateDialog() {
  isEditing.value = false
  resetForm()
  isDialogVisible.value = true
}

function openEditDialog(item: Parent) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.fullName = item.fullName
  form.nationalId = item.nationalId
  form.phone = item.phone ?? ''
  form.relationship = item.relationship ?? ''
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  const requestPayload = {
    ...parsed.data,
    phone: normalizeNullable(parsed.data.phone ?? ''),
    relationship: normalizeNullable(parsed.data.relationship ?? ''),
  }

  const requestCheck = CreateParentRequestSchema.safeParse(requestPayload)
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Parent>(API.parents.byId(selectedId.value), requestCheck.data)
      if (response.success) {
        showSuccess('Parent updated')
        closeDialog()
        await loadParents()
        return
      }
      setGlobalError(response.message ?? 'Failed to update parent')
      return
    }

    const response = await api.post<Parent>(API.parents.base, requestCheck.data)
    if (response.success) {
      showSuccess('Parent created')
      closeDialog()
      await loadParents()
      return
    }
    setGlobalError(response.message ?? 'Failed to create parent')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Parent) {
  confirmDelete(
    `Delete "${item.fullName}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.parents.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Parent deleted')
          await loadParents()
          return
        }
        showError(response.message ?? 'Failed to delete parent')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete parent'))
      }
    },
  )
}

onMounted(async () => {
  await loadParents()
})
</script>
