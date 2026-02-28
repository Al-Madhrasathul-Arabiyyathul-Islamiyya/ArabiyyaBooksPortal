export function useOperationReadinessGuard() {
  const { showError } = useAppToast()
  const { isOperationBlocked, canManageSetup, blockingMessage, ensureLoaded } = useSetupReadiness()

  async function guard(actionLabel = 'complete this action'): Promise<boolean> {
    await ensureLoaded()
    if (!isOperationBlocked.value) {
      return true
    }

    showError(blockingMessage.value || `System setup is incomplete. Unable to ${actionLabel}.`)

    if (canManageSetup.value) {
      void navigateTo('/admin/settings/setup')
    }

    return false
  }

  return {
    guard,
    isOperationBlocked,
    blockingMessage,
  }
}
