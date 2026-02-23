import type { BulkImportJobSnapshot, BulkImportReport } from '~/types/api'

interface BulkImportEndpoints {
  validate: string
  commit: string
  commitAsync?: string
  jobStatus?: (id: string) => string
  jobReport?: (id: string) => string
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

  async function commitFileAsync(file: File, endpoints: BulkImportEndpoints) {
    if (!endpoints.commitAsync) {
      throw new Error('Async commit endpoint is not configured.')
    }

    const formData = new FormData()
    formData.append('file', file)
    return await api.post<{ jobId: string }>(endpoints.commitAsync, formData)
  }

  async function getJobStatus(jobId: string, endpoints: BulkImportEndpoints) {
    if (!endpoints.jobStatus) {
      throw new Error('Job status endpoint is not configured.')
    }

    return await api.get<BulkImportJobSnapshot>(endpoints.jobStatus(jobId))
  }

  async function downloadJobReport(jobId: string, endpoints: BulkImportEndpoints, fallbackName: string) {
    if (!endpoints.jobReport) {
      throw new Error('Job report endpoint is not configured.')
    }

    await api.downloadBlob(endpoints.jobReport(jobId), fallbackName)
  }

  async function downloadTemplate(endpoints: BulkImportEndpoints) {
    await api.downloadBlob(endpoints.template, endpoints.templateFileName)
  }

  return {
    validateFile,
    commitFile,
    commitFileAsync,
    getJobStatus,
    downloadJobReport,
    downloadTemplate,
  }
}
