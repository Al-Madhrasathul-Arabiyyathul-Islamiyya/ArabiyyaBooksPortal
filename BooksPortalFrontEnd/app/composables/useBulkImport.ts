import type { BulkImportReport } from '~/types/api'

interface BulkImportEndpoints {
  validate: string
  commit: string
  template: string
  templateFileName: string
}

export function useBulkImport() {
  const api = useApi()

  async function validateFile(file: File, endpoints: BulkImportEndpoints) {
    const formData = new FormData()
    formData.append('file', file)
    return await api.post<BulkImportReport>(endpoints.validate, formData)
  }

  async function commitFile(file: File, endpoints: BulkImportEndpoints) {
    const formData = new FormData()
    formData.append('file', file)
    return await api.post<BulkImportReport>(endpoints.commit, formData)
  }

  async function downloadTemplate(endpoints: BulkImportEndpoints) {
    await api.downloadBlob(endpoints.template, endpoints.templateFileName)
  }

  return {
    validateFile,
    commitFile,
    downloadTemplate,
  }
}
