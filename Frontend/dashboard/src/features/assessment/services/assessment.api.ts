import { apiUrl } from '../../../shared/lib/apiBase'
import type {
  AssessmentCheckResult,
  AssessmentDashboardBundle,
  EmailCheckResult,
  HeadersCheckResult,
  ReputationCheckResult,
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
  return encodeURIComponent(domain.trim())
}

export async function fetchAssessmentDashboardBundle(
  domain: string,
  signal?: AbortSignal,
): Promise<AssessmentDashboardBundle> {
  const encoded = encodeDomain(domain)

  const [assessment, ssl, headers, email, reputation] = await Promise.all([
    fetchJson<AssessmentCheckResult>(apiUrl(`/api/assessment/check/${encoded}`), { signal }),
    fetchJson<SslCheckResult>(apiUrl(`/api/ssl/check/${encoded}`), { signal }),
    fetchJson<HeadersCheckResult>(apiUrl(`/api/headers/check/${encoded}`), { signal }),
    fetchJson<EmailCheckResult>(apiUrl(`/api/email/check/${encoded}`), { signal }),
    fetchJson<ReputationCheckResult>(apiUrl(`/api/reputation/check/${encoded}`), { signal }),
  ])

  return { assessment, ssl, headers, email, reputation }
}

export async function fetchSslCheck(domain: string, signal?: AbortSignal): Promise<SslCheckResult> {
  return fetchJson<SslCheckResult>(apiUrl(`/api/ssl/check/${encodeDomain(domain)}`), { signal })
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
