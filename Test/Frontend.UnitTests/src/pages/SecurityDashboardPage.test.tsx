import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { SecurityDashboardPage } from '../../../../Frontend/dashboard/src/pages/SecurityDashboardPage'
import type { AssessmentDashboardState } from '../../../../Frontend/dashboard/src/features/assessment/hooks/useAssessmentDashboard'
import type { AssessmentDashboardBundle } from '../../../../Frontend/dashboard/src/features/assessment/model/assessment.types'

const mockUseAssessmentDashboard = vi.fn()
const mockGetLastScannedDomain = vi.fn()
const mockSaveLastScannedDomain = vi.fn()

vi.mock('../../../../Frontend/dashboard/src/features/assessment/hooks/useAssessmentDashboard', () => ({
  useAssessmentDashboard: (domain: string) => mockUseAssessmentDashboard(domain),
}))

vi.mock('../../../../Frontend/dashboard/src/shared/lib/lastScan', () => ({
  getLastScannedDomain: () => mockGetLastScannedDomain(),
  saveLastScannedDomain: (domain: string) => mockSaveLastScannedDomain(domain),
}))

vi.mock('../../../../Frontend/dashboard/src/features/assessment/components/PqcInsightModal', () => ({
  PqcInsightModal: ({ open }: { open: boolean }) => (open ? <div>Post-quantum insight modal</div> : null),
}))

describe('SecurityDashboardPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockGetLastScannedDomain.mockReturnValue('')
  })

  it('shows the empty state when no domain is in the URL', () => {
    mockUseAssessmentDashboard.mockReturnValue({ state: { status: 'idle' }, refetch: vi.fn() })

    renderWithDashboardRoute('/dashboard')

    expect(screen.getByText('No scan yet')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /go to home and start scan/i })).toBeInTheDocument()
  })

  it('shows API errors and retry affordance', () => {
    mockUseAssessmentDashboard.mockReturnValue({
      state: { status: 'error', message: 'API request failed.' } satisfies AssessmentDashboardState,
      refetch: vi.fn(),
    })

    renderWithDashboardRoute('/dashboard?domain=example.com')

    expect(screen.getByText('API request failed.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument()
  })

  it('renders a successful dashboard and persists the scanned domain', async () => {
    mockUseAssessmentDashboard.mockReturnValue({
      state: {
        status: 'success',
        data: createBundle(),
        scannedAtIso: '2026-04-27T10:00:00Z',
      } satisfies AssessmentDashboardState,
      refetch: vi.fn(),
    })

    renderWithDashboardRoute('/dashboard?domain=example.com')

    expect(screen.getByText(/partial security assessment/i)).toBeInTheDocument()
    expect(screen.getByText(/e-mail security could not be evaluated reliably/i)).toBeInTheDocument()
    expect(screen.getByText('Could not evaluate')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /post-quantum insight/i })).not.toBeInTheDocument()
    const readMoreLinks = screen.getAllByRole('link', { name: /read more/i })
    expect(readMoreLinks.some((link) => link.getAttribute('href')?.includes('/dashboard/example.com/pqc'))).toBe(true)

    await waitFor(() => {
      expect(mockSaveLastScannedDomain).toHaveBeenCalledWith('example.com')
    })
  })
})

function renderWithDashboardRoute(initialEntry: string) {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route path="/dashboard" element={<SecurityDashboardPage />} />
      </Routes>
    </MemoryRouter>,
  )
}

function createBundle(): AssessmentDashboardBundle {
  return {
    assessment: {
      domain: 'example.com',
      overallScore: 71,
      maxScore: 100,
      status: 'PARTIAL',
      grade: 'C',
      emailModuleIncluded: false,
      pqcReadiness: {
        domain: 'example.com',
        pqcDetected: false,
        status: 'INFO',
        mode: 'Classical TLS with modern groups',
        readinessLevel: 'Unknown / not verifiable',
        algorithmFamily: 'Classical TLS',
        handshakeSupported: true,
        confidence: 'LOW',
        notes: 'No explicit PQC support was detected.',
        evidence: [],
      },
      weights: { sslTls: 43.75, httpHeaders: 31.25, emailSecurity: 0, reputation: 25 },
      modules: {
        sslTls: { included: true, weightPercent: 43.75, rawScore: 24, rawMaxScore: 30, normalizedScore: 80, weightedContribution: 35, status: 'WARNING' },
        httpHeaders: { included: true, weightPercent: 31.25, rawScore: 10, rawMaxScore: 10, normalizedScore: 100, weightedContribution: 31.25, status: 'PASS' },
        emailSecurity: { included: false, weightPercent: 0, rawScore: 0, rawMaxScore: 20, normalizedScore: 0, weightedContribution: 0, status: 'ERROR' },
        reputation: { included: true, weightPercent: 25, rawScore: 4, rawMaxScore: 20, normalizedScore: 20, weightedContribution: 5, status: 'FAIL' },
      },
      alerts: [
        { type: 'WARNING', message: 'Email security analysis could not be completed reliably, so the module was excluded from the final weighted score.' },
      ],
    },
    ssl: {
      domain: 'example.com',
      overallScore: 24,
      maxScore: 30,
      status: 'WARNING',
      criteria: {
        tlsVersion: { score: 7, details: 'TLS 1.2' },
        certificateValidity: { score: 4, details: 'Valid' },
        remainingLifetime: { score: 6, details: '120 days remaining' },
        cipherStrength: { score: 7, details: '128-bit cipher' },
      },
      alerts: [],
    },
    headers: {
      domain: 'example.com',
      overallScore: 10,
      maxScore: 10,
      status: 'PASS',
      criteria: {
        strictTransportSecurity: { score: 3, details: 'Present' },
        contentSecurityPolicy: { score: 4, details: 'Present' },
        clickjackingProtection: { score: 3, details: 'Present' },
        mimeSniffingProtection: { score: 0, details: 'X-Content-Type-Options is configured: nosniff' },
        referrerPolicy: { score: 0, details: 'Referrer-Policy is configured: strict-origin-when-cross-origin' },
      },
      observatory: { grade: 'B', score: 75, testsPassed: 8, testsFailed: 2, testsQuantity: 10, detailsUrl: '' },
      alerts: [],
    },
    email: {
      domain: 'example.com',
      hasMailService: false,
      moduleApplicable: true,
      overallScore: 0,
      maxScore: 20,
      status: 'ERROR',
      criteria: {
        spfVerification: { score: 0, confidence: 'HIGH', details: '' },
        dkimActivated: { score: 0, confidence: 'HIGH', details: '' },
        dmarcEnforcement: { score: 0, confidence: 'HIGH', details: '' },
      },
      dnsSummary: { mxRecords: [], spfRecord: '', dmarcRecord: '', dkimSelectorsFound: [] },
      alerts: [{ type: 'WARNING', message: 'Email security DNS lookups could not be completed reliably. MX lookup could not be completed.' }],
    },
    reputation: {
      domain: 'example.com',
      overallScore: 4,
      maxScore: 20,
      status: 'FAIL',
      criteria: {
        blacklistStatus: { score: 0, confidence: 'HIGH', details: 'malicious' },
        malwareAssociation: { score: 4, confidence: 'MEDIUM', details: 'mixed' },
      },
      summary: {
        maliciousDetections: 1,
        suspiciousDetections: 0,
        harmlessDetections: 0,
        undetectedDetections: 0,
        reputation: -5,
        communityMaliciousVotes: 1,
        communityHarmlessVotes: 0,
        lastAnalysisDate: '',
        permalink: '',
      },
      alerts: [],
    },
  }
}
