import { describe, expect, it } from 'vitest'
import {
  buildModuleCards,
  dashboardEmailSubtitle,
  dashboardHeadline,
  formatHeaderPresence,
  isHeaderControlMissingOrWeak,
  reputationVerdict,
} from '../../../../Frontend/dashboard/src/features/assessment/model/assessment.mappers'
import type { AssessmentDashboardBundle } from '../../../../Frontend/dashboard/src/features/assessment/model/assessment.types'

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
    expect(dashboardEmailSubtitle(true, 'PASS')).toContain('evaluated')
    expect(dashboardEmailSubtitle(false, 'NOT_APPLICABLE')).toContain('not evaluated')
    expect(dashboardEmailSubtitle(false, 'ERROR')).toContain('could not be evaluated reliably')
  })

  it('distinguishes missing or weak header controls from configured zero-score context rows', () => {
    expect(isHeaderControlMissingOrWeak({ score: 0, details: 'X-Content-Type-Options is configured: nosniff' })).toBe(false)
    expect(isHeaderControlMissingOrWeak({ score: 0, details: 'Referrer-Policy is configured: strict-origin-when-cross-origin' })).toBe(false)
    expect(isHeaderControlMissingOrWeak({ score: 0, details: 'Content-Security-Policy header is missing.' })).toBe(true)
    expect(isHeaderControlMissingOrWeak({ score: 2, details: 'Content-Security-Policy is present, but contains unsafe directives: unsafe-inline.' })).toBe(true)
  })

  it('maps reputation verdicts with malicious detections taking priority', () => {
    expect(reputationVerdict('PASS', 0, 0)).toBe('Clean')
    expect(reputationVerdict('WARNING', 2, 0)).toBe('Mixed signals')
    expect(reputationVerdict('PASS', 0, 1)).toBe('Malicious signals')
    expect(reputationVerdict('ERROR', 0, 0)).toBe('Unknown')
  })

  it('renders email provider failures differently from not-applicable email modules', () => {
    const bundle = createBundle()
    bundle.assessment.emailModuleIncluded = false
    bundle.assessment.modules.emailSecurity.included = false
    bundle.email.status = 'ERROR'
    bundle.email.moduleApplicable = true
    bundle.email.hasMailService = false
    bundle.email.alerts = [
      {
        type: 'WARNING',
        message: 'Email security DNS lookups could not be completed reliably. MX lookup could not be completed.',
      },
    ]

    const emailCard = buildModuleCards(bundle).find((card) => card.key === 'email')

    expect(emailCard?.statusLine).toBe('Could not evaluate')
    expect(emailCard?.callout?.tone).toBe('warning')
    expect(emailCard?.facts.every((fact) => fact.value === 'Unavailable')).toBe(true)
  })

  it('uses internal header score for the visible module grade while keeping Observatory as context', () => {
    const bundle = createBundle()
    bundle.headers.overallScore = 10
    bundle.headers.maxScore = 10
    bundle.headers.observatory.grade = 'B'
    bundle.headers.observatory.score = 75

    const headersCard = buildModuleCards(bundle).find((card) => card.key === 'http-headers')

    expect(headersCard?.moduleGrade).toBe('A')
    expect(headersCard?.statusLine).toContain('Observatory grade B')
  })
})

function createBundle(): AssessmentDashboardBundle {
  return {
    assessment: {
      domain: 'example.com',
      overallScore: 80,
      maxScore: 100,
      status: 'PASS',
      grade: 'B',
      emailModuleIncluded: true,
      pqcReadiness: {
        domain: 'example.com',
        pqcDetected: false,
        status: 'INFO',
        mode: 'Unknown',
        readinessLevel: 'Unknown',
        algorithmFamily: 'Unknown',
        handshakeSupported: false,
        confidence: 'LOW',
        notes: '',
        evidence: [],
      },
      weights: { sslTls: 35, httpHeaders: 25, emailSecurity: 20, reputation: 20 },
      modules: {
        sslTls: { included: true, weightPercent: 35, rawScore: 30, rawMaxScore: 30, normalizedScore: 100, weightedContribution: 35, status: 'PASS' },
        httpHeaders: { included: true, weightPercent: 25, rawScore: 10, rawMaxScore: 10, normalizedScore: 100, weightedContribution: 25, status: 'PASS' },
        emailSecurity: { included: true, weightPercent: 20, rawScore: 13, rawMaxScore: 20, normalizedScore: 65, weightedContribution: 13, status: 'WARNING' },
        reputation: { included: true, weightPercent: 20, rawScore: 20, rawMaxScore: 20, normalizedScore: 100, weightedContribution: 20, status: 'PASS' },
      },
      alerts: [],
    },
    ssl: {
      domain: 'example.com',
      overallScore: 30,
      maxScore: 30,
      status: 'PASS',
      criteria: {
        tlsVersion: { score: 10, details: 'ok' },
        certificateValidity: { score: 4, details: 'ok' },
        remainingLifetime: { score: 6, details: 'ok' },
        cipherStrength: { score: 10, details: 'ok' },
      },
      alerts: [],
    },
    headers: {
      domain: 'example.com',
      overallScore: 10,
      maxScore: 10,
      status: 'PASS',
      criteria: {
        strictTransportSecurity: { score: 3, details: 'ok' },
        contentSecurityPolicy: { score: 4, details: 'ok' },
        clickjackingProtection: { score: 3, details: 'ok' },
        mimeSniffingProtection: { score: 0, details: 'ok' },
        referrerPolicy: { score: 0, details: 'ok' },
      },
      observatory: { grade: 'A', score: 100, testsPassed: 10, testsFailed: 0, testsQuantity: 10, detailsUrl: '' },
      alerts: [],
    },
    email: {
      domain: 'example.com',
      hasMailService: true,
      moduleApplicable: true,
      overallScore: 13,
      maxScore: 20,
      status: 'WARNING',
      criteria: {
        spfVerification: { score: 3, confidence: 'HIGH', details: 'ok' },
        dkimActivated: { score: 7, confidence: 'HIGH', details: 'ok' },
        dmarcEnforcement: { score: 3, confidence: 'HIGH', details: 'ok' },
      },
      dnsSummary: { mxRecords: [], spfRecord: '', dmarcRecord: '', dkimSelectorsFound: [] },
      alerts: [],
    },
    reputation: {
      domain: 'example.com',
      overallScore: 20,
      maxScore: 20,
      status: 'PASS',
      criteria: {
        blacklistStatus: { score: 10, confidence: 'HIGH', details: 'ok' },
        malwareAssociation: { score: 10, confidence: 'HIGH', details: 'ok' },
      },
      summary: {
        maliciousDetections: 0,
        suspiciousDetections: 0,
        harmlessDetections: 0,
        undetectedDetections: 0,
        reputation: 0,
        communityMaliciousVotes: 0,
        communityHarmlessVotes: 0,
        lastAnalysisDate: '',
        permalink: '',
      },
      alerts: [],
    },
  }
}
