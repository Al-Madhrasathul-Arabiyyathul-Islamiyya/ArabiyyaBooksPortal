<template>
  <Dialog
    v-model:visible="visibleModel"
    modal
    :header="`${entityLabel} Bulk Import`"
    :style="{ width: '46rem' }"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-wrap items-center gap-2">
        <Button
          label="Download Template"
          icon="pi pi-download"
          severity="secondary"
          text
          @click="$emit('template')"
        />
        <input
          ref="fileInput"
          type="file"
          accept=".xlsx"
          class="hidden"
          @change="onFileChange"
        >
        <Button
          label="Select File"
          icon="pi pi-upload"
          severity="secondary"
          @click="fileInput?.click()"
        />
        <span class="text-sm text-surface-600 dark:text-surface-300">{{ selectedFileName || 'No file selected' }}</span>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <Button
          label="Validate"
          icon="pi pi-check-circle"
          :loading="isValidating"
          :disabled="!selectedFile || isCommitting"
          @click="emitValidate"
        />
        <Button
          label="Commit"
          icon="pi pi-save"
          severity="success"
          :loading="isCommitting"
          :disabled="!selectedFile || !report?.canCommit || isValidating"
          @click="emitCommit"
        />
      </div>

      <Message
        v-if="errorMessage"
        severity="error"
        :closable="false"
      >
        {{ errorMessage }}
      </Message>

      <Card v-if="report">
        <template #content>
          <div class="grid grid-cols-2 gap-3 text-sm md:grid-cols-4">
            <div><span class="font-semibold">Total:</span> {{ report.totalRows }}</div>
            <div><span class="font-semibold">Valid:</span> {{ report.validRows }}</div>
            <div><span class="font-semibold">Invalid:</span> {{ report.invalidRows }}</div>
            <div><span class="font-semibold">Inserted:</span> {{ report.insertedRows }}</div>
          </div>
          <div class="mt-3 text-sm">
            <span class="font-semibold">Failed:</span> {{ report.failedRows }}
          </div>

          <DataTable
            v-if="report.issues.length"
            class="mt-4"
            :value="report.issues"
            size="small"
            scrollable
            scroll-height="14rem"
          >
            <Column
              field="rowNumber"
              header="Row"
              style="width: 5rem;"
            />
            <Column
              field="field"
              header="Field"
            />
            <Column
              field="code"
              header="Code"
            />
            <Column
              field="message"
              header="Message"
            />
          </DataTable>
        </template>
      </Card>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import type { BulkImportReport } from '~/types/api'

const props = defineProps<{
  visible: boolean
  entityLabel: string
  report: BulkImportReport | null
  errorMessage: string
  isValidating: boolean
  isCommitting: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'validate' | 'commit', file: File): void
  (e: 'template'): void
}>()

const fileInput = ref<HTMLInputElement | null>(null)
const selectedFile = ref<File | null>(null)

const selectedFileName = computed(() => selectedFile.value?.name ?? '')

const visibleModel = computed({
  get: () => props.visible,
  set: (value: boolean) => emit('update:visible', value),
})

function onFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  selectedFile.value = target.files?.[0] ?? null
}

function emitValidate() {
  if (!selectedFile.value) return
  emit('validate', selectedFile.value)
}

function emitCommit() {
  if (!selectedFile.value) return
  emit('commit', selectedFile.value)
}
</script>
