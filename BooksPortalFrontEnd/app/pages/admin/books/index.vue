<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Books
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage book catalog entries and prepare inventory workflows.
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <Button
          label="Bulk Import Books"
          icon="pi pi-file-import"
          severity="secondary"
          outlined
          @click="isBulkDialogVisible = true"
        />
        <Button
          label="Download Template"
          icon="pi pi-download"
          severity="secondary"
          text
          @click="downloadBookTemplate"
        />
        <Button
          label="New Book"
          icon="pi pi-plus"
          @click="openCreateDialog"
        />
      </div>
    </div>

    <Card>
      <template #content>
        <div class="grid gap-3 md:grid-cols-3">
          <FormsSearchInput
            id="books-search"
            v-model="searchTerm"
            name="books-search"
            persist-key="bp.search.admin.books"
            placeholder="Search by code, title, ISBN, author..."
            @search="handleSearch"
          />

          <Select
            v-model="selectedSubjectId"
            :options="subjectOptions"
            option-label="label"
            option-value="value"
            placeholder="Filter by subject"
            show-clear
            fluid
            @change="handleFilterChange"
          />

          <div class="flex items-center justify-end">
            <Button
              label="Reset"
              icon="pi pi-refresh"
              severity="secondary"
              outlined
              @click="resetFilters"
            />
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <CommonAdminDataTable
          :value="books"
          :loading="isLoading"
          data-key="id"
          paginator
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
          paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          current-page-report-template="Showing {first} to {last} of {totalRecords}"
          responsive-layout="scroll"
          size="small"
          @row-click="onRowClick"
          @page="onPage($event); void loadBooks()"
        >
          <Column
            field="code"
            header="Code"
            style="min-width: 8rem;"
          />
          <Column
            field="title"
            header="Title"
            style="min-width: 14rem;"
          />
          <Column
            field="subjectName"
            header="Subject"
            style="min-width: 10rem;"
          />
          <Column
            field="grade"
            header="Grade"
            style="min-width: 8rem;"
          />
          <Column
            header="Available"
            style="min-width: 8rem;"
          >
            <template #body="{ data }">
              <BooksStockBadge :available="data.available" />
            </template>
          </Column>
          <Column
            field="totalStock"
            header="Total"
            style="min-width: 7rem;"
          />
          <Column
            header="Actions"
            :exportable="false"
            style="width: 12rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-1">
                <CommonIconActionButton
                  icon="pi pi-eye"
                  tooltip="View details"
                  @click.stop="openDetailsDialog(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit book"
                  @click.stop="openEditDialog(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-box"
                  tooltip="Add stock entry"
                  @click.stop="openStockEntryDialog(data)"
                />
                <CommonIconActionButton
                  v-if="isSuperAdmin"
                  icon="pi pi-trash"
                  severity="danger"
                  tooltip="Delete book"
                  @click.stop="handleDelete(data)"
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
      :header="isEditing ? 'Edit Book' : 'Create Book'"
      :style="{ width: '44rem' }"
    >
      <form
        class="grid gap-4 md:grid-cols-2"
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
          label="Title"
          required
          field-id="title"
          :error="errors.title"
        >
          <InputText
            id="title"
            v-model.trim="form.title"
            fluid
            :invalid="!!errors.title"
            @blur="touchField('title')"
          />
        </FormsFormField>

        <FormsFormField
          label="ISBN"
          field-id="isbn"
          :error="errors.isbn"
        >
          <InputText
            id="isbn"
            v-model.trim="form.isbn"
            fluid
            :invalid="!!errors.isbn"
            @blur="touchField('isbn')"
          />
        </FormsFormField>

        <FormsFormField
          label="Author"
          field-id="author"
          :error="errors.author"
        >
          <InputText
            id="author"
            v-model.trim="form.author"
            fluid
            :invalid="!!errors.author"
            @blur="touchField('author')"
          />
        </FormsFormField>

        <FormsFormField
          label="Edition"
          field-id="edition"
          :error="errors.edition"
        >
          <InputText
            id="edition"
            v-model.trim="form.edition"
            fluid
            :invalid="!!errors.edition"
            @blur="touchField('edition')"
          />
        </FormsFormField>

        <FormsFormField
          label="Publisher"
          required
          field-id="publisher"
          :error="errors.publisher"
        >
          <InputText
            id="publisher"
            v-model.trim="form.publisher"
            fluid
            :invalid="!!errors.publisher"
            @blur="touchField('publisher')"
          />
        </FormsFormField>

        <FormsFormField
          label="Published Year"
          required
          field-id="publishedYear"
          :error="errors.publishedYear"
        >
          <InputNumber
            id="publishedYear"
            v-model="form.publishedYear"
            :min="1"
            :use-grouping="false"
            fluid
            :invalid="!!errors.publishedYear"
            @blur="touchField('publishedYear')"
          />
        </FormsFormField>

        <FormsFormField
          label="Subject"
          required
          field-id="subjectId"
          :error="errors.subjectId"
        >
          <Select
            id="subjectId"
            v-model="form.subjectId"
            :options="subjectOptions"
            option-label="label"
            option-value="value"
            placeholder="Select subject"
            show-clear
            fluid
            :invalid="!!errors.subjectId"
            @blur="touchField('subjectId')"
          />
        </FormsFormField>

        <FormsFormField
          class="md:col-span-2"
          label="Grade"
          field-id="grade"
          :error="errors.grade"
        >
          <InputText
            id="grade"
            v-model.trim="form.grade"
            fluid
            :invalid="!!errors.grade"
            @blur="touchField('grade')"
          />
        </FormsFormField>

        <Message
          v-if="formError"
          severity="error"
          :closable="false"
          class="md:col-span-2"
        >
          {{ formError }}
        </Message>

        <div class="md:col-span-2 flex justify-end gap-2 pt-2">
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

    <BooksStockEntryDialog
      v-model:visible="isStockEntryDialogVisible"
      :loading="isStockActionSubmitting"
      :academic-year-options="academicYearOptions"
      :form="stockEntryForm"
      :errors="stockEntryErrors"
      @submit="submitStockEntry"
      @cancel="closeStockEntryDialog"
      @dismiss="closeStockEntryDialog"
    />

    <FormsBulkImportDialog
      v-model:visible="isBulkDialogVisible"
      entity-label="Books"
      :report="bulkReport"
      :error-message="bulkError"
      :is-validating="isBulkValidating"
      :is-committing="isBulkCommitting"
      @template="downloadBookTemplate"
      @validate="validateBookBulk"
      @commit="commitBookBulk"
    />
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Book, Lookup } from '~/types/entities'
import type { PaginatedList, BulkImportReport } from '~/types/api'
import { CreateBookRequestSchema } from '~/types/forms'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', books: 'Books',
  },
})

const api = useApi()
const route = useRoute()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin } = useAuth()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()
const bulkImport = useBulkImport()
const bulkJobs = useBulkImportJobs()

const books = ref<Book[]>([])
const subjects = ref<Lookup[]>([])
const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const isLoading = ref(false)
const isSubmitting = ref(false)
const isStockActionSubmitting = ref(false)
const isDialogVisible = ref(false)
const isStockEntryDialogVisible = ref(false)
const isEditing = ref(false)
const selectedId = ref<number | null>(null)
const selectedBook = ref<Book | null>(null)

const searchTerm = ref('')
const selectedSubjectId = ref<number | null>(null)

const isBulkDialogVisible = ref(false)
const isBulkValidating = ref(false)
const isBulkCommitting = ref(false)
const bulkError = ref('')
const bulkReport = ref<BulkImportReport | null>(null)
const initialStockEntryForm = {
  academicYearId: null as number | null,
  quantity: null as number | null,
  source: '',
  notes: '',
}

const BookValidationSchema = z.object({
  isbn: z.string().nullable().optional(),
  code: z.string().min(1, 'Code is required'),
  title: z.string().min(1, 'Title is required'),
  author: z.string().nullable().optional(),
  edition: z.string().nullable().optional(),
  publisher: z.string().min(1, 'Publisher is required'),
  publishedYear: z.number().int().min(1, 'Published year is required').nullable(),
  subjectId: z.number().int().min(1, 'Subject is required').nullable(),
  grade: z.string().nullable().optional(),
}).superRefine((values, ctx) => {
  if (values.publishedYear === null) {
    ctx.addIssue({ code: 'custom', message: 'Published year is required', path: ['publishedYear'] })
  }
  if (values.subjectId === null) {
    ctx.addIssue({ code: 'custom', message: 'Subject is required', path: ['subjectId'] })
  }
})

const StockEntryValidationSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required').nullable(),
  quantity: z.number().int().min(1, 'Quantity is required').nullable(),
  source: z.string().optional(),
  notes: z.string().optional(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.quantity === null) {
    ctx.addIssue({ code: 'custom', message: 'Quantity is required', path: ['quantity'] })
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
  resetForm: resetBookFormValidation,
} = useAppValidation(
  {
    isbn: '',
    code: '',
    title: '',
    author: '',
    edition: '',
    publisher: 'Other',
    publishedYear: null as number | null,
    subjectId: null as number | null,
    grade: '',
  },
  BookValidationSchema,
)

const {
  state: stockEntryForm,
  errors: stockEntryErrors,
  validateWithSchema: validateStockEntryWithSchema,
  resetValidation: resetStockEntryValidation,
} = useAppValidation(
  initialStockEntryForm,
  StockEntryValidationSchema,
)

const subjectOptions = computed(() =>
  subjects.value.map(subject => ({
    label: subject.name,
    value: subject.id,
  })),
)

function resetBookForm() {
  resetBookFormValidation()
  selectedId.value = null
  isEditing.value = false
}
const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
    isActive: year.id === activeAcademicYearId.value,
  })),
)

async function loadSubjects() {
  try {
    const response = await api.get<Lookup[]>(API.lookups.subjects)
    if (response.success) {
      subjects.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load subject options')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load subject options'))
  }
}

async function loadStockLookups() {
  try {
    const yearRes = await api.get<Lookup[]>(API.lookups.academicYears)
    if (yearRes.success)
      academicYears.value = yearRes.data
  }
  catch {
    showError('Failed to load stock lookup options')
  }
}

async function loadActiveAcademicYear() {
  try {
    const response = await api.get<{ id: number }>(API.academicYears.active)
    if (response.success) {
      activeAcademicYearId.value = response.data.id
      if (!stockEntryForm.academicYearId) {
        stockEntryForm.academicYearId = response.data.id
      }
    }
  }
  catch {
    // No active year should not block the page.
  }
}

async function loadBooks() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<Book>>(API.books.base, {
      ...queryParams.value,
      search: searchTerm.value || undefined,
      subjectId: selectedSubjectId.value ?? undefined,
    })
    if (response.success) {
      books.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load books')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load books'))
  }
  finally {
    isLoading.value = false
  }
}

function openCreateDialog() {
  resetBookForm()
  isDialogVisible.value = true
}

async function openEditDialog(item: Book) {
  await openEditDialogById(item.id)
}

async function openEditDialogById(id: number) {
  resetBookForm()
  selectedId.value = id
  isEditing.value = true

  try {
    const response = await api.get<Book>(API.books.byId(id))
    if (!response.success) {
      showError(response.message ?? 'Failed to load book')
      return
    }

    const book = response.data
    form.isbn = book.isbn ?? ''
    form.code = book.code
    form.title = book.title
    form.author = book.author ?? ''
    form.edition = book.edition ?? ''
    form.publisher = book.publisher
    form.publishedYear = book.publishedYear
    form.subjectId = book.subjectId
    form.grade = book.grade ?? ''
    isDialogVisible.value = true
    if (route.query.edit) {
      void navigateTo('/admin/books', { replace: true })
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load book'))
  }
}

function resetStockForms() {
  resetStockEntryValidation()
  stockEntryForm.academicYearId = activeAcademicYearId.value
  stockEntryForm.quantity = null
  stockEntryForm.source = ''
  stockEntryForm.notes = ''
}

async function openDetailsDialog(item: Book) {
  void navigateTo(`/admin/books/${item.id}`)
}

function onRowClick(event: { data: Book }) {
  void navigateTo(`/admin/books/${event.data.id}`)
}

function openStockEntryDialog(item: Book) {
  selectedBook.value = item
  resetStockForms()
  isStockEntryDialogVisible.value = true
}

function closeStockEntryDialog() {
  isStockEntryDialogVisible.value = false
  resetStockForms()
}

function closeDialog() {
  isDialogVisible.value = false
  resetBookForm()
  if (route.query.edit) {
    void navigateTo('/admin/books', { replace: true })
  }
}

async function handleSubmit() {
  const parsed = await validateWithSchema({
    isbn: form.isbn || null,
    code: form.code,
    title: form.title,
    author: form.author || null,
    edition: form.edition || null,
    publisher: form.publisher,
    publishedYear: form.publishedYear,
    subjectId: form.subjectId,
    grade: form.grade || null,
  })
  if (!parsed.success) return
  if (parsed.data.publishedYear === null || parsed.data.subjectId === null) {
    setGlobalError('Invalid form payload')
    return
  }

  const requestCheck = CreateBookRequestSchema.safeParse({
    ...parsed.data,
    publishedYear: parsed.data.publishedYear,
    subjectId: parsed.data.subjectId,
  })
  if (!requestCheck.success) {
    setGlobalError('Invalid form payload')
    return
  }

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedId.value) {
      const response = await api.put<Book>(API.books.byId(selectedId.value), requestCheck.data)
      if (response.success) {
        showSuccess('Book updated')
        closeDialog()
        await loadBooks()
        return
      }
      setGlobalError(response.message ?? 'Failed to update book')
      return
    }

    const response = await api.post<Book>(API.books.base, requestCheck.data)
    if (response.success) {
      showSuccess('Book created')
      closeDialog()
      await loadBooks()
      return
    }
    setGlobalError(response.message ?? 'Failed to create book')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function handleDelete(item: Book) {
  confirmDelete(
    `Delete "${item.title}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.books.byId(item.id))
        if (response.success) {
          showSuccess(response.message ?? 'Book deleted')
          await loadBooks()
          return
        }
        showError(response.message ?? 'Failed to delete book')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to delete book'))
      }
    },
  )
}

async function handleSearch() {
  reset()
  await loadBooks()
}

async function handleFilterChange() {
  reset()
  await loadBooks()
}

async function resetFilters() {
  searchTerm.value = ''
  selectedSubjectId.value = null
  reset()
  await loadBooks()
}

async function downloadBookTemplate() {
  try {
    await bulkImport.downloadTemplate({
      validate: API.books.bulkValidate,
      commit: API.books.bulkCommit,
      template: API.importTemplates.books,
      templateFileName: 'books-import-template.xlsx',
    })
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to download template'))
  }
}

async function validateBookBulk(file: File) {
  bulkError.value = ''
  isBulkValidating.value = true
  try {
    const response = await bulkImport.validateFile(file, {
      validate: API.books.bulkValidate,
      commit: API.books.bulkCommit,
      template: API.importTemplates.books,
      templateFileName: 'books-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Book import validation complete')
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

async function commitBookBulk(file: File) {
  bulkError.value = ''
  isBulkCommitting.value = true
  try {
    const response = await bulkJobs.queueJob(file, 'Books', {
      commitAsync: API.books.bulkCommitAsync,
      jobStatus: API.books.bulkJob,
      jobReport: API.books.bulkJobReport,
    })
    if (response.success) {
      showSuccess('Book import started. You can monitor progress from the header notifier.')
      isBulkDialogVisible.value = false
      return
    }
    bulkError.value = response.message ?? 'Failed to start import'
  }
  catch (error: unknown) {
    bulkError.value = getFriendlyErrorMessage(error, 'Failed to start import')
  }
  finally {
    isBulkCommitting.value = false
  }
}

async function submitStockEntry(payload: typeof stockEntryForm) {
  stockEntryForm.academicYearId = payload.academicYearId
  stockEntryForm.quantity = payload.quantity
  stockEntryForm.source = payload.source ?? ''
  stockEntryForm.notes = payload.notes ?? ''

  const parsed = await validateStockEntryWithSchema(stockEntryForm)
  if (!parsed.success) {
    showError('Please correct stock entry form errors')
    return
  }
  if (!selectedBook.value || parsed.data.academicYearId === null || parsed.data.quantity === null) {
    showError('Invalid stock entry payload')
    return
  }
  isStockActionSubmitting.value = true
  try {
    const response = await api.post<Book>(API.books.stockEntry(selectedBook.value.id), {
      academicYearId: parsed.data.academicYearId,
      quantity: parsed.data.quantity,
      source: parsed.data.source || null,
      notes: parsed.data.notes || null,
    })
    if (response.success) {
      showSuccess('Stock entry added')
      isStockEntryDialogVisible.value = false
      await loadBooks()
      return
    }
    showError(response.message ?? 'Failed to add stock entry')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to add stock entry'))
  }
  finally {
    isStockActionSubmitting.value = false
  }
}

onMounted(async () => {
  await Promise.all([
    loadSubjects(),
    loadStockLookups(),
    loadActiveAcademicYear(),
    loadBooks(),
  ])

  const rawEdit = Array.isArray(route.query.edit) ? route.query.edit[0] : route.query.edit
  const editId = Number(rawEdit)
  if (Number.isFinite(editId) && editId > 0) {
    await openEditDialogById(editId)
  }
})
</script>
