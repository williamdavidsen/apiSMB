import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useScanProgress } from '../../../../Frontend/dashboard/src/features/scan/hooks/useScanProgress'

describe('useScanProgress', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('stays on the initial snapshot when autoStart is disabled', () => {
    const { result } = renderHook(() => useScanProgress({ autoStart: false }))

    act(() => {
      vi.advanceTimersByTime(5000)
    })

    expect(result.current.progress).toBe(2)
    expect(result.current.secondsRemaining).toBe(10)
    expect(result.current.isComplete).toBe(false)
  })

  it('advances on an interval and exposes the completion label', () => {
    const { result } = renderHook(() => useScanProgress())

    act(() => {
      vi.advanceTimersByTime(10000)
    })

    expect(result.current.progress).toBe(100)
    expect(result.current.secondsRemaining).toBe(0)
    expect(result.current.isComplete).toBe(true)
    expect(result.current.estimatedLabel).toBe('Scan is complete. Redirecting to dashboard...')
  })
})
