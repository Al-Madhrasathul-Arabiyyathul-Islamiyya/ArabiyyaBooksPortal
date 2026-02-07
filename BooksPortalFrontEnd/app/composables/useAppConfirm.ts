export function useAppConfirm() {
  const confirm = useConfirm()

  function confirmDelete(
    message: string,
    onAccept: () => void | Promise<void>,
    header = 'Confirm Delete',
  ) {
    confirm.require({
      message,
      header,
      icon: 'pi pi-exclamation-triangle',
      acceptClass: 'p-button-danger',
      accept: onAccept,
    })
  }

  function confirmAction(
    message: string,
    onAccept: () => void | Promise<void>,
    header = 'Confirm',
    acceptLabel = 'Yes',
    rejectLabel = 'No',
  ) {
    confirm.require({
      message,
      header,
      icon: 'pi pi-question-circle',
      accept: onAccept,
      acceptLabel,
      rejectLabel,
    })
  }

  return {
    confirmDelete,
    confirmAction,
  }
}
