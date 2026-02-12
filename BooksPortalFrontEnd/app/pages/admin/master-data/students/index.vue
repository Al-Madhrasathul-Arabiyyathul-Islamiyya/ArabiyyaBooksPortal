<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Students
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage students and link parents to each student.
        </p>
      </div>
      <div class="flex items-center gap-2">
        <Button
          label="Bulk Import"
          icon="pi pi-file-import"
          severity="secondary"
          outlined
          @click="isBulkDialogVisible = true"
        />
        <Button
          label="New Student"
          icon="pi pi-plus"
          @click="openCreateDialog"
        />
      </div>
    </div>

    <Card>
      <template #content>
        <div class="mb-4 grid grid-cols-1 items-end gap-4 lg:grid-cols-3">
          <FormsFormField
            label="Search"
            field-id="studentSearch"
          >
            <FormsSearchInput
              id="studentSearch"
              v-model="searchTerm"
              name="students-search"
              persist-key="bp.search.admin.students"
              placeholder="Search by name or index number"
              @search="handleSearch"
            />
          </FormsFormField>

          <FormsFormField
            label="Class Filter"
            field-id="classFilter"
          >
            <Select
              id="classFilter"
              v-model="selectedClassSectionFilter"
              :options="classSectionFilterOptions"
              option-label="label"
              option-value="value"
              fluid
              @change="handleFilterChange"
            />
          </FormsFormField>
        </div>

        <DataTable
          :value="students"
          :loading="isLoading"
          data-key="id"
          paginator
          lazy
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          :rows-per-page-options="[10, 20, 50, 100]"
          size="small"
          @page="onPageAndLoad"
        >
          <Column
            field="fullName"
            header="Full Name"
          />
          <Column
            field="indexNo"
            header="Index No"
          />
          <Column
            field="nationalId"
            header="National ID"
          />
          <Column
            field="classSectionDisplayName"
            header="Class"
          />
          <Column header="Parents">
            <template #body="{ data }">
              <div
                v-if="data.parents?.length"
                class="flex flex-wrap gap-2"
              >
                <Tag
                  v-for="parent in data.parents"
                  :key="`${data.id}-${parent.parentId}`"
                  :value="`${parent.fullName}${parent.isPrimary ? ' (Primary)' : ''}`"
                  :severity="parent.isPrimary ? 'success' : 'secondary'"
                />
              </div>
              <span
                v-else
                class="text-xs text-surface-500"
              >No linked parents</span>
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
                  tooltip="Edit student"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete student"
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
      :header="isEditing ? 'Edit Student' : 'Create Student'"
      :style="{ width: '52rem' }"
    >
      <form
        class="flex flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
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
            label="Index No"
            required
            field-id="indexNo"
            :error="errors.indexNo"
          >
            <InputText
              id="indexNo"
              v-model.trim="form.indexNo"
              fluid
              :invalid="!!errors.indexNo"
              :disabled="isEditing"
              @blur="touchField('indexNo')"
            />
          </FormsFormField>
        </div>

        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
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
            label="Class"
            required
            field-id="classSectionId"
            :error="errors.classSectionId"
          >
            <Select
              id="classSectionId"
              v-model="form.classSectionId"
              :options="classSectionOptions"
              option-label="label"
              option-value="value"
              placeholder="Select class"
              fluid
              :invalid="!!errors.classSectionId"
              @blur="touchField('classSectionId')"
            />
          </FormsFormField>
        </div>

        <div class="rounded-lg border border-surface-200 p-4 dark:border-surface-700">
          <div class="mb-3 flex items-center justify-between gap-4">
            <h3 class="text-sm font-semibold text-surface-800 dark:text-surface-100">
              Linked Parents
            </h3>
            <div class="flex w-full max-w-xl items-end gap-2">
              <FormsFormField
                class="flex-1"
                label="Search parent and add"
                field-id="parentSearch"
              >
                <AutoComplete
                  id="parentSearch"
                  v-model="selectedParentCandidate"
                  :suggestions="parentSuggestions"
                  option-label="fullName"
                  dropdown
                  force-selection
                  fluid
                  @complete="searchParents"
                >
                  <template #option="{ option }">
                    <div class="flex flex-col">
                      <span>{{ option.fullName }}</span>
                      <small class="text-surface-500">
                        {{ option.nationalId }}{{ option.relationship ? ` • ${option.relationship}` : '' }}
                      </small>
                    </div>
                  </template>
                </AutoComplete>
              </FormsFormField>
              <Button
                type="button"
                label="Add"
                icon="pi pi-plus"
                :disabled="!selectedParentCandidate"
                @click="addSelectedParent"
              />
            </div>
          </div>

          <DataTable
            :value="linkedParents"
            data-key="parentId"
            size="small"
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
              field="relationship"
              header="Relationship"
            />
            <Column header="Primary">
              <template #body="{ data }">
                <Checkbox
                  :model-value="data.isPrimary"
                  binary
                  @update:model-value="setPrimaryParent(data.parentId, $event)"
                />
              </template>
            </Column>
            <Column
              header="Actions"
              :exportable="false"
              style="width: 5rem;"
            >
              <template #body="{ data }">
                <CommonIconActionButton
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Remove parent link"
                  @click="removeLinkedParent(data.parentId)"
                />
              </template>
            </Column>
          </DataTable>
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

    <FormsBulkImportDialog
      v-model:visible="isBulkDialogVisible"
      entity-label="Students"
      :report="bulkReport"
      :error-message="bulkError"
      :is-validating="isBulkValidating"
      :is-committing="isBulkCommitting"
      @template="downloadStudentTemplate"
      @validate="validateStudentBulk"
      @commit="commitStudentBulk"
    />
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { BulkImportReport, PaginatedList } from '~/types/api'
import type {
  ClassSection,
  Parent,
  Student,
  StudentParent,
} from '~/types/entities'
import {
  CreateStudentRequestSchema,
  UpdateStudentRequestSchema,
} from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

interface OptionItem {
  label: string
  value: number
}

interface FilterOption {
  label: string
  value: number | null
}

interface LinkedParent extends StudentParent {
  parentId: number
}

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'students': 'Students',
  },
})

const api = useApi()
const bulkImport = useBulkImport()
const { showError, showSuccess, showInfo } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const students = ref<Student[]>([])
const classSections = ref<ClassSection[]>([])
const parentSuggestions = ref<Parent[]>([])
const activeAcademicYearId = ref<number | null>(null)

const searchTerm = ref('')
const selectedClassSectionFilter = ref<number | null>(null)
const selectedParentCandidate = ref<Parent | null>(null)

const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isBulkDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)
const isBulkValidating = ref(false)
const isBulkCommitting = ref(false)
const bulkError = ref('')
const bulkReport = ref<BulkImportReport | null>(null)

const linkedParents = ref<LinkedParent[]>([])

const classSectionFilterOptions = computed<FilterOption[]>(() => [
  { label: 'All Classes', value: null },
  ...classSections.value.map(section => ({
    label: section.displayName,
    value: section.id,
  })),
])

const classSectionOptions = computed<OptionItem[]>(() =>
  classSections.value.map(section => ({
    label: section.displayName,
    value: section.id,
  })),
)

const StudentValidationSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  indexNo: z.string().optional(),
  nationalId: z.string().min(1, 'National ID is required'),
  classSectionId: z.number().int().min(1, 'Class is required').nullable(),
}).superRefine((values, ctx) => {
  if (!isEditing.value && !values.indexNo?.trim()) {
    ctx.addIssue({ code: 'custom', message: 'Index number is required', path: ['indexNo'] })
  }
  if (values.classSectionId === null) {
    ctx.addIssue({ code: 'custom', message: 'Class is required', path: ['classSectionId'] })
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
    fullName: '',
    indexNo: '',
    nationalId: '',
    classSectionId: null as number | null,
  },
  StudentValidationSchema,
)

function resetForm() {
  resetValidationForm()
  linkedParents.value = []
  selectedParentCandidate.value = null
  selectedId.value = null
}

async function loadClassSections() {
  try {
    const response = await api.get<ClassSection[]>(API.classSections.base, {
      academicYearId: activeAcademicYearId.value ?? undefined,
    })
    if (response.success) {
      classSections.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load classes')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load classes'))
  }
}

async function loadActiveAcademicYear() {
  try {
    const response = await api.get<{ id: number }>(API.academicYears.active)
    if (response.success) {
      activeAcademicYearId.value = response.data.id
    }
  }
  catch {
    // no active year configured yet
  }
}

async function loadStudents() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<Student>>(API.students.base, {
      ...queryParams.value,
      search: searchTerm.value || undefined,
      classSectionId: selectedClassSectionFilter.value || undefined,
    })
    if (response.success) {
      students.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load students')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load students'))
  }
  finally {
    isLoading.value = false
  }
}

async function searchParents(event: { query: string }) {
  try {
    const response = await api.get<PaginatedList<Parent>>(API.parents.base, {
      pageNumber: 1,
      pageSize: 20,
      search: event.query || undefined,
    })
    if (response.success) {
      parentSuggestions.value = response.data.items
      return
    }
    parentSuggestions.value = []
  }
  catch {
    parentSuggestions.value = []
  }
}

function addSelectedParent() {
  if (!selectedParentCandidate.value) return

  const candidate = selectedParentCandidate.value
  const exists = linkedParents.value.some(parent => parent.parentId === candidate.id)
  if (exists) {
    showInfo('Parent is already linked')
    return
  }

  linkedParents.value.push({
    parentId: candidate.id,
    fullName: candidate.fullName,
    nationalId: candidate.nationalId,
    phone: candidate.phone,
    relationship: candidate.relationship,
    isPrimary: linkedParents.value.length === 0,
  })

  selectedParentCandidate.value = null
}

function setPrimaryParent(parentId: number, checked: boolean) {
  if (!checked) {
    const currentPrimary = linkedParents.value.find(parent => parent.isPrimary)
    if (currentPrimary?.parentId === parentId) {
      currentPrimary.isPrimary = false
    }
    return
  }

  linkedParents.value = linkedParents.value.map(parent => ({
    ...parent,
    isPrimary: parent.parentId === parentId,
  }))
}

function removeLinkedParent(parentId: number) {
  const removed = linkedParents.value.find(parent => parent.parentId === parentId)
  linkedParents.value = linkedParents.value.filter(parent => parent.parentId !== parentId)

  if (removed?.isPrimary && linkedParents.value.length > 0) {
    linkedParents.value[0]!.isPrimary = true
  }
}

async function onPageAndLoad(event: { page: number, rows: number }) {
  onPage(event)
  await loadStudents()
}

async function handleSearch() {
  reset()
  await loadStudents()
}

async function handleFilterChange() {
  reset()
  await loadStudents()
}

async function downloadStudentTemplate() {
  await bulkImport.downloadTemplate({
    validate: API.students.bulkValidate,
    commit: API.students.bulkCommit,
    template: API.importTemplates.students,
    templateFileName: 'students-import-template.xlsx',
  })
}

async function validateStudentBulk(file: File) {
  bulkError.value = ''
  isBulkValidating.value = true
  try {
    const response = await bulkImport.validateFile(file, {
      validate: API.students.bulkValidate,
      commit: API.students.bulkCommit,
      template: API.importTemplates.students,
      templateFileName: 'students-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Student import validation complete')
      return
    }
    bulkError.value = response.message ?? 'Validation failed'
  }
  catch (error: unknown) {
    bulkError.value = getFriendlyErrorMessage(error, 'Validation failed')
  }
  finally {
    isBulkValidating.value = false
  }
}

async function commitStudentBulk(file: File) {
  bulkError.value = ''
  isBulkCommitting.value = true
  try {
    const response = await bulkImport.commitFile(file, {
      validate: API.students.bulkValidate,
      commit: API.students.bulkCommit,
      template: API.importTemplates.students,
      templateFileName: 'students-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Student import committed')
      await loadStudents()
      return
    }
    bulkError.value = response.message ?? 'Commit failed'
  }
  catch (error: unknown) {
    bulkError.value = getFriendlyErrorMessage(error, 'Commit failed')
  }
  finally {
    isBulkCommitting.value = false
  }
}

function openCreateDialog() {
  isEditing.value = false
  resetForm()
  isDialogVisible.value = true
}

function openEditDialog(item: Student) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.fullName = item.fullName
  form.indexNo = item.indexNo
  form.nationalId = item.nationalId ?? ''
  form.classSectionId = item.classSectionId
  linkedParents.value = item.parents.map(parent => ({ ...parent }))
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (parsed.data.classSectionId === null) {
    setGlobalError('Invalid form payload')
    return
  }

  if (isEditing.value) {
    const requestCheck = UpdateStudentRequestSchema.safeParse({
      fullName: parsed.data.fullName,
      nationalId: parsed.data.nationalId,
      classSectionId: parsed.data.classSectionId,
      parents: linkedParents.value.map(parent => ({
        parentId: parent.parentId,
        isPrimary: parent.isPrimary,
      })),
    })

    if (!requestCheck.success || !selectedId.value) {
      setGlobalError('Invalid form payload')
      return
    }

    isSubmitting.value = true
    try {
      const response = await api.put<Student>(API.students.byId(selectedId.value), requestCheck.data)
      if (response.success) {
        showSuccess('Student updated')
        closeDialog()
        await loadStudents()
        return
      }
      setGlobalError(response.message ?? 'Failed to update student')
    }
    catch (error: unknown) {
      applyBackendErrors(error)
    }
    finally {
      isSubmitting.value = false
    }
    return
  }

  const requestCheck = CreateStudentRequestSchema.safeParse({
    fullName: parsed.data.fullName,
    indexNo: parsed.data.indexNo ?? '',
    nationalId: parsed.data.nationalId,
    classSectionId: parsed.data.classSectionId,
    parents: linkedParents.value.map(parent => ({
      parentId: parent.parentId,
      isPrimary: parent.isPrimary,
    })),
  })
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    const response = await api.post<Student>(API.students.base, requestCheck.data)
    if (response.success) {
      showSuccess('Student created')
      closeDialog()
      await loadStudents()
      return
    }
    setGlobalError(response.message ?? 'Failed to create student')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Student) {
  confirmDelete(
    `Delete "${item.fullName}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.students.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Student deleted')
          await loadStudents()
          return
        }
        showError(response.message ?? 'Failed to delete student')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete student'))
      }
    },
  )
}

onMounted(async () => {
  await loadActiveAcademicYear()
  await Promise.all([
    loadClassSections(),
    loadStudents(),
  ])
})
</script>
