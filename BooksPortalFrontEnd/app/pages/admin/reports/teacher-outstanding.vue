<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Teacher Outstanding
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Track unreturned quantities from teacher issue slips.
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
          v-model="filters.teacherId"
          :options="teacherOptions"
          option-label="label"
          option-value="value"
          placeholder="Filter by teacher"
          show-clear
          filter
          fluid
        />
      </div>
    </ReportsReportFilters>

    <Card>
      <template #content>
        <DataTable
          :value="rows"
          :loading="isLoading"
          data-key="referenceNo"
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
          <Column field="teacherName" header="Teacher" style="min-width: 12rem;" />
          <Column field="bookCode" header="Book Code" style="min-width: 9rem;" />
          <Column field="bookTitle" header="Book Title" style="min-width: 14rem;" />
          <Column field="quantity" header="Issued" style="min-width: 7rem;" />
          <Column field="returnedQuantity" header="Returned" style="min-width: 8rem;" />
          <Column field="outstanding" header="Outstanding" style="min-width: 8rem;" />
          <Column field="status" header="Status" style="min-width: 10rem;">
            <template #body="{ data }">
              <Tag
                :value="teacherIssueStatusLabels[data.status] ?? String(data.status)"
                :severity="teacherIssueStatusSeverities[data.status] ?? 'secondary'"
              />
            </template>
          </Column>
          <Column field="issuedAt" header="Issued At" style="min-width: 11rem;">
            <template #body="{ data }">
              {{ new Date(data.issuedAt).toLocaleString() }}
            </template>
          </Column>
          <Column field="expectedReturnDate" header="Expected Return" style="min-width: 11rem;">
            <template #body="{ data }">
              {{ data.expectedReturnDate ? new Date(data.expectedReturnDate).toLocaleDateString() : '-' }}
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { Teacher, TeacherOutstandingReport } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { teacherIssueStatusLabels, teacherIssueStatusSeverities } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'reports': 'Reports', 'teacher-outstanding': 'Teacher Outstanding',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const rows = ref<TeacherOutstandingReport[]>([])
const teachers = ref<Teacher[]>([])
const isLoading = ref(false)
const isExporting = ref(false)

const filters = reactive({
  teacherId: null as number | null,
})

const teacherOptions = computed(() =>
  teachers.value.map(teacher => ({
    label: `${teacher.fullName} (${teacher.nationalId ?? teacher.email ?? 'N/A'})`,
    value: teacher.id,
  })),
)

async function loadTeachers() {
  try {
    const response = await api.get<Teacher[]>(API.teachers.base)
    if (response.success) {
      teachers.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load teachers')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teachers'))
  }
}

async function loadRows() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<TeacherOutstandingReport>>(API.reports.teacherOutstanding, {
      ...queryParams.value,
      teacherId: filters.teacherId ?? undefined,
    })
    if (response.success) {
      rows.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load teacher outstanding report')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher outstanding report'))
  }
  finally {
    isLoading.value = false
  }
}

async function exportReport() {
  isExporting.value = true
  try {
    const qs = new URLSearchParams()
    if (filters.teacherId) qs.set('teacherId', String(filters.teacherId))
    const suffix = qs.toString()
    await api.downloadBlob(
      `${API.reports.exportTeacherOutstanding}${suffix ? `?${suffix}` : ''}`,
      'teacher-outstanding.xlsx',
    )
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to export teacher outstanding report'))
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
  filters.teacherId = null
  reset()
  await loadRows()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadRows()
}

onMounted(async () => {
  await Promise.all([loadTeachers(), loadRows()])
})
</script>
