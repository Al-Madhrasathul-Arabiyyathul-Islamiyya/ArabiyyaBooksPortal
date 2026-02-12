<template>
  <div class="flex flex-col gap-4">
    <SlipsSlipDetail
      title="Teacher Issue Slip"
      :subtitle="slip?.teacherName ?? ''"
      :reference-no="slip?.referenceNo"
      :academic-year-name="slip?.academicYearName"
      :date="slip?.issuedAt"
      :notes="slipNotes"
      :items="slip?.items ?? []"
      :loading="isLoading"
    >
      <template #actions>
        <Button
          label="Done"
          icon="pi pi-check"
          severity="secondary"
          text
          @click="navigateTo('/teacher-issues')"
        />
        <Button
          label="Process Return"
          icon="pi pi-replay"
          severity="warn"
          outlined
          :disabled="totalOutstanding <= 0"
          @click="navigateTo(`/teacher-returns?issueId=${slipId}`)"
        />
        <Button
          label="Print"
          icon="pi pi-print"
          outlined
          :loading="isPrintLoading"
          @click="printSlip"
        />
        <Button
          label="Cancel Slip"
          icon="pi pi-times-circle"
          severity="danger"
          outlined
          :disabled="totalOutstanding <= 0"
          :loading="isCancelLoading"
          @click="cancelSlip"
        />
      </template>
      <template #items-columns>
        <Column
          field="bookCode"
          header="Book Code"
          style="min-width: 9rem;"
        />
        <Column
          field="bookTitle"
          header="Book Title"
          style="min-width: 14rem;"
        />
        <Column
          field="quantity"
          header="Issued"
          style="min-width: 8rem;"
        />
        <Column
          field="returnedQuantity"
          header="Returned"
          style="min-width: 8rem;"
        />
        <Column
          field="outstandingQuantity"
          header="Outstanding"
          style="min-width: 8rem;"
        />
      </template>
    </SlipsSlipDetail>
  </div>
</template>

<script setup lang="ts">
import type { TeacherIssue } from '~/types/entities'
import { API } from '~/utils/constants'
import { teacherIssueStatusLabels } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    'teacher-issues': 'Teacher Issues',
    'id': 'Details',
  },
})

const route = useRoute()
const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()

const slip = ref<TeacherIssue | null>(null)
const isLoading = ref(false)
const isPrintLoading = ref(false)
const isCancelLoading = ref(false)

const slipId = computed(() => Number(route.params.id))

const totalOutstanding = computed(() =>
  slip.value?.items.reduce((sum, item) => sum + item.outstandingQuantity, 0) ?? 0,
)

const slipNotes = computed(() => {
  if (!slip.value) return null
  const status = teacherIssueStatusLabels[slip.value.status] ?? String(slip.value.status)
  const summary = [`Status: ${status}`, `Outstanding: ${totalOutstanding.value}`]
  if (slip.value.notes) summary.push(`Notes: ${slip.value.notes}`)
  return summary.join(' | ')
})

async function loadSlip() {
  if (!Number.isFinite(slipId.value)) {
    showError('Invalid teacher issue route.')
    return
  }
  isLoading.value = true
  try {
    const response = await api.get<TeacherIssue>(API.teacherIssues.byId(slipId.value))
    if (response.success) {
      slip.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load teacher issue slip')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher issue slip'))
  }
  finally {
    isLoading.value = false
  }
}

async function printSlip() {
  if (!Number.isFinite(slipId.value)) return
  isPrintLoading.value = true
  try {
    await api.downloadBlob(API.teacherIssues.print(slipId.value), `teacher-issue-${slipId.value}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print teacher issue slip'))
  }
  finally {
    isPrintLoading.value = false
  }
}

function cancelSlip() {
  if (!Number.isFinite(slipId.value)) return
  confirmAction(
    'Cancel this teacher issue slip? This will reverse stock movements.',
    async () => {
      isCancelLoading.value = true
      try {
        const response = await api.del<string>(API.teacherIssues.byId(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Teacher issue cancelled')
          void navigateTo('/teacher-issues')
          return
        }
        showError(response.message ?? 'Failed to cancel slip')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to cancel slip'))
      }
      finally {
        isCancelLoading.value = false
      }
    },
    'Cancel Teacher Issue Slip',
    'Cancel Slip',
    'Keep',
  )
}

onMounted(async () => {
  await loadSlip()
})
</script>
