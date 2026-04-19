import { describe, expect, it } from 'vitest'
import {
  mapAssessmentStatus,
  mapModuleStatusLabel,
} from '../../../../Frontend/dashboard/src/shared/lib/status'

describe('status mapping', () => {
  it('maps API assessment statuses to user-facing labels', () => {
    expect(mapAssessmentStatus('PASS', 95)).toMatchObject({ label: 'Low risk', severity: 'success' })
    expect(mapAssessmentStatus('WARNING', 65)).toMatchObject({ label: 'Moderate risk', severity: 'warning' })
    expect(mapAssessmentStatus('PARTIAL', 65)).toMatchObject({ label: 'Partial assessment', severity: 'warning' })
    expect(mapAssessmentStatus('FAIL', 0)).toMatchObject({ label: 'Critical risk', severity: 'error' })
    expect(mapAssessmentStatus('UNKNOWN', 0)).toMatchObject({ label: 'Unknown status', severity: 'info' })
  })

  it('maps module statuses to short labels', () => {
    expect(mapModuleStatusLabel('PASS')).toBe('Good')
    expect(mapModuleStatusLabel('WARNING')).toBe('Needs attention')
    expect(mapModuleStatusLabel('FAIL')).toBe('Issues detected')
    expect(mapModuleStatusLabel('ERROR')).toBe('Could not evaluate')
  })
})
