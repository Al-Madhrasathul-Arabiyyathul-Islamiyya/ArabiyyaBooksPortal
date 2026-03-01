import { SlipLifecycleStatusValue } from '~/types/enums'
import { slipLifecycleLabels, slipLifecycleSeverity } from '~/utils/formatters'

export function useSlipLifecycle() {
  function normalizeLifecycleStatus(value: unknown): number {
    if (typeof value === 'number') return value
    if (typeof value === 'string') {
      const normalized = value.trim().toLowerCase()
      if (normalized === 'finalized') return SlipLifecycleStatusValue.Finalized
      if (normalized === 'cancelled' || normalized === 'canceled') return SlipLifecycleStatusValue.Cancelled
      if (normalized === 'processing') return SlipLifecycleStatusValue.Processing
    }
    return SlipLifecycleStatusValue.Processing
  }

  function getLifecycleLabel(value: unknown): string {
    const status = normalizeLifecycleStatus(value)
    return slipLifecycleLabels[status] ?? 'Processing'
  }

  function getLifecycleSeverity(value: unknown): string {
    const status = normalizeLifecycleStatus(value)
    return slipLifecycleSeverity[status] ?? 'warn'
  }

  function getLifecycleIcon(value: unknown): string {
    const status = normalizeLifecycleStatus(value)
    if (status === SlipLifecycleStatusValue.Finalized) return 'pi pi-check-circle'
    if (status === SlipLifecycleStatusValue.Cancelled) return 'pi pi-times-circle'
    return 'pi pi-clock'
  }

  function getLifecycleBadgeClass(value: unknown): string {
    const status = normalizeLifecycleStatus(value)
    if (status === SlipLifecycleStatusValue.Finalized) {
      return 'border-green-200 bg-green-50 text-green-700 dark:border-green-700/40 dark:bg-green-500/10 dark:text-green-300'
    }
    if (status === SlipLifecycleStatusValue.Cancelled) {
      return 'border-red-200 bg-red-50 text-red-700 dark:border-red-700/40 dark:bg-red-500/10 dark:text-red-300'
    }
    return 'border-amber-200 bg-amber-50 text-amber-700 dark:border-amber-700/40 dark:bg-amber-500/10 dark:text-amber-300'
  }

  function isProcessing(value: unknown): boolean {
    return normalizeLifecycleStatus(value) === SlipLifecycleStatusValue.Processing
  }

  function isFinalized(value: unknown): boolean {
    return normalizeLifecycleStatus(value) === SlipLifecycleStatusValue.Finalized
  }

  function isCancelled(value: unknown): boolean {
    return normalizeLifecycleStatus(value) === SlipLifecycleStatusValue.Cancelled
  }

  return {
    normalizeLifecycleStatus,
    getLifecycleLabel,
    getLifecycleSeverity,
    getLifecycleIcon,
    getLifecycleBadgeClass,
    isProcessing,
    isFinalized,
    isCancelled,
  }
}
