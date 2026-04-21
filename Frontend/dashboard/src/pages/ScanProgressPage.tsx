import Paper from '@mui/material/Paper'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import { useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ScanCancelButton } from '../features/scan/components/ScanCancelButton'
import { ScanSpinner } from '../features/scan/components/ScanSpinner'
import { useScanProgress } from '../features/scan/hooks/useScanProgress'
import { routes } from '../shared/constants/routes'
import { saveLastScannedDomain } from '../shared/lib/lastScan'

export function ScanProgressPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const domain = searchParams.get('domain')?.trim() ?? ''
  const { progress, currentStep, estimatedLabel, isComplete } = useScanProgress()

  useEffect(() => {
    if (!domain) {
      navigate(routes.home, { replace: true })
    }
  }, [domain, navigate])

  useEffect(() => {
    if (!isComplete) return

    const timer = window.setTimeout(() => {
      saveLastScannedDomain(domain)
      const params = new URLSearchParams({ domain })
      navigate(`${routes.dashboard}?${params.toString()}`)
    }, 1200)

    return () => window.clearTimeout(timer)
  }, [domain, isComplete, navigate])

  return (
    <Box sx={{ maxWidth: 760, mx: 'auto', py: { xs: 1, md: 4 } }}>
      <Paper variant="outlined" sx={{ p: { xs: 3, md: 5 }, textAlign: 'center' }}>
        <Typography component="h2" variant="h5" sx={{ mb: 3 }}>
          Domain Security Assessment in Progress
        </Typography>

        <ScanSpinner progress={progress} />

        <Typography
          aria-live="polite"
          sx={{ mt: 3, color: 'text.secondary', minHeight: 24 }}
        >
          {currentStep}...
        </Typography>

        <Typography
          aria-live="polite"
          variant="body2"
          sx={{ mt: 1, color: 'text.secondary' }}
        >
          {estimatedLabel}
        </Typography>

        <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
          <ScanCancelButton onCancel={() => navigate(routes.home)} />
        </Box>
      </Paper>
    </Box>
  )
}
