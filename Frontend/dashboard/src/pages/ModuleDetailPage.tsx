import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { isDashboardModuleKey } from '../features/assessment/model/assessment.constants'
import {
  fetchEmailCheck,
  fetchHeadersCheck,
  fetchReputationCheck,
  fetchSslCheck,
} from '../features/assessment/services/assessment.api'
import { routes } from '../shared/constants/routes'

type AsyncState =
  | { status: 'idle' | 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; payload: unknown }

export function ModuleDetailPage() {
  const navigate = useNavigate()
  const params = useParams()

  const domain = useMemo(() => {
    const raw = params.domain ?? ''
    try {
      return decodeURIComponent(raw).trim()
    } catch {
      return raw.trim()
    }
  }, [params.domain])

  const moduleKey = params.module ?? ''
  const invalidModule = moduleKey.length > 0 && !isDashboardModuleKey(moduleKey)

  const [state, setState] = useState<AsyncState>({ status: 'idle' })

  useEffect(() => {
    if (invalidModule || !domain) {
      return
    }

    const controller = new AbortController()

    void (async () => {
      await Promise.resolve()
      if (controller.signal.aborted) return

      setState({ status: 'loading' })

      try {
        const payload =
          moduleKey === 'ssl-tls'
            ? await fetchSslCheck(domain, controller.signal)
            : moduleKey === 'http-headers'
              ? await fetchHeadersCheck(domain, controller.signal)
              : moduleKey === 'email'
                ? await fetchEmailCheck(domain, controller.signal)
                : await fetchReputationCheck(domain, controller.signal)

        if (controller.signal.aborted) return
        setState({ status: 'success', payload })
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
          <Typography color="text.secondary">Loading module details…</Typography>
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

  return (
    <Box sx={{ maxWidth: 900, mx: 'auto', width: '100%' }}>
      <Stack spacing={2}>
        <Button variant="text" color="secondary" onClick={backToDashboard} sx={{ alignSelf: 'flex-start', fontWeight: 800 }}>
          ← Back to dashboard
        </Button>

        <Typography component="h2" variant="h5" sx={{ fontWeight: 900, color: 'secondary.dark' }}>
          {moduleKey.replace(/-/g, ' ').toUpperCase()} — {domain}
        </Typography>

        <Paper variant="outlined" sx={{ p: 2, borderRadius: 2 }}>
          <Typography variant="subtitle2" sx={{ mb: 1, color: 'text.secondary' }}>
            Raw API payload (debug-friendly)
          </Typography>
          <Box
            component="pre"
            sx={{
              m: 0,
              p: 2,
              borderRadius: 1,
              bgcolor: 'background.default',
              overflow: 'auto',
              fontSize: 12,
              lineHeight: 1.5,
            }}
          >
            {JSON.stringify(state.payload, null, 2)}
          </Box>
        </Paper>
      </Stack>
    </Box>
  )
}
