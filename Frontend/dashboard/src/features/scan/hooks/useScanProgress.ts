import { useEffect, useMemo, useState } from 'react'
import type { ScanSnapshot } from '../model/scan.types'
import { getInitialScanState, getNextScanState } from '../services/scanPolling.service'

type UseScanProgressOptions = {
  autoStart?: boolean
}

export function useScanProgress(options: UseScanProgressOptions = {}) {
  const { autoStart = true } = options
  const [snapshot, setSnapshot] = useState<ScanSnapshot>(getInitialScanState)

  useEffect(() => {
    if (!autoStart || snapshot.isComplete) return

    const timer = window.setInterval(() => {
      setSnapshot((previous) => getNextScanState(previous))
    }, 1000)

    return () => window.clearInterval(timer)
  }, [autoStart, snapshot.isComplete])

  const estimatedLabel = useMemo(() => {
    if (snapshot.isComplete) return 'Scan is complete. Redirecting to dashboard...'
    if (snapshot.secondsRemaining < 60) {
      return `Estimated time remaining: about ${snapshot.secondsRemaining} seconds.`
    }

    const minutes = Math.floor(snapshot.secondsRemaining / 60)
    const seconds = snapshot.secondsRemaining % 60
    return `Estimated time remaining: about ${minutes} min ${seconds} sec.`
  }, [snapshot.isComplete, snapshot.secondsRemaining])

  return {
    ...snapshot,
    estimatedLabel,
  }
}
