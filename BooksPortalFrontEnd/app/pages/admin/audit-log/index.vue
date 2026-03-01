<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div>
      <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
        Audit Log
      </h1>
      <p class="text-sm text-surface-600 dark:text-surface-400">
        Track system changes by action, entity, user, and timestamp.
      </p>
    </div>

    <Card>
      <template #content>
        <div class="grid gap-3 md:grid-cols-6">
          <InputText
            v-model.trim="filters.entityType"
            placeholder="Entity type"
            fluid
            class="md:col-span-1"
            @keyup.enter="void applyFilters()"
          />
          <InputText
            v-model.trim="filters.action"
            placeholder="Action"
            fluid
            class="md:col-span-1"
            @keyup.enter="void applyFilters()"
          />
          <Select
            v-model="filters.userId"
            :options="userOptions"
            option-label="label"
            option-value="value"
            placeholder="User"
            show-clear
            fluid
            class="md:col-span-1"
          />
          <DatePicker
            v-model="filters.from"
            placeholder="From date"
            date-format="yy-mm-dd"
            show-icon
            fluid
            class="md:col-span-1"
          />
          <DatePicker
            v-model="filters.to"
            placeholder="To date"
            date-format="yy-mm-dd"
            show-icon
            fluid
            class="md:col-span-1"
          />
          <div class="flex items-center justify-end gap-2 md:col-span-1">
            <Button
              icon="pi pi-search"
              label="Apply"
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
      </template>
    </Card>

    <Card>
      <template #content>
        <CommonAdminDataTable
          v-model:expanded-rows="expandedRows"
          :value="logs"
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
          @page="onPageChange"
        >
          <Column
            expander
            style="width: 3rem;"
          />
          <Column
            field="timestamp"
            header="Timestamp"
            style="min-width: 12rem;"
          >
            <template #body="{ data }">
              {{ formatDateTime(data.timestamp) }}
            </template>
          </Column>
          <Column
            field="action"
            header="Action"
            style="min-width: 8rem;"
          />
          <Column
            field="entityType"
            header="Entity"
            style="min-width: 10rem;"
          />
          <Column
            field="entityId"
            header="Entity ID"
            style="min-width: 8rem;"
          />
          <Column
            field="userName"
            header="User"
            style="min-width: 10rem;"
          >
            <template #body="{ data }">
              {{ data.userName ?? 'System' }}
            </template>
          </Column>
          <template #expansion="{ data }">
            <div class="grid gap-3 md:grid-cols-2">
              <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                <h3 class="mb-2 text-sm font-semibold text-surface-900 dark:text-surface-0">
                  Old Values
                </h3>
                <pre class="overflow-x-auto rounded bg-surface-100 p-2 text-xs dark:bg-surface-900">{{ formatJson(data.oldValues) }}</pre>
              </div>
              <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
                <h3 class="mb-2 text-sm font-semibold text-surface-900 dark:text-surface-0">
                  New Values
                </h3>
                <pre class="overflow-x-auto rounded bg-surface-100 p-2 text-xs dark:bg-surface-900">{{ formatJson(data.newValues) }}</pre>
              </div>
            </div>
          </template>
        </CommonAdminDataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { AuditLog, User } from '~/types/entities'
import type { PaginatedList } from '~/types/api'
import { API, PAGINATION } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    'admin': 'Admin',
    'audit-log': 'Audit Log',
  },
})

const api = useApi()
const { showError } = useAppToast()
const { page, pageSize, totalRecords, onPage, queryParams, reset } = usePagination()

const logs = ref<AuditLog[]>([])
const users = ref<User[]>([])
const isLoading = ref(false)
const expandedRows = ref<AuditLog[]>([])

const filters = reactive({
  entityType: '',
  action: '',
  userId: null as number | null,
  from: null as Date | null,
  to: null as Date | null,
})

const userOptions = computed(() =>
  users.value.map(user => ({
    label: `${user.fullName} (${user.userName})`,
    value: user.id,
  })),
)

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function toIsoDate(value: Date | null) {
  if (!value) return undefined
  const utc = new Date(Date.UTC(value.getFullYear(), value.getMonth(), value.getDate()))
  return utc.toISOString()
}

function formatJson(raw: string | null) {
  if (!raw) return 'No data'
  try {
    return JSON.stringify(JSON.parse(raw), null, 2)
  }
  catch {
    return raw
  }
}

async function loadUsers() {
  try {
    const response = await api.get<PaginatedList<User>>(API.users.base, {
      pageNumber: 1,
      pageSize: 500,
    })
    if (response.success) {
      users.value = response.data.items
      return
    }
    showError(response.message ?? 'Failed to load users')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load users'))
  }
}

async function loadAuditLogs() {
  isLoading.value = true
  try {
    const response = await api.get<PaginatedList<AuditLog>>(API.auditLogs.base, {
      ...queryParams.value,
      entityType: filters.entityType || undefined,
      action: filters.action || undefined,
      userId: filters.userId ?? undefined,
      from: toIsoDate(filters.from),
      to: toIsoDate(filters.to),
    })

    if (response.success) {
      logs.value = response.data.items
      totalRecords.value = response.data.totalCount
      expandedRows.value = []
      return
    }

    showError(response.message ?? 'Failed to load audit logs')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load audit logs'))
  }
  finally {
    isLoading.value = false
  }
}

async function applyFilters() {
  reset()
  await loadAuditLogs()
}

async function resetFilters() {
  filters.entityType = ''
  filters.action = ''
  filters.userId = null
  filters.from = null
  filters.to = null
  reset()
  await loadAuditLogs()
}

async function onPageChange(event: { page: number, rows: number }) {
  onPage(event)
  await loadAuditLogs()
}

onMounted(async () => {
  await Promise.all([loadUsers(), loadAuditLogs()])
})
</script>
