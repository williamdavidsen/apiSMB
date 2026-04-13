export type UiSeverity = 'success' | 'warning' | 'error' | 'info'

export type AssessmentUiStatus = {
  label: string
  severity: UiSeverity
  /** MUI palette key or hex for text accents (paired with readable descriptions). */
  accent: 'success' | 'warning' | 'error' | 'info' | 'text.secondary'
}

export function mapAssessmentStatus(status: string, overallScore: number): AssessmentUiStatus {
  const normalized = status.trim().toUpperCase()

  if (normalized === 'PASS') {
    return { label: 'Low risk', severity: 'success', accent: 'success' }
  }

  if (normalized === 'PARTIAL') {
    return { label: 'Partial assessment', severity: 'warning', accent: 'warning' }
  }

  if (normalized === 'WARNING') {
    return { label: 'Moderate risk', severity: 'warning', accent: 'warning' }
  }

  if (normalized === 'FAIL') {
    if (overallScore <= 0) {
      return { label: 'Critical risk', severity: 'error', accent: 'error' }
    }
    if (overallScore < 50) {
      return { label: 'High risk', severity: 'error', accent: 'error' }
    }
    return { label: 'High risk', severity: 'error', accent: 'error' }
  }

  return { label: 'Unknown status', severity: 'info', accent: 'text.secondary' }
}

export function mapModuleStatusLabel(status: string): string {
  const key = status.trim().toUpperCase()
  if (key === 'PASS') return 'Good'
  if (key === 'WARNING') return 'Needs attention'
  if (key === 'FAIL') return 'Issues detected'
  if (key === 'ERROR') return 'Could not evaluate'
  return status || 'Unknown'
}
