<template>
  <div class="flex flex-col gap-4">
    <SlipsSlipDetail
      title="Teacher Return Slip"
      :subtitle="slip?.teacherName ?? ''"
      :reference-no="slip?.referenceNo"
      :academic-year-name="slip?.academicYearName"
      :date="slip?.receivedAt"
      :notes="detailNotes"
      :items="slip?.items ?? []"
      :loading="isLoading"
    >
      <template #header-meta>
        <span
          v-if="slip && lifecycleLabel"
          class="inline-flex items-center gap-2 rounded-md border px-2.5 py-1 text-xs font-semibold"
          :class="lifecycleBadgeClass"
        >
          <i :class="lifecycleIcon" />
          {{ lifecycleLabel }}
        </span>
      </template>

      <template #actions>
        <Button
          label="Done"
          icon="pi pi-check"
          severity="secondary"
          text
          @click="navigateTo('/teacher-returns')"
        />
        <Button
          v-if="canFinalize"
          label="Finalize"
          icon="pi pi-check-circle"
          severity="success"
          outlined
          :loading="isFinalizeLoading"
          @click="finalizeSlip"
        />
        <Button
          label="Open Issuance"
          icon="pi pi-file"
          severity="info"
          outlined
          @click="navigateTo(`/teacher-issues/${slip?.teacherIssueId}`)"
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
          :disabled="!canCancel"
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
          header="Quantity"
          style="min-width: 8rem;"
        />
      </template>
    </SlipsSlipDetail>
  </div>
</template>

<script setup lang="ts">
import type { TeacherReturnSlip } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    'teacher-returns': 'Teacher Returns',
    'id': 'Details',
  },
})

const route = useRoute()
const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()
const { getLifecycleLabel, getLifecycleIcon, getLifecycleBadgeClass, isProcessing } = useSlipLifecycle()

const slip = ref<TeacherReturnSlip | null>(null)
const isLoading = ref(false)
const isPrintLoading = ref(false)
const isCancelLoading = ref(false)
const isFinalizeLoading = ref(false)

const slipId = computed(() => Number(route.params.id))
const lifecycleLabel = computed(() => getLifecycleLabel(slip.value?.lifecycleStatus))
const lifecycleIcon = computed(() => getLifecycleIcon(slip.value?.lifecycleStatus))
const lifecycleBadgeClass = computed(() => getLifecycleBadgeClass(slip.value?.lifecycleStatus))
const canFinalize = computed(() => Boolean(slip.value) && isProcessing(slip.value?.lifecycleStatus))
const canCancel = computed(() => Boolean(slip.value) && isProcessing(slip.value?.lifecycleStatus))

const detailNotes = computed(() => {
  if (!slip.value) return null
  const summary: string[] = [`Lifecycle: ${lifecycleLabel.value}`]
  if (slip.value.finalizedAt) {
    summary.push(`Finalized: ${new Date(slip.value.finalizedAt).toLocaleString()}`)
  }
  if (slip.value.cancelledAt) {
    summary.push(`Cancelled: ${new Date(slip.value.cancelledAt).toLocaleString()}`)
  }
  if (slip.value.notes) {
    summary.push(`Notes: ${slip.value.notes}`)
  }
  return summary.join(' | ')
})

async function loadSlip() {
  if (!Number.isFinite(slipId.value)) {
    showError('Invalid teacher return route.')
    return
  }

  isLoading.value = true
  try {
    const response = await api.get<TeacherReturnSlip>(API.teacherReturns.byId(slipId.value))
    if (response.success) {
      slip.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load teacher return slip')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load teacher return slip'))
  }
  finally {
    isLoading.value = false
  }
}

async function printSlip() {
  if (!Number.isFinite(slipId.value)) return

  isPrintLoading.value = true
  try {
    await api.downloadBlob(API.teacherReturns.print(slipId.value), `teacher-return-${slipId.value}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print teacher return slip'))
  }
  finally {
    isPrintLoading.value = false
  }
}

function finalizeSlip() {
  if (!Number.isFinite(slipId.value) || !canFinalize.value) return

  confirmAction(
    'Finalize this teacher return slip? Finalized slips cannot be cancelled.',
    async () => {
      isFinalizeLoading.value = true
      try {
        const response = await api.post<string>(API.teacherReturns.finalize(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Teacher return slip finalized')
          await loadSlip()
          return
        }
        showError(response.message ?? 'Failed to finalize slip')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to finalize slip'))
      }
      finally {
        isFinalizeLoading.value = false
      }
    },
    'Finalize Teacher Return Slip',
    'Finalize',
    'Close',
  )
}

function cancelSlip() {
  if (!Number.isFinite(slipId.value) || !canCancel.value) return

  confirmAction(
    'Cancel this teacher return slip? This will reverse return stock movements.',
    async () => {
      isCancelLoading.value = true
      try {
        const response = await api.del<string>(API.teacherReturns.byId(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Teacher return slip cancelled')
          void navigateTo('/teacher-returns')
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
    'Cancel Teacher Return Slip',
    'Cancel Slip',
    'Close',
  )
}

onMounted(async () => {
  await loadSlip()
})
</script>
