<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Create Return
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Record returned books and their condition.
        </p>
      </div>
      <Button
        label="Back"
        icon="pi pi-arrow-left"
        severity="secondary"
        text
        @click="navigateTo('/returns')"
      />
    </div>

    <Card>
      <template #content>
        <form
          class="grid gap-4"
          @submit.prevent="submitForm"
        >
          <FormsFormField
            label="Academic Year"
            required
            field-id="returnAcademicYear"
            :error="errors.academicYearId"
          >
            <Select
              id="returnAcademicYear"
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

          <div class="grid gap-4 lg:grid-cols-[1fr_26rem]">
            <div class="grid gap-4">
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
                  Item Conditions
                </template>
                <template #content>
                  <div
                    v-if="selectedBooks.length === 0"
                    class="text-sm text-surface-500"
                  >
                    Select books to set conditions.
                  </div>
                  <div
                    v-for="book in selectedBooks"
                    :key="book.id"
                    class="mb-4 rounded-lg border border-surface-200 p-3 dark:border-surface-700"
                  >
                    <div class="mb-2 text-sm font-semibold text-surface-900 dark:text-surface-0">
                      {{ book.title }} ({{ book.code }})
                    </div>
                    <SlipsConditionSelector
                      :condition="getBookCondition(book.id)"
                      :notes="getBookConditionNotes(book.id)"
                      :condition-error="itemConditionErrors[book.id]?.condition"
                      :notes-error="itemConditionErrors[book.id]?.conditionNotes"
                      @update:condition="setBookCondition(book.id, $event)"
                      @update:notes="setBookConditionNotes(book.id, $event)"
                    />
                  </div>
                </template>
              </Card>
            </div>

            <Card>
              <template #title>
                Return Preview
              </template>
              <template #content>
                <div class="grid gap-3 text-sm">
                  <div class="flex items-center justify-between gap-2">
                    <span class="text-surface-600 dark:text-surface-400">Academic Year</span>
                    <span class="font-medium">{{ selectedAcademicYearName || '-' }}</span>
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
                      <span class="font-semibold">Returned By</span>
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
            field-id="returnNotes"
            :error="errors.notes"
          >
            <Textarea
              id="returnNotes"
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
              @click="navigateTo('/returns')"
            />
            <Button
              type="submit"
              label="Create Return"
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
        v-model="form.returnedById"
        :student="selectedStudent"
        :error-message="errors.returnedById"
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
import type { CreateReturnSlipItemRequest } from '~/types/forms'
import { CreateReturnSlipRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { BookConditionValue } from '~/types/enums'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    returns: 'Returns',
    create: 'Create',
  },
})

type SelectedBook = {
  id: number
  title: string
  code: string
  available: number
  quantity: number
}

type ReturnFormState = {
  academicYearId: number | null
  studentId: number | null
  returnedById: number | null
  notes: string
}

type ItemConditionState = {
  condition: number | null
  conditionNotes: string
}

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { isAdmin } = useAuth()

const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const selectedStudent = ref<Student | null>(null)
const selectedParent = ref<Parent | null>(null)
const selectedBooks = ref<SelectedBook[]>([])
const itemConditionMap = ref<Record<number, ItemConditionState>>({})
const itemConditionErrors = ref<Record<number, { condition?: string, conditionNotes?: string }>>({})
const isSubmitting = ref(false)
const isStudentDialogVisible = ref(false)
const isParentDialogVisible = ref(false)
const studentDialogMode = ref<'lookup' | 'create'>('lookup')
const parentDialogMode = ref<'lookup' | 'create'>('lookup')

const FormSchema = z.object({
  academicYearId: z.number().int().min(1).nullable(),
  studentId: z.number().int().min(1).nullable(),
  returnedById: z.number().int().min(1).nullable(),
  notes: z.string(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.studentId === null) {
    ctx.addIssue({ code: 'custom', message: 'Student is required', path: ['studentId'] })
  }
  if (values.returnedById === null) {
    ctx.addIssue({ code: 'custom', message: 'Returned by is required', path: ['returnedById'] })
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
} = useAppValidation<ReturnFormState>(
  {
    academicYearId: null,
    studentId: null,
    returnedById: null,
    notes: '',
  },
  FormSchema as z.ZodType<ReturnFormState>,
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

function ensureConditionState(bookId: number): ItemConditionState {
  if (!itemConditionMap.value[bookId]) {
    itemConditionMap.value[bookId] = {
      condition: BookConditionValue.Good,
      conditionNotes: '',
    }
  }
  return itemConditionMap.value[bookId]
}

function getBookCondition(bookId: number) {
  return ensureConditionState(bookId).condition
}

function setBookCondition(bookId: number, condition: number | null) {
  ensureConditionState(bookId).condition = condition
}

function getBookConditionNotes(bookId: number) {
  return ensureConditionState(bookId).conditionNotes
}

function setBookConditionNotes(bookId: number, notes: string) {
  ensureConditionState(bookId).conditionNotes = notes ?? ''
}

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
  form.returnedById = parent.id
}

function clearParent() {
  selectedParent.value = null
  form.returnedById = null
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
    showError(getFriendlyErrorMessage(error, 'Failed to load return lookups'))
  }
}

function validateItemConditions() {
  let hasError = false
  itemConditionErrors.value = {}

  for (const book of selectedBooks.value) {
    const state = ensureConditionState(book.id)
    const nextErrors: { condition?: string, conditionNotes?: string } = {}
    if (state.condition === null) {
      nextErrors.condition = 'Condition is required'
      hasError = true
    }
    itemConditionErrors.value[book.id] = nextErrors
  }
  return !hasError
}

function buildReturnItems(): CreateReturnSlipItemRequest[] {
  return selectedBooks.value.map((book) => {
    const state = ensureConditionState(book.id)
    return {
      bookId: book.id,
      quantity: book.quantity,
      condition: state.condition ?? BookConditionValue.Good,
      conditionNotes: state.conditionNotes || null,
    }
  })
}

async function submitForm() {
  form.studentId = selectedStudent.value?.id ?? null
  form.returnedById = selectedParent.value?.id ?? form.returnedById

  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (selectedBooks.value.length === 0) {
    setGlobalError('At least one returned book is required.')
    return
  }
  if (!validateItemConditions()) {
    setGlobalError('Please correct item condition errors.')
    return
  }

  if (
    parsed.data.academicYearId === null
    || parsed.data.studentId === null
    || parsed.data.returnedById === null
  ) {
    setGlobalError('Invalid return payload.')
    return
  }

  const payload = {
    academicYearId: parsed.data.academicYearId,
    studentId: parsed.data.studentId,
    returnedById: parsed.data.returnedById,
    notes: parsed.data.notes || null,
    items: buildReturnItems(),
  }

  const requestCheck = CreateReturnSlipRequestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid return payload.')
    return
  }

  isSubmitting.value = true
  try {
    const response = await api.post(API.returns.base, requestCheck.data)
    if (response.success) {
      showSuccess('Return created successfully')
      const slip = response.data as { id: number }
      void navigateTo(`/returns/${slip.id}`)
      return
    }
    setGlobalError(response.message ?? 'Failed to create return')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

watch(
  selectedBooks,
  (books) => {
    const ids = new Set(books.map(book => book.id))
    const nextMap: Record<number, ItemConditionState> = {}
    for (const key of Object.keys(itemConditionMap.value)) {
      const id = Number(key)
      if (!ids.has(id)) {
        continue
      }
      nextMap[id] = itemConditionMap.value[id]!
    }
    itemConditionMap.value = nextMap
    for (const book of books) {
      ensureConditionState(book.id)
    }
  },
  { immediate: true, deep: true },
)

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
