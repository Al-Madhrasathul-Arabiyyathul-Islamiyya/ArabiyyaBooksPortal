import { describe, expect, it } from 'vitest'
import { useSlipLifecycle } from '~/composables/useSlipLifecycle'
import { SlipLifecycleStatusValue } from '~/types/enums'

describe('useSlipLifecycle', () => {
  const lifecycle = useSlipLifecycle()

  it('normalizes numeric and string values', () => {
    expect(lifecycle.normalizeLifecycleStatus(1)).toBe(SlipLifecycleStatusValue.Finalized)
    expect(lifecycle.normalizeLifecycleStatus('cancelled')).toBe(SlipLifecycleStatusValue.Cancelled)
    expect(lifecycle.normalizeLifecycleStatus('canceled')).toBe(SlipLifecycleStatusValue.Cancelled)
    expect(lifecycle.normalizeLifecycleStatus('processing')).toBe(SlipLifecycleStatusValue.Processing)
  })

  it('falls back to processing for unknown values', () => {
    expect(lifecycle.normalizeLifecycleStatus('unknown')).toBe(SlipLifecycleStatusValue.Processing)
    expect(lifecycle.getLifecycleLabel(undefined)).toBe('Processing')
  })

  it('returns consistent icon and state helpers', () => {
    expect(lifecycle.getLifecycleIcon('finalized')).toContain('check-circle')
    expect(lifecycle.isProcessing('processing')).toBe(true)
    expect(lifecycle.isFinalized(SlipLifecycleStatusValue.Finalized)).toBe(true)
    expect(lifecycle.isCancelled(SlipLifecycleStatusValue.Cancelled)).toBe(true)
  })

  it('maps label/severity/icon for all lifecycle states', () => {
    expect(lifecycle.getLifecycleLabel('processing')).toBe('Processing')
    expect(lifecycle.getLifecycleSeverity('processing')).toBe('warn')
    expect(lifecycle.getLifecycleIcon('processing')).toContain('clock')

    expect(lifecycle.getLifecycleLabel('finalized')).toBe('Finalized')
    expect(lifecycle.getLifecycleSeverity('finalized')).toBe('success')
    expect(lifecycle.getLifecycleIcon('finalized')).toContain('check-circle')

    expect(lifecycle.getLifecycleLabel('cancelled')).toBe('Cancelled')
    expect(lifecycle.getLifecycleSeverity('cancelled')).toBe('danger')
    expect(lifecycle.getLifecycleIcon('cancelled')).toContain('times-circle')
  })

  it('returns deterministic badge classes for each lifecycle state', () => {
    const processingClass = lifecycle.getLifecycleBadgeClass(SlipLifecycleStatusValue.Processing)
    const finalizedClass = lifecycle.getLifecycleBadgeClass(SlipLifecycleStatusValue.Finalized)
    const cancelledClass = lifecycle.getLifecycleBadgeClass(SlipLifecycleStatusValue.Cancelled)

    expect(processingClass).toContain('amber')
    expect(finalizedClass).toContain('green')
    expect(cancelledClass).toContain('red')
  })
})
