<template>
  <div class="flex h-full min-h-0 flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Master Data Bulk Upload
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Upload academic years, keystages, grades, and class sections using a JSON template.
        </p>
      </div>
      <Button
        label="Download JSON Template"
        icon="pi pi-download"
        :loading="isDownloadingTemplate"
        @click="downloadTemplate"
      />
    </div>

    <Card>
      <template #content>
        <div class="grid gap-4">
          <Message
            severity="info"
            :closable="false"
          >
            Upload a <code>.json</code> file generated from the template. Existing records are upserted by natural keys.
          </Message>

          <FileUpload
            mode="basic"
            choose-label="Choose JSON File"
            choose-icon="pi pi-upload"
            accept="application/json,.json"
            :custom-upload="true"
            :auto="false"
            @select="onFileSelected"
          />

          <div
            v-if="selectedFileName"
            class="text-sm text-surface-600 dark:text-surface-400"
          >
            Selected: <span class="font-medium">{{ selectedFileName }}</span>
          </div>

          <div class="flex justify-end gap-2">
            <Button
              label="Clear"
              severity="secondary"
              text
              :disabled="!selectedFile"
              @click="clearSelection"
            />
            <Button
              label="Upload & Apply"
              icon="pi pi-check"
              :loading="isSubmitting"
              :disabled="!selectedFile"
              @click="submit"
            />
          </div>
        </div>
      </template>
    </Card>

    <Card v-if="result">
      <template #title>
        Upload Result
      </template>
      <template #content>
        <div class="grid gap-3">
          <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            <Tag
              :value="`Created: ${result.createdCount}`"
              severity="success"
            />
            <Tag
              :value="`Updated: ${result.updatedCount}`"
              severity="info"
            />
            <Tag
              :value="`Skipped: ${result.skippedCount}`"
              severity="secondary"
            />
            <Tag
              :value="`Failed: ${result.failedCount}`"
              severity="danger"
            />
            <Tag
              :value="`Total Rows: ${result.results.length}`"
              severity="contrast"
            />
          </div>

          <CommonAdminDataTable
            :value="result.results"
            data-key="path"
            size="small"
          >
            <Column
              field="entityType"
              header="Entity"
            />
            <Column
              field="operation"
              header="Operation"
            />
            <Column
              field="path"
              header="Path"
            />
            <Column
              field="message"
              header="Message"
            />
          </CommonAdminDataTable>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { API, ROLES } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

type HierarchyBulkResultRow = {
  path: string
  entityType: string
  operation: string
  message: string
}

type HierarchyBulkResponse = {
  createdCount: number
  updatedCount: number
  skippedCount: number
  failedCount: number
  results: HierarchyBulkResultRow[]
}

definePageMeta({
  layout: 'admin',
  middleware: ['admin', 'role'],
  roles: [ROLES.superAdmin],
  breadcrumb: {
    'admin': 'Admin',
    'settings': 'Settings',
    'master-data-hierarchy-bulk': 'Master Data Bulk Upload',
  },
})

const api = useApi()
const { showSuccess, showError } = useAppToast()

const selectedFile = ref<File | null>(null)
const selectedFileName = ref('')
const isSubmitting = ref(false)
const isDownloadingTemplate = ref(false)
const result = ref<HierarchyBulkResponse | null>(null)

function onFileSelected(event: { files: File[] }) {
  const file = event.files?.[0]
  if (!file) return
  selectedFile.value = file
  selectedFileName.value = file.name
}

function clearSelection() {
  selectedFile.value = null
  selectedFileName.value = ''
}

async function downloadTemplate() {
  isDownloadingTemplate.value = true
  try {
    await api.downloadBlob(API.importTemplates.masterDataHierarchy, 'master-data-hierarchy-template.json')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to download template'))
  }
  finally {
    isDownloadingTemplate.value = false
  }
}

async function submit() {
  if (!selectedFile.value) return
  isSubmitting.value = true
  try {
    const text = await selectedFile.value.text()
    const payload = JSON.parse(text)
    const response = await api.post<HierarchyBulkResponse>(API.masterDataHierarchy.bulkUpsert, payload)
    if (response.success) {
      result.value = response.data
      showSuccess(response.message ?? 'Hierarchy bulk upload completed')
      return
    }
    showError(response.message ?? 'Hierarchy bulk upload failed')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Invalid JSON or failed to upload hierarchy payload'))
  }
  finally {
    isSubmitting.value = false
  }
}
</script>
