<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Teacher Returns
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Track teacher return slips and process additional returns for finalized issues.
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
          <div class="flex gap-2 md:col-span-2">
            <InputText
              v-model.trim="referenceSearch"
              placeholder="Find by return reference no"
              class="flex-1"
              @keyup.enter="searchByReference"
            />
            <Button
              icon="pi pi-search"
              label="Find"
              :loading="isReferenceLoading"
              @click="searchByReference"
            />
            <Button
              v-if="referenceMode"
              icon="pi pi-refresh"
              severity="secondary"
              outlined
              @click="clearReferenceSearch"
            />
          </div>
          <div class="flex items-center gap-2 text-sm text-surface-500">
            <i class="pi pi-info-circle" />
            Open a teacher issue and click "Process Return" to create a new return slip.
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <DataTable
          :value="slips"
          :loading="isLoading || isReferenceLoading"
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
            header="Reference"
            style="min-width: 11rem;"
          />
          <Column
            field="teacherName"
            header="Teacher"
            style="min-width: 14rem;"
          />
          <Column
            field="receivedAt"
            header="Received At"
            style="min-width: 11rem;"
          >
            <template #body="{ data }">
              {{ formatDateTime(data.receivedAt) }}
            </template>
          </Column>
          <Column
            header="Items"
            style="min-width: 6rem;"
          >
            <template #body="{ data }">
              {{ data.items.length }}
            </template>
          </Column>
          <Column
            header="Status"
            style="min-width: 9rem;"
          >
            <template #body="{ data }">
              <Tag
                :value="getLifecycleLabel(data.lifecycleStatus)"
                :severity="getLifecycleSeverity(data.lifecycleStatus)"
              />
            </template>
          </Column>
          <Column
            header="Actions"
            :exportable="false"
            style="width: 12rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-eye"
                  tooltip="Open return details"
                  @click.stop="navigateTo(`/teacher-returns/${data.id}`)"
                />
                <CommonIconActionButton
                  icon="pi pi-print"
                  tooltip="Print return slip"
                  :loading="printLoadingSlipId === data.id"
                  @click.stop="printSlip(data.id)"
                />
                <CommonIconActionButton
                  icon="pi pi-file"
                  tooltip="Open associated issue"
                  severity="info"
                  @click.stop="navigateTo(`/teacher-issues/${data.teacherIssueId}`)"
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
import type { Lookup, TeacherIssue, TeacherReturnSlip } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { z } from 'zod/v4'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  title: 'Teacher Returns',
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
const { getLifecycleLabel, getLifecycleSeverity, isFinalized } = useSlipLifecycle()

const slips = ref<TeacherReturnSlip[]>([])
const academicYears = ref<Lookup[]>([])
const isLoading = ref(false)
const isReferenceLoading = ref(false)
const printLoadingSlipId = ref<number | null>(null)
const referenceMode = ref(false)
const referenceSearch = ref('')

const isSubmittingReturn = ref(false)
const isReturnDialogVisible = ref(false)
const selectedIssue = ref<TeacherIssue | null>(null)
const returnRows = ref<ReturnRow[]>([])

const filters = reactive({
  academicYearId: null as number | null,
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
    const response = await api.get<PaginatedList<TeacherReturnSlip>>(API.teacherReturns.base, {
      ...queryParams.value,
      academicYearId: filters.academicYearId ?? undefined,
      includeCancelled: false,
    })
    if (response.success) {
      slips.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load teacher returns')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher returns'))
  }
  finally {
    isLoading.value = false
  }
}

async function searchByReference() {
  if (!referenceSearch.value) return
  isReferenceLoading.value = true
  try {
    const response = await api.get<TeacherReturnSlip>(API.teacherReturns.byReference(referenceSearch.value))
    if (response.success) {
      slips.value = [response.data]
      totalRecords.value = 1
      referenceMode.value = true
      return
    }
    showError(response.message ?? 'Teacher return reference not found')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Teacher return reference not found'))
  }
  finally {
    isReferenceLoading.value = false
  }
}

async function printSlip(slipId: number) {
  printLoadingSlipId.value = slipId
  try {
    await api.downloadBlob(API.teacherReturns.print(slipId), `teacher-return-${slipId}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print teacher return slip'))
  }
  finally {
    printLoadingSlipId.value = null
  }
}

async function clearReferenceSearch() {
  referenceSearch.value = ''
  referenceMode.value = false
  reset()
  await loadSlips()
}

async function applyFilters() {
  referenceMode.value = false
  reset()
  await loadSlips()
}

async function onPageChange(event: { page: number, rows: number }) {
  if (referenceMode.value) return
  onPage(event)
  await loadSlips()
}

function onRowClick(event: { data: TeacherReturnSlip }) {
  void navigateTo(`/teacher-returns/${event.data.id}`)
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

async function openDialogFromIssueId(issueId: number) {
  try {
    const response = await api.get<TeacherIssue>(API.teacherIssues.byId(issueId))
    if (!response.success) {
      showError(response.message ?? 'Failed to load teacher issue for return processing')
      return
    }

    const issue = response.data
    if (!isFinalized(issue.lifecycleStatus)) {
      showError('Only finalized teacher issues can be processed for returns.')
      return
    }
    if (getOutstandingQuantity(issue) <= 0) {
      showError('Selected teacher issue has no outstanding items.')
      return
    }

    openReturnDialog(issue)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher issue for return processing'))
  }
}

async function submitReturn() {
  if (!selectedIssue.value) return

  const teacherIssueId = selectedIssue.value.id
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
    const response = await api.post(
      API.teacherIssues.return(teacherIssueId),
      {
        notes: returnForm.notes.trim() ? returnForm.notes.trim() : null,
        items: selectedItems,
      },
    )
    if (response.success) {
      showSuccess(response.message ?? 'Teacher return slip created')
      closeReturnDialog()
      await loadSlips()
      const latestResponse = await api.get<PaginatedList<TeacherReturnSlip>>(API.teacherReturns.base, {
        pageNumber: 1,
        pageSize: 1,
        teacherIssueId,
        includeCancelled: true,
      })

      if (latestResponse.success && latestResponse.data.items.length > 0) {
        void navigateTo(`/teacher-returns/${latestResponse.data.items[0]!.id}`)
      }
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

onMounted(async () => {
  await loadLookups()
  await loadSlips()

  const issueId = Number(route.query.issueId)
  if (Number.isFinite(issueId)) {
    await openDialogFromIssueId(issueId)
  }
})
</script>
