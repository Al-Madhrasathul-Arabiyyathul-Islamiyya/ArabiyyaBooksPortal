<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Teacher Returns
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Process partial and full returns for teacher-issued slips.
        </p>
      </div>
    </div>

    <Card>
      <template #content>
        <div class="grid gap-3 md:grid-cols-4">
          <Select
            v-model="filters.academicYearId"
            :options="academicYearOptions"
            option-label="label"
            option-value="value"
            placeholder="Academic year"
            show-clear
            fluid
            @change="applyFilters"
          />
          <FormsSearchInput
            id="teacher-returns-teacher-search"
            v-model="filters.teacherSearch"
            name="teacher-returns-teacher-search"
            persist-key="bp.search.operations.teacher-returns.teacher"
            placeholder="Teacher search"
            @search="applyFilters"
          />
          <Select
            v-model="filters.status"
            :options="statusOptions"
            option-label="label"
            option-value="value"
            placeholder="Status"
            show-clear
            fluid
            @change="applyFilters"
          />
          <div class="flex items-center gap-2 text-sm text-surface-500">
            <i class="pi pi-info-circle" />
            Select an issue and process returned items.
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <DataTable
          :value="filteredSlips"
          :loading="isLoading"
          data-key="id"
          paginator
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          :rows-per-page-options="[...PAGINATION.pageSizeOptions]"
          current-page-report-template="Showing {first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          responsive-layout="scroll"
          @row-click="onRowClick"
          @page="onPageChange"
        >
          <Column
            field="referenceNo"
            header="Issue Reference"
            style="min-width: 10rem;"
          />
          <Column
            field="teacherName"
            header="Teacher"
            style="min-width: 14rem;"
          />
          <Column
            field="issuedAt"
            header="Issued At"
            style="min-width: 11rem;"
          >
            <template #body="{ data }">
              {{ formatDateTime(data.issuedAt) }}
            </template>
          </Column>
          <Column
            header="Status"
            style="min-width: 10rem;"
          >
            <template #body="{ data }">
              <Tag
                :value="teacherIssueStatusLabels[data.status] ?? String(data.status)"
                :severity="teacherIssueStatusSeverities[data.status] ?? 'secondary'"
              />
            </template>
          </Column>
          <Column
            header="Outstanding"
            style="min-width: 8rem;"
          >
            <template #body="{ data }">
              {{ getOutstandingQuantity(data) }}
            </template>
          </Column>
          <Column
            header="Actions"
            :exportable="false"
            style="width: 8rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-replay"
                  tooltip="Process return"
                  severity="warn"
                  :disabled="getOutstandingQuantity(data) === 0"
                  @click.stop="openReturnDialog(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-eye"
                  tooltip="Open issue details"
                  @click.stop="navigateTo(`/teacher-issues/${data.id}`)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="isReturnDialogVisible"
      modal
      header="Process Teacher Return"
      :style="{ width: '62rem' }"
    >
      <div class="grid gap-4">
        <div
          v-if="selectedIssue"
          class="rounded-lg border border-surface-200 p-3 text-sm dark:border-surface-700"
        >
          <div class="font-semibold">
            {{ selectedIssue.referenceNo }} - {{ selectedIssue.teacherName }}
          </div>
          <div class="text-surface-500">
            Outstanding: {{ getOutstandingQuantity(selectedIssue) }}
          </div>
        </div>

        <DataTable
          :value="returnRows"
          size="small"
          data-key="teacherIssueItemId"
          responsive-layout="scroll"
        >
          <Column
            field="bookCode"
            header="Book Code"
            style="min-width: 8rem;"
          />
          <Column
            field="bookTitle"
            header="Book Title"
            style="min-width: 14rem;"
          />
          <Column
            field="outstandingQuantity"
            header="Outstanding"
            style="min-width: 8rem;"
          />
          <Column
            header="Return Qty"
            style="min-width: 10rem;"
          >
            <template #body="{ data }">
              <InputNumber
                v-model="data.quantity"
                input-class="w-24 text-center"
                show-buttons
                button-layout="vertical"
                :min="0"
                :max="data.outstandingQuantity"
                :step="1"
              />
            </template>
          </Column>
        </DataTable>

        <FormsFormField
          label="Notes"
          field-id="teacherReturnNotes"
          :error="returnErrors.notes"
        >
          <Textarea
            id="teacherReturnNotes"
            v-model.trim="returnForm.notes"
            rows="3"
            fluid
          />
        </FormsFormField>

        <Message
          v-if="returnError"
          severity="error"
          :closable="false"
        >
          {{ returnError }}
        </Message>

        <div class="flex justify-end gap-2">
          <Button
            label="Cancel"
            severity="secondary"
            text
            @click="closeReturnDialog"
          />
          <Button
            label="Create Return Slip"
            :loading="isSubmittingReturn"
            @click="submitReturn"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import type { Lookup, TeacherIssue } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { z } from 'zod/v4'
import { API, PAGINATION } from '~/utils/constants'
import {
  teacherIssueStatusLabels,
  teacherIssueStatusOptions,
  teacherIssueStatusSeverities,
} from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    'teacher-returns': 'Teacher Returns',
  },
})

type ReturnRow = {
  teacherIssueItemId: number
  bookId: number
  bookCode: string
  bookTitle: string
  outstandingQuantity: number
  quantity: number
}

const api = useApi()
const route = useRoute()
const { showError, showSuccess } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const slips = ref<TeacherIssue[]>([])
const academicYears = ref<Lookup[]>([])
const isLoading = ref(false)
const isSubmittingReturn = ref(false)
const isReturnDialogVisible = ref(false)
const selectedIssue = ref<TeacherIssue | null>(null)
const returnRows = ref<ReturnRow[]>([])

const filters = reactive({
  academicYearId: null as number | null,
  teacherSearch: '',
  status: null as number | null,
})

const ReturnSchema = z.object({
  notes: z.string(),
})

const {
  state: returnForm,
  errors: returnErrors,
  globalError: returnError,
  setGlobalError: setReturnError,
} = useAppValidation(
  {
    notes: '',
  },
  ReturnSchema,
)

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
  })),
)

const statusOptions = computed(() =>
  teacherIssueStatusOptions.map(item => ({
    label: item.label,
    value: item.value,
  })),
)

const filteredSlips = computed(() => {
  if (filters.status === null) return slips.value
  return slips.value.filter(slip => slip.status === filters.status)
})

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function getOutstandingQuantity(slip: TeacherIssue) {
  return slip.items.reduce((sum, item) => sum + item.outstandingQuantity, 0)
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
    showError(getFriendlyErrorMessage(error, 'Failed to load filters'))
  }
}

async function loadSlips() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<TeacherIssue>>(API.teacherIssues.base, {
      ...queryParams.value,
      academicYearId: filters.academicYearId ?? undefined,
      search: filters.teacherSearch || undefined,
    })
    if (response.success) {
      slips.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load teacher issues')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher issues'))
  }
  finally {
    isLoading.value = false
  }
}

async function applyFilters() {
  reset()
  await loadSlips()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadSlips()
}

function onRowClick(event: { data: TeacherIssue }) {
  if (getOutstandingQuantity(event.data) === 0) {
    void navigateTo(`/teacher-issues/${event.data.id}`)
    return
  }
  openReturnDialog(event.data)
}

function buildReturnRows(issue: TeacherIssue): ReturnRow[] {
  return issue.items
    .filter(item => item.outstandingQuantity > 0)
    .map(item => ({
      teacherIssueItemId: item.id,
      bookId: item.bookId,
      bookCode: item.bookCode,
      bookTitle: item.bookTitle,
      outstandingQuantity: item.outstandingQuantity,
      quantity: 0,
    }))
}

function openReturnDialog(issue: TeacherIssue) {
  selectedIssue.value = issue
  returnRows.value = buildReturnRows(issue)
  returnForm.notes = ''
  setReturnError('')
  isReturnDialogVisible.value = true
}

function closeReturnDialog() {
  isReturnDialogVisible.value = false
  selectedIssue.value = null
  returnRows.value = []
  returnForm.notes = ''
  setReturnError('')
}

async function submitReturn() {
  if (!selectedIssue.value) return

  const selectedItems = returnRows.value
    .filter(row => row.quantity > 0)
    .map(row => ({
      teacherIssueItemId: row.teacherIssueItemId,
      quantity: row.quantity,
    }))

  if (selectedItems.length === 0) {
    setReturnError('Enter return quantity for at least one item.')
    return
  }

  isSubmittingReturn.value = true
  try {
    const response = await api.post<{ referenceNo: string }>(
      API.teacherIssues.return(selectedIssue.value.id),
      {
        notes: returnForm.notes.trim() ? returnForm.notes.trim() : null,
        items: selectedItems,
      },
    )
    if (response.success) {
      showSuccess(response.message ?? `Teacher return created (${response.data.referenceNo})`)
      closeReturnDialog()
      await loadSlips()
      return
    }
    setReturnError(response.message ?? 'Failed to process teacher return')
  }
  catch (error: unknown) {
    setReturnError(getFriendlyErrorMessage(error, 'Failed to process teacher return'))
  }
  finally {
    isSubmittingReturn.value = false
  }
}

watch(
  () => route.query.issueId,
  (value) => {
    const issueId = Number(value)
    if (!Number.isFinite(issueId)) return
    const issue = slips.value.find(item => item.id === issueId)
    if (issue && getOutstandingQuantity(issue) > 0) {
      openReturnDialog(issue)
    }
  },
)

onMounted(async () => {
  await loadLookups()
  await loadSlips()

  const issueId = Number(route.query.issueId)
  if (!Number.isFinite(issueId)) return
  const issue = slips.value.find(item => item.id === issueId)
  if (issue && getOutstandingQuantity(issue) > 0) {
    openReturnDialog(issue)
  }
})
</script>
