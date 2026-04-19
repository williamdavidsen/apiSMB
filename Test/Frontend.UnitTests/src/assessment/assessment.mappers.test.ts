import { describe, expect, it } from 'vitest'
import {
  dashboardEmailSubtitle,
  dashboardHeadline,
  formatHeaderPresence,
  reputationVerdict,
} from '../../../../Frontend/dashboard/src/features/assessment/model/assessment.mappers'

describe('assessment dashboard mappers', () => {
  it('uses boundary-oriented headline decisions', () => {
    expect(dashboardHeadline('F', 'FAIL', 0)).toBe('Critical security failure')
    expect(dashboardHeadline('E', 'FAIL', 50)).toBe('Security improvements needed')
    expect(dashboardHeadline('C', 'WARNING', 70)).toBe('Security posture needs attention')
    expect(dashboardHeadline('B', 'PASS', 85)).toBe('Security analysis dashboard')
  })

  it('formats module facts for header and e-mail inclusion decisions', () => {
    expect(formatHeaderPresence({ score: 0, details: 'Missing' })).toBe('Missing')
    expect(formatHeaderPresence({ score: 3, details: 'Present' })).toBe('Present')
    expect(dashboardEmailSubtitle(true)).toContain('evaluated')
    expect(dashboardEmailSubtitle(false)).toContain('not evaluated')
  })

  it('maps reputation verdicts with malicious detections taking priority', () => {
    expect(reputationVerdict('PASS', 0, 0)).toBe('Clean')
    expect(reputationVerdict('WARNING', 2, 0)).toBe('Mixed signals')
    expect(reputationVerdict('PASS', 0, 1)).toBe('Malicious signals')
    expect(reputationVerdict('ERROR', 0, 0)).toBe('Unknown')
  })
})
