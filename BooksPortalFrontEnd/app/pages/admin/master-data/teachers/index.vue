<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Teachers
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage teachers and their subject/class assignments.
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
          label="New Teacher"
          icon="pi pi-plus"
          @click="openCreateDialog"
        />
      </div>
    </div>

    <Card>
      <template #content>
        <div class="mb-4 max-w-lg">
          <FormsSearchInput
            id="teachers-search"
            v-model="searchTerm"
            name="teachers-search"
            persist-key="bp.search.admin.teachers"
            placeholder="Search by name or national ID"
            @search="handleSearch"
          />
        </div>

        <DataTable
          :value="teachers"
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
            field="email"
            header="Email"
          />
          <Column
            field="phone"
            header="Phone"
          />
          <Column header="Assignments">
            <template #body="{ data }">
              <Tag
                :value="String(data.assignments?.length ?? 0)"
                severity="info"
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
                  icon="pi pi-sitemap"
                  severity="info"
                  tooltip="Manage assignments"
                  @click="openAssignmentsDialog(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit teacher"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete teacher"
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
      :header="isEditing ? 'Edit Teacher' : 'Create Teacher'"
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
          label="Email"
          field-id="email"
          :error="errors.email"
        >
          <InputText
            id="email"
            v-model.trim="form.email"
            fluid
            :invalid="!!errors.email"
            @blur="touchField('email')"
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

    <Dialog
      v-model:visible="isAssignmentsDialogVisible"
      modal
      :header="`Manage Assignments - ${selectedTeacherName}`"
      :style="{ width: '52rem' }"
    >
      <div class="flex flex-col gap-4">
        <div class="grid grid-cols-1 gap-4 md:grid-cols-[1fr_1fr_auto] md:items-end">
          <FormsFormField
            label="Subject"
            field-id="subjectId"
            :error="assignmentErrors.subjectId"
          >
            <Select
              id="subjectId"
              v-model="assignmentForm.subjectId"
              :options="subjectOptions"
              option-label="label"
              option-value="value"
              placeholder="Select subject"
              fluid
              :invalid="!!assignmentErrors.subjectId"
              @blur="touchAssignmentField('subjectId')"
            />
          </FormsFormField>

          <FormsFormField
            label="Class"
            field-id="assignmentClassSectionId"
            :error="assignmentErrors.classSectionId"
          >
            <Select
              id="assignmentClassSectionId"
              v-model="assignmentForm.classSectionId"
              :options="classSectionOptions"
              option-label="label"
              option-value="value"
              placeholder="Select class"
              fluid
              :invalid="!!assignmentErrors.classSectionId"
              @blur="touchAssignmentField('classSectionId')"
            />
          </FormsFormField>

          <Button
            label="Add"
            icon="pi pi-plus"
            :loading="isAssignmentSubmitting"
            @click="handleAddAssignment"
          />
        </div>

        <Message
          v-if="assignmentError"
          severity="error"
          :closable="false"
        >
          {{ assignmentError }}
        </Message>

        <DataTable
          :value="selectedTeacherAssignments"
          data-key="id"
          size="small"
        >
          <Column
            field="subjectName"
            header="Subject"
          />
          <Column
            field="classSectionDisplayName"
            header="Class"
          />
          <Column
            header="Actions"
            :exportable="false"
            style="width: 6rem;"
          >
            <template #body="{ data }">
              <CommonIconActionButton
                icon="pi pi-trash"
                severity="danger"
                tooltip="Remove assignment"
                @click="handleRemoveAssignment(data.id)"
              />
            </template>
          </Column>
        </DataTable>
      </div>
    </Dialog>

    <FormsBulkImportDialog
      v-model:visible="isBulkDialogVisible"
      entity-label="Teachers"
      :report="bulkReport"
      :error-message="bulkError"
      :is-validating="isBulkValidating"
      :is-committing="isBulkCommitting"
      @template="downloadTeacherTemplate"
      @validate="validateTeacherBulk"
      @commit="commitTeacherBulk"
    />
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { BulkImportReport, PaginatedList } from '~/types/api'
import type {
  ClassSection,
  Subject,
  Teacher,
  TeacherAssignment,
} from '~/types/entities'
import {
  CreateTeacherAssignmentRequestSchema,
  CreateTeacherRequestSchema,
} from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

interface OptionItem {
  label: string
  value: number
}

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'teachers': 'Teachers',
  },
})

const api = useApi()
const bulkImport = useBulkImport()
const { showError, showSuccess, showInfo } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const teachers = ref<Teacher[]>([])
const subjects = ref<Subject[]>([])
const classSections = ref<ClassSection[]>([])
const searchTerm = ref('')

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

const isAssignmentsDialogVisible = ref(false)
const selectedTeacherId = ref<number | null>(null)
const selectedTeacherName = ref('')
const selectedTeacherAssignments = ref<TeacherAssignment[]>([])
const isAssignmentSubmitting = ref(false)

const subjectOptions = computed<OptionItem[]>(() =>
  subjects.value.map(subject => ({
    label: subject.name,
    value: subject.id,
  })),
)

const classSectionOptions = computed<OptionItem[]>(() =>
  classSections.value.map(section => ({
    label: section.displayName,
    value: section.id,
  })),
)

const FormSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().min(1, 'National ID is required'),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
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
    nationalId: '',
    email: '',
    phone: '',
  },
  FormSchema,
)

const {
  state: assignmentState,
  errors: assignmentErrors,
  globalError: assignmentError,
  touchField: touchAssignmentField,
  validateWithSchema: validateAssignmentWithSchema,
  setGlobalError: setAssignmentGlobalError,
  resetForm: resetAssignmentValidationForm,
} = useAppValidation(
  {
    subjectId: null as number | null,
    classSectionId: null as number | null,
  },
  z.object({
    subjectId: z.number().int().min(1, 'Subject is required').nullable(),
    classSectionId: z.number().int().min(1, 'Class is required').nullable(),
  }).superRefine((values, ctx) => {
    if (values.subjectId === null) {
      ctx.addIssue({ code: 'custom', message: 'Subject is required', path: ['subjectId'] })
    }
    if (values.classSectionId === null) {
      ctx.addIssue({ code: 'custom', message: 'Class is required', path: ['classSectionId'] })
    }
  }),
)
const assignmentForm = assignmentState

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

function clearAssignmentErrors() {
  setAssignmentGlobalError('')
  resetAssignmentValidationForm()
}

function normalizeNullable(value: string) {
  const trimmed = value.trim()
  return trimmed.length ? trimmed : null
}

async function loadLookups() {
  try {
    const [subjectsResponse, classSectionsResponse] = await Promise.all([
      api.get<Subject[]>(API.subjects.base),
      api.get<ClassSection[]>(API.classSections.base),
    ])

    if (subjectsResponse.success) {
      subjects.value = subjectsResponse.data
    }
    else {
      showError(subjectsResponse.message ?? 'Failed to load subjects')
    }

    if (classSectionsResponse.success) {
      classSections.value = classSectionsResponse.data
    }
    else {
      showError(classSectionsResponse.message ?? 'Failed to load classes')
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load assignment lookups'))
  }
}

async function loadTeachers() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<Teacher>>(API.teachers.base, {
      ...queryParams.value,
      search: searchTerm.value || undefined,
    })
    if (response.success) {
      teachers.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load teachers')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teachers'))
  }
  finally {
    isLoading.value = false
  }
}

async function onPageAndLoad(event: { page: number, rows: number }) {
  onPage(event)
  await loadTeachers()
}

async function handleSearch() {
  reset()
  await loadTeachers()
}

async function downloadTeacherTemplate() {
  await bulkImport.downloadTemplate({
    validate: API.teachers.bulkValidate,
    commit: API.teachers.bulkCommit,
    template: API.importTemplates.teachers,
    templateFileName: 'teachers-import-template.xlsx',
  })
}

async function validateTeacherBulk(file: File) {
  bulkError.value = ''
  isBulkValidating.value = true
  try {
    const response = await bulkImport.validateFile(file, {
      validate: API.teachers.bulkValidate,
      commit: API.teachers.bulkCommit,
      template: API.importTemplates.teachers,
      templateFileName: 'teachers-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Teacher import validation complete')
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

async function commitTeacherBulk(file: File) {
  bulkError.value = ''
  isBulkCommitting.value = true
  try {
    const response = await bulkImport.commitFile(file, {
      validate: API.teachers.bulkValidate,
      commit: API.teachers.bulkCommit,
      template: API.importTemplates.teachers,
      templateFileName: 'teachers-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Teacher import committed')
      await loadTeachers()
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

function openEditDialog(item: Teacher) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.fullName = item.fullName
  form.nationalId = item.nationalId
  form.email = item.email ?? ''
  form.phone = item.phone ?? ''
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  const requestCheck = CreateTeacherRequestSchema.safeParse({
    ...parsed.data,
    email: normalizeNullable(parsed.data.email ?? ''),
    phone: normalizeNullable(parsed.data.phone ?? ''),
  })

  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Teacher>(API.teachers.byId(selectedId.value), requestCheck.data)
      if (response.success) {
        showSuccess('Teacher updated')
        closeDialog()
        await loadTeachers()
        return
      }
      setGlobalError(response.message ?? 'Failed to update teacher')
      return
    }

    const response = await api.post<Teacher>(API.teachers.base, requestCheck.data)
    if (response.success) {
      showSuccess('Teacher created')
      closeDialog()
      await loadTeachers()
      return
    }
    setGlobalError(response.message ?? 'Failed to create teacher')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Teacher) {
  confirmDelete(
    `Delete "${item.fullName}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.teachers.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Teacher deleted')
          await loadTeachers()
          return
        }
        showError(response.message ?? 'Failed to delete teacher')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete teacher'))
      }
    },
  )
}

async function openAssignmentsDialog(item: Teacher) {
  selectedTeacherId.value = item.id
  selectedTeacherName.value = item.fullName
  assignmentForm.subjectId = null
  assignmentForm.classSectionId = null
  clearAssignmentErrors()

  try {
    const response = await api.get<Teacher>(API.teachers.byId(item.id))
    if (response.success) {
      selectedTeacherAssignments.value = response.data.assignments
      isAssignmentsDialogVisible.value = true
      return
    }
    showError(response.message ?? 'Failed to load assignments')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load assignments'))
  }
}

async function handleAddAssignment() {
  clearAssignmentErrors()
  const parsed = await validateAssignmentWithSchema(assignmentState)
  if (!parsed.success) return
  if (parsed.data.subjectId === null || parsed.data.classSectionId === null) {
    setAssignmentGlobalError('Invalid assignment payload')
    return
  }

  const requestCheck = CreateTeacherAssignmentRequestSchema.safeParse({
    subjectId: parsed.data.subjectId,
    classSectionId: parsed.data.classSectionId,
  })
  if (!requestCheck.success || !selectedTeacherId.value) {
    setAssignmentGlobalError('Invalid assignment payload')
    return
  }

  const exists = selectedTeacherAssignments.value.some(assignment =>
    assignment.subjectId === requestCheck.data.subjectId
    && assignment.classSectionId === requestCheck.data.classSectionId,
  )
  if (exists) {
    showInfo('Assignment already exists')
    return
  }

  isAssignmentSubmitting.value = true
  try {
    const response = await api.post<TeacherAssignment>(
      API.teachers.assignments(selectedTeacherId.value),
      requestCheck.data,
    )
    if (response.success) {
      showSuccess('Assignment added')
      selectedTeacherAssignments.value = [...selectedTeacherAssignments.value, response.data]
      assignmentForm.subjectId = null
      assignmentForm.classSectionId = null
      await loadTeachers()
      return
    }
    setAssignmentGlobalError(response.message ?? 'Failed to add assignment')
  }
  catch (error: unknown) {
    setAssignmentGlobalError(getFriendlyErrorMessage(error, 'Failed to add assignment'))
  }
  finally {
    isAssignmentSubmitting.value = false
  }
}

function handleRemoveAssignment(assignmentId: number) {
  if (!selectedTeacherId.value) return

  confirmDelete(
    'Remove this assignment from the teacher?',
    async () => {
      try {
        const response = await api.del<string>(API.teachers.assignmentById(selectedTeacherId.value!, assignmentId))
        if (response.success) {
          showSuccess(response.message ?? 'Assignment removed')
          selectedTeacherAssignments.value = selectedTeacherAssignments.value.filter(assignment => assignment.id !== assignmentId)
          await loadTeachers()
          return
        }
        showError(response.message ?? 'Failed to remove assignment')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to remove assignment'))
      }
    },
  )
}

onMounted(async () => {
  await Promise.all([
    loadLookups(),
    loadTeachers(),
  ])
})
</script>
