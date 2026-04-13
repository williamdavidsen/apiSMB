import { useEffect, useMemo, useState } from 'react'
import { fetchAssessmentDashboardBundle } from '../services/assessment.api'
import type { AssessmentDashboardBundle } from '../model/assessment.types'

type Idle = { status: 'idle' }
type Loading = { status: 'loading' }
type Success = { status: 'success'; data: AssessmentDashboardBundle; scannedAtIso: string }
type ErrorState = { status: 'error'; message: string }

export type AssessmentDashboardState = Idle | Loading | Success | ErrorState

export function useAssessmentDashboard(domain: string) {
  const normalizedDomain = useMemo(() => domain.trim(), [domain])
  const [state, setState] = useState<AssessmentDashboardState>({ status: 'idle' })
  const [refreshToken, setRefreshToken] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    void (async () => {
      await Promise.resolve()
      if (controller.signal.aborted) return

      if (!normalizedDomain) {
        setState({ status: 'idle' })
        return
      }

      setState({ status: 'loading' })

      try {
        const data = await fetchAssessmentDashboardBundle(normalizedDomain, controller.signal)
        if (controller.signal.aborted) return
        setState({ status: 'success', data, scannedAtIso: new Date().toISOString() })
      } catch (error) {
        if (controller.signal.aborted) return
        const message = error instanceof Error ? error.message : 'Could not load assessment.'
        setState({ status: 'error', message })
      }
    })()

    return () => controller.abort()
  }, [normalizedDomain, refreshToken])

  return { state, refetch: () => setRefreshToken((value) => value + 1) }
}
