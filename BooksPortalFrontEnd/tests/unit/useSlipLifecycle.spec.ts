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
})
