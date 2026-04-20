import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import HomeOutlined from '@mui/icons-material/HomeOutlined'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { AlertBox } from '../features/assessment/components/AlertBox'
import { DashboardSummaryCard } from '../features/assessment/components/DashboardSummaryCard'
import { ModuleCardList } from '../features/assessment/components/ModuleCardList'
import { PqcInsightModal } from '../features/assessment/components/PqcInsightModal'
import { useAssessmentDashboard } from '../features/assessment/hooks/useAssessmentDashboard'
import {
  buildModuleCards,
  dashboardEmailSubtitle,
  dashboardHeadline,
  pickExecutiveBanner,
} from '../features/assessment/model/assessment.mappers'
import { routes } from '../shared/constants/routes'
import { mapAssessmentStatus } from '../shared/lib/status'

const PQC_AUTO_CLOSE_MS = 5000
const apiBaseForMessage =
  import.meta.env.VITE_API_URL || import.meta.env.VITE_DEV_API_PROXY || 'http://localhost:1071'
const helpText = import.meta.env.DEV
  ? `Start the API from the API folder (default: ${apiBaseForMessage}), then retry. Vite proxies /api to that host; set VITE_DEV_API_PROXY in .env.development if your API uses another URL.`
  : `API request failed. Please try again. Backend: ${apiBaseForMessage}`

function formatNorwegianDateTime(iso: string): string {
  try {
    return new Intl.DateTimeFormat('nb-NO', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(iso))
  } catch {
    return iso
  }
}

export function SecurityDashboardPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const domain = useMemo(() => searchParams.get('domain')?.trim() ?? '', [searchParams])
  const { state, refetch } = useAssessmentDashboard(domain)
  const [pqcOpen, setPqcOpen] = useState(false)
  const pqcCloseTimerRef = useRef<ReturnType<typeof window.setTimeout> | null>(null)

  const clearPqcTimer = useCallback(() => {
    if (pqcCloseTimerRef.current != null) {
      window.clearTimeout(pqcCloseTimerRef.current)
      pqcCloseTimerRef.current = null
    }
  }, [])

  const handleClosePqc = useCallback(() => {
    clearPqcTimer()
    setPqcOpen(false)
  }, [clearPqcTimer])

  const schedulePqcAutoClose = useCallback(() => {
    clearPqcTimer()
    pqcCloseTimerRef.current = window.setTimeout(() => {
      setPqcOpen(false)
      pqcCloseTimerRef.current = null
    }, PQC_AUTO_CLOSE_MS)
  }, [clearPqcTimer])

  const handleOpenPqcManual = useCallback(() => {
    setPqcOpen(true)
    schedulePqcAutoClose()
  }, [schedulePqcAutoClose])

  const pqcSessionKey =
    state.status === 'success' ? `${state.data.assessment.domain}|${state.scannedAtIso}` : null

  useEffect(() => {
    if (state.status !== 'loading' && state.status !== 'idle' && state.status !== 'error') {
      return
    }

    void Promise.resolve().then(() => {
      clearPqcTimer()
      setPqcOpen(false)
    })
  }, [state.status, clearPqcTimer])

  useEffect(() => {
    if (!pqcSessionKey) return

    void Promise.resolve().then(() => {
      setPqcOpen(true)
      schedulePqcAutoClose()
    })

    return () => clearPqcTimer()
  }, [pqcSessionKey, schedulePqcAutoClose, clearPqcTimer])

  if (!domain) {
    return (
      <Box sx={{ maxWidth: 720, mx: 'auto' }}>
        <Alert severity="info" role="status">
          No domain was provided. Start a scan from the home page.
        </Alert>
        <Box sx={{ mt: 2 }}>
          <Button variant="contained" color="secondary" onClick={() => navigate(routes.home)}>
            Go to home
          </Button>
        </Box>
      </Box>
    )
  }

  if (state.status === 'loading' || state.status === 'idle') {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }} role="status" aria-live="polite">
        <Stack spacing={2} sx={{ alignItems: 'center' }}>
          <CircularProgress aria-label="Loading assessment" />
          <Typography color="text.secondary">Results are loading…</Typography>
          <Button
            variant="outlined"
            color="secondary"
            size="small"
            startIcon={<HomeOutlined />}
            onClick={() => navigate(routes.home)}
            aria-label="Leave analysis and go to home page"
          >
            Back to home
          </Button>
        </Stack>
      </Box>
    )
  }

  if (state.status === 'error') {
    return (
      <Box sx={{ maxWidth: 720, mx: 'auto' }}>
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => refetch()}>
              Retry
            </Button>
          }
        >
          {state.message}
        </Alert>
        <Box sx={{ mt: 2, color: 'text.secondary', typography: 'body2' }}>
          {helpText}
        </Box>
        <Button
          variant="outlined"
          color="secondary"
          size="small"
          startIcon={<HomeOutlined />}
          onClick={() => navigate(routes.home)}
          aria-label="Leave analysis and go to home page"
          sx={{ mt: 2 }}
        >
          Back to home
        </Button>
      </Box>
    )
  }

  const { data, scannedAtIso } = state
  const uiStatus = mapAssessmentStatus(data.assessment.status, data.assessment.overallScore)
  const headline = dashboardHeadline(data.assessment.grade, data.assessment.status, data.assessment.overallScore)
  const subtitle = dashboardEmailSubtitle(data.assessment.emailModuleIncluded)
  const banner = pickExecutiveBanner(data)
  const cards = buildModuleCards(data)

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', width: '100%' }}>
      <Stack spacing={2.5}>
        <DashboardSummaryCard
          title={headline}
          subtitle={subtitle}
          grade={data.assessment.grade}
          score={data.assessment.overallScore}
          maxScore={data.assessment.maxScore}
          uiStatus={uiStatus}
          modules={data.assessment.modules}
          onTestAnother={() => navigate(routes.home)}
          extraActions={
            <Button variant="text" color="secondary" onClick={handleOpenPqcManual} sx={{ fontWeight: 800 }}>
              Post-quantum insight
            </Button>
          }
        />

        {banner ? <AlertBox banner={banner} /> : null}

        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={1.5}
          sx={{
            alignItems: { xs: 'flex-start', sm: 'center' },
            justifyContent: 'space-between',
          }}
        >
          <Typography component="h2" variant="h5" sx={{ fontWeight: 900, color: 'secondary.dark' }}>
            {data.assessment.domain}
          </Typography>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1}
            sx={{ alignItems: { xs: 'stretch', sm: 'center' }, width: { xs: '100%', sm: 'auto' } }}
          >
            <Button
              variant="outlined"
              color="secondary"
              size="small"
              startIcon={<HomeOutlined />}
              onClick={() => navigate(routes.home)}
              aria-label="Leave analysis and go to home page"
              sx={{ alignSelf: { xs: 'stretch', sm: 'center' }, whiteSpace: 'nowrap' }}
            >
              Back to home
            </Button>
            <Typography variant="body2" sx={{ color: 'text.secondary', alignSelf: 'center' }}>
              Last scanned: {formatNorwegianDateTime(scannedAtIso)}
            </Typography>
          </Stack>
        </Stack>

        <ModuleCardList domain={data.assessment.domain} cards={cards} />
      </Stack>

      <PqcInsightModal
        open={pqcOpen}
        onClose={handleClosePqc}
        pqc={data.assessment.pqcReadiness}
        autoCloseSeconds={PQC_AUTO_CLOSE_MS / 1000}
      />
    </Box>
  )
}
