import { describe, expect, it } from 'vitest'
import { getInitialScanState, getNextScanState } from '../../../../Frontend/dashboard/src/features/scan/services/scanPolling.service'

describe('scanPolling.service', () => {
  it('starts with the expected initial progress snapshot', () => {
    expect(getInitialScanState()).toEqual({
      progress: 2,
      currentStep: 'Collecting DNS and endpoint metadata',
      secondsRemaining: 10,
      isComplete: false,
    })
  })

  it('advances progress and eventually reaches a stable complete state', () => {
    let state = getInitialScanState()

    for (let index = 0; index < 10; index += 1) {
      state = getNextScanState(state)
    }

    expect(state.progress).toBe(100)
    expect(state.secondsRemaining).toBe(0)
    expect(state.isComplete).toBe(true)
    expect(getNextScanState(state)).toEqual(state)
  })
})
