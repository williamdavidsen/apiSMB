import { scanSteps } from '../model/scan.enums'
import type { ScanSnapshot } from '../model/scan.types'

const TOTAL_SCAN_SECONDS = 10

export function getInitialScanState(): ScanSnapshot {
  return {
    progress: 2,
    currentStep: scanSteps[0],
    secondsRemaining: TOTAL_SCAN_SECONDS,
    isComplete: false,
  }
}

export function getNextScanState(previous: ScanSnapshot): ScanSnapshot {
  if (previous.isComplete) return previous

  const nextRemaining = Math.max(previous.secondsRemaining - 1, 0)
  const progress = Math.min(
    100,
    Math.round(((TOTAL_SCAN_SECONDS - nextRemaining) / TOTAL_SCAN_SECONDS) * 100),
  )

  const stepIndex = Math.min(
    scanSteps.length - 1,
    Math.floor((progress / 100) * scanSteps.length),
  )

  return {
    progress,
    currentStep: scanSteps[stepIndex],
    secondsRemaining: nextRemaining,
    isComplete: progress >= 100,
  }
}
