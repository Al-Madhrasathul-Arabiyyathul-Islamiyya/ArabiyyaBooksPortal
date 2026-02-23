<template>
  <div class="relative">
    <Button
      v-tooltip.bottom="'Bulk import jobs'"
      :icon="runningJobs.length > 0 ? 'pi pi-spin pi-sync' : 'pi pi-inbox'"
      text
      severity="secondary"
      rounded
      @click="togglePanel"
    />
    <Badge
      v-if="runningJobs.length > 0 || hasUnreadCompleted"
      :value="String(runningJobs.length || completedCount)"
      :severity="runningJobs.length > 0 ? 'info' : 'success'"
      class="bulk-job-badge"
    />

    <Popover
      ref="popoverRef"
      class="w-[28rem]"
    >
      <div class="flex items-center justify-between pb-2">
        <h3 class="text-sm font-semibold">
          Bulk Import Jobs
        </h3>
        <div class="flex items-center gap-2">
          <Button
            v-if="completedCount > 0"
            label="Clear Completed"
            size="small"
            text
            icon="pi pi-times"
            @click="clearCompleted"
          />
          <Button
            label="Refresh"
            size="small"
            text
            icon="pi pi-refresh"
            @click="pollOnce"
          />
        </div>
      </div>

      <div
        v-if="jobs.length === 0"
        class="py-6 text-center text-sm text-surface-500"
      >
        No active or recent bulk jobs.
      </div>

      <div
        v-else
        class="max-h-80 space-y-2 overflow-auto pr-1"
      >
        <div
          v-for="job in jobs"
          :key="job.id"
          class="rounded-lg border border-surface-200 p-3 dark:border-surface-700"
        >
          <div class="mb-1 flex items-center justify-between gap-2">
            <span class="text-sm font-medium">{{ job.entityLabel }} import</span>
            <div class="flex items-center gap-2">
              <Tag
                :severity="statusSeverity(job.status)"
                :value="job.status"
              />
              <Button
                v-if="canDismiss(job.status)"
                v-tooltip.left="'Dismiss notification'"
                icon="pi pi-times"
                size="small"
                text
                rounded
                aria-label="Dismiss notification"
                @click="dismiss(job.id)"
              />
            </div>
          </div>
          <p class="mb-2 text-xs text-surface-500">
            {{ job.fileName }}
          </p>
          <div class="text-xs text-surface-600 dark:text-surface-300">
            <template v-if="job.totalRows > 0">
              {{ job.processedRows }} / {{ job.totalRows }} rows
            </template>
            <template v-else>
              Preparing...
            </template>
          </div>
          <ProgressBar
            :value="progressPercent(job)"
            class="my-2 h-2"
          />
          <Message
            v-if="job.error"
            severity="error"
            size="small"
            :closable="false"
          >
            {{ job.error }}
          </Message>
          <div class="flex justify-end">
            <Button
              v-if="job.reportReady"
              label="Download Report"
              icon="pi pi-download"
              size="small"
              text
              @click="download(job)"
            />
          </div>
        </div>
      </div>
    </Popover>
  </div>
</template>

<script setup lang="ts">
const popoverRef = ref()
const { jobs, runningJobs, hasUnreadCompleted, pollOnce, downloadReport, dismissJob, clearCompleted } = useBulkImportJobs()

const completedCount = computed(() =>
  jobs.value.filter(job => job.status === 'Completed' || job.status === 'Failed').length,
)

function statusSeverity(status: string) {
  if (status === 'Completed')
    return 'success'
  if (status === 'Failed')
    return 'danger'
  return 'info'
}

function progressPercent(job: { processedRows: number, totalRows: number }) {
  if (!job.totalRows)
    return 0
  return Math.min(100, Math.round((job.processedRows / job.totalRows) * 100))
}

function canDismiss(status: string) {
  return status === 'Completed' || status === 'Failed'
}

function togglePanel(event: Event) {
  popoverRef.value.toggle(event)
}

function dismiss(jobId: string) {
  dismissJob(jobId)
}

async function download(job: {
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
}) {
  await downloadReport(job)
}
</script>

<style scoped>
.bulk-job-badge {
  position: absolute;
  top: 0.1rem;
  right: 0.1rem;
}
</style>
