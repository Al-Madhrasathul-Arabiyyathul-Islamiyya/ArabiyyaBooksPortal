<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          {{ book?.title ?? 'Book Details' }}
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Code: {{ book?.code ?? '-' }}
        </p>
      </div>
      <div class="flex flex-wrap gap-2">
        <Button
          label="Back to Books"
          icon="pi pi-arrow-left"
          severity="secondary"
          text
          @click="goBack"
        />
        <Button
          label="Edit"
          icon="pi pi-pencil"
          outlined
          @click="goEdit"
        />
        <Button
          v-if="isSuperAdmin"
          label="Delete"
          icon="pi pi-trash"
          severity="danger"
          outlined
          @click="handleDelete"
        />
      </div>
    </div>

    <Card v-if="book">
      <template #content>
        <div class="grid gap-3 md:grid-cols-4">
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Available
            </div>
            <div class="pt-1">
              <BooksStockBadge :available="book.available" />
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Total Stock
            </div>
            <div class="text-lg font-semibold">
              {{ book.totalStock }}
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Distributed
            </div>
            <div class="text-lg font-semibold">
              {{ book.distributed }}
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              With Teachers
            </div>
            <div class="text-lg font-semibold">
              {{ book.withTeachers }}
            </div>
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <div class="mb-3 flex flex-wrap gap-2">
          <Button
            label="Add Stock"
            icon="pi pi-box"
            @click="isStockEntryDialogVisible = true"
          />
          <Button
            v-if="isAdmin"
            label="Adjust Stock"
            icon="pi pi-sliders-h"
            severity="secondary"
            outlined
            @click="isAdjustStockDialogVisible = true"
          />
        </div>

        <div class="grid gap-4 lg:grid-cols-2">
          <Card>
            <template #title>
              Stock Entries
            </template>
            <template #content>
              <CommonAdminDataTable
                :value="stockEntries"
                :loading="isLoading"
                size="small"
                responsive-layout="scroll"
                paginator
                :rows="20"
                :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
                scrollable
                scroll-height="24rem"
              >
                <Column
                  field="academicYearId"
                  header="Academic Year"
                />
                <Column
                  field="quantity"
                  header="Quantity"
                />
                <Column
                  field="source"
                  header="Source"
                />
                <Column
                  field="notes"
                  header="Notes"
                />
                <Column
                  field="enteredAt"
                  header="Entered At"
                >
                  <template #body="{ data }">
                    {{ formatDateTime(data.enteredAt) }}
                  </template>
                </Column>
              </CommonAdminDataTable>
            </template>
          </Card>

          <Card>
            <template #title>
              Stock Movements
            </template>
            <template #content>
              <CommonAdminDataTable
                :value="stockMovements"
                :loading="isLoading"
                size="small"
                responsive-layout="scroll"
                paginator
                :rows="20"
                :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
                scrollable
                scroll-height="24rem"
              >
                <Column
                  field="movementType"
                  header="Type"
                >
                  <template #body="{ data }">
                    {{ movementTypeLabels[data.movementType] ?? data.movementType }}
                  </template>
                </Column>
                <Column
                  field="quantity"
                  header="Quantity"
                />
                <Column
                  field="referenceType"
                  header="Reference"
                />
                <Column
                  field="notes"
                  header="Notes"
                />
                <Column
                  field="processedAt"
                  header="Processed At"
                >
                  <template #body="{ data }">
                    {{ formatDateTime(data.processedAt) }}
                  </template>
                </Column>
              </CommonAdminDataTable>
            </template>
          </Card>
        </div>
      </template>
    </Card>

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

    <BooksStockAdjustDialog
      v-model:visible="isAdjustStockDialogVisible"
      :loading="isStockActionSubmitting"
      :academic-year-options="academicYearOptions"
      :movement-type-options="movementTypeOptions"
      :form="adjustStockForm"
      :errors="adjustStockErrors"
      @submit="submitAdjustStock"
      @cancel="closeAdjustStockDialog"
      @dismiss="closeAdjustStockDialog"
    />
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Book, Lookup, StockEntry, StockMovement } from '~/types/entities'
import { API, PAGINATION } from '~/utils/constants'
import { movementTypeLabels } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    books: 'Books',
    id: 'Details',
  },
})

const route = useRoute()
const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmDelete } = useAppConfirm()
const { isSuperAdmin, isAdmin } = useAuth()

const book = ref<Book | null>(null)
const stockEntries = ref<StockEntry[]>([])
const stockMovements = ref<StockMovement[]>([])
const academicYears = ref<Lookup[]>([])
const movementTypes = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const isLoading = ref(false)
const isStockActionSubmitting = ref(false)
const isStockEntryDialogVisible = ref(false)
const isAdjustStockDialogVisible = ref(false)

const bookId = computed(() => Number(route.params.id))

const initialStockEntryForm = {
  academicYearId: null as number | null,
  quantity: null as number | null,
  source: '',
  notes: '',
}

const initialAdjustStockForm = {
  academicYearId: null as number | null,
  movementType: null as number | null,
  quantity: null as number | null,
  notes: '',
}

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

const AdjustStockValidationSchema = z.object({
  academicYearId: z.number().int().min(1, 'Academic year is required').nullable(),
  movementType: z.number().int().min(1, 'Movement type is required').nullable(),
  quantity: z.number().int().min(1, 'Quantity is required').nullable(),
  notes: z.string().optional(),
}).superRefine((values, ctx) => {
  if (values.academicYearId === null) {
    ctx.addIssue({ code: 'custom', message: 'Academic year is required', path: ['academicYearId'] })
  }
  if (values.movementType === null) {
    ctx.addIssue({ code: 'custom', message: 'Movement type is required', path: ['movementType'] })
  }
  if (values.quantity === null) {
    ctx.addIssue({ code: 'custom', message: 'Quantity is required', path: ['quantity'] })
  }
})

const {
  state: stockEntryForm,
  errors: stockEntryErrors,
  validateWithSchema: validateStockEntryWithSchema,
  resetValidation: resetStockEntryValidation,
} = useAppValidation(initialStockEntryForm, StockEntryValidationSchema)

const {
  state: adjustStockForm,
  errors: adjustStockErrors,
  validateWithSchema: validateAdjustStockWithSchema,
  resetValidation: resetAdjustStockValidation,
} = useAppValidation(initialAdjustStockForm, AdjustStockValidationSchema)

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
    isActive: year.id === activeAcademicYearId.value,
  })),
)

const movementTypeOptions = computed(() =>
  movementTypes.value.map(item => ({
    label: item.name,
    value: item.id,
  })),
)

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function resetStockForms() {
  resetStockEntryValidation()
  resetAdjustStockValidation()
  stockEntryForm.academicYearId = activeAcademicYearId.value
  stockEntryForm.quantity = null
  stockEntryForm.source = ''
  stockEntryForm.notes = ''
  adjustStockForm.academicYearId = activeAcademicYearId.value
  adjustStockForm.movementType = null
  adjustStockForm.quantity = null
  adjustStockForm.notes = ''
}

function closeStockEntryDialog() {
  isStockEntryDialogVisible.value = false
  resetStockForms()
}

function closeAdjustStockDialog() {
  isAdjustStockDialogVisible.value = false
  resetStockForms()
}

async function loadLookups() {
  const [yearRes, movementRes] = await Promise.all([
    api.get<Lookup[]>(API.lookups.academicYears),
    api.get<Lookup[]>(API.lookups.movementTypes),
  ])
  if (yearRes.success) {
    academicYears.value = yearRes.data
  }
  if (movementRes.success) {
    movementTypes.value = movementRes.data
  }
}

async function loadActiveAcademicYear() {
  const response = await api.get<{ id: number }>(API.academicYears.active)
  if (response.success) {
    activeAcademicYearId.value = response.data.id
  }
}

async function loadBookDetails() {
  if (!Number.isFinite(bookId.value)) {
    showError('Invalid book route')
    return
  }
  isLoading.value = true
  try {
    const [bookRes, entriesRes, movementsRes] = await Promise.all([
      api.get<Book>(API.books.byId(bookId.value)),
      api.get<StockEntry[]>(API.books.stockEntries(bookId.value)),
      api.get<StockMovement[]>(API.books.stockMovements(bookId.value)),
    ])
    if (!bookRes.success) {
      showError(bookRes.message ?? 'Failed to load book details')
      return
    }
    book.value = bookRes.data
    stockEntries.value = entriesRes.success ? entriesRes.data : []
    stockMovements.value = movementsRes.success ? movementsRes.data : []
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load book details'))
  }
  finally {
    isLoading.value = false
  }
}

async function submitStockEntry(payload: { academicYearId: number | null, quantity: number | null, source?: string, notes?: string }) {
  stockEntryForm.academicYearId = payload.academicYearId
  stockEntryForm.quantity = payload.quantity
  stockEntryForm.source = payload.source ?? ''
  stockEntryForm.notes = payload.notes ?? ''

  const parsed = await validateStockEntryWithSchema(stockEntryForm)
  if (!parsed.success || parsed.data.academicYearId === null || parsed.data.quantity === null) {
    showError('Please correct stock entry form errors')
    return
  }
  isStockActionSubmitting.value = true
  try {
    const response = await api.post<Book>(API.books.stockEntry(bookId.value), {
      academicYearId: parsed.data.academicYearId,
      quantity: parsed.data.quantity,
      source: parsed.data.source || null,
      notes: parsed.data.notes || null,
    })
    if (response.success) {
      showSuccess('Stock entry added')
      closeStockEntryDialog()
      await loadBookDetails()
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

async function submitAdjustStock(payload: typeof adjustStockForm) {
  adjustStockForm.academicYearId = payload.academicYearId
  adjustStockForm.movementType = payload.movementType
  adjustStockForm.quantity = payload.quantity
  adjustStockForm.notes = payload.notes ?? ''

  const parsed = await validateAdjustStockWithSchema(adjustStockForm)
  if (
    !parsed.success
    || parsed.data.academicYearId === null
    || parsed.data.movementType === null
    || parsed.data.quantity === null
  ) {
    showError('Please correct stock adjustment form errors')
    return
  }
  isStockActionSubmitting.value = true
  try {
    const response = await api.post<string>(API.books.adjustStock(bookId.value), {
      academicYearId: parsed.data.academicYearId,
      movementType: parsed.data.movementType,
      quantity: parsed.data.quantity,
      notes: parsed.data.notes || null,
    })
    if (response.success) {
      showSuccess(response.message ?? 'Stock adjusted')
      closeAdjustStockDialog()
      await loadBookDetails()
      return
    }
    showError(response.message ?? 'Failed to adjust stock')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to adjust stock'))
  }
  finally {
    isStockActionSubmitting.value = false
  }
}

function handleDelete() {
  if (!book.value) return
  confirmDelete(
    `Delete "${book.value.title}"? This is a soft delete operation.`,
    async () => {
      try {
        const response = await api.del<string>(API.books.byId(bookId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Book deleted')
          await navigateTo('/admin/books')
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

function goBack() {
  void navigateTo('/admin/books')
}

function goEdit() {
  void navigateTo(`/admin/books?edit=${bookId.value}`)
}

await loadLookups()
await loadActiveAcademicYear()
resetStockForms()
await loadBookDetails()
</script>
