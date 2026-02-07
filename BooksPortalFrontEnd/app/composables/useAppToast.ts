export function useAppToast() {
  const toast = useToast()

  function showSuccess(message: string, summary = 'Success') {
    toast.add({
      severity: 'success',
      summary,
      detail: message,
      life: 3000,
    })
  }

  function showError(message: string, summary = 'Error') {
    toast.add({
      severity: 'error',
      summary,
      detail: message,
      life: 5000,
    })
  }

  function showInfo(message: string, summary = 'Info') {
    toast.add({
      severity: 'info',
      summary,
      detail: message,
      life: 3000,
    })
  }

  function showWarn(message: string, summary = 'Warning') {
    toast.add({
      severity: 'warn',
      summary,
      detail: message,
      life: 4000,
    })
  }

  return {
    showSuccess,
    showError,
    showInfo,
    showWarn,
  }
}
