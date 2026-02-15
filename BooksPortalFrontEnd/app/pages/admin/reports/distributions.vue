<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Distribution Summary
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Distribution activity across students within an academic year.
        </p>
      </div>
      <ReportsExportButton
        label="Export Excel"
        :loading="isExporting"
        :disabled="!filters.academicYearId"
        @export="exportReport"
      />
    </div>

    <ReportsReportFilters
      @apply="applyFilters"
      @reset="resetFilters"
    >
      <div class="grid gap-3 md:grid-cols-3">
        <Select
          v-model="filters.academicYearId"
          :options="academicYearOptions"
          option-label="label"
          option-value="value"
          placeholder="Academic year"
          show-clear
          fluid
        />
        <DatePicker
          v-model="filters.from"
          placeholder="From date"
          show-icon
          date-format="dd/mm/yy"
          fluid
        />
        <DatePicker
          v-model="filters.to"
          placeholder="To date"
          show-icon
          date-format="dd/mm/yy"
          fluid
        />
      </div>
    </ReportsReportFilters>

    <Message
      v-if="!filters.academicYearId"
      severity="warn"
      :closable="false"
    >
      Academic year is required for this report.
    </Message>

    <Card>
      <template #content>
        <DataTable
          :value="rows"
          :loading="isLoading"
          data-key="slipId"
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
          <Column field="referenceNo" header="Reference" style="min-width: 10rem;" />
          <Column field="studentName" header="Student" style="min-width: 14rem;" />
          <Column field="studentIndexNo" header="Index No" style="min-width: 9rem;" />
          <Column field="parentName" header="Parent" style="min-width: 12rem;" />
          <Column field="issuedAt" header="Issued At" style="min-width: 11rem;">
            <template #body="{ data }">
              {{ new Date(data.issuedAt).toLocaleString() }}
            </template>
          </Column>
          <Column field="totalBooks" header="Books" style="min-width: 7rem;" />
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { DistributionSummaryReport, Lookup } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', reports: 'Reports', distributions: 'Distributions',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const rows = ref<DistributionSummaryReport[]>([])
const academicYears = ref<Lookup[]>([])
const isLoading = ref(false)
const isExporting = ref(false)

const filters = reactive({
  academicYearId: null as number | null,
  from: null as Date | null,
  to: null as Date | null,
})

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
  })),
)

function toIsoDate(value: Date | null) {
  if (!value) return undefined
  const utc = new Date(Date.UTC(value.getFullYear(), value.getMonth(), value.getDate()))
  return utc.toISOString()
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
    if (activeResponse.success && filters.academicYearId === null) {
      filters.academicYearId = activeResponse.data.id
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load academic years'))
  }
}

async function loadRows() {
  if (!filters.academicYearId) {
    rows.value = []
    totalRecords.value = 0
    return
  }
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<DistributionSummaryReport>>(API.reports.distributionSummary, {
      ...queryParams.value,
      academicYearId: filters.academicYearId,
      from: toIsoDate(filters.from),
      to: toIsoDate(filters.to),
    })
    if (response.success) {
      rows.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load distribution summary')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load distribution summary'))
  }
  finally {
    isLoading.value = false
  }
}

async function exportReport() {
  if (!filters.academicYearId) {
    showError('Academic year is required for export')
    return
  }
  isExporting.value = true
  try {
    const qs = new URLSearchParams({ academicYearId: String(filters.academicYearId) })
    if (filters.from) qs.set('from', toIsoDate(filters.from) as string)
    if (filters.to) qs.set('to', toIsoDate(filters.to) as string)
    await api.downloadBlob(`${API.reports.exportDistribution}?${qs.toString()}`, 'distribution-summary.xlsx')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to export distribution summary'))
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
  filters.academicYearId = null
  filters.from = null
  filters.to = null
  reset()
  await loadRows()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadRows()
}

onMounted(async () => {
  await loadLookups()
  await loadRows()
})
</script>
