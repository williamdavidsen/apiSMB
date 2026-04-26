import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ModuleDetailPage } from '../../../../Frontend/dashboard/src/pages/ModuleDetailPage'
import type { EmailCheckResult } from '../../../../Frontend/dashboard/src/features/assessment/model/assessment.types'

const fetchSslCheck = vi.fn()
const fetchSslDetails = vi.fn()
const fetchHeadersCheck = vi.fn()
const fetchEmailCheck = vi.fn()
const fetchReputationCheck = vi.fn()

vi.mock('../../../../Frontend/dashboard/src/features/assessment/services/assessment.api', () => ({
  fetchSslCheck: (...args: unknown[]) => fetchSslCheck(...args),
  fetchSslDetails: (...args: unknown[]) => fetchSslDetails(...args),
  fetchHeadersCheck: (...args: unknown[]) => fetchHeadersCheck(...args),
  fetchEmailCheck: (...args: unknown[]) => fetchEmailCheck(...args),
  fetchReputationCheck: (...args: unknown[]) => fetchReputationCheck(...args),
}))

describe('ModuleDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows a warning for invalid module routes', () => {
    renderWithModuleRoute('/dashboard/example.com/not-a-module')

    expect(screen.getByText('This module page does not exist.')).toBeInTheDocument()
  })

  it('shows an error state when module data loading fails', async () => {
    fetchEmailCheck.mockRejectedValue(new Error('Could not load email details.'))

    renderWithModuleRoute('/dashboard/example.com/email')

    expect(await screen.findByText('Could not load email details.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /back to dashboard/i })).toBeInTheDocument()
  })

  it('renders the e-mail error narrative when DNS-based checks fail', async () => {
    fetchEmailCheck.mockResolvedValue(createEmailErrorResult())

    renderWithModuleRoute('/dashboard/example.com/email')

    await waitFor(() => {
      expect(fetchEmailCheck).toHaveBeenCalled()
    })

    expect(await screen.findByText('E-mail security analysis')).toBeInTheDocument()
    expect(screen.getByText(/dns-based e-mail security checks could not be completed reliably/i)).toBeInTheDocument()
    expect(screen.getByText('ERROR')).toBeInTheDocument()
  })
})

function renderWithModuleRoute(initialEntry: string) {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route path="/dashboard/:domain/:module" element={<ModuleDetailPage />} />
      </Routes>
    </MemoryRouter>,
  )
}

function createEmailErrorResult(): EmailCheckResult {
  return {
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
    dnsSummary: {
      mxRecords: [],
      spfRecord: '',
      dmarcRecord: '',
      dkimSelectorsFound: [],
    },
    alerts: [
      {
        type: 'WARNING',
        message: 'Email security DNS lookups could not be completed reliably. MX lookup could not be completed.',
      },
    ],
  }
}
