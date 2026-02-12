<template>
  <div class="flex flex-col gap-4">
    <SlipsSlipDetail
      title="Return Slip"
      :subtitle="slip ? `${slip.studentName} • ${slip.studentClassName}` : ''"
      :reference-no="slip?.referenceNo"
      :academic-year-name="slip?.academicYearName"
      :date="slip?.receivedAt"
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
          @click="navigateTo('/returns')"
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

const slip = ref<ReturnSlip | null>(null)
const isLoading = ref(false)
const isPrintLoading = ref(false)
const isCancelLoading = ref(false)

const slipId = computed(() => Number(route.params.id))

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

function cancelSlip() {
  if (!Number.isFinite(slipId.value)) return
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
    'Keep',
  )
}

onMounted(async () => {
  await loadSlip()
})
</script>
