<template>
  <div class="flex h-[calc(100vh-8.5rem)] flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Class Sections
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage class sections grouped by academic year and keystage.
        </p>
      </div>
      <Button
        label="New Class Section"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card class="flex min-h-0 flex-1 flex-col">
      <template #content>
        <div class="mb-4 grid grid-cols-1 gap-4 md:grid-cols-[32rem_22rem]">
          <FormsFormField
            label="Search"
            field-id="class-sections-search"
          >
            <FormsSearchInput
              id="class-sections-search"
              v-model="searchTerm"
              name="class-sections-search"
              persist-key="bp.search.admin.class-sections"
              placeholder="Search by class, grade, section or keystage"
              @search="handleSearch"
            />
          </FormsFormField>

          <FormsFormField
            label="Filter by Academic Year"
            field-id="yearFilter"
          >
            <Select
              id="yearFilter"
              v-model="selectedAcademicYearFilter"
              :options="academicYearFilterOptions"
              option-label="label"
              option-value="value"
              fluid
              @change="handleFilterChange"
            >
              <template #option="{ option }">
                <div class="flex items-center gap-2">
                  <span>{{ option.label }}</span>
                  <Tag
                    v-if="option.isActive"
                    value="Active"
                    severity="success"
                  />
                </div>
              </template>
            </Select>
          </FormsFormField>
        </div>

        <CommonAdminDataTable
          :value="filteredClassSections"
          :loading="isLoading"
          data-key="id"
          size="small"
          paginator
          :rows="20"
          :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
          scrollable
          scroll-height="flex"
          class="h-[calc(100%-4.5rem)]"
        >
          <Column
            field="displayName"
            header="Class"
          />
          <Column
            field="academicYearName"
            header="Academic Year"
          />
          <Column
            field="keystageName"
            header="Keystage"
          />
          <Column
            field="grade"
            header="Grade"
          />
          <Column
            field="section"
            header="Section"
          />
          <Column
            field="studentCount"
            header="Students"
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
                  tooltip="Edit class"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete class"
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
      :header="isEditing ? 'Edit Class Section' : 'Create Class Section'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex h-full min-h-0 flex-col gap-4"
        @submit.prevent="handleSubmit"
      >
        <FormsFormField
          label="Academic Year"
          required
          field-id="academicYearId"
          :error="errors.academicYearId"
        >
          <Select
            id="academicYearId"
            v-model="form.academicYearId"
            :options="academicYearOptions"
            option-label="label"
            option-value="value"
            placeholder="Select academic year"
            fluid
            :invalid="!!errors.academicYearId"
            @blur="touchField('academicYearId')"
          >
            <template #option="{ option }">
              <div class="flex items-center gap-2">
                <span>{{ option.label }}</span>
                <Tag
                  v-if="option.isActive"
                  value="Active"
                  severity="success"
                />
              </div>
            </template>
          </Select>
        </FormsFormField>

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
            placeholder="Select keystage"
            fluid
            :invalid="!!errors.keystageId"
            @blur="touchField('keystageId')"
          />
        </FormsFormField>

        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
          <FormsFormField
            label="Grade"
            required
            field-id="gradeId"
            :error="errors.gradeId"
          >
            <Select
              id="gradeId"
              v-model="form.gradeId"
              :options="gradeOptions"
              option-label="label"
              option-value="value"
              placeholder="Select grade"
              fluid
              :disabled="!form.keystageId"
              :invalid="!!errors.gradeId"
              @blur="touchField('gradeId')"
            />
          </FormsFormField>

          <FormsFormField
            label="Section"
            required
            field-id="section"
            :error="errors.section"
          >
            <InputText
              id="section"
              v-model.trim="form.section"
              fluid
              :invalid="!!errors.section"
              @blur="touchField('section')"
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
import type { PaginatedList } from '~/types/api'
import type {
  AcademicYear,
  ClassSection,
  Grade,
  Keystage,
} from '~/types/entities'
import { CreateClassSectionRequestSchema } from '~/types/forms'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

interface SelectOption {
  label: string
  value: number
  isActive?: boolean
}

interface YearFilterOption {
  label: string
  value: number | null
  isActive?: boolean
}

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'master-data': 'Master Data',
    'class-sections': 'Classes',
  },
})

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()

const classSections = ref<ClassSection[]>([])
const academicYears = ref<AcademicYear[]>([])
const keystages = ref<Keystage[]>([])
const grades = ref<Grade[]>([])
const selectedAcademicYearFilter = ref<number | null>(null)
const searchTerm = ref('')

const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)
const FormSchema = CreateClassSectionRequestSchema.extend({
  academicYearId: CreateClassSectionRequestSchema.shape.academicYearId.nullable(),
  keystageId: CreateClassSectionRequestSchema.shape.keystageId.nullable(),
  gradeId: CreateClassSectionRequestSchema.shape.gradeId.nullable(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.keystageId === null) {
    ctx.addIssue({ code: 'custom', message: 'Keystage is required', path: ['keystageId'] })
  }
  if (values.gradeId === null) {
    ctx.addIssue({ code: 'custom', message: 'Grade is required', path: ['gradeId'] })
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
    academicYearId: null as number | null,
    keystageId: null as number | null,
    gradeId: null as number | null,
    section: '',
  },
  FormSchema,
)

const academicYearFilterOptions = computed<YearFilterOption[]>(() => {
  return [
    { label: 'All Academic Years', value: null },
    ...academicYears.value.map(year => ({
      label: year.name,
      value: year.id,
      isActive: year.isActive,
    })),
  ]
})

const academicYearOptions = computed<SelectOption[]>(() => {
  return academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
    isActive: year.isActive,
  }))
})

const keystageOptions = computed<SelectOption[]>(() => {
  return keystages.value.map(stage => ({
    label: stage.name,
    value: stage.id,
  }))
})

const gradeOptions = computed<SelectOption[]>(() => {
  return grades.value.map(grade => ({
    label: grade.name,
    value: grade.id,
  }))
})

const filteredClassSections = computed(() => {
  const query = searchTerm.value.trim().toLowerCase()
  if (!query) {
    return classSections.value
  }

  return classSections.value.filter(section =>
    section.displayName.toLowerCase().includes(query)
    || section.grade.toLowerCase().includes(query)
    || section.section.toLowerCase().includes(query)
    || section.keystageName.toLowerCase().includes(query)
    || section.academicYearName.toLowerCase().includes(query),
  )
})

function resetForm() {
  resetValidationForm()
  selectedId.value = null
}

async function loadLookups() {
  try {
    const [yearsResponse, keystagesResponse, gradesResponse] = await Promise.all([
      api.get<AcademicYear[]>(API.academicYears.base),
      api.get<Keystage[]>(API.keystages.base),
      api.get<Grade[]>(API.lookups.grades),
    ])

    if (yearsResponse.success) {
      academicYears.value = yearsResponse.data
    }
    else {
      showError(yearsResponse.message ?? 'Failed to load academic years')
    }

    if (keystagesResponse.success) {
      keystages.value = keystagesResponse.data
    }
    else {
      showError(keystagesResponse.message ?? 'Failed to load keystages')
    }

    if (gradesResponse.success) {
      grades.value = gradesResponse.data
    }
    else {
      showError(gradesResponse.message ?? 'Failed to load grades')
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load class section lookups'))
  }
}

async function loadActiveAcademicYear() {
  try {
    const response = await api.get<AcademicYear>(API.academicYears.active)
    if (response.success) {
      selectedAcademicYearFilter.value = response.data.id
    }
  }
  catch {
    // no active year is valid for initial setup
  }
}

async function loadClassSections() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<ClassSection>>(API.classSections.base, {
      pageNumber: 1,
      pageSize: 500,
      academicYearId: selectedAcademicYearFilter.value ?? undefined,
    })
    if (response.success) {
      classSections.value = response.data.items
      return
    }
    showError(response.message ?? 'Failed to load class sections')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load class sections'))
  }
  finally {
    isLoading.value = false
  }
}

function openCreateDialog() {
  isEditing.value = false
  resetForm()
  form.academicYearId = selectedAcademicYearFilter.value
  isDialogVisible.value = true
}

function openEditDialog(item: ClassSection) {
  isEditing.value = true
  selectedId.value = item.id
  setGlobalError('')
  form.academicYearId = item.academicYearId
  form.keystageId = item.keystageId
  form.gradeId = item.gradeId
  form.section = item.section
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function handleSubmit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (parsed.data.academicYearId === null || parsed.data.keystageId === null || parsed.data.gradeId === null) {
    setGlobalError('Invalid form payload')
    return
  }

  const requestCheck = CreateClassSectionRequestSchema.safeParse({
    ...parsed.data,
    academicYearId: parsed.data.academicYearId,
    keystageId: parsed.data.keystageId,
    gradeId: parsed.data.gradeId,
  })
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<ClassSection>(
        API.classSections.byId(selectedId.value),
        requestCheck.data,
      )
      if (response.success) {
        showSuccess('Class section updated')
        closeDialog()
        await loadClassSections()
        return
      }
      setGlobalError(response.message ?? 'Failed to update class section')
      return
    }

    const response = await api.post<ClassSection>(API.classSections.base, requestCheck.data)
    if (response.success) {
      showSuccess('Class section created')
      closeDialog()
      await loadClassSections()
      return
    }
    setGlobalError(response.message ?? 'Failed to create class section')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: ClassSection) {
  confirmDelete(
    `Delete "${item.displayName}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.classSections.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Class section deleted')
          await loadClassSections()
          return
        }
        showError(response.message ?? 'Failed to delete class section')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete class section'))
      }
    },
  )
}

async function handleFilterChange() {
  await loadClassSections()
}

async function handleSearch() {
  await loadClassSections()
}

watch(
  () => form.keystageId,
  async (keystageId) => {
    if (!keystageId) {
      form.gradeId = null
      return
    }

    try {
      const response = await api.get<Grade[]>(API.lookups.grades, { keystageId })
      if (response.success) {
        grades.value = response.data
        if (!grades.value.some(grade => grade.id === form.gradeId)) {
          form.gradeId = null
        }
        return
      }
      showError(response.message ?? 'Failed to load grades')
    }
    catch (error: unknown) {
      showError(getFriendlyErrorMessage(error, 'Failed to load grades'))
    }
  },
)

onMounted(async () => {
  await Promise.all([
    loadLookups(),
    loadActiveAcademicYear(),
  ])
  await loadClassSections()
})
</script>
