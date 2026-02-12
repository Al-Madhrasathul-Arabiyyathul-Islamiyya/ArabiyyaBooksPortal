<template>
  <div class="flex flex-col gap-4">
    <SlipsSlipDetail
      title="Distribution Slip"
      :subtitle="slip ? `${slip.studentName} • ${slip.studentClassName}` : ''"
      :reference-no="slip?.referenceNo"
      :academic-year-name="slip?.academicYearName"
      :date="slip?.issuedAt"
      :notes="slip?.notes"
      :items="slip?.items ?? []"
      :loading="isLoading"
    >
      <template #actions>
        <Button
          label="Done"
          icon="pi pi-check"
          severity="secondary"
          text
          @click="navigateTo('/distribution')"
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
import type { DistributionSlip } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  breadcrumb: {
    distribution: 'Distributions',
    id: 'Details',
  },
})

const route = useRoute()
const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()

const slip = ref<DistributionSlip | null>(null)
const isLoading = ref(false)
const isPrintLoading = ref(false)
const isCancelLoading = ref(false)

const slipId = computed(() => Number(route.params.id))

async function loadSlip() {
  if (!Number.isFinite(slipId.value)) {
    showError('Invalid distribution slip route.')
    return
  }
  isLoading.value = true
  try {
    const response = await api.get<DistributionSlip>(API.distributions.byId(slipId.value))
    if (response.success) {
      slip.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load distribution slip')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load distribution slip'))
  }
  finally {
    isLoading.value = false
  }
}

async function printSlip() {
  if (!Number.isFinite(slipId.value)) return
  isPrintLoading.value = true
  try {
    await api.downloadBlob(API.distributions.print(slipId.value), `distribution-${slipId.value}.pdf`, true)
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to print distribution slip'))
  }
  finally {
    isPrintLoading.value = false
  }
}

function cancelSlip() {
  if (!Number.isFinite(slipId.value)) return
  confirmAction(
    'Cancel this distribution slip? This will reverse stock movements.',
    async () => {
      isCancelLoading.value = true
      try {
        const response = await api.del<string>(API.distributions.byId(slipId.value))
        if (response.success) {
          showSuccess(response.message ?? 'Distribution slip cancelled')
          void navigateTo('/distribution')
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
    'Cancel Distribution Slip',
    'Cancel Slip',
    'Keep',
  )
}

onMounted(async () => {
  await loadSlip()
})
</script>
