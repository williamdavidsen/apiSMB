import { gradeFromPercent, modulePercent } from '../../../shared/lib/score'
import type { AssessmentDashboardBundle } from './assessment.types'
import type { DashboardModuleKey } from './assessment.types'

export type ExecutiveBanner = {
  title: string
  body: string
  severity: 'critical' | 'warning' | 'info'
}

const alertPriority: Record<string, number> = {
  CRITICAL_ALARM: 0,
  CRITICAL_WARNING: 1,
  WARNING: 2,
  INFO: 3,
}

function alertRank(type: string): number {
  const key = type.trim().toUpperCase()
  return alertPriority[key] ?? 9
}

export function pickExecutiveBanner(bundle: AssessmentDashboardBundle): ExecutiveBanner | null {
  const ranked = [...bundle.assessment.alerts].sort((a, b) => alertRank(a.type) - alertRank(b.type))
  const top = ranked[0]
  if (!top) return null

  const severity =
    alertRank(top.type) <= 1 ? 'critical' : alertRank(top.type) === 2 ? 'warning' : 'info'

  const title = inferBannerTitle(bundle, top.type)
  return { title, body: top.message, severity }
}

function inferBannerTitle(bundle: AssessmentDashboardBundle, topType: string): string {
  const type = topType.trim().toUpperCase()
  if (type === 'CRITICAL_ALARM' || type === 'CRITICAL_WARNING') {
    if (bundle.ssl.alerts.some((a) => a.type.toUpperCase().includes('CRITICAL'))) {
      return 'HTTPS / certificate risk'
    }
  }

  if (bundle.ssl.status.toUpperCase() === 'FAIL' && bundle.ssl.overallScore === 0) {
    return 'HTTPS / certificate risk'
  }

  if (topType.toUpperCase().includes('HEADER') || bundle.headers.status.toUpperCase() === 'FAIL') {
    return 'HTTP security headers'
  }

  if (topType.toUpperCase().includes('EMAIL')) {
    return 'E-mail authentication'
  }

  if (topType.toUpperCase().includes('REPUTATION')) {
    return 'Domain / IP reputation'
  }

  return 'Assessment summary'
}

export function dashboardHeadline(grade: string, status: string, score: number): string {
  const g = grade.trim().toUpperCase()
  const s = status.trim().toUpperCase()

  if (s === 'PARTIAL') {
    return 'Partial security assessment'
  }

  if (score <= 0 || g === 'F' || (s === 'FAIL' && score < 50)) {
    return 'Critical security failure'
  }

  if (s === 'FAIL' || g === 'E' || g === 'D') {
    return 'Security improvements needed'
  }

  if (s === 'WARNING' || g === 'C') {
    return 'Security posture needs attention'
  }
  return 'Security analysis dashboard'
}

export function dashboardEmailSubtitle(emailModuleIncluded: boolean, emailStatus: string): string {
  const status = emailStatus.trim().toUpperCase()
  if (status === 'ERROR') {
    return '(E-mail security could not be evaluated reliably)'
  }

  return emailModuleIncluded
    ? '(E-mail security evaluated from DNS records)'
    : '(E-mail security not evaluated)'
}

export function formatHeaderPresence(detail: { score: number; details: string }): string {
  if (detail.score <= 0) return 'Missing'
  return 'Present'
}

export function isHeaderControlMissingOrWeak(detail: { score: number; details: string }): boolean {
  const text = detail.details.trim().toLowerCase()
  if (!text) return detail.score <= 0

  return (
    text.includes('missing') ||
    text.includes('was not found') ||
    text.includes('neither x-frame-options nor csp frame-ancestors was found') ||
    text.includes('unsafe directives') ||
    text.includes('weaker than recommended')
  )
}

export function formatEmailPresence(detail: { score: number }): string {
  if (detail.score <= 0) return 'Missing'
  return 'Detected'
}

export function reputationVerdict(status: string, _suspicious: number, malicious: number): string {
  const key = status.trim().toUpperCase()
  if (key === 'ERROR') return 'Unknown'
  if (malicious > 0) return 'Malicious signals'
  if (key === 'FAIL') return 'Suspicious'
  if (key === 'WARNING') return 'Mixed signals'
  if (key === 'PASS') return 'Clean'
  return status || 'Unknown'
}

/** Row highlight for chips (driven by API-mapped labels/values in `buildModuleCards`). */
export type ModuleFactTone = 'success' | 'warning' | 'error' | 'neutral'

export type ModuleCardFact = {
  label: string
  value: string
  tone: ModuleFactTone
}

export type ModuleCardView = {
  key: DashboardModuleKey
  title: string
  moduleGrade: string
  /** Raw module status from the API (SSL/Headers/Email/Reputation). */
  moduleApiStatus: string
  /** Filled portion of the module score bar (API rawScore / rawMaxScore). */
  scoreFill?: { current: number; max: number }
  /** Extra context above the score bar (e.g. Observatory). Omit when the only text would repeat the module score. */
  statusLine?: string
  facts: ModuleCardFact[]
  bullet?: string
  callout?: { message: string; tone: 'critical' | 'warning' | 'info' }
}

type ModuleCardSummary = {
  bullet?: string
  callout?: { message: string; tone: 'critical' | 'warning' | 'info' }
}

/** Avoid grey bullet + colored callout showing the same API line. */
function hideBulletIfSameAsCallout(
  bulletWithPrefix: string | undefined,
  calloutMessage: string | undefined,
): string | undefined {
  if (!bulletWithPrefix || !calloutMessage?.trim()) return bulletWithPrefix
  const body = bulletWithPrefix.replace(/^\s*•\s*/u, '').trim()
  if (body === calloutMessage.trim()) return undefined
  return bulletWithPrefix
}

function toneTlsStatus(status: string): ModuleFactTone {
  const u = status.trim().toUpperCase()
  if (u === 'PASS') return 'success'
  if (u === 'WARNING') return 'warning'
  if (u === 'FAIL') return 'error'
  if (u === 'ERROR') return 'neutral'
  return 'neutral'
}

function toneHeaderPresence(value: string): ModuleFactTone {
  if (value === 'Missing') return 'error'
  if (value === 'Present') return 'success'
  return 'neutral'
}

function toneRisk(value: string): ModuleFactTone {
  if (value === 'High') return 'error'
  if (value === 'Moderate') return 'warning'
  if (value === 'Low') return 'success'
  return 'neutral'
}

function toneEmailRow(value: string): ModuleFactTone {
  if (value === 'Missing') return 'error'
  if (value === 'Detected') return 'success'
  if (value === 'Not evaluated') return 'neutral'
  if (value === 'Unavailable') return 'warning'
  return 'neutral'
}

function toneReputationVerdict(value: string): ModuleFactTone {
  if (value === 'Unknown') return 'warning'
  if (value === 'Clean') return 'success'
  if (value === 'Suspicious' || value.includes('Malicious')) return 'error'
  if (value === 'Mixed signals') return 'warning'
  return 'neutral'
}

function isObservatoryOnlyHeaderAlert(message: string): boolean {
  const text = message.trim().toLowerCase()
  return text.includes('mozilla observatory')
}

function headerPositiveSummary(headers: AssessmentDashboardBundle['headers']): string {
  const hasStrongHsts = headers.criteria.strictTransportSecurity.score >= 3
  const hasStrongCsp = headers.criteria.contentSecurityPolicy.score >= 4
  const hasClickjackingProtection = headers.criteria.clickjackingProtection.score >= 3

  if (hasStrongHsts && hasStrongCsp && hasClickjackingProtection) {
    return 'Strong HSTS, CSP, and clickjacking protections were detected in this scan.'
  }

  if (hasStrongHsts && hasStrongCsp) {
    return 'Core browser-facing protections, including HSTS and CSP, were detected in this scan.'
  }

  if (hasStrongCsp) {
    return 'Content Security Policy was detected and no major browser-side header weakness was identified.'
  }

  return 'Core HTTP security headers were detected in this scan.'
}

function pickHeaderSummary(headers: AssessmentDashboardBundle['headers']): ModuleCardSummary {
  const status = headers.status.trim().toUpperCase()
  const rankedAlert = [...headers.alerts]
    .filter((alert) => !isObservatoryOnlyHeaderAlert(alert.message))
    .sort((a, b) => alertRank(a.type) - alertRank(b.type))[0]

  if (rankedAlert && alertRank(rankedAlert.type) <= 2) {
    return {
      callout: {
        message: rankedAlert.message,
        tone: alertRank(rankedAlert.type) <= 1 ? 'critical' : 'warning',
      },
    }
  }

  if (status === 'ERROR') {
    return {
      callout: {
        message: rankedAlert?.message || 'HTTP header analysis could not be completed reliably.',
        tone: 'warning',
      },
    }
  }

  if (status === 'FAIL') {
    return {
      callout: {
        message: rankedAlert?.message || 'Critical HTTP security headers were missing or the target was not served over HTTPS.',
        tone: 'critical',
      },
    }
  }

  if (status === 'WARNING') {
    return {
      callout: {
        message: rankedAlert?.message || 'Some recommended HTTP security headers were missing or configured more weakly than recommended.',
        tone: 'warning',
      },
    }
  }

  return {
    bullet: `• ${headerPositiveSummary(headers)}`,
  }
}

export function buildModuleCards(bundle: AssessmentDashboardBundle): ModuleCardView[] {
  const { assessment, ssl, headers, email, reputation } = bundle

  const sslPercent = modulePercent(ssl.overallScore, ssl.maxScore)
  const sslGrade = gradeFromPercent(sslPercent)

  const sslBullet =
    ssl.criteria.remainingLifetime.details ||
    ssl.alerts[0]?.message ||
    ssl.criteria.certificateValidity.details

  const sslCallout =
    ssl.alerts.find((a) => a.type.toUpperCase().includes('CRITICAL')) ??
    ssl.alerts.find((a) => a.type.toUpperCase().includes('WARNING'))

  const headersRisk =
    headers.status.toUpperCase() === 'ERROR'
      ? 'Unknown'
      : headers.status.toUpperCase() === 'PASS'
        ? 'Low'
        : headers.status.toUpperCase() === 'WARNING'
          ? 'Moderate'
          : 'High'

  const headersPercent = modulePercent(headers.overallScore, headers.maxScore)
  const headersGrade = gradeFromPercent(headersPercent)
  const headerSummary = pickHeaderSummary(headers)
  const emailIncluded = assessment.emailModuleIncluded
  const emailUnavailable = email.status.toUpperCase() === 'ERROR'

  const emailFacts: ModuleCardFact[] = emailIncluded
    ? [
        {
          label: 'SPF',
          value: formatEmailPresence(email.criteria.spfVerification),
          tone: toneEmailRow(formatEmailPresence(email.criteria.spfVerification)),
        },
        {
          label: 'DKIM',
          value: formatEmailPresence(email.criteria.dkimActivated),
          tone: toneEmailRow(formatEmailPresence(email.criteria.dkimActivated)),
        },
        {
          label: 'DMARC',
          value: formatEmailPresence(email.criteria.dmarcEnforcement),
          tone: toneEmailRow(formatEmailPresence(email.criteria.dmarcEnforcement)),
        },
      ]
    : emailUnavailable
      ? [
          { label: 'SPF', value: 'Unavailable', tone: 'warning' },
          { label: 'DKIM', value: 'Unavailable', tone: 'warning' },
          { label: 'DMARC', value: 'Unavailable', tone: 'warning' },
        ]
    : [
        { label: 'SPF', value: 'Not evaluated', tone: 'neutral' },
        { label: 'DKIM', value: 'Not evaluated', tone: 'neutral' },
        { label: 'DMARC', value: 'Not evaluated', tone: 'neutral' },
      ]

  const emailBullet = emailIncluded
    ? email.alerts[0]?.message || email.criteria.spfVerification.details
    : emailUnavailable
      ? email.alerts[0]?.message || 'E-mail security DNS lookups could not be completed reliably.'
      : 'No MX records were found for this hostname, so e-mail authentication was not scored.'

  const emailCallout = emailIncluded
    ? email.alerts.find((a) => a.type.toUpperCase().includes('CRITICAL'))
    : emailUnavailable
      ? { type: 'WARNING', message: email.alerts[0]?.message || 'E-mail security DNS lookups could not be completed reliably.' }
      : undefined

  const repVerdict = reputationVerdict(
    reputation.status,
    reputation.summary.suspiciousDetections,
    reputation.summary.maliciousDetections,
  )
  const reputationIncluded = assessment.modules.reputation.included

  const reputationFacts: ModuleCardFact[] = reputationIncluded
    ? [
        { label: 'Verdict', value: repVerdict, tone: toneReputationVerdict(repVerdict) },
        {
          label: 'Signals',
          value: `malicious ${reputation.summary.maliciousDetections}, suspicious ${reputation.summary.suspiciousDetections}`,
          tone: 'neutral',
        },
      ]
    : [
        { label: 'Verdict', value: 'Not evaluated', tone: 'neutral' },
        { label: 'Signals', value: 'Not evaluated', tone: 'neutral' },
      ]

  const repBullet =
    (reputationIncluded ? reputation.criteria.blacklistStatus.details : undefined) ||
    reputation.alerts[0]?.message ||
    (reputationIncluded
      ? `Sampled detections: malicious ${reputation.summary.maliciousDetections}, suspicious ${reputation.summary.suspiciousDetections}.`
      : 'Domain / IP reputation was not included in the final weighted score because the upstream provider could not be reached reliably.')

  const repCallout = reputation.alerts.find((a) => a.type.toUpperCase().includes('CRITICAL'))

  const hstsVal = formatHeaderPresence(headers.criteria.strictTransportSecurity)
  const cspVal = formatHeaderPresence(headers.criteria.contentSecurityPolicy)

  const emailNotInFinalScoreMessage =
    'E-mail authentication was not included in the final weighted score.'

  const cards: ModuleCardView[] = [
    {
      key: 'ssl-tls',
      title: 'TLS / SSL',
      moduleGrade: sslGrade,
      moduleApiStatus: ssl.status,
      scoreFill: { current: ssl.overallScore, max: ssl.maxScore },
      facts: [{ label: 'TLS status', value: ssl.status, tone: toneTlsStatus(ssl.status) }],
      bullet: hideBulletIfSameAsCallout(
        sslBullet ? `• ${sslBullet}` : undefined,
        sslCallout?.message,
      ),
      callout: sslCallout
        ? {
            message: sslCallout.message,
            tone: sslCallout.type.toUpperCase().includes('CRITICAL') ? 'critical' : 'warning',
          }
        : undefined,
    },
    {
      key: 'http-headers',
      title: 'HTTP Headers',
      moduleGrade: headersGrade,
      moduleApiStatus: headers.status,
      scoreFill: { current: headers.overallScore, max: headers.maxScore },
      facts: [
        { label: 'HSTS', value: hstsVal, tone: toneHeaderPresence(hstsVal) },
        { label: 'CSP', value: cspVal, tone: toneHeaderPresence(cspVal) },
        { label: 'Risk', value: headersRisk, tone: toneRisk(headersRisk) },
      ],
      bullet: headerSummary.bullet,
      callout: headerSummary.callout,
    },
    {
      key: 'email',
      title: 'E-mail',
      moduleGrade: emailIncluded ? gradeFromPercent(modulePercent(email.overallScore, email.maxScore)) : '—',
      moduleApiStatus: email.status,
      scoreFill: emailIncluded ? { current: email.overallScore, max: email.maxScore } : undefined,
      statusLine: emailIncluded ? undefined : emailUnavailable ? 'Could not evaluate' : 'Not evaluated',
      facts: emailFacts,
      bullet: hideBulletIfSameAsCallout(
        emailBullet ? `• ${emailBullet}` : undefined,
        emailIncluded ? emailCallout?.message : emailUnavailable ? emailCallout?.message : emailNotInFinalScoreMessage,
      ),
      callout: emailCallout
        ? {
            message: emailCallout.message,
            tone: emailUnavailable ? 'warning' : 'critical',
          }
        : emailIncluded
          ? undefined
          : emailUnavailable
            ? {
                message: email.alerts[0]?.message || 'E-mail security DNS lookups could not be completed reliably.',
                tone: 'warning',
              }
            : { message: emailNotInFinalScoreMessage, tone: 'info' },
    },
    {
      key: 'reputation',
      title: 'Domain / IP reputation',
      moduleGrade: reputationIncluded ? gradeFromPercent(modulePercent(reputation.overallScore, reputation.maxScore)) : '—',
      moduleApiStatus: reputation.status,
      scoreFill: reputationIncluded ? { current: reputation.overallScore, max: reputation.maxScore } : undefined,
      statusLine: reputationIncluded ? undefined : 'Not evaluated',
      facts: reputationFacts,
      bullet: hideBulletIfSameAsCallout(
        repBullet ? `• ${repBullet}` : undefined,
        repCallout?.message,
      ),
      callout: repCallout
        ? { message: repCallout.message, tone: 'critical' }
        : undefined,
    },
  ]

  return cards
}


