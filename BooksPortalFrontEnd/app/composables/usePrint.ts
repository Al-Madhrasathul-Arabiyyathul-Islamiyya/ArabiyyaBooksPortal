export function usePrint() {
  const api = useApi()
  const toast = useToast()

  async function printPdf(url: string, filename: string, openInNewTab = true) {
    try {
      const token = api.getAccessToken()
      const config = useRuntimeConfig()
      const baseURL = config.public.apiBase as string

      const blob = await $fetch<Blob>(`${baseURL}${url}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        responseType: 'blob',
      })

      const objectUrl = URL.createObjectURL(blob)

      if (openInNewTab) {
        window.open(objectUrl, '_blank')
        // Clean up after a delay to allow browser to load
        setTimeout(() => URL.revokeObjectURL(objectUrl), 100)
      }
      else {
        const link = document.createElement('a')
        link.href = objectUrl
        link.download = filename
        link.click()
        URL.revokeObjectURL(objectUrl)
      }
    }
    catch (error) {
      console.error('Failed to fetch PDF:', error)
      toast.showError('Failed to load PDF')
    }
  }

  function downloadPdf(url: string, filename: string) {
    return printPdf(url, filename, false)
  }

  return {
    printPdf,
    downloadPdf,
  }
}
