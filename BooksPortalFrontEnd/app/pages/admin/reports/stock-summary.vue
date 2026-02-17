<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Stock Summary
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Inventory overview by book, subject, and grade.
        </p>
      </div>
      <ReportsExportButton
        label="Export Excel"
        :loading="isExporting"
        @export="exportReport"
      />
    </div>

    <ReportsReportFilters
      @apply="applyFilters"
      @reset="resetFilters"
    >
      <div class="grid gap-3 md:grid-cols-2">
        <Select
          v-model="filters.subjectId"
          :options="subjectOptions"
          option-label="label"
          option-value="value"
          placeholder="Filter by subject"
          show-clear
          fluid
        />
        <InputText
          v-model.trim="filters.grade"
          placeholder="Filter by grade"
          fluid
          @keyup.enter="applyFilters"
        />
      </div>
    </ReportsReportFilters>

    <Card>
      <template #content>
        <CommonAdminDataTable
          :value="rows"
          :loading="isLoading"
          data-key="bookId"
          paginator
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
          current-page-report-template="Showing {first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          responsive-layout="scroll"
          @page="onPageChange"
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
            field="totalStock"
            header="Total"
            style="min-width: 7rem;"
          />
          <Column
            field="distributed"
            header="Distributed"
            style="min-width: 8rem;"
          />
          <Column
            field="withTeachers"
            header="With Teachers"
            style="min-width: 9rem;"
          />
          <Column
            field="damaged"
            header="Damaged"
            style="min-width: 8rem;"
          />
          <Column
            field="lost"
            header="Lost"
            style="min-width: 7rem;"
          />
          <Column
            header="Available"
            style="min-width: 8rem;"
          >
            <template #body="{ data }">
              <BooksStockBadge :available="data.available" />
            </template>
          </Column>
        </CommonAdminDataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { Lookup, StockSummaryReport } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'reports': 'Reports', 'stock-summary': 'Stock Summary',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const rows = ref<StockSummaryReport[]>([])
const subjects = ref<Lookup[]>([])
const isLoading = ref(false)
const isExporting = ref(false)

const filters = reactive({
  subjectId: null as number | null,
  grade: '',
})

const subjectOptions = computed(() =>
  subjects.value.map(subject => ({
    label: subject.name,
    value: subject.id,
  })),
)

async function loadLookups() {
  try {
    const response = await api.get<Lookup[]>(API.lookups.subjects)
    if (response.success) {
      subjects.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load subjects')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load subjects'))
  }
}

async function loadRows() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<StockSummaryReport>>(API.reports.stockSummary, {
      ...queryParams.value,
      subjectId: filters.subjectId ?? undefined,
      grade: filters.grade || undefined,
    })
    if (response.success) {
      rows.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load stock summary')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load stock summary'))
  }
  finally {
    isLoading.value = false
  }
}

async function exportReport() {
  isExporting.value = true
  try {
    const qs = new URLSearchParams()
    if (filters.subjectId) qs.set('subjectId', String(filters.subjectId))
    if (filters.grade) qs.set('grade', filters.grade)
    const suffix = qs.toString()
    const url = `${API.reports.exportStock}${suffix ? `?${suffix}` : ''}`
    await api.downloadBlob(url, 'stock-summary.xlsx')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to export stock summary'))
  }
  finally {
    isExporting.value = false
  }
}

async function applyFilters() {
  reset()
  await loadRows()
}

async function resetFilters() {
  filters.subjectId = null
  filters.grade = ''
  reset()
  await loadRows()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadRows()
}

onMounted(async () => {
  await Promise.all([loadLookups(), loadRows()])
})
</script>
