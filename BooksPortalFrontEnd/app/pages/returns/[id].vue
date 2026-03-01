<template>
  <div class="flex flex-col gap-4">
    <SlipsSlipDetail
      title="Return Slip"
      :subtitle="slip ? `${slip.studentName} · ${slip.studentClassName}` : ''"
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
          @click="navigateTo('/returns')"
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
        <Column
          field="condition"
          header="Condition"
          style="min-width: 9rem;"
        >
          <template #body="{ data }">
            {{ conditionLabels[data.condition] ?? data.condition }}
          </template>
        </Column>
        <Column
          field="conditionNotes"
          header="Condition Notes"
          style="min-width: 12rem;"
        />
      </template>
    </SlipsSlipDetail>
  </div>
</template>

<script setup lang="ts">
import type { ReturnSlip } from '~/types/entities'
import { API } from '~/utils/constants'
import { conditionLabels } from '~/utils/formatters'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    returns: 'Returns',
    id: 'Details',
  },
})

const route = useRoute()
const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()
const { getLifecycleLabel, getLifecycleIcon, getLifecycleBadgeClass, isProcessing } = useSlipLifecycle()
const { guard, isOperationBlocked } = useOperationReadinessGuard()

const slip = ref<ReturnSlip | null>(null)
const isLoading = ref(false)
const isPrintLoading = ref(false)
const isCancelLoading = ref(false)
const isFinalizeLoading = ref(false)

const slipId = computed(() => Number(route.params.id))
const lifecycleLabel = computed(() => getLifecycleLabel(slip.value?.lifecycleStatus))
const lifecycleIcon = computed(() => getLifecycleIcon(slip.value?.lifecycleStatus))
const lifecycleBadgeClass = computed(() => getLifecycleBadgeClass(slip.value?.lifecycleStatus))
const canFinalize = computed(() => Boolean(slip.value) && isProcessing(slip.value?.lifecycleStatus) && !isOperationBlocked.value)
const canCancel = computed(() => Boolean(slip.value) && isProcessing(slip.value?.lifecycleStatus) && !isOperationBlocked.value)

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
    showError('Invalid return slip route.')
    return
  }

  isLoading.value = true
  try {
    const response = await api.get<ReturnSlip>(API.returns.byId(slipId.value))
    if (response.success) {
      slip.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load return slip')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load return slip'))
  }
  finally {
    isLoading.value = false
  }
}

async function printSlip() {
  if (!Number.isFinite(slipId.value)) return

  isPrintLoading.value = true
  try {
    await api.downloadBlob(API.returns.print(slipId.value), `return-${slipId.value}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print return slip'))
  }
  finally {
    isPrintLoading.value = false
  }
}

async function finalizeSlip() {
  if (!Number.isFinite(slipId.value) || !canFinalize.value) return
  if (!await guard('finalize this return slip')) return

  confirmAction(
    'Finalize this return slip? Finalized slips cannot be cancelled.',
    async () => {
      isFinalizeLoading.value = true
      try {
        const response = await api.post<string>(API.returns.finalize(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Return slip finalized')
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
    'Finalize Return Slip',
    'Finalize',
    'Close',
  )
}

async function cancelSlip() {
  if (!Number.isFinite(slipId.value) || !canCancel.value) return
  if (!await guard('cancel this return slip')) return

  confirmAction(
    'Cancel this return slip? This will reverse stock movements.',
    async () => {
      isCancelLoading.value = true
      try {
        const response = await api.del<string>(API.returns.byId(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Return slip cancelled')
          void navigateTo('/returns')
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
    'Cancel Return Slip',
    'Cancel Slip',
    'Close',
  )
}

onMounted(async () => {
  await loadSlip()
})
</script>
