import type { BulkImportJobSnapshot } from '~/types/api'

interface BulkImportJobEndpoints {
  commitAsync: string
  jobStatus: (id: string) => string
  jobReport: (id: string) => string
}

interface BulkImportJobItem {
  id: string
  entityLabel: string
  fileName: string
  status: string
  processedRows: number
  totalRows: number
  reportReady: boolean
  error: string | null
  statusEndpoint: (id: string) => string
  reportEndpoint: (id: string) => string
  startedAtUtc: string
  completedAtUtc: string | null
  lastUpdatedAt: number
  acknowledged: boolean
}

const POLL_INTERVAL_MS = 2500
let pollTimer: ReturnType<typeof setInterval> | null = null

export function useBulkImportJobs() {
  const api = useApi()
  const jobs = useState<BulkImportJobItem[]>('bulk-import-jobs', () => [])
  const isPanelVisible = useState<boolean>('bulk-import-jobs-panel-visible', () => false)

  const runningJobs = computed(() => jobs.value.filter(job => job.status === 'Queued' || job.status === 'Running'))
  const hasUnreadCompleted = computed(() =>
    jobs.value.some(job => (job.status === 'Completed' || job.status === 'Failed') && !job.acknowledged),
  )

  function upsertJob(
    snapshot: BulkImportJobSnapshot,
    endpoints: Pick<BulkImportJobItem, 'statusEndpoint' | 'reportEndpoint'>,
    fallback: Pick<BulkImportJobItem, 'entityLabel' | 'fileName'>,
  ) {
    const existing = jobs.value.find(job => job.id === snapshot.id)
    const next: BulkImportJobItem = {
      id: snapshot.id,
      entityLabel: existing?.entityLabel ?? fallback.entityLabel,
      fileName: existing?.fileName ?? fallback.fileName,
      status: snapshot.status,
      processedRows: snapshot.processedRows ?? 0,
      totalRows: snapshot.totalRows ?? 0,
      reportReady: snapshot.reportReady ?? false,
      error: snapshot.error ?? null,
      statusEndpoint: existing?.statusEndpoint ?? endpoints.statusEndpoint,
      reportEndpoint: existing?.reportEndpoint ?? endpoints.reportEndpoint,
      startedAtUtc: snapshot.startedAtUtc ?? existing?.startedAtUtc ?? new Date().toISOString(),
      completedAtUtc: snapshot.completedAtUtc ?? null,
      lastUpdatedAt: Date.now(),
      acknowledged: existing?.acknowledged ?? false,
    }

    if (!existing) {
      jobs.value.unshift(next)
      return
    }

    const index = jobs.value.findIndex(job => job.id === snapshot.id)
    if (index !== -1) {
      jobs.value[index] = {
        ...next,
        acknowledged: next.status === 'Completed' || next.status === 'Failed' ? false : existing.acknowledged,
      }
    }
  }

  function startPolling() {
    if (pollTimer || !import.meta.client) return

    pollTimer = setInterval(() => {
      void pollOnce()
    }, POLL_INTERVAL_MS)
  }

  function stopPollingIfIdle() {
    if (runningJobs.value.length > 0) return
    if (!pollTimer) return
    clearInterval(pollTimer)
    pollTimer = null
  }

  async function pollOnce() {
    const active = [...runningJobs.value]
    if (!active.length) {
      stopPollingIfIdle()
      return
    }

    for (const job of active) {
      try {
        const response = await api.get<BulkImportJobSnapshot>(job.statusEndpoint(job.id))
        if (!response.success) continue
        upsertJob(
          response.data,
          { statusEndpoint: job.statusEndpoint, reportEndpoint: job.reportEndpoint },
          { entityLabel: job.entityLabel, fileName: job.fileName },
        )
      }
      catch {
        // Keep silent to avoid intrusive polling noise.
      }
    }

    stopPollingIfIdle()
  }

  async function queueJob(file: File, entityLabel: string, endpoints: BulkImportJobEndpoints) {
    const formData = new FormData()
    formData.append('file', file)

    const response = await api.post<{ jobId: string }>(endpoints.commitAsync, formData)
    if (!response.success || !response.data?.jobId) {
      return response
    }

    jobs.value.unshift({
      id: response.data.jobId,
      entityLabel,
      fileName: file.name,
      status: 'Queued',
      processedRows: 0,
      totalRows: 0,
      reportReady: false,
      error: null,
      statusEndpoint: endpoints.jobStatus,
      reportEndpoint: endpoints.jobReport,
      startedAtUtc: new Date().toISOString(),
      completedAtUtc: null,
      lastUpdatedAt: Date.now(),
      acknowledged: false,
    })

    startPolling()
    return response
  }

  async function downloadReport(job: BulkImportJobItem) {
    const safeEntity = job.entityLabel.toLowerCase().replace(/\s+/g, '-')
    const fileName = `${safeEntity}-bulk-import-report-${job.id}.xlsx`
    await api.downloadBlob(job.reportEndpoint(job.id), fileName)
  }

  function markAllAcknowledged() {
    jobs.value = jobs.value.map(job => ({
      ...job,
      acknowledged: true,
    }))
  }

  function dismissJob(jobId: string) {
    const job = jobs.value.find(item => item.id === jobId)
    if (!job) return
    if (job.status === 'Queued' || job.status === 'Running') return
    jobs.value = jobs.value.filter(item => item.id !== jobId)
  }

  function clearCompleted() {
    jobs.value = jobs.value.filter(job => job.status === 'Queued' || job.status === 'Running')
  }

  function togglePanel() {
    isPanelVisible.value = !isPanelVisible.value
    if (isPanelVisible.value) {
      markAllAcknowledged()
    }
  }

  return {
    jobs: readonly(jobs),
    runningJobs,
    hasUnreadCompleted,
    isPanelVisible,
    queueJob,
    pollOnce,
    downloadReport,
    togglePanel,
    markAllAcknowledged,
    dismissJob,
    clearCompleted,
  }
}
