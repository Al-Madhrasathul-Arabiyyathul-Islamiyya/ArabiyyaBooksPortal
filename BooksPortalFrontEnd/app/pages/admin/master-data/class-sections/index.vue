<template>
  <div class="flex flex-col gap-4">
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

    <Card>
      <template #content>
        <div class="mb-4 grid grid-cols-1 gap-4 md:max-w-sm">
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
            />
          </FormsFormField>
        </div>

        <DataTable
          :value="classSections"
          :loading="isLoading"
          data-key="id"
          size="small"
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
      :header="isEditing ? 'Edit Class Section' : 'Create Class Section'"
      :style="{ width: '34rem' }"
    >
      <form
        class="flex flex-col gap-4"
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
          />
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
import { z } from 'zod/v4'
import type {
  AcademicYear,
  ClassSection,
  Grade,
  Keystage,
} from '~/types/entities'
import { CreateClassSectionRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'

interface SelectOption {
  label: string
  value: number
}

interface YearFilterOption {
  label: string
  value: number | null
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

const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)
const formError = ref('')

const form = reactive({
  academicYearId: null as number | null,
  keystageId: null as number | null,
  gradeId: null as number | null,
  section: '',
})

const errors = reactive({
  academicYearId: '',
  keystageId: '',
  gradeId: '',
  section: '',
})

const FormSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required'),
  keystageId: z.number().int().min(1, 'Keystage is required'),
  gradeId: z.number().int().min(1, 'Grade is required'),
  section: z.string().min(1, 'Section is required'),
})

const academicYearFilterOptions = computed<YearFilterOption[]>(() => {
  return [
    { label: 'All Academic Years', value: null },
    ...academicYears.value.map(year => ({
      label: year.name,
      value: year.id,
    })),
  ]
})

const academicYearOptions = computed<SelectOption[]>(() => {
  return academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
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

function clearErrors() {
  errors.academicYearId = ''
  errors.keystageId = ''
  errors.gradeId = ''
  errors.section = ''
  formError.value = ''
}

function resetForm() {
  form.academicYearId = null
  form.keystageId = null
  form.gradeId = null
  form.section = ''
  selectedId.value = null
  clearErrors()
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
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load class section lookups')
  }
}

async function loadClassSections() {
  isLoading.value = true
  try {
    const query = selectedAcademicYearFilter.value
      ? `?academicYearId=${selectedAcademicYearFilter.value}`
      : ''
    const response = await api.get<ClassSection[]>(`${API.classSections.base}${query}`)
    if (response.success) {
      classSections.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load class sections')
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to load class sections')
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

function openEditDialog(item: ClassSection) {
  isEditing.value = true
  selectedId.value = item.id
  clearErrors()
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

function mapValidationErrors(result: Extract<ReturnType<typeof FormSchema.safeParse>, { success: false }>) {
  clearErrors()
  for (const issue of result.error.issues) {
    const field = issue.path[0]
    if (field === 'academicYearId') errors.academicYearId = issue.message
    if (field === 'keystageId') errors.keystageId = issue.message
    if (field === 'gradeId') errors.gradeId = issue.message
    if (field === 'section') errors.section = issue.message
  }
}

async function handleSubmit() {
  clearErrors()
  const parsed = FormSchema.safeParse({
    academicYearId: form.academicYearId,
    keystageId: form.keystageId,
    gradeId: form.gradeId,
    section: form.section,
  })

  if (!parsed.success) {
    mapValidationErrors(parsed)
    return
  }

  const requestCheck = CreateClassSectionRequestSchema.safeParse(parsed.data)
  if (!requestCheck.success) {
    formError.value = 'Invalid form payload'
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
      formError.value = response.message ?? 'Failed to update class section'
      return
    }

    const response = await api.post<ClassSection>(API.classSections.base, requestCheck.data)
    if (response.success) {
      showSuccess('Class section created')
      closeDialog()
      await loadClassSections()
      return
    }
    formError.value = response.message ?? 'Failed to create class section'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    formError.value = fetchError.data?.message ?? 'Unable to save class section'
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
        const fetchError = error as { data?: { message?: string } }
        showError(fetchError.data?.message ?? 'Failed to delete class section')
      }
    },
  )
}

async function handleFilterChange() {
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
      const fetchError = error as { data?: { message?: string } }
      showError(fetchError.data?.message ?? 'Failed to load grades')
    }
  },
)

onMounted(async () => {
  await loadLookups()
  await loadClassSections()
})
</script>
