<template>
  <div class="flex flex-col gap-4">
    <div>
      <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
        Student History
      </h1>
      <p class="text-sm text-surface-600 dark:text-surface-400">
        View all distribution and return movements for a selected student.
      </p>
    </div>

    <div class="flex flex-col gap-3">
      <SlipsStudentLookup
        v-model="selectedStudent"
        :show-create-action="false"
        @selected="onStudentSelected"
        @cleared="onStudentCleared"
      />
      <div class="flex justify-end gap-2">
        <Button
          icon="pi pi-search"
          label="Apply"
          :disabled="!selectedStudent"
          @click="applyFilters"
        />
        <Button
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="resetFilters"
        />
      </div>
    </div>

    <Message
      v-if="!selectedStudent"
      severity="warn"
      :closable="false"
    >
      Select a student to load history.
    </Message>

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
          <Column field="type" header="Type" style="min-width: 8rem;" />
          <Column field="referenceNo" header="Reference" style="min-width: 10rem;" />
          <Column field="date" header="Date" style="min-width: 11rem;">
            <template #body="{ data }">
              {{ new Date(data.date).toLocaleString() }}
            </template>
          </Column>
          <Column field="bookCode" header="Book Code" style="min-width: 9rem;" />
          <Column field="bookTitle" header="Book Title" style="min-width: 14rem;" />
          <Column field="quantity" header="Quantity" style="min-width: 8rem;" />
          <Column field="condition" header="Condition" style="min-width: 10rem;">
            <template #body="{ data }">
              {{ data.condition ?? '-' }}
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { Student, StudentHistoryReport } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin', 'reports': 'Reports', 'student-history': 'Student History',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const rows = ref<StudentHistoryReport[]>([])
const selectedStudent = ref<Student | null>(null)
const isLoading = ref(false)

async function loadRows() {
  if (!selectedStudent.value) {
    rows.value = []
    totalRecords.value = 0
    return
  }

  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<StudentHistoryReport>>(
      API.reports.studentHistory(selectedStudent.value.id),
      { ...queryParams.value },
    )

    if (response.success) {
      rows.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load student history')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load student history'))
  }
  finally {
    isLoading.value = false
  }
}

function onStudentSelected(student: Student) {
  selectedStudent.value = student
}

function onStudentCleared() {
  selectedStudent.value = null
  rows.value = []
  totalRecords.value = 0
}

async function applyFilters() {
  reset()
  await loadRows()
}

async function resetFilters() {
  selectedStudent.value = null
  rows.value = []
  totalRecords.value = 0
  reset()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadRows()
}
</script>
