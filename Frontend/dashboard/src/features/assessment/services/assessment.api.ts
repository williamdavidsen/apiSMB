import { apiUrl } from '../../../shared/lib/apiBase'
import { normalizeDomainInput } from '../../../shared/lib/domain'
import type {
  AssessmentCheckResult,
  AssessmentDashboardBundle,
  EmailCheckResult,
  HeadersCheckResult,
  ReputationCheckResult,
  SslDetailResult,
  SslCheckResult,
} from '../model/assessment.types'

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: {
      Accept: 'application/json',
      ...(init?.headers ?? {}),
    },
  })

  if (!response.ok) {
    const body = await response.text().catch(() => '')
    throw new Error(body || `Request failed (${response.status})`)
  }

  return response.json() as Promise<T>
}

function encodeDomain(domain: string): string {
  return encodeURIComponent(normalizeDomainInput(domain))
}

export async function fetchAssessmentDashboardBundle(
  domain: string,
  signal?: AbortSignal,
): Promise<AssessmentDashboardBundle> {
  const encoded = encodeDomain(domain)
  const assessment = await fetchJson<AssessmentCheckResult>(apiUrl(`/api/assessment/check/${encoded}`), { signal })

  const [sslResult, headersResult, emailResult, reputationResult] = await Promise.allSettled([
    fetchJson<SslCheckResult>(apiUrl(`/api/ssl/check/${encoded}`), { signal }),
    fetchJson<HeadersCheckResult>(apiUrl(`/api/headers/check/${encoded}`), { signal }),
    fetchJson<EmailCheckResult>(apiUrl(`/api/email/check/${encoded}`), { signal }),
    fetchJson<ReputationCheckResult>(apiUrl(`/api/reputation/check/${encoded}`), { signal }),
  ])

  return {
    assessment,
    ssl: sslResult.status === 'fulfilled' ? sslResult.value : createFallbackSslResult(assessment),
    headers: headersResult.status === 'fulfilled' ? headersResult.value : createFallbackHeadersResult(assessment),
    email: emailResult.status === 'fulfilled' ? emailResult.value : createFallbackEmailResult(assessment),
    reputation:
      reputationResult.status === 'fulfilled'
        ? reputationResult.value
        : createFallbackReputationResult(assessment),
  }
}

export async function fetchAssessmentCheck(domain: string, signal?: AbortSignal): Promise<AssessmentCheckResult> {
  return fetchJson<AssessmentCheckResult>(apiUrl(`/api/assessment/check/${encodeDomain(domain)}`), { signal })
}

export async function fetchSslCheck(domain: string, signal?: AbortSignal): Promise<SslCheckResult> {
  return fetchJson<SslCheckResult>(apiUrl(`/api/ssl/check/${encodeDomain(domain)}`), { signal })
}

export async function fetchSslDetails(domain: string, signal?: AbortSignal): Promise<SslDetailResult> {
  return fetchJson<SslDetailResult>(apiUrl(`/api/ssl/details/${encodeDomain(domain)}`), { signal })
}

export async function fetchHeadersCheck(domain: string, signal?: AbortSignal): Promise<HeadersCheckResult> {
  return fetchJson<HeadersCheckResult>(apiUrl(`/api/headers/check/${encodeDomain(domain)}`), { signal })
}

export async function fetchEmailCheck(domain: string, signal?: AbortSignal): Promise<EmailCheckResult> {
  return fetchJson<EmailCheckResult>(apiUrl(`/api/email/check/${encodeDomain(domain)}`), { signal })
}

export async function fetchReputationCheck(
  domain: string,
  signal?: AbortSignal,
): Promise<ReputationCheckResult> {
  return fetchJson<ReputationCheckResult>(apiUrl(`/api/reputation/check/${encodeDomain(domain)}`), { signal })
}

function createFallbackSslResult(assessment: AssessmentCheckResult): SslCheckResult {
  const module = assessment.modules.sslTls
  const status = module.status || (module.rawScore <= 0 ? 'ERROR' : 'WARNING')

  return {
    domain: assessment.domain,
    overallScore: module.rawScore,
    maxScore: module.rawMaxScore,
    status,
    criteria: {
      tlsVersion: { score: 0, details: 'Detailed TLS version data could not be loaded.' },
      certificateValidity: { score: 0, details: 'Certificate validity details could not be loaded.' },
      remainingLifetime: { score: 0, details: 'Certificate lifetime details could not be loaded.' },
      cipherStrength: { score: 0, details: 'Cipher suite details could not be loaded.' },
    },
    alerts: [
      {
        type: status.toUpperCase() === 'ERROR' ? 'WARNING' : 'INFO',
        message: 'Detailed SSL/TLS results could not be loaded. The summary score is shown from the combined assessment.',
      },
    ],
  }
}

function createFallbackHeadersResult(assessment: AssessmentCheckResult): HeadersCheckResult {
  const module = assessment.modules.httpHeaders
  const status = module.status || (module.rawScore <= 0 ? 'ERROR' : 'WARNING')

  return {
    domain: assessment.domain,
    overallScore: module.rawScore,
    maxScore: module.rawMaxScore,
    status,
    criteria: {
      strictTransportSecurity: { score: 0, details: 'HSTS details could not be loaded.' },
      contentSecurityPolicy: { score: 0, details: 'CSP details could not be loaded.' },
      clickjackingProtection: { score: 0, details: 'Clickjacking protection details could not be loaded.' },
      mimeSniffingProtection: { score: 0, details: 'MIME sniffing protection details could not be loaded.' },
      referrerPolicy: { score: 0, details: 'Referrer-Policy details could not be loaded.' },
    },
    observatory: {
      grade: 'UNAVAILABLE',
      score: 0,
      testsPassed: 0,
      testsFailed: 0,
      testsQuantity: 0,
      detailsUrl: '',
    },
    alerts: [
      {
        type: status.toUpperCase() === 'ERROR' ? 'WARNING' : 'INFO',
        message: 'Detailed HTTP header results could not be loaded. The summary score is shown from the combined assessment.',
      },
    ],
  }
}

function createFallbackEmailResult(assessment: AssessmentCheckResult): EmailCheckResult {
  const module = assessment.modules.emailSecurity
  const included = assessment.emailModuleIncluded
  const status = included ? module.status || 'ERROR' : 'NOT_EVALUATED'

  return {
    domain: assessment.domain,
    hasMailService: included,
    moduleApplicable: included,
    overallScore: module.rawScore,
    maxScore: module.rawMaxScore,
    status,
    criteria: {
      spfVerification: { score: 0, confidence: 'LOW', details: 'SPF details could not be loaded.' },
      dkimActivated: { score: 0, confidence: 'LOW', details: 'DKIM details could not be loaded.' },
      dmarcEnforcement: { score: 0, confidence: 'LOW', details: 'DMARC details could not be loaded.' },
    },
    dnsSummary: {
      mxRecords: [],
      spfRecord: '',
      dmarcRecord: '',
      dkimSelectorsFound: [],
    },
    alerts: [
      {
        type: included ? 'WARNING' : 'INFO',
        message: included
          ? 'Detailed e-mail security results could not be loaded. The summary score is shown from the combined assessment.'
          : 'E-mail security was not included in the weighted score for this domain.',
      },
    ],
  }
}

function createFallbackReputationResult(assessment: AssessmentCheckResult): ReputationCheckResult {
  const module = assessment.modules.reputation
  const included = module.included
  const status = included ? module.status || 'ERROR' : 'NOT_EVALUATED'

  return {
    domain: assessment.domain,
    overallScore: module.rawScore,
    maxScore: module.rawMaxScore,
    status,
    criteria: {
      blacklistStatus: {
        score: 0,
        confidence: 'LOW',
        details: 'Blacklist status details could not be loaded.',
      },
      malwareAssociation: {
        score: 0,
        confidence: 'LOW',
        details: 'Malware association details could not be loaded.',
      },
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
    alerts: [
      {
        type: included ? 'WARNING' : 'INFO',
        message: included
          ? 'Detailed reputation results could not be loaded. The summary score is shown from the combined assessment.'
          : 'Reputation data was not included in the weighted score for this domain.',
      },
    ],
  }
}
