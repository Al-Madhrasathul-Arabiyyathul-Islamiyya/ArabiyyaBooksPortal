<template>
  <div class="flex flex-col gap-4">
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
            v-model="searchTerm"
            placeholder="Search by name or national ID"
            @search="handleSearch"
          />
        </div>

        <DataTable
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
      :header="isEditing ? 'Edit Parent' : 'Create Parent'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex flex-col gap-4"
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
const formError = ref('')

const form = reactive({
  fullName: '',
  nationalId: '',
  phone: '',
  relationship: '',
})

const errors = reactive({
  fullName: '',
  nationalId: '',
  phone: '',
  relationship: '',
})

const FormSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().min(1, 'National ID is required'),
  phone: z.string().optional(),
  relationship: z.string().optional(),
})

function clearErrors() {
  errors.fullName = ''
  errors.nationalId = ''
  errors.phone = ''
  errors.relationship = ''
  formError.value = ''
}

function resetForm() {
  form.fullName = ''
  form.nationalId = ''
  form.phone = ''
  form.relationship = ''
  selectedId.value = null
  clearErrors()
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
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load parents')
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
  clearErrors()
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

function mapValidationErrors(result: Extract<ReturnType<typeof FormSchema.safeParse>, { success: false }>) {
  clearErrors()
  for (const issue of result.error.issues) {
    const field = issue.path[0]
    if (field === 'fullName') errors.fullName = issue.message
    if (field === 'nationalId') errors.nationalId = issue.message
  }
}

async function handleSubmit() {
  clearErrors()
  const parsed = FormSchema.safeParse({
    fullName: form.fullName,
    nationalId: form.nationalId,
    phone: form.phone,
    relationship: form.relationship,
  })

  if (!parsed.success) {
    mapValidationErrors(parsed)
    return
  }

  const requestCheck = CreateParentRequestSchema.safeParse({
    ...parsed.data,
    phone: normalizeNullable(parsed.data.phone ?? ''),
    relationship: normalizeNullable(parsed.data.relationship ?? ''),
  })
  if (!requestCheck.success) {
    formError.value = 'Invalid form payload'
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
      formError.value = response.message ?? 'Failed to update parent'
      return
    }

    const response = await api.post<Parent>(API.parents.base, requestCheck.data)
    if (response.success) {
      showSuccess('Parent created')
      closeDialog()
      await loadParents()
      return
    }
    formError.value = response.message ?? 'Failed to create parent'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    formError.value = fetchError.data?.message ?? 'Unable to save parent'
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
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to delete parent')
      }
    },
  )
}

onMounted(async () => {
  await loadParents()
})
</script>
