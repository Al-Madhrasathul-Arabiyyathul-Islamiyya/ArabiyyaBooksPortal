<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div>
        <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
          Admin Dashboard
        </h1>
        <p class="mt-2 text-surface-600 dark:text-surface-400">
          Operational overview across master data, slips, and stock movement.
        </p>
      </div>
      <div class="flex items-center gap-2">
        <Tag
          v-if="totals.activeYearName"
          severity="info"
          :value="`Year: ${totals.activeYearName}`"
        />
        <ProgressSpinner
          v-if="isLoading"
          style="width: 1.5rem; height: 1.5rem;"
          stroke-width="6"
        />
      </div>
    </div>

    <Message
      v-if="errorMessage"
      severity="warn"
      :closable="false"
    >
      {{ errorMessage }}
    </Message>

    <div class="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
      <Card
        v-for="metric in metrics"
        :key="metric.key"
      >
        <template #content>
          <div class="flex items-start justify-between gap-2">
            <div>
              <p class="text-xs uppercase tracking-wide text-surface-500">
                {{ metric.label }}
              </p>
              <p class="mt-1 text-2xl font-semibold text-surface-900 dark:text-surface-0">
                {{ metric.value }}
              </p>
            </div>
            <i
              :class="metric.icon"
              class="text-xl text-primary-500"
            />
          </div>
        </template>
      </Card>
    </div>

    <div class="grid gap-4 xl:grid-cols-2">
      <Card>
        <template #title>
          Daily Overview ({{ todayLabel }})
        </template>
        <template #content>
          <Chart
            type="pie"
            :data="dailyMovementChartData"
            :options="doughnutChartOptions"
            class="h-80"
          />
        </template>
      </Card>

      <Card>
        <template #title>
          Books Moved Today
        </template>
        <template #content>
          <Chart
            type="doughnut"
            :data="dailyBooksChartData"
            :options="dailyBooksChartOptions"
            class="h-80"
          />
        </template>
      </Card>
    </div>

    <div class="grid gap-4 xl:grid-cols-2">
      <Card>
        <template #title>
          Slip Lifecycle
        </template>
        <template #content>
          <Chart
            type="bar"
            :data="lifecycleChartData"
            :options="barChartOptions"
            class="h-80"
          />
        </template>
      </Card>

      <Card>
        <template #title>
          Monthly Activity (Active Year)
        </template>
        <template #content>
          <Chart
            type="line"
            :data="activityChartData"
            :options="lineChartOptions"
            class="h-80"
          />
        </template>
      </Card>
    </div>

    <div class="grid gap-4 xl:grid-cols-[1.2fr_1fr]">
      <Card>
        <template #title>
          Top Teachers by Outstanding Books
        </template>
        <template #content>
          <Chart
            type="bar"
            :data="outstandingChartData"
            :options="horizontalBarChartOptions"
            class="h-80"
          />
        </template>
      </Card>

      <Card>
        <template #title>
          Stock Composition
        </template>
        <template #content>
          <Chart
            type="doughnut"
            :data="stockChartData"
            :options="doughnutChartOptions"
            class="h-80"
          />
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'chart.js/auto'
import type { PaginatedList } from '~/types/api'
import type {
  DistributionSlip,
  DistributionSummaryReport,
  ReturnSlip,
  StockSummaryReport,
  TeacherIssue,
  TeacherOutstandingReport,
  TeacherReturnSlip,
} from '~/types/entities'
import { SlipLifecycleStatusValue } from '~/types/enums'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
  },
})

type MetricItem = {
  key: string
  label: string
  value: string
  icon: string
}

type LifecycleTotals = {
  processing: number
  finalized: number
  cancelled: number
}

const api = useApi()
const { showError } = useAppToast()

const isLoading = ref(false)
const errorMessage = ref('')

const totals = reactive({
  students: 0,
  parents: 0,
  teachers: 0,
  books: 0,
  activeYearName: '',
  totalStock: 0,
  availableStock: 0,
})

const lifecycle = reactive({
  distributions: { processing: 0, finalized: 0, cancelled: 0 } as LifecycleTotals,
  returns: { processing: 0, finalized: 0, cancelled: 0 } as LifecycleTotals,
  teacherIssues: { processing: 0, finalized: 0, cancelled: 0 } as LifecycleTotals,
  teacherReturns: { processing: 0, finalized: 0, cancelled: 0 } as LifecycleTotals,
})

const monthly = reactive({
  labels: [] as string[],
  distributions: [] as number[],
  returns: [] as number[],
})

const outstandingByTeacher = ref<Array<{ teacherName: string, outstanding: number }>>([])
const daily = reactive({
  distributions: 0,
  returns: 0,
  teacherIssues: 0,
  teacherReturns: 0,
})
const dailyBooksMoved = ref<Array<{ label: string, quantity: number }>>([])
const todayLabel = new Date().toLocaleDateString(undefined, {
  weekday: 'short',
  day: '2-digit',
  month: 'short',
  year: 'numeric',
})

const chartPalette = computed(() => {
  if (!import.meta.client) {
    return {
      text: '#374151',
      muted: '#6b7280',
      grid: '#e5e7eb',
      processing: '#f59e0b',
      finalized: '#10b981',
      cancelled: '#ef4444',
      distributions: '#14b8a6',
      returns: '#0ea5e9',
      primary: '#10b981',
      danger: '#ef4444',
      warning: '#f59e0b',
      info: '#3b82f6',
      neutral: '#64748b',
    }
  }

  const styles = getComputedStyle(document.documentElement)
  return {
    text: styles.getPropertyValue('--p-text-color').trim() || '#374151',
    muted: styles.getPropertyValue('--p-text-muted-color').trim() || '#6b7280',
    grid: styles.getPropertyValue('--p-content-border-color').trim() || '#e5e7eb',
    processing: '#f59e0b',
    finalized: '#10b981',
    cancelled: '#ef4444',
    distributions: '#14b8a6',
    returns: '#0ea5e9',
    primary: '#10b981',
    danger: '#ef4444',
    warning: '#f59e0b',
    info: '#3b82f6',
    neutral: '#64748b',
  }
})

const metrics = computed<MetricItem[]>(() => [
  {
    key: 'people',
    label: 'Students / Parents',
    value: `${totals.students} / ${totals.parents}`,
    icon: 'pi pi-users',
  },
  {
    key: 'teachers',
    label: 'Teachers',
    value: String(totals.teachers),
    icon: 'pi pi-user',
  },
  {
    key: 'books',
    label: 'Book Titles',
    value: String(totals.books),
    icon: 'pi pi-book',
  },
  {
    key: 'stock',
    label: 'Available / Total Stock',
    value: `${totals.availableStock} / ${totals.totalStock}`,
    icon: 'pi pi-box',
  },
])

const lifecycleChartData = computed(() => ({
  labels: ['Distributions', 'Returns', 'Teacher Issues', 'Teacher Returns'],
  datasets: [
    {
      label: 'Processing',
      backgroundColor: chartPalette.value.processing,
      borderRadius: 4,
      data: [
        lifecycle.distributions.processing,
        lifecycle.returns.processing,
        lifecycle.teacherIssues.processing,
        lifecycle.teacherReturns.processing,
      ],
    },
    {
      label: 'Finalized',
      backgroundColor: chartPalette.value.finalized,
      borderRadius: 4,
      data: [
        lifecycle.distributions.finalized,
        lifecycle.returns.finalized,
        lifecycle.teacherIssues.finalized,
        lifecycle.teacherReturns.finalized,
      ],
    },
    {
      label: 'Cancelled',
      backgroundColor: chartPalette.value.cancelled,
      borderRadius: 4,
      data: [
        lifecycle.distributions.cancelled,
        lifecycle.returns.cancelled,
        lifecycle.teacherIssues.cancelled,
        lifecycle.teacherReturns.cancelled,
      ],
    },
  ],
}))

const activityChartData = computed(() => ({
  labels: monthly.labels,
  datasets: [
    {
      label: 'Distributions',
      borderColor: chartPalette.value.distributions,
      backgroundColor: `${chartPalette.value.distributions}33`,
      fill: true,
      tension: 0.35,
      data: monthly.distributions,
    },
    {
      label: 'Returns',
      borderColor: chartPalette.value.returns,
      backgroundColor: `${chartPalette.value.returns}33`,
      fill: true,
      tension: 0.35,
      data: monthly.returns,
    },
  ],
}))

const dailyMovementChartData = computed(() => ({
  labels: ['Distributions', 'Returns', 'Teacher Issues', 'Teacher Returns'],
  datasets: [
    {
      data: [
        daily.distributions,
        daily.returns,
        daily.teacherIssues,
        daily.teacherReturns,
      ],
      backgroundColor: [
        chartPalette.value.distributions,
        chartPalette.value.returns,
        chartPalette.value.warning,
        chartPalette.value.primary,
      ],
    },
  ],
}))

const dailyBooksChartData = computed(() => ({
  labels: dailyBooksMoved.value.map(item => item.label),
  datasets: [
    {
      label: 'Quantity',
      backgroundColor: [
        '#14b8a6',
        '#0ea5e9',
        '#3b82f6',
        '#6366f1',
        '#8b5cf6',
        '#d946ef',
        '#f59e0b',
        '#ef4444',
        '#64748b',
        '#22c55e',
      ],
      data: dailyBooksMoved.value.map(item => item.quantity),
    },
  ],
}))

const outstandingChartData = computed(() => ({
  labels: outstandingByTeacher.value.map(item => item.teacherName),
  datasets: [
    {
      label: 'Outstanding',
      borderRadius: 4,
      backgroundColor: chartPalette.value.warning,
      data: outstandingByTeacher.value.map(item => item.outstanding),
    },
  ],
}))

const stockChartData = computed(() => ({
  labels: ['Available', 'Distributed', 'With Teachers', 'Damaged', 'Lost'],
  datasets: [
    {
      data: [
        totals.availableStock,
        Math.max(totals.totalStock - totals.availableStock, 0),
        Math.max(totalWithTeachers.value, 0),
        Math.max(totalDamaged.value, 0),
        Math.max(totalLost.value, 0),
      ],
      backgroundColor: [
        chartPalette.value.primary,
        chartPalette.value.info,
        chartPalette.value.neutral,
        chartPalette.value.warning,
        chartPalette.value.danger,
      ],
    },
  ],
}))

const totalWithTeachers = ref(0)
const totalDamaged = ref(0)
const totalLost = ref(0)

const baseChartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      labels: {
        color: chartPalette.value.text,
      },
    },
  },
}))

const barChartOptions = computed(() => ({
  ...baseChartOptions.value,
  scales: {
    x: {
      stacked: true,
      ticks: { color: chartPalette.value.muted },
      grid: { color: chartPalette.value.grid },
    },
    y: {
      stacked: true,
      beginAtZero: true,
      ticks: { color: chartPalette.value.muted, precision: 0 },
      grid: { color: chartPalette.value.grid },
    },
  },
}))

const horizontalBarChartOptions = computed(() => ({
  ...baseChartOptions.value,
  indexAxis: 'y' as const,
  scales: {
    x: {
      beginAtZero: true,
      ticks: { color: chartPalette.value.muted, precision: 0 },
      grid: { color: chartPalette.value.grid },
    },
    y: {
      ticks: { color: chartPalette.value.muted },
      grid: { color: chartPalette.value.grid },
    },
  },
}))

const lineChartOptions = computed(() => ({
  ...baseChartOptions.value,
  scales: {
    x: {
      ticks: { color: chartPalette.value.muted },
      grid: { color: chartPalette.value.grid },
    },
    y: {
      beginAtZero: true,
      ticks: { color: chartPalette.value.muted, precision: 0 },
      grid: { color: chartPalette.value.grid },
    },
  },
}))

const doughnutChartOptions = computed(() => ({
  ...baseChartOptions.value,
  cutout: '62%',
}))

const dailyBooksChartOptions = computed(() => ({
  ...doughnutChartOptions.value,
  plugins: {
    ...doughnutChartOptions.value.plugins,
    legend: {
      position: 'bottom' as const,
      labels: {
        color: chartPalette.value.text,
        boxWidth: 12,
      },
    },
  },
}))

function normalizeLifecycle(value: unknown) {
  if (value === SlipLifecycleStatusValue.Finalized) return 'finalized' as const
  if (value === SlipLifecycleStatusValue.Cancelled) return 'cancelled' as const
  return 'processing' as const
}

function bucketLifecycle(values: Array<{ lifecycleStatus?: number }>) {
  const bucket: LifecycleTotals = {
    processing: 0,
    finalized: 0,
    cancelled: 0,
  }

  for (const item of values) {
    const key = normalizeLifecycle(item.lifecycleStatus)
    bucket[key] += 1
  }

  return bucket
}

function toMonthKey(value: string) {
  const date = new Date(value)
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  return `${date.getFullYear()}-${month}`
}

function toMonthLabel(key: string) {
  const [yearRaw, monthRaw] = key.split('-')
  const year = Number(yearRaw)
  const month = Number(monthRaw)
  if (!Number.isFinite(year) || !Number.isFinite(month)) return key
  return new Date(year, month - 1, 1).toLocaleDateString(undefined, {
    month: 'short',
    year: '2-digit',
  })
}

function buildLastMonths(count: number) {
  const now = new Date()
  const keys: string[] = []
  for (let i = count - 1; i >= 0; i -= 1) {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1)
    const month = `${d.getMonth() + 1}`.padStart(2, '0')
    keys.push(`${d.getFullYear()}-${month}`)
  }
  return keys
}

function isSameLocalDay(value: string, reference: Date) {
  const d = new Date(value)
  return (
    d.getFullYear() === reference.getFullYear()
    && d.getMonth() === reference.getMonth()
    && d.getDate() === reference.getDate()
  )
}

async function loadTotal(endpoint: string, extraParams: Record<string, unknown> = {}) {
  const response = await api.get<PaginatedList<unknown>>(endpoint, {
    pageNumber: 1,
    pageSize: 1,
    ...extraParams,
  })
  if (!response.success) {
    throw new Error(response.message ?? `Failed to load ${endpoint}`)
  }
  return response.data.totalCount
}

async function loadAllPages<T>(endpoint: string, extraParams: Record<string, unknown> = {}, pageSize = 100) {
  const aggregated: T[] = []
  let pageNumber = 1
  let hasNext = true

  while (hasNext) {
    const response = await api.get<PaginatedList<T>>(endpoint, {
      pageNumber,
      pageSize,
      ...extraParams,
    })
    if (!response.success) {
      throw new Error(response.message ?? `Failed to load ${endpoint}`)
    }

    aggregated.push(...response.data.items)
    hasNext = response.data.hasNext
    pageNumber += 1
  }

  return aggregated
}

async function loadDashboard() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const activeYear = await api.get<{ id: number, name?: string }>(API.academicYears.active)
    const activeYearId = activeYear.success ? activeYear.data.id : undefined
    totals.activeYearName = activeYear.success ? (activeYear.data.name ?? 'Active Year') : 'Active Year'

    const [
      studentCount,
      parentCount,
      teacherCount,
      bookCount,
      stockRows,
      distributions,
      returns,
      teacherIssues,
      teacherReturns,
      teacherOutstanding,
      distributionSummary,
    ] = await Promise.all([
      loadTotal(API.students.base),
      loadTotal(API.parents.base),
      loadTotal(API.teachers.base),
      loadTotal(API.books.base),
      loadAllPages<StockSummaryReport>(API.reports.stockSummary, {}, 100),
      loadAllPages<DistributionSlip>(API.distributions.base, { includeCancelled: true, academicYearId: activeYearId }, 100),
      loadAllPages<ReturnSlip>(API.returns.base, { includeCancelled: true, academicYearId: activeYearId }, 100),
      loadAllPages<TeacherIssue>(API.teacherIssues.base, { includeCancelled: true, academicYearId: activeYearId }, 100),
      loadAllPages<TeacherReturnSlip>(API.teacherReturns.base, { includeCancelled: true, academicYearId: activeYearId }, 100),
      loadAllPages<TeacherOutstandingReport>(API.reports.teacherOutstanding, {}, 100),
      activeYearId
        ? loadAllPages<DistributionSummaryReport>(API.reports.distributionSummary, { academicYearId: activeYearId }, 100)
        : Promise.resolve([]),
    ])

    totals.students = studentCount
    totals.parents = parentCount
    totals.teachers = teacherCount
    totals.books = bookCount

    totals.totalStock = stockRows.reduce((sum, row) => sum + row.totalStock, 0)
    totals.availableStock = stockRows.reduce((sum, row) => sum + row.available, 0)
    totalWithTeachers.value = stockRows.reduce((sum, row) => sum + row.withTeachers, 0)
    totalDamaged.value = stockRows.reduce((sum, row) => sum + row.damaged, 0)
    totalLost.value = stockRows.reduce((sum, row) => sum + row.lost, 0)

    lifecycle.distributions = bucketLifecycle(distributions)
    lifecycle.returns = bucketLifecycle(returns)
    lifecycle.teacherIssues = bucketLifecycle(teacherIssues)
    lifecycle.teacherReturns = bucketLifecycle(teacherReturns)

    const now = new Date()
    daily.distributions = distributions.filter(row => isSameLocalDay(row.issuedAt, now)).length
    daily.returns = returns.filter(row => isSameLocalDay(row.receivedAt, now)).length
    daily.teacherIssues = teacherIssues.filter(row => isSameLocalDay(row.issuedAt, now)).length
    daily.teacherReturns = teacherReturns.filter(row => isSameLocalDay(row.receivedAt, now)).length

    const dailyBookMap = new Map<string, number>()
    for (const row of distributions) {
      if (!isSameLocalDay(row.issuedAt, now)) continue
      for (const item of row.items) {
        const key = `${item.bookCode} - ${item.bookTitle}`
        dailyBookMap.set(key, (dailyBookMap.get(key) ?? 0) + item.quantity)
      }
    }
    for (const row of returns) {
      if (!isSameLocalDay(row.receivedAt, now)) continue
      for (const item of row.items) {
        const key = `${item.bookCode} - ${item.bookTitle}`
        dailyBookMap.set(key, (dailyBookMap.get(key) ?? 0) + item.quantity)
      }
    }
    for (const row of teacherIssues) {
      if (!isSameLocalDay(row.issuedAt, now)) continue
      for (const item of row.items) {
        const key = `${item.bookCode} - ${item.bookTitle}`
        dailyBookMap.set(key, (dailyBookMap.get(key) ?? 0) + item.quantity)
      }
    }
    for (const row of teacherReturns) {
      if (!isSameLocalDay(row.receivedAt, now)) continue
      for (const item of row.items) {
        const key = `${item.bookCode} - ${item.bookTitle}`
        dailyBookMap.set(key, (dailyBookMap.get(key) ?? 0) + item.quantity)
      }
    }
    dailyBooksMoved.value = Array.from(dailyBookMap.entries())
      .map(([label, quantity]) => ({ label, quantity }))
      .sort((a, b) => b.quantity - a.quantity)
      .slice(0, 10)

    const monthKeys = buildLastMonths(6)
    const distributionsByMonth = new Map(monthKeys.map(key => [key, 0]))
    const returnsByMonth = new Map(monthKeys.map(key => [key, 0]))

    for (const row of distributionSummary) {
      const key = toMonthKey(row.issuedAt)
      if (distributionsByMonth.has(key)) {
        distributionsByMonth.set(key, (distributionsByMonth.get(key) ?? 0) + 1)
      }
    }

    for (const row of returns) {
      const key = toMonthKey(row.receivedAt)
      if (returnsByMonth.has(key)) {
        returnsByMonth.set(key, (returnsByMonth.get(key) ?? 0) + 1)
      }
    }

    monthly.labels = monthKeys.map(toMonthLabel)
    monthly.distributions = monthKeys.map(key => distributionsByMonth.get(key) ?? 0)
    monthly.returns = monthKeys.map(key => returnsByMonth.get(key) ?? 0)

    const outstandingMap = new Map<string, number>()
    for (const row of teacherOutstanding) {
      outstandingMap.set(row.teacherName, (outstandingMap.get(row.teacherName) ?? 0) + row.outstanding)
    }
    outstandingByTeacher.value = Array.from(outstandingMap.entries())
      .map(([teacherName, outstanding]) => ({ teacherName, outstanding }))
      .sort((a, b) => b.outstanding - a.outstanding)
      .slice(0, 8)
  }
  catch (error: unknown) {
    const message = getFriendlyErrorMessage(error, 'Failed to load dashboard overview')
    errorMessage.value = message
    showError(message)
  }
  finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await loadDashboard()
})
</script>
