<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Create Distribution
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Issue books to a student and generate a distribution slip.
        </p>
      </div>
      <Button
        label="Back"
        icon="pi pi-arrow-left"
        severity="secondary"
        text
        @click="navigateTo('/distribution')"
      />
    </div>

    <Card>
      <template #content>
        <form
          class="grid gap-4"
          @submit.prevent="submitForm"
        >
          <div class="grid gap-4 md:grid-cols-2">
            <FormsFormField
              label="Academic Year"
              required
              field-id="distributionAcademicYear"
              :error="errors.academicYearId"
            >
              <Select
                id="distributionAcademicYear"
                v-model="form.academicYearId"
                :options="academicYearOptions"
                option-label="label"
                option-value="value"
                fluid
                :disabled="!isAdmin"
                :invalid="!!errors.academicYearId"
                @blur="touchField('academicYearId')"
              />
              <small
                v-if="!isAdmin"
                class="text-xs text-surface-500"
              >
                Locked to active academic year for non-admin users.
              </small>
            </FormsFormField>

            <FormsFormField
              label="Term"
              required
              field-id="distributionTerm"
              :error="errors.term"
            >
              <Select
                id="distributionTerm"
                v-model="form.term"
                :options="termOptions"
                option-label="label"
                option-value="value"
                fluid
                :invalid="!!errors.term"
                @blur="touchField('term')"
              />
            </FormsFormField>
          </div>

          <div class="grid gap-4 lg:grid-cols-[1fr_26rem]">
            <Card>
              <template #title>
                Book Selection
              </template>
              <template #content>
                <BooksBookSelector
                  v-model="selectedBooks"
                  :academic-year-id="form.academicYearId"
                />
              </template>
            </Card>

            <Card>
              <template #title>
                Slip Preview
              </template>
              <template #content>
                <div class="grid gap-3 text-sm">
                  <div class="flex items-center justify-between gap-2">
                    <span class="text-surface-600 dark:text-surface-400">Academic Year</span>
                    <span class="font-medium">{{ selectedAcademicYearName || '-' }}</span>
                  </div>
                  <div class="flex items-center justify-between gap-2">
                    <span class="text-surface-600 dark:text-surface-400">Term</span>
                    <span class="font-medium">{{ selectedTermLabel || '-' }}</span>
                  </div>

                  <Divider />

                  <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                    <div class="mb-2 flex items-center justify-between">
                      <span class="font-semibold">Student</span>
                      <div class="flex gap-1">
                        <Button
                          label="Select / Add"
                          icon="pi pi-user"
                          size="small"
                          text
                          @click="isStudentDialogVisible = true"
                        />
                        <Button
                          v-if="selectedStudent"
                          icon="pi pi-times"
                          size="small"
                          text
                          severity="secondary"
                          @click="clearStudent"
                        />
                      </div>
                    </div>
                    <div class="text-surface-700 dark:text-surface-200">
                      {{ selectedStudent?.fullName ?? 'Not selected' }}
                    </div>
                    <div
                      v-if="selectedStudent"
                      class="text-xs text-surface-500"
                    >
                      {{ selectedStudent.indexNo }} • {{ selectedStudent.classSectionDisplayName }}
                    </div>
                  </div>

                  <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                    <div class="mb-2 flex items-center justify-between">
                      <span class="font-semibold">Parent / Guardian</span>
                      <div class="flex gap-1">
                        <Button
                          label="Select / Add"
                          icon="pi pi-users"
                          size="small"
                          text
                          :disabled="!selectedStudent"
                          @click="isParentDialogVisible = true"
                        />
                        <Button
                          v-if="selectedParent"
                          icon="pi pi-times"
                          size="small"
                          text
                          severity="secondary"
                          @click="clearParent"
                        />
                      </div>
                    </div>
                    <div class="text-surface-700 dark:text-surface-200">
                      {{ selectedParent?.fullName ?? 'Not selected' }}
                    </div>
                  </div>

                  <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                    <div class="mb-2 font-semibold">
                      Selected Books
                    </div>
                    <ul
                      v-if="selectedBooks.length > 0"
                      class="grid gap-1 text-xs"
                    >
                      <li
                        v-for="book in selectedBooks"
                        :key="book.id"
                        class="flex items-center justify-between gap-2"
                      >
                        <span class="truncate">{{ book.code }} - {{ book.title }}</span>
                        <span class="font-medium">x{{ book.quantity }}</span>
                      </li>
                    </ul>
                    <div
                      v-else
                      class="text-xs text-surface-500"
                    >
                      No books selected.
                    </div>
                  </div>
                </div>
              </template>
            </Card>
          </div>

          <FormsFormField
            label="Notes"
            field-id="distributionNotes"
            :error="errors.notes"
          >
            <Textarea
              id="distributionNotes"
              v-model.trim="form.notes"
              rows="3"
              fluid
              :invalid="!!errors.notes"
              @blur="touchField('notes')"
            />
          </FormsFormField>

          <Message
            v-if="formError"
            severity="error"
            :closable="false"
          >
            {{ formError }}
          </Message>

          <div class="sticky bottom-0 z-10 -mx-4 flex justify-end gap-2 border-t border-surface-200 bg-surface-0 px-4 py-3 dark:border-surface-700 dark:bg-surface-900">
            <Button
              type="button"
              label="Cancel"
              severity="secondary"
              text
              @click="navigateTo('/distribution')"
            />
            <Button
              type="submit"
              label="Create Slip"
              :loading="isSubmitting"
            />
          </div>
        </form>
      </template>
    </Card>

    <Dialog
      v-model:visible="isStudentDialogVisible"
      modal
      :header="studentDialogMode === 'lookup' ? 'Select / Add Student' : 'Add New Student'"
      :style="{ width: '62rem' }"
    >
      <SlipsStudentLookup
        v-if="studentDialogMode === 'lookup'"
        v-model="selectedStudent"
        :academic-year-id="form.academicYearId"
        @selected="onStudentSelected"
        @cleared="clearStudent"
        @create-requested="onCreateStudentRequested"
      />
      <SlipsStudentQuickCreateForm
        v-else
        :academic-year-id="form.academicYearId"
        @created="onQuickStudentCreated"
        @cancel="studentDialogMode = 'lookup'"
      />
    </Dialog>

    <Dialog
      v-model:visible="isParentDialogVisible"
      modal
      :header="parentDialogMode === 'lookup' ? 'Select / Add Parent / Guardian' : 'Add New Parent / Guardian'"
      :style="{ width: '56rem' }"
    >
      <SlipsParentLookup
        v-if="parentDialogMode === 'lookup'"
        v-model="form.parentId"
        :student="selectedStudent"
        :error-message="errors.parentId"
        @selected="onParentSelected"
        @cleared="clearParent"
        @create-requested="onCreateParentRequested"
      />
      <SlipsParentQuickCreateForm
        v-else
        :student="selectedStudent"
        @created="onQuickParentCreated"
        @cancel="parentDialogMode = 'lookup'"
      />
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Lookup, Parent, Student } from '~/types/entities'
import { CreateDistributionSlipRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { termLabels, termOptions } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    distribution: 'Distributions',
    create: 'Create',
  },
})

type DistributionFormState = {
  academicYearId: number | null
  term: number | null
  studentId: number | null
  parentId: number | null
  notes: string
}

type SelectedBook = {
  id: number
  title: string
  code: string
  available: number
  quantity: number
}

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { isAdmin } = useAuth()

const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const selectedStudent = ref<Student | null>(null)
const selectedParent = ref<Parent | null>(null)
const selectedBooks = ref<SelectedBook[]>([])
const isSubmitting = ref(false)
const isStudentDialogVisible = ref(false)
const isParentDialogVisible = ref(false)
const studentDialogMode = ref<'lookup' | 'create'>('lookup')
const parentDialogMode = ref<'lookup' | 'create'>('lookup')

const FormSchema = z.object({
  academicYearId: z.number().int().min(1).nullable(),
  term: z.number().int().min(1).nullable(),
  studentId: z.number().int().min(1).nullable(),
  parentId: z.number().int().min(1).nullable(),
  notes: z.string(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.term === null) {
    ctx.addIssue({ code: 'custom', message: 'Term is required', path: ['term'] })
  }
  if (values.studentId === null) {
    ctx.addIssue({ code: 'custom', message: 'Student is required', path: ['studentId'] })
  }
  if (values.parentId === null) {
    ctx.addIssue({ code: 'custom', message: 'Parent is required', path: ['parentId'] })
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
} = useAppValidation<DistributionFormState>(
  {
    academicYearId: null,
    term: null,
    studentId: null,
    parentId: null,
    notes: '',
  },
  FormSchema as z.ZodType<DistributionFormState>,
)

const academicYearOptions = computed(() => {
  const mapped = academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
  }))
  if (isAdmin.value) return mapped
  return mapped.filter(year => year.value === activeAcademicYearId.value)
})

const selectedAcademicYearName = computed(() => {
  const selected = academicYears.value.find(year => year.id === form.academicYearId)
  return selected?.name ?? ''
})

const selectedTermLabel = computed(() => {
  if (form.term === null) return ''
  return termLabels[form.term] ?? ''
})

function onStudentSelected(student: Student) {
  selectedStudent.value = student
  form.studentId = student.id
  autoSelectParentFromStudent(student)
}

function clearStudent() {
  selectedStudent.value = null
  form.studentId = null
  clearParent()
}

function onParentSelected(parent: Parent) {
  selectedParent.value = parent
  form.parentId = parent.id
}

function clearParent() {
  selectedParent.value = null
  form.parentId = null
}

function onCreateStudentRequested() {
  studentDialogMode.value = 'create'
}

function onCreateParentRequested() {
  parentDialogMode.value = 'create'
}

function autoSelectParentFromStudent(student: Student) {
  const linkedParents = student.parents ?? []
  if (linkedParents.length === 0) {
    clearParent()
    return
  }

  const chosen = linkedParents.find(parent => parent.isPrimary) ?? linkedParents[0]
  if (!chosen) {
    clearParent()
    return
  }

  onParentSelected({
    id: chosen.parentId,
    fullName: chosen.fullName,
    nationalId: chosen.nationalId,
    phone: chosen.phone,
    relationship: chosen.relationship,
  })
}

function onQuickStudentCreated(student: Student) {
  onStudentSelected(student)
  studentDialogMode.value = 'lookup'
  isStudentDialogVisible.value = false
}

function onQuickParentCreated(parent: Parent) {
  onParentSelected(parent)
  parentDialogMode.value = 'lookup'
  isParentDialogVisible.value = false
}

async function loadLookups() {
  try {
    const [yearsResponse, activeResponse] = await Promise.all([
      api.get<Lookup[]>(API.lookups.academicYears),
      api.get<{ id: number }>(API.academicYears.active),
    ])
    if (yearsResponse.success) {
      academicYears.value = yearsResponse.data
    }
    if (activeResponse.success) {
      activeAcademicYearId.value = activeResponse.data.id
      form.academicYearId = activeResponse.data.id
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load distribution lookups'))
  }
}

async function submitForm() {
  form.studentId = selectedStudent.value?.id ?? null
  form.parentId = selectedParent.value?.id ?? form.parentId

  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (selectedBooks.value.length === 0) {
    setGlobalError('At least one book is required.')
    return
  }

  const invalidBook = selectedBooks.value.find(book =>
    book.available <= 0
    || book.quantity < 1
    || book.quantity > book.available,
  )
  if (invalidBook) {
    setGlobalError(`"${invalidBook.title}" quantity exceeds available stock (${invalidBook.available}).`)
    return
  }

  if (
    parsed.data.academicYearId === null
    || parsed.data.term === null
    || parsed.data.studentId === null
    || parsed.data.parentId === null
  ) {
    setGlobalError('Invalid distribution payload.')
    return
  }

  const payload = {
    academicYearId: parsed.data.academicYearId,
    term: parsed.data.term,
    studentId: parsed.data.studentId,
    parentId: parsed.data.parentId,
    notes: parsed.data.notes || null,
    items: selectedBooks.value.map(book => ({
      bookId: book.id,
      quantity: book.quantity,
    })),
  }

  const requestCheck = CreateDistributionSlipRequestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid distribution payload.')
    return
  }

  isSubmitting.value = true
  try {
    const response = await api.post(API.distributions.base, requestCheck.data)
    if (response.success) {
      showSuccess('Distribution created successfully')
      const slip = response.data as { id: number }
      void navigateTo(`/distribution/${slip.id}`)
      return
    }
    setGlobalError(response.message ?? 'Failed to create distribution')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

watch(
  () => isAdmin.value,
  (admin) => {
    if (!admin && activeAcademicYearId.value !== null) {
      form.academicYearId = activeAcademicYearId.value
    }
  },
)

watch(
  () => isStudentDialogVisible.value,
  (visible) => {
    if (!visible) {
      studentDialogMode.value = 'lookup'
    }
  },
)

watch(
  () => isParentDialogVisible.value,
  (visible) => {
    if (!visible) {
      parentDialogMode.value = 'lookup'
    }
  },
)

onMounted(async () => {
  await loadLookups()
})
</script>
