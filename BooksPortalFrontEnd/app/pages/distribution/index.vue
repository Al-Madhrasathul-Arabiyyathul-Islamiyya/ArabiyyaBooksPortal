<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Distributions
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Track issued textbook slips and create new distributions.
        </p>
      </div>
      <Button
        label="New Distribution"
        icon="pi pi-plus"
        @click="navigateTo('/distribution/create')"
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
            id="distribution-student-search"
            v-model="filters.studentSearch"
            name="distribution-student-search"
            persist-key="bp.search.operations.distribution.student"
            placeholder="Student search"
            @search="applyFilters"
          />
          <div class="flex gap-2 md:col-span-2">
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
            style="min-width: 10rem;"
          />
          <Column
            field="studentName"
            header="Student"
            style="min-width: 14rem;"
          />
          <Column
            field="studentClassName"
            header="Class"
            style="min-width: 10rem;"
          />
          <Column
            field="parentName"
            header="Parent"
            style="min-width: 12rem;"
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
            style="width: 8rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-eye"
                  tooltip="Open details"
                  @click.stop="navigateTo(`/distribution/${data.id}`)"
                />
                <CommonIconActionButton
                  icon="pi pi-print"
                  tooltip="Print slip"
                  :loading="printLoadingSlipId === data.id"
                  @click.stop="printSlip(data.id)"
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
import type { DistributionSlip, Lookup } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  title: 'Students Distribution',
  breadcrumb: {
    distribution: 'Distributions',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()
const { getLifecycleLabel, getLifecycleSeverity } = useSlipLifecycle()
const { guard, isOperationBlocked } = useOperationReadinessGuard()

const slips = ref<DistributionSlip[]>([])
const academicYears = ref<Lookup[]>([])
const activeAcademicYearId = ref<number | null>(null)
const isLoading = ref(false)
const isReferenceLoading = ref(false)
const printLoadingSlipId = ref<number | null>(null)
const referenceMode = ref(false)
const referenceSearch = ref('')

const filters = reactive({
  academicYearId: null as number | null,
  studentSearch: '',
})

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
  })),
)

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

async function loadLookups() {
  if (isOperationBlocked.value) return

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
      if (filters.academicYearId === null) {
        filters.academicYearId = activeResponse.data.id
      }
    }
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load filters'))
  }
}

async function loadSlips() {
  if (isOperationBlocked.value) return

  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<DistributionSlip>>(API.distributions.base, {
      ...queryParams.value,
      academicYearId: filters.academicYearId ?? undefined,
      studentId: undefined,
      search: filters.studentSearch || undefined,
      includeCancelled: false,
    })
    if (response.success) {
      slips.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to load distributions')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load distributions'))
  }
  finally {
    isLoading.value = false
  }
}

async function searchByReference() {
  if (!referenceSearch.value) return
  isReferenceLoading.value = true
  try {
    const response = await api.get<DistributionSlip>(API.distributions.byReference(referenceSearch.value))
    if (response.success) {
      slips.value = [response.data]
      totalRecords.value = 1
      referenceMode.value = true
      return
    }
    showError(response.message ?? 'Distribution reference not found')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Distribution reference not found'))
  }
  finally {
    isReferenceLoading.value = false
  }
}

async function printSlip(slipId: number) {
  printLoadingSlipId.value = slipId
  try {
    await api.downloadBlob(API.distributions.print(slipId), `distribution-${slipId}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print distribution slip'))
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

function onRowClick(event: { data: DistributionSlip }) {
  void navigateTo(`/distribution/${event.data.id}`)
}

onMounted(async () => {
  const allowed = await guard('load distributions')
  if (!allowed) return

  await loadLookups()
  await loadSlips()
})
</script>
