<template>
  <div class="flex flex-col gap-4">
    <PhasePlaceholder
      title="Books Management"
      description="Books catalog and stock management screens will be implemented in Phase 4 under admin."
    />

    <Card>
      <template #title>
        Bulk Import
      </template>
      <template #content>
        <div class="flex flex-wrap items-center gap-2">
          <Button
            label="Bulk Import Books"
            icon="pi pi-file-import"
            severity="secondary"
            outlined
            @click="isBulkDialogVisible = true"
          />
          <Button
            label="Download Template"
            icon="pi pi-download"
            severity="secondary"
            text
            @click="downloadBookTemplate"
          />
        </div>
      </template>
    </Card>

    <FormsBulkImportDialog
      v-model:visible="isBulkDialogVisible"
      entity-label="Books"
      :report="bulkReport"
      :error-message="bulkError"
      :is-validating="isBulkValidating"
      :is-committing="isBulkCommitting"
      @template="downloadBookTemplate"
      @validate="validateBookBulk"
      @commit="commitBookBulk"
    />
  </div>
</template>

<script setup lang="ts">
import type { BulkImportReport } from '~/types/api'
import { API } from '~/utils/constants'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin', books: 'Books',
  },
})

const { showError, showSuccess } = useAppToast()
const bulkImport = useBulkImport()

const isBulkDialogVisible = ref(false)
const isBulkValidating = ref(false)
const isBulkCommitting = ref(false)
const bulkError = ref('')
const bulkReport = ref<BulkImportReport | null>(null)

async function downloadBookTemplate() {
  try {
    await bulkImport.downloadTemplate({
      validate: API.books.bulkValidate,
      commit: API.books.bulkCommit,
      template: API.importTemplates.books,
      templateFileName: 'books-import-template.xlsx',
    })
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    showError(fetchError.data?.message ?? 'Failed to download template')
  }
}

async function validateBookBulk(file: File) {
  bulkError.value = ''
  isBulkValidating.value = true
  try {
    const response = await bulkImport.validateFile(file, {
      validate: API.books.bulkValidate,
      commit: API.books.bulkCommit,
      template: API.importTemplates.books,
      templateFileName: 'books-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Book import validation complete')
      return
    }
    bulkError.value = response.message ?? 'Validation failed'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    bulkError.value = fetchError.data?.message ?? 'Validation failed'
  }
  finally {
    isBulkValidating.value = false
  }
}

async function commitBookBulk(file: File) {
  bulkError.value = ''
  isBulkCommitting.value = true
  try {
    const response = await bulkImport.commitFile(file, {
      validate: API.books.bulkValidate,
      commit: API.books.bulkCommit,
      template: API.importTemplates.books,
      templateFileName: 'books-import-template.xlsx',
    })
    if (response.success) {
      bulkReport.value = response.data
      showSuccess('Book import committed')
      return
    }
    bulkError.value = response.message ?? 'Commit failed'
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string } }
    bulkError.value = fetchError.data?.message ?? 'Commit failed'
  }
  finally {
    isBulkCommitting.value = false
  }
}
</script>
