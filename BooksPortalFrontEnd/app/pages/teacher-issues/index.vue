<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Teacher Issues
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Track issued books for teachers and open detail slips.
        </p>
      </div>
      <Button
        label="New Teacher Issue"
        icon="pi pi-plus"
        @click="navigateTo('/teacher-issues/create')"
      />
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
            id="teacher-issues-teacher-search"
            v-model="filters.teacherSearch"
            name="teacher-issues-teacher-search"
            persist-key="bp.search.operations.teacher-issues.teacher"
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
          <div class="flex gap-2">
            <InputText
              v-model.trim="referenceSearch"
              placeholder="Find by reference no"
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
        </div>
      </template>
    </Card>

    <Card>
      <template #content>
        <DataTable
          :value="filteredSlips"
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
            field="expectedReturnDate"
            header="Expected Return"
            style="min-width: 11rem;"
          >
            <template #body="{ data }">
              {{ data.expectedReturnDate ? formatDate(data.expectedReturnDate) : '-' }}
            </template>
          </Column>
          <Column
            header="Issue Status"
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
            style="width: 8rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-eye"
                  tooltip="Open details"
                  @click.stop="navigateTo(`/teacher-issues/${data.id}`)"
                />
                <CommonIconActionButton
                  icon="pi pi-replay"
                  tooltip="Process return"
                  severity="warn"
                  :disabled="!canProcessReturn(data)"
                  @click.stop="navigateTo(`/teacher-returns?issueId=${data.id}`)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { Lookup, TeacherIssue } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import {
  teacherIssueStatusLabels,
  teacherIssueStatusOptions,
  teacherIssueStatusSeverities,
} from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    'teacher-issues': 'Teacher Issues',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()
const { getLifecycleLabel, getLifecycleSeverity, isFinalized } = useSlipLifecycle()

const slips = ref<TeacherIssue[]>([])
const academicYears = ref<Lookup[]>([])
const isLoading = ref(false)
const isReferenceLoading = ref(false)
const referenceMode = ref(false)
const referenceSearch = ref('')

const filters = reactive({
  academicYearId: null as number | null,
  teacherSearch: '',
  status: null as number | null,
})

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

function formatDate(value: string) {
  return new Date(value).toLocaleDateString()
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function getOutstandingQuantity(slip: TeacherIssue) {
  return slip.items.reduce((sum, item) => sum + item.outstandingQuantity, 0)
}

function canProcessReturn(slip: TeacherIssue) {
  return isFinalized(slip.lifecycleStatus) && getOutstandingQuantity(slip) > 0
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
      includeCancelled: false,
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

async function searchByReference() {
  if (!referenceSearch.value) return
  isReferenceLoading.value = true
  try {
    const localMatch = slips.value.find(slip => slip.referenceNo === referenceSearch.value)
    if (localMatch) {
      slips.value = [localMatch]
      totalRecords.value = 1
      referenceMode.value = true
      return
    }

    const response = await api.get<PaginatedList<TeacherIssue>>(API.teacherIssues.base, {
      pageNumber: 1,
      pageSize: 50,
      academicYearId: filters.academicYearId ?? undefined,
      search: referenceSearch.value,
    })
    if (response.success) {
      const match = response.data.items.find(slip => slip.referenceNo === referenceSearch.value)
        ?? response.data.items[0]
      if (!match) {
        showError('Teacher issue reference not found')
        return
      }
      slips.value = [match]
      totalRecords.value = 1
      referenceMode.value = true
      return
    }
    showError(response.message ?? 'Teacher issue reference not found')
  }
  catch {
    showError('Teacher issue reference not found')
  }
  finally {
    isReferenceLoading.value = false
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

function onRowClick(event: { data: TeacherIssue }) {
  void navigateTo(`/teacher-issues/${event.data.id}`)
}

onMounted(async () => {
  await loadLookups()
  await loadSlips()
})
</script>
