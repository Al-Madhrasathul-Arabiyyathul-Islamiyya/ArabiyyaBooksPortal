<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          {{ isRevisionMode ? 'Edit Teacher Issue' : 'Create Teacher Issue' }}
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          {{ isRevisionMode ? 'Revise a processing teacher issue slip.' : 'Issue books to a teacher and generate a teacher issue slip.' }}
        </p>
      </div>
      <Button
        label="Back"
        icon="pi pi-arrow-left"
        severity="secondary"
        text
        @click="navigateTo(isRevisionMode ? `/teacher-issues/${reviseSlipId}` : '/teacher-issues')"
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
              field-id="teacherIssueAcademicYear"
              :error="errors.academicYearId"
            >
              <Select
                id="teacherIssueAcademicYear"
                v-model="form.academicYearId"
                :options="academicYearOptions"
                option-label="label"
                option-value="value"
                fluid
                :disabled="!isAdmin"
                :invalid="!!errors.academicYearId"
                @blur="touchField('academicYearId')"
              />
            </FormsFormField>

            <FormsFormField
              label="Expected Return Date"
              field-id="teacherIssueExpectedReturnDate"
              :error="errors.expectedReturnDate"
            >
              <DatePicker
                id="teacherIssueExpectedReturnDate"
                v-model="expectedReturnDateValue"
                date-format="dd/mm/yy"
                fluid
                show-icon
                :invalid="!!errors.expectedReturnDate"
                @blur="touchField('expectedReturnDate')"
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
                    <span class="text-surface-600 dark:text-surface-400">Expected Return</span>
                    <span class="font-medium">{{ expectedReturnDateLabel || '-' }}</span>
                  </div>

                  <Divider />

                  <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                    <div class="mb-2 flex items-center justify-between">
                      <span class="font-semibold">Teacher</span>
                      <div class="flex gap-1">
                        <Button
                          label="Select / Add"
                          icon="pi pi-user"
                          size="small"
                          text
                          @click="isTeacherDialogVisible = true"
                        />
                        <Button
                          v-if="selectedTeacher"
                          icon="pi pi-times"
                          size="small"
                          text
                          severity="secondary"
                          @click="clearTeacher"
                        />
                      </div>
                    </div>
                    <div class="text-surface-700 dark:text-surface-200">
                      {{ selectedTeacher?.fullName ?? 'Not selected' }}
                    </div>
                    <div
                      v-if="selectedTeacher"
                      class="text-xs text-surface-500"
                    >
                      {{ selectedTeacher.nationalId }}
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
            field-id="teacherIssueNotes"
            :error="errors.notes"
          >
            <Textarea
              id="teacherIssueNotes"
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
              @click="navigateTo(isRevisionMode ? `/teacher-issues/${reviseSlipId}` : '/teacher-issues')"
            />
            <Button
              type="submit"
              :label="isRevisionMode ? 'Save Changes' : 'Create Slip'"
              :loading="isSubmitting"
              :disabled="isOperationBlocked"
            />
          </div>
        </form>
      </template>
    </Card>

    <Dialog
      v-model:visible="isTeacherDialogVisible"
      modal
      :header="teacherDialogMode === 'lookup' ? 'Select / Add Teacher' : 'Add New Teacher'"
      :style="{ width: '62rem' }"
    >
      <SlipsTeacherLookup
        v-if="teacherDialogMode === 'lookup'"
        v-model="selectedTeacher"
        @selected="onTeacherSelected"
        @cleared="clearTeacher"
        @create-requested="onCreateTeacherRequested"
      />
      <SlipsTeacherQuickCreateForm
        v-else
        @created="onQuickTeacherCreated"
        @cancel="teacherDialogMode = 'lookup'"
      />
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { PaginatedList } from '~/types/api'
import type { Book, Lookup, Teacher, TeacherIssue } from '~/types/entities'
import { CreateTeacherIssueRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    'teacher-issues': 'Teacher Issues',
    'create': 'Create',
  },
})

type SelectedBook = {
  id: number
  title: string
  code: string
  available: number
  quantity: number
}

type TeacherIssueFormState = {
  academicYearId: number | null
  teacherId: number | null
  expectedReturnDate: string | null
  notes: string
}

const api = useApi()
const route = useRoute()
const { showError, showSuccess } = useAppToast()
const { isAdmin } = useAuth()
const { isProcessing } = useSlipLifecycle()
const { guard, isOperationBlocked } = useOperationReadinessGuard()

const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const selectedTeacher = ref<Teacher | null>(null)
const selectedBooks = ref<SelectedBook[]>([])
const isSubmitting = ref(false)
const isTeacherDialogVisible = ref(false)
const teacherDialogMode = ref<'lookup' | 'create'>('lookup')
const isRevisionLoading = ref(false)
const reviseSlipId = computed(() => Number(route.query.reviseId))
const isRevisionMode = computed(() => Number.isFinite(reviseSlipId.value) && reviseSlipId.value > 0)

const FormSchema = z.object({
  academicYearId: z.number().int().min(1).nullable(),
  teacherId: z.number().int().min(1).nullable(),
  expectedReturnDate: z.string().nullable(),
  notes: z.string(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.teacherId === null) {
    ctx.addIssue({ code: 'custom', message: 'Teacher is required', path: ['teacherId'] })
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
} = useAppValidation<TeacherIssueFormState>(
  {
    academicYearId: null,
    teacherId: null,
    expectedReturnDate: null,
    notes: '',
  },
  FormSchema as z.ZodType<TeacherIssueFormState>,
)

const expectedReturnDateValue = computed<Date | null>({
  get() {
    if (!form.expectedReturnDate) return null
    return new Date(form.expectedReturnDate)
  },
  set(value) {
    form.expectedReturnDate = value ? value.toISOString() : null
  },
})

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

const expectedReturnDateLabel = computed(() => {
  if (!expectedReturnDateValue.value) return ''
  return expectedReturnDateValue.value.toLocaleDateString()
})

function onTeacherSelected(teacher: Teacher) {
  selectedTeacher.value = teacher
  form.teacherId = teacher.id
}

function clearTeacher() {
  selectedTeacher.value = null
  form.teacherId = null
}

function onCreateTeacherRequested() {
  teacherDialogMode.value = 'create'
}

function onQuickTeacherCreated(teacher: Teacher) {
  teacherDialogMode.value = 'lookup'
  selectedTeacher.value = teacher
  form.teacherId = teacher.id
  isTeacherDialogVisible.value = false
}

async function loadAcademicYears() {
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
      if (form.academicYearId === null) {
        form.academicYearId = activeResponse.data.id
      }
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load academic years'))
  }
}

function mapBookToSelected(book: Book, quantity = 1): SelectedBook {
  return {
    id: book.id,
    title: book.title,
    code: book.code,
    available: book.available,
    quantity,
  }
}

async function hydrateRevisionBooks(items: TeacherIssue['items'], academicYearId: number) {
  const resolved = await Promise.all(items.map(async (item) => {
    try {
      const search = await api.get<Book[] | PaginatedList<Book>>(API.books.search, {
        q: item.bookCode,
        academicYearId,
        page: 1,
        pageSize: 5,
      })
      if (search.success) {
        const candidates = Array.isArray(search.data) ? search.data : search.data.items
        const matched = candidates.find(book => book.id === item.bookId || book.code === item.bookCode)
        if (matched) return mapBookToSelected(matched, item.quantity)
      }
    }
    catch {
      // best effort hydration only
    }

    return {
      id: item.bookId,
      title: item.bookTitle,
      code: item.bookCode,
      available: Math.max(item.quantity, 1),
      quantity: item.quantity,
    }
  }))

  selectedBooks.value = resolved
}

async function loadRevisionSlip() {
  if (!isRevisionMode.value) return

  isRevisionLoading.value = true
  try {
    const slipResponse = await api.get<TeacherIssue>(API.teacherIssues.byId(reviseSlipId.value))
    if (!slipResponse.success) {
      showError(slipResponse.message ?? 'Failed to load teacher issue for editing')
      return
    }

    const current = slipResponse.data
    if (!isProcessing(current.lifecycleStatus)) {
      showError('Only processing teacher issue slips can be edited.')
      void navigateTo(`/teacher-issues/${current.id}`)
      return
    }

    form.academicYearId = current.academicYearId
    form.teacherId = current.teacherId
    form.expectedReturnDate = current.expectedReturnDate
    form.notes = current.notes ?? ''

    try {
      const teacherResponse = await api.get<Teacher>(API.teachers.byId(current.teacherId))
      if (teacherResponse.success) {
        selectedTeacher.value = teacherResponse.data
      }
    }
    catch {
      selectedTeacher.value = {
        id: current.teacherId,
        fullName: current.teacherName,
        nationalId: '',
        email: null,
        phone: null,
        assignments: [],
      }
    }

    await hydrateRevisionBooks(current.items, current.academicYearId)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher issue for editing'))
  }
  finally {
    isRevisionLoading.value = false
  }
}

function mapSelectedItems() {
  return selectedBooks.value.map(item => ({
    bookId: item.id,
    quantity: item.quantity,
  }))
}

async function submitForm() {
  if (!await guard(isRevisionMode.value ? 'save teacher issue changes' : 'create a teacher issue slip')) {
    return
  }

  setGlobalError('')
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  if (selectedBooks.value.length === 0) {
    setGlobalError('Select at least one book before creating the slip.')
    return
  }

  const requestPayload = {
    academicYearId: form.academicYearId as number,
    teacherId: form.teacherId as number,
    expectedReturnDate: form.expectedReturnDate,
    notes: form.notes.trim() ? form.notes.trim() : null,
    items: mapSelectedItems(),
  }

  const requestValidation = await CreateTeacherIssueRequestSchema.safeParseAsync(requestPayload)
  if (!requestValidation.success) {
    setGlobalError(requestValidation.error.issues[0]?.message ?? 'Invalid teacher issue payload')
    return
  }

  isSubmitting.value = true
  try {
    const response = isRevisionMode.value
      ? await api.put<{ id: number }>(API.teacherIssues.byId(reviseSlipId.value), requestValidation.data)
      : await api.post<{ id: number }>(API.teacherIssues.base, requestValidation.data)
    if (response.success) {
      showSuccess(response.message ?? (isRevisionMode.value ? 'Teacher issue updated' : 'Teacher issue created'))
      void navigateTo(`/teacher-issues/${response.data.id ?? reviseSlipId.value}`)
      return
    }
    setGlobalError(response.message ?? (isRevisionMode.value ? 'Failed to update teacher issue' : 'Failed to create teacher issue'))
  }
  catch (error: unknown) {
    applyBackendErrors(error)
    showError(getFriendlyErrorMessage(error, isRevisionMode.value ? 'Failed to update teacher issue' : 'Failed to create teacher issue'))
  }
  finally {
    isSubmitting.value = false
  }
}

onMounted(async () => {
  await loadAcademicYears()
  await loadRevisionSlip()
})
</script>
