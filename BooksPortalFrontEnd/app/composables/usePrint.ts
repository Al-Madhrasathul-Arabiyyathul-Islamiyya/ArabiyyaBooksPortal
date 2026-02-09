export function usePrint() {
  const api = useApi()
  const toast = useAppToast()

  async function printPdf(url: string, filename: string, openInNewTab = true) {
    if (!import.meta.client) return

    try {
      await api.downloadBlob(url, filename, openInNewTab)
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
