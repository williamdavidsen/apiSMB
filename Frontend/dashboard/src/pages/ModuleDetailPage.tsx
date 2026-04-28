import DownloadRounded from '@mui/icons-material/DownloadRounded'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import CircularProgress from '@mui/material/CircularProgress'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { alpha } from '@mui/material/styles'
import { useEffect, useMemo, useRef, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { isDashboardModuleKey } from '../features/assessment/model/assessment.constants'
import type {
  DashboardModuleKey,
  EmailCheckResult,
  HeadersCheckResult,
  ReputationCheckResult,
  SslCheckResult,
  SslDetailResult,
} from '../features/assessment/model/assessment.types'
import {
  fetchEmailCheck,
  fetchHeadersCheck,
  fetchSslDetails,
  fetchReputationCheck,
  fetchSslCheck,
} from '../features/assessment/services/assessment.api'
import { isHeaderControlMissingOrWeak } from '../features/assessment/model/assessment.mappers'
import { exportElementAsPdf } from '../features/module-detail/services/pdfExport'
import { routes } from '../shared/constants/routes'
import { normalizeDomainInput } from '../shared/lib/domain'
import { gradeFromPercent, modulePercent } from '../shared/lib/score'

type ModulePayload = SslCheckResult | HeadersCheckResult | EmailCheckResult | ReputationCheckResult

type LoadedData = {
  moduleKey: DashboardModuleKey
  modulePayload: ModulePayload
  sslDetails?: SslDetailResult
  sslDetailsStatus?: 'loading' | 'ready' | 'unavailable'
  loadedAtIso: string
}

type AsyncState =
  | { status: 'idle' | 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; data: LoadedData }

type Narrative = {
  moduleTitle: string
  score: number
  maxScore: number
  moduleGrade: string
  status: string
  summary: string[]
  recommendation: string
  checklist: string[]
  evidence: string[]
}

type ReadMoreField = {
  pdfSubject: string
  value: string
}

type ReadMoreSection = {
  title: string
  fields: ReadMoreField[]
}

type ReadMoreOutput = {
  overviewTitle?: string
  overview?: ReadMoreField[]
  criteria: ReadMoreSection[]
}

const moduleTabs: Array<{ key: DashboardModuleKey; label: string }> = [
  { key: 'ssl-tls', label: 'TLS / SSL' },
  { key: 'http-headers', label: 'HTTP Headers' },
  { key: 'email', label: 'E-mail' },
  { key: 'reputation', label: 'Domain / IP reputation' },
]

export function ModuleDetailPage() {
  const navigate = useNavigate()
  const params = useParams()
  const printableRef = useRef<HTMLDivElement | null>(null)

  const domain = useMemo(() => {
    const raw = params.domain ?? ''
    try {
      return normalizeDomainInput(decodeURIComponent(raw))
    } catch {
      return normalizeDomainInput(raw)
    }
  }, [params.domain])

  const moduleKey = params.module ?? ''
  const invalidModule = moduleKey.length > 0 && !isDashboardModuleKey(moduleKey)

  const [state, setState] = useState<AsyncState>({ status: 'idle' })
  const [isDownloadingPdf, setIsDownloadingPdf] = useState(false)

  useEffect(() => {
    if (invalidModule || !domain) {
      return
    }

    const controller = new AbortController()
    setState({ status: 'loading' })

    void (async () => {
      try {
        const modulePayload =
          moduleKey === 'ssl-tls'
            ? await fetchSslCheck(domain, controller.signal)
            : moduleKey === 'http-headers'
              ? await fetchHeadersCheck(domain, controller.signal)
              : moduleKey === 'email'
                ? await fetchEmailCheck(domain, controller.signal)
                : await fetchReputationCheck(domain, controller.signal)

        if (controller.signal.aborted) return
        setState({
          status: 'success',
          data: {
            moduleKey: moduleKey as DashboardModuleKey,
            modulePayload,
            sslDetailsStatus: moduleKey === 'ssl-tls' ? 'loading' : undefined,
            loadedAtIso: new Date().toISOString(),
          },
        })

        // SSL details can be slower; load them in background after first render.
        if (moduleKey === 'ssl-tls') {
          void fetchSslDetails(domain, controller.signal)
            .then((sslDetails) => {
              if (controller.signal.aborted) return
              setState((prev) =>
                prev.status === 'success' && prev.data.moduleKey === 'ssl-tls'
                  ? {
                      status: 'success',
                      data: {
                        ...prev.data,
                        sslDetails,
                        sslDetailsStatus: 'ready',
                      },
                    }
                  : prev,
              )
            })
            .catch(() => {
              if (controller.signal.aborted) return
              setState((prev) =>
                prev.status === 'success' && prev.data.moduleKey === 'ssl-tls'
                  ? {
                      status: 'success',
                      data: {
                        ...prev.data,
                        sslDetailsStatus: 'unavailable',
                      },
                    }
                  : prev,
              )
            })
        }
      } catch (error) {
        if (controller.signal.aborted) return
        const message = error instanceof Error ? error.message : 'Could not load module details.'
        setState({ status: 'error', message })
      }
    })()

    return () => controller.abort()
  }, [domain, invalidModule, moduleKey])

  const backToDashboard = () => {
    const query = new URLSearchParams({ domain })
    navigate(`${routes.dashboard}?${query.toString()}`)
  }

  async function handleDownloadPdf() {
    if (!printableRef.current || state.status !== 'success') return

    setIsDownloadingPdf(true)
    try {
      const safeDomain = domain.replace(/[^a-z0-9.-]/gi, '_')
      await exportElementAsPdf(printableRef.current, `read-more-${moduleKey}-${safeDomain}.pdf`)
    } catch {
      // Keep the detail page usable even if client-side PDF export fails.
    } finally {
      setIsDownloadingPdf(false)
    }
  }

  if (invalidModule) {
    return (
      <Box sx={{ maxWidth: 720, mx: 'auto' }}>
        <Alert severity="warning">This module page does not exist.</Alert>
        <Button sx={{ mt: 2 }} variant="contained" color="secondary" onClick={() => navigate(routes.home)}>
          Go to home
        </Button>
      </Box>
    )
  }

  if (!domain) {
    return (
      <Box sx={{ maxWidth: 720, mx: 'auto' }}>
        <Alert severity="info">Missing domain in the URL.</Alert>
        <Button sx={{ mt: 2 }} variant="outlined" color="secondary" onClick={() => navigate(routes.home)}>
          Go to home
        </Button>
      </Box>
    )
  }

  if (state.status === 'loading' || state.status === 'idle') {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }} role="status" aria-live="polite">
        <Stack spacing={2} sx={{ alignItems: 'center' }}>
          <CircularProgress aria-label="Loading module details" />
          <Typography color="text.secondary">Loading module details...</Typography>
        </Stack>
      </Box>
    )
  }

  if (state.status === 'error') {
    return (
      <Box sx={{ maxWidth: 720, mx: 'auto' }}>
        <Alert severity="error">{state.message}</Alert>
        <Button sx={{ mt: 2 }} variant="outlined" color="secondary" onClick={backToDashboard}>
          Back to dashboard
        </Button>
      </Box>
    )
  }

  if (state.status !== 'success') {
    return null
  }

  const safeModuleKey = moduleKey as DashboardModuleKey
  if (state.data.moduleKey !== safeModuleKey) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }} role="status" aria-live="polite">
        <Stack spacing={2} sx={{ alignItems: 'center' }}>
          <CircularProgress aria-label="Switching module details" />
          <Typography color="text.secondary">Switching module details...</Typography>
        </Stack>
      </Box>
    )
  }

  const narrative = buildNarrative(safeModuleKey, state.data.modulePayload, state.data.sslDetails)
  const encodedDomain = encodeURIComponent(domain)

  return (
    <Box sx={{ maxWidth: 1040, mx: 'auto', width: '100%' }}>
      <Stack spacing={2.5}>
        <Stack direction={{ xs: 'column', sm: 'row' }} sx={{ justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' }, gap: 1.5 }}>
          <Button variant="text" color="secondary" onClick={backToDashboard} sx={{ alignSelf: 'flex-start', fontWeight: 800 }}>
            {'<- Back to results'}
          </Button>
          <Button
            variant="outlined"
            color="secondary"
            startIcon={<DownloadRounded />}
            onClick={handleDownloadPdf}
            disabled={isDownloadingPdf}
            sx={{ fontWeight: 700 }}
          >
            {isDownloadingPdf ? 'Preparing PDF...' : 'Download report (PDF)'}
          </Button>
        </Stack>

        <Stack spacing={0.5}>
          <Box
            sx={{
              display: 'flex',
              gap: 1,
              overflowX: 'auto',
              pb: 0.5,
              '&::-webkit-scrollbar': { height: 6 },
              '&::-webkit-scrollbar-thumb': { backgroundColor: alpha('#00acc1', 0.35), borderRadius: 999 },
            }}
          >
            {moduleTabs.map((tab) => {
              const isActive = tab.key === safeModuleKey
              return (
                <Button
                  key={tab.key}
                  component={RouterLink}
                  to={`${routes.dashboard}/${encodedDomain}/${tab.key}`}
                  variant={isActive ? 'contained' : 'outlined'}
                  color="secondary"
                  sx={{
                    flexShrink: 0,
                    borderRadius: 999,
                    px: 2,
                    py: 0.75,
                    fontWeight: 800,
                    whiteSpace: 'nowrap',
                    boxShadow: isActive ? '0 6px 18px rgba(0, 172, 193, 0.28)' : 'none',
                  }}
                  aria-current={isActive ? 'page' : undefined}
                >
                  {tab.label}
                </Button>
              )
            })}
          </Box>
          <Typography variant="caption" sx={{ color: 'text.secondary', px: 0.5 }}>
            You can view other module analyses from these tabs.
          </Typography>
        </Stack>

        <Stack ref={printableRef} spacing={2.5}>
          <Paper
            variant="outlined"
            sx={{
              p: { xs: 2, sm: 3 },
              borderRadius: 3,
              borderColor: alpha('#00acc1', 0.25),
              background: 'linear-gradient(180deg, #f7fdff 0%, #ffffff 100%)',
            }}
          >
            <Stack spacing={1.5}>
              <Typography component="h1" variant="h4" sx={{ fontWeight: 900, color: 'secondary.dark' }}>
                {narrative.moduleTitle}
              </Typography>
              <Typography variant="overline" sx={{ color: 'secondary.dark', fontWeight: 800 }}>
                Target domain
              </Typography>
              <Typography component="h2" variant="h4" sx={{ fontWeight: 900, color: 'text.primary' }}>
                {domain}
              </Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ alignItems: { sm: 'center' }, flexWrap: 'wrap' }}>
                <Typography variant="body2" color="text.secondary">
                  {narrative.moduleTitle} deep dive
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  - scanned {formatDateTime(state.data.loadedAtIso)}
                </Typography>
              </Stack>
              <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
                <Chip label={`Score ${narrative.score}/${narrative.maxScore}`} color={statusToChipColor(narrative.status)} />
                <Chip label={`Grade ${narrative.moduleGrade}`} variant="outlined" color="secondary" />
                <Chip label={narrative.status} variant="outlined" color={statusToChipColor(narrative.status)} />
              </Stack>
            </Stack>
          </Paper>

          <SectionCard title="Summary">
            <BulletList items={narrative.summary} />
          </SectionCard>

          {safeModuleKey === 'ssl-tls' ? (
            <ReadMoreDetailCards
              output={buildSslReadMoreOutput(
                domain,
                state.data.modulePayload as SslCheckResult,
                state.data.sslDetails,
              )}
              extendedDataState={state.data.sslDetailsStatus}
            />
          ) : null}

          {safeModuleKey === 'email' ? (
            <ReadMoreDetailCards
              output={buildEmailReadMoreOutput(state.data.modulePayload as EmailCheckResult)}
            />
          ) : null}

          {safeModuleKey === 'reputation' ? (
            <ReadMoreDetailCards
              output={buildReputationReadMoreOutput(state.data.modulePayload as ReputationCheckResult)}
            />
          ) : null}

          {safeModuleKey === 'http-headers' ? (
            <ReadMoreDetailCards
              output={buildHeadersReadMoreOutput(state.data.modulePayload as HeadersCheckResult)}
            />
          ) : null}
        </Stack>
      </Stack>
    </Box>
  )
}

function ReadMoreDetailCards({
  output,
  extendedDataState,
}: {
  output: ReadMoreOutput
  extendedDataState?: 'loading' | 'ready' | 'unavailable'
}) {
  return (
    <Stack spacing={2}>
      {extendedDataState === 'loading' ? (
        <Alert severity="info">
          Extended SSL endpoint data is still loading. The page will fill in certificate and cipher details when the
          API response arrives.
        </Alert>
      ) : null}

      {extendedDataState === 'unavailable' ? (
        <Alert severity="warning">
          Extended SSL endpoint data could not be loaded. The page is using the summary response and keeping the report
          available with reduced detail.
        </Alert>
      ) : null}

      {output.overviewTitle && output.overview?.length ? (
        <SectionCard title={output.overviewTitle}>
          <KeyValueRows rows={output.overview} />
        </SectionCard>
      ) : null}

      {output.criteria.map((section) => (
        <SectionCard key={section.title} title={section.title}>
          <KeyValueRows rows={section.fields} />
        </SectionCard>
      ))}
    </Stack>
  )
}

function KeyValueRows({ rows }: { rows: ReadMoreField[] }) {
  return (
    <Stack spacing={0.75}>
      {rows.map((row) => (
        <Box
          key={row.pdfSubject}
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'minmax(0, 1fr) minmax(120px, 0.32fr)' },
            gap: { xs: 0.25, sm: 2 },
            py: 0.85,
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography
            variant="body2"
            sx={{
              color: 'text.secondary',
              fontWeight: 700,
              minWidth: 0,
              overflowWrap: 'anywhere',
              wordBreak: 'break-word',
            }}
          >
            {row.pdfSubject}
          </Typography>
          <Typography
            variant="body2"
            sx={{
              color: 'text.primary',
              minWidth: 0,
              overflowWrap: 'anywhere',
              wordBreak: 'break-word',
            }}
          >
            {row.value}
          </Typography>
        </Box>
      ))}
    </Stack>
  )
}

function SectionCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, sm: 2.5 }, borderRadius: 3 }}>
      <Typography variant="h6" sx={{ fontWeight: 900, color: 'secondary.dark', mb: 1.25 }}>
        {title}
      </Typography>
      {children}
    </Paper>
  )
}

function BulletList({ items }: { items: string[] }) {
  return (
    <Stack component="ul" spacing={0.9} sx={{ m: 0, pl: 2.3 }}>
      {items.map((item) => (
        <Typography component="li" key={item} variant="body1" sx={{ lineHeight: 1.55 }}>
          {item}
        </Typography>
      ))}
    </Stack>
  )
}

function statusToChipColor(status: string): 'default' | 'success' | 'warning' | 'error' {
  const key = status.trim().toUpperCase()
  if (key === 'PASS') return 'success'
  if (key === 'WARNING') return 'warning'
  if (key === 'ERROR') return 'error'
  if (key === 'FAIL') return 'error'
  return 'default'
}

function formatDateTime(value: string): string {
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return value
  return new Intl.DateTimeFormat('en-GB', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(parsed)
}

function formatReadMoreDate(value?: string | null): string {
  if (!value) return 'Not provided by API'

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return value

  return new Intl.DateTimeFormat('en-GB', {
    weekday: 'short',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(parsed)
}

function fallback(value: string | undefined | null): string {
  const clean = value?.trim()
  return clean ? clean : 'Not provided by API'
}

function formatBoolean(value: boolean): string {
  return value ? 'Yes' : 'No'
}

function formatList(values: string[], emptyLabel = 'None found'): string {
  return values.length > 0 ? values.join(', ') : emptyLabel
}

function headerResultLabel(details: string): string {
  const lower = details.toLowerCase()
  if (lower.includes('missing') || lower.includes('not found') || lower.includes('neither')) {
    return 'Missing'
  }

  if (lower.includes('unsafe') || lower.includes('weaker')) {
    return 'Needs attention'
  }

  return 'Present'
}

function certificateCommonName(sslDetails?: SslDetailResult): string {
  const commonName = sslDetails?.certificate?.commonNames?.[0]
  if (commonName) return commonName

  const subject = sslDetails?.certificate?.subject ?? ''
  const match = subject.match(/(?:^|,\s*)CN\s*=\s*([^,]+)/i)
  return fallback(match?.[1])
}

function hasTlsVersion(sslDetails: SslDetailResult | undefined, version: string, summaryDetails: string): string {
  const source = [
    ...(sslDetails?.supportedTlsVersions ?? []),
    summaryDetails,
  ].join(' ')

  return source.toLowerCase().includes(version.toLowerCase()) ? 'Yes' : 'No'
}

function describeRemainingValidity(daysRemaining?: number | null): string {
  if (daysRemaining == null) return 'Not provided by API'
  if (daysRemaining < 0) return 'Expired'
  if (daysRemaining < 45) return `~${daysRemaining} days remaining`

  const months = Math.max(1, Math.round(daysRemaining / 30))
  return `~${months} months remaining`
}

function describeCertificateType(validFrom?: string | null, validUntil?: string | null): string {
  if (!validFrom || !validUntil) return 'Not provided by API'

  const start = new Date(validFrom)
  const end = new Date(validUntil)
  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return 'Not provided by API'

  const lifetimeDays = (end.getTime() - start.getTime()) / 86_400_000
  return lifetimeDays <= 7 ? 'Short-term' : 'Long-term'
}

function cipherStrengthLabel(cipher: string): string {
  const upper = cipher.toUpperCase()
  if (upper.includes('RC4') || upper.includes('3DES') || upper.includes('NULL') || upper.includes('MD5')) {
    return 'WEAK'
  }

  if (upper.includes('_CBC_')) {
    return 'WEAK'
  }

  if (upper.includes('AES') || upper.includes('CHACHA20')) {
    return 'Strong'
  }

  return 'Observed'
}

function buildSslReadMoreOutput(
  domain: string,
  data: SslCheckResult,
  sslDetails: SslDetailResult | undefined,
): ReadMoreOutput {
  const certificate = sslDetails?.certificate
  const endpoint = sslDetails?.endpoints?.[0]
  const ciphers = sslDetails?.notableCipherSuites?.length
    ? sslDetails.notableCipherSuites
    : data.criteria.cipherStrength.details
      ? [data.criteria.cipherStrength.details]
      : []

  const cipherFields = ciphers.map((cipher) => ({
    pdfSubject: cipher.replace(/\s*\(\d+\s*bits\)\s*$/i, ''),
    value: cipherStrengthLabel(cipher),
  }))

  return {
    overviewTitle: 'Domain overview',
    overview: [
      { pdfSubject: 'Domain', value: domain },
      { pdfSubject: 'Server IP', value: fallback(endpoint?.ipAddress) },
      { pdfSubject: 'Common Name', value: certificateCommonName(sslDetails) },
      { pdfSubject: 'Fingerprint SHA256', value: fallback(certificate?.fingerprintSha256) },
      { pdfSubject: 'Signature Algorithm', value: fallback(certificate?.signatureAlgorithm) },
      { pdfSubject: 'Key', value: fallback(certificate?.key) },
      { pdfSubject: 'Issuer', value: fallback(certificate?.issuer) },
      { pdfSubject: 'Certificate valid from', value: formatReadMoreDate(certificate?.validFrom) },
      { pdfSubject: 'Certificate valid until', value: formatReadMoreDate(certificate?.validUntil) },
    ],
    criteria: [
      {
        title: 'TLS version',
        fields: [
          { pdfSubject: 'TLS 1.3', value: hasTlsVersion(sslDetails, 'TLS 1.3', data.criteria.tlsVersion.details) },
          { pdfSubject: 'TLS 1.2', value: hasTlsVersion(sslDetails, 'TLS 1.2', data.criteria.tlsVersion.details) },
          { pdfSubject: 'TLS 1.1', value: hasTlsVersion(sslDetails, 'TLS 1.1', data.criteria.tlsVersion.details) },
          { pdfSubject: 'TLS 1.0', value: hasTlsVersion(sslDetails, 'TLS 1.0', data.criteria.tlsVersion.details) },
        ],
      },
      {
        title: 'Certificate validity',
        fields: [
          { pdfSubject: 'Valid from', value: formatReadMoreDate(certificate?.validFrom) },
          { pdfSubject: 'Valid until', value: formatReadMoreDate(certificate?.validUntil) },
          { pdfSubject: 'Remaining validity', value: describeRemainingValidity(certificate?.daysRemaining) },
          { pdfSubject: 'Certificate Type', value: describeCertificateType(certificate?.validFrom, certificate?.validUntil) },
        ],
      },
      {
        title: 'Cipher strength',
        fields: [
          ...cipherFields,
          { pdfSubject: 'Forward Secrecy', value: ciphers.some((cipher) => cipher.toUpperCase().includes('ECDHE')) ? 'Yes' : 'Not provided by API' },
          { pdfSubject: 'Key Size', value: fallback(certificate?.key) },
          { pdfSubject: 'Signature Algorithm', value: fallback(certificate?.signatureAlgorithm) },
        ],
      },
    ],
  }
}

function buildEmailReadMoreOutput(data: EmailCheckResult): ReadMoreOutput {
  return {
    criteria: [
      {
        title: 'SPF verification',
        fields: [
          { pdfSubject: 'Confidence', value: data.criteria.spfVerification.confidence },
          { pdfSubject: 'Details', value: fallback(data.criteria.spfVerification.details) },
          { pdfSubject: 'SPF record', value: fallback(data.dnsSummary.spfRecord) },
          { pdfSubject: 'MX records', value: formatList(data.dnsSummary.mxRecords) },
          { pdfSubject: 'Module applicable', value: formatBoolean(data.moduleApplicable) },
        ],
      },
      {
        title: 'DKIM activated',
        fields: [
          { pdfSubject: 'Confidence', value: data.criteria.dkimActivated.confidence },
          { pdfSubject: 'Details', value: fallback(data.criteria.dkimActivated.details) },
          { pdfSubject: 'DKIM selectors found', value: formatList(data.dnsSummary.dkimSelectorsFound) },
          { pdfSubject: 'MX records', value: formatList(data.dnsSummary.mxRecords) },
        ],
      },
      {
        title: 'DMARC enforcement',
        fields: [
          { pdfSubject: 'Confidence', value: data.criteria.dmarcEnforcement.confidence },
          { pdfSubject: 'Details', value: fallback(data.criteria.dmarcEnforcement.details) },
          { pdfSubject: 'DMARC record', value: fallback(data.dnsSummary.dmarcRecord) },
          { pdfSubject: 'SPF context', value: fallback(data.dnsSummary.spfRecord) },
          { pdfSubject: 'Module applicable', value: formatBoolean(data.moduleApplicable) },
        ],
      },
    ],
  }
}

function buildHeadersReadMoreOutput(data: HeadersCheckResult): ReadMoreOutput {
  return {
    criteria: [
      {
        title: 'CSP-header (Content-Security-Policy)',
        fields: [
          { pdfSubject: 'Result', value: headerResultLabel(data.criteria.contentSecurityPolicy.details) },
          { pdfSubject: 'Details', value: fallback(data.criteria.contentSecurityPolicy.details) },
          { pdfSubject: 'Source', value: 'Mozilla Observatory API and live HTTP header probe' },
          { pdfSubject: 'Details URL', value: fallback(data.observatory.detailsUrl) },
        ],
      },
      {
        title: 'X-Frame-Options / Clickjacking',
        fields: [
          { pdfSubject: 'Result', value: headerResultLabel(data.criteria.clickjackingProtection.details) },
          { pdfSubject: 'Details', value: fallback(data.criteria.clickjackingProtection.details) },
          { pdfSubject: 'Related risk', value: 'Clickjacking' },
          { pdfSubject: 'HSTS context', value: fallback(data.criteria.strictTransportSecurity.details) },
          { pdfSubject: 'MIME confusion context', value: fallback(data.criteria.mimeSniffingProtection.details) },
          { pdfSubject: 'Referrer-Policy context', value: fallback(data.criteria.referrerPolicy.details) },
        ],
      },
    ],
  }
}

function buildReputationReadMoreOutput(data: ReputationCheckResult): ReadMoreOutput {
  return {
    overviewTitle: 'Reputation overview',
    overview: [
      { pdfSubject: 'Domain', value: data.domain },
      { pdfSubject: 'Status', value: data.status },
      { pdfSubject: 'Source', value: 'VirusTotal Public API' },
      { pdfSubject: 'Permalink', value: fallback(data.summary.permalink) },
    ],
    criteria: [
      {
        title: 'Blacklist status',
        fields: [
          { pdfSubject: 'Malicious detections', value: `${data.summary.maliciousDetections}` },
          { pdfSubject: 'Suspicious detections', value: `${data.summary.suspiciousDetections}` },
          { pdfSubject: 'Details', value: fallback(data.criteria.blacklistStatus.details) },
        ],
      },
      {
        title: 'Malware association',
        fields: [
          { pdfSubject: 'Malicious detections', value: `${data.summary.maliciousDetections}` },
          { pdfSubject: 'Community malicious votes', value: `${data.summary.communityMaliciousVotes}` },
          { pdfSubject: 'Details', value: fallback(data.criteria.malwareAssociation.details) },
        ],
      },
      {
        title: 'Historical compromise signals',
        fields: [
          { pdfSubject: 'Malicious detections', value: `${data.summary.maliciousDetections}` },
          { pdfSubject: 'Suspicious detections', value: `${data.summary.suspiciousDetections}` },
          {
            pdfSubject: 'Assessment',
            value:
              data.summary.maliciousDetections > 0 || data.summary.suspiciousDetections > 0
                ? 'VirusTotal has historical risk signals for this domain.'
                : 'No historical malicious or suspicious VirusTotal detections were reported.',
          },
        ],
      },
    ],
  }
}

function buildNarrative(moduleKey: DashboardModuleKey, payload: ModulePayload, sslDetails?: SslDetailResult): Narrative {
  if (moduleKey === 'ssl-tls') {
    const data = payload as SslCheckResult
    const daysRemaining = sslDetails?.certificate?.daysRemaining
    const endpointSummary =
      sslDetails?.endpoints?.slice(0, 2).map((endpoint) => `${endpoint.serverName || endpoint.ipAddress}: grade ${endpoint.grade}`).join('; ') ||
      'No endpoint-grade breakdown was returned from SSL details.'

    return {
      moduleTitle: 'TLS / SSL analysis',
      score: data.overallScore,
      maxScore: data.maxScore,
      moduleGrade: gradeFromPercent(modulePercent(data.overallScore, data.maxScore)),
      status: data.status,
      summary: [
        data.criteria.tlsVersion.details || 'TLS protocol posture was measured from available handshake data.',
        data.criteria.certificateValidity.details || 'Certificate validity checks completed without a detailed note.',
        daysRemaining != null
          ? `Certificate has approximately ${daysRemaining} day(s) remaining before expiry.`
          : data.criteria.remainingLifetime.details || 'Certificate remaining lifetime is available in endpoint evidence.',
      ],
      recommendation:
        data.alerts.find((alert) => alert.type.toUpperCase().includes('CRITICAL'))?.message ||
        data.alerts[0]?.message ||
        'Prioritize certificate lifecycle monitoring, then tighten legacy TLS and weaker cipher options.',
      checklist: [
        'Enable certificate expiry monitoring and route alerts to your operations channel.',
        'Keep TLS 1.2+ only in production; retire older protocols and weak ciphers.',
        'Use automated renewal (ACME or enterprise CA workflow) to reduce outage risk.',
      ],
      evidence: [
        `Module status: ${data.status} (${data.overallScore}/${data.maxScore}).`,
        `TLS detail: ${data.criteria.tlsVersion.details || 'Not specified'}.`,
        `Certificate detail: ${data.criteria.certificateValidity.details || 'Not specified'}.`,
        `Endpoint notes: ${endpointSummary}`,
      ],
    }
  }

  if (moduleKey === 'http-headers') {
    const data = payload as HeadersCheckResult
    const missingBits: string[] = []
    if (isHeaderControlMissingOrWeak(data.criteria.strictTransportSecurity)) missingBits.push('HSTS')
    if (isHeaderControlMissingOrWeak(data.criteria.contentSecurityPolicy)) missingBits.push('CSP')
    if (isHeaderControlMissingOrWeak(data.criteria.clickjackingProtection)) missingBits.push('clickjacking protection')
    if (isHeaderControlMissingOrWeak(data.criteria.mimeSniffingProtection)) missingBits.push('MIME sniffing protection')
    if (isHeaderControlMissingOrWeak(data.criteria.referrerPolicy)) missingBits.push('referrer policy')

    return {
      moduleTitle: 'HTTP headers analysis',
      score: data.overallScore,
      maxScore: data.maxScore,
      moduleGrade: gradeFromPercent(modulePercent(data.overallScore, data.maxScore)),
      status: data.status,
      summary: [
        data.criteria.strictTransportSecurity.details,
        data.criteria.contentSecurityPolicy.details,
        missingBits.length > 0
          ? `Missing or weak controls detected: ${missingBits.join(', ')}.`
          : 'Core browser-facing security headers were detected.',
      ],
      recommendation:
        data.alerts.find((alert) => alert.type.toUpperCase().includes('CRITICAL'))?.message ||
        data.alerts[0]?.message ||
        'Roll out missing headers in staged environments and validate with browser compatibility tests.',
      checklist: [
        'Set HSTS with a safe max-age and includeSubDomains after validation.',
        'Adopt CSP with strict defaults and remove unsafe-inline / unsafe-eval where possible.',
        'Add X-Content-Type-Options and a strict Referrer-Policy baseline.',
      ],
      evidence: [
        `Observatory grade: ${data.observatory.grade || 'N/A'} (${data.observatory.score}/100).`,
        `Tests passed: ${data.observatory.testsPassed}/${data.observatory.testsQuantity}.`,
        `Module status: ${data.status} (${data.overallScore}/${data.maxScore}).`,
        data.observatory.detailsUrl ? `Reference URL: ${data.observatory.detailsUrl}` : 'No external details URL provided.',
      ],
    }
  }

  if (moduleKey === 'email') {
    const data = payload as EmailCheckResult
    const moduleErrored = data.status.toUpperCase() === 'ERROR'
    const moduleScored = data.moduleApplicable && !moduleErrored
    return {
      moduleTitle: 'E-mail security analysis',
      score: data.overallScore,
      maxScore: data.maxScore,
      moduleGrade: moduleScored ? gradeFromPercent(modulePercent(data.overallScore, data.maxScore)) : 'N/A',
      status: data.status,
      summary: [
        moduleErrored
          ? 'DNS-based e-mail security checks could not be completed reliably for this domain.'
          : data.criteria.spfVerification.details,
        moduleErrored
          ? 'SPF, DKIM, and DMARC evidence may be incomplete because the upstream DNS lookup failed.'
          : data.criteria.dkimActivated.details,
        moduleErrored
          ? 'Re-run the scan after the DNS provider or network path is available again.'
          : data.criteria.dmarcEnforcement.details,
      ],
      recommendation:
        data.alerts.find((alert) => alert.type.toUpperCase().includes('CRITICAL'))?.message ||
        data.alerts[0]?.message ||
        (moduleScored
          ? 'Strengthen SPF, DKIM, and DMARC together to reduce spoofing and phishing risk.'
          : moduleErrored
            ? 'Verify DNS provider reachability and repeat the scan before drawing conclusions from the e-mail module.'
            : 'No active mail profile found on this domain; validate mail setup before enabling policy enforcement.'),
      checklist: [
        'Publish SPF with explicit sender sources and an enforcement policy.',
        'Enable DKIM signing for all outbound mail flows and rotate keys periodically.',
        'Use DMARC with quarantine/reject and monitor aggregate reports.',
      ],
      evidence: [
        `Module applicable: ${data.moduleApplicable ? 'yes' : 'no'}.`,
        `MX records: ${data.dnsSummary.mxRecords.length > 0 ? data.dnsSummary.mxRecords.join(', ') : 'none found'}.`,
        `SPF: ${moduleErrored ? 'lookup unavailable' : data.dnsSummary.spfRecord || 'not found'}.`,
        `DMARC: ${moduleErrored ? 'lookup unavailable' : data.dnsSummary.dmarcRecord || 'not found'}.`,
      ],
    }
  }

  const data = payload as ReputationCheckResult
  return {
    moduleTitle: 'Reputation analysis',
    score: data.overallScore,
    maxScore: data.maxScore,
    moduleGrade: gradeFromPercent(modulePercent(data.overallScore, data.maxScore)),
    status: data.status,
    summary: [
      data.criteria.blacklistStatus.details,
      data.criteria.malwareAssociation.details,
      `Detection profile: malicious ${data.summary.maliciousDetections}, suspicious ${data.summary.suspiciousDetections}, harmless ${data.summary.harmlessDetections}.`,
    ],
    recommendation:
      data.alerts.find((alert) => alert.type.toUpperCase().includes('CRITICAL'))?.message ||
      data.alerts[0]?.message ||
      'Continue monitoring reputation changes and investigate sudden spikes in suspicious detections.',
    checklist: [
      'Track malicious/suspicious detection deltas over time, not only point-in-time values.',
      'Correlate reputation shifts with hosting, DNS, and certificate changes.',
      'Create an incident response playbook for domain reputation degradation.',
    ],
    evidence: [
      `Reputation score: ${data.summary.reputation}.`,
      `Community votes: malicious ${data.summary.communityMaliciousVotes}, harmless ${data.summary.communityHarmlessVotes}.`,
      `Last analysis date: ${data.summary.lastAnalysisDate || 'not provided'}.`,
      data.summary.permalink ? `Source permalink: ${data.summary.permalink}` : 'No source permalink provided.',
    ],
  }
}
