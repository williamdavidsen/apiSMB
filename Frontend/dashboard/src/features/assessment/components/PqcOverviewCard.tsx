import AutoAwesomeOutlined from '@mui/icons-material/AutoAwesomeOutlined'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Chip from '@mui/material/Chip'
import Link from '@mui/material/Link'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { alpha } from '@mui/material/styles'
import { Link as RouterLink } from 'react-router-dom'
import type { PqcCheckResult } from '../model/assessment.types'
import { shadows } from '../../../styles/designTokens'

type PqcOverviewCardProps = {
  domain: string
  pqc: PqcCheckResult
  readMoreTo: string
}

function readinessTone(readinessLevel: string): 'success' | 'warning' | 'default' {
  const text = readinessLevel.trim().toLowerCase()
  if (text.includes('hybrid pqc')) return 'success'
  if (text.includes('not supported') || text.includes('unknown')) return 'warning'
  return 'default'
}

export function PqcOverviewCard({ domain, pqc, readMoreTo }: PqcOverviewCardProps) {
  const transitionNote = pqc.pqcDetected
    ? 'Please continue maturing your PQC transition plan to maintain long-term cryptographic resilience.'
    : 'Please begin the transition to PQC to improve long-term cryptographic resilience.'
  const cleanedApiNote = (pqc.notes || 'No additional notes were returned for this domain.')
    .trim()
    .replace(/^[\s.,;:!?-]+/, '')

  return (
    <Card
      variant="outlined"
      sx={{
        borderRadius: 2,
        borderColor: 'divider',
        boxShadow: shadows.cardSoft,
      }}
    >
      <Box
        sx={(theme) => ({
          px: 2,
          py: 1.5,
          color: 'primary.contrastText',
          display: 'flex',
          alignItems: 'center',
          gap: 1.25,
          background: `linear-gradient(115deg, ${theme.palette.primary.dark} 0%, ${theme.palette.primary.main} 52%, #26c6da 100%)`,
        })}
      >
        <Box
          sx={{
            display: 'grid',
            placeItems: 'center',
            width: 28,
            height: 28,
            borderRadius: 1.2,
            color: 'rgba(255,255,255,0.95)',
            bgcolor: 'rgba(255,255,255,0.14)',
          }}
          aria-hidden
        >
          <AutoAwesomeOutlined sx={{ fontSize: 20 }} />
        </Box>
        <Typography component="h3" variant="subtitle1" sx={{ fontWeight: 800 }}>
          Post-quantum readiness
        </Typography>
      </Box>

      <CardContent sx={{ p: { xs: 2.25, md: 2.5 } }}>
        <Stack spacing={2}>
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            spacing={1.25}
            sx={{ justifyContent: 'space-between', alignItems: { xs: 'flex-start', md: 'center' } }}
          >
            <Stack spacing={0.5}>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                Domain
              </Typography>
              <Typography variant="body1" sx={{ fontWeight: 800, color: 'text.primary' }}>
                {domain}
              </Typography>
            </Stack>
            <Chip
              label={pqc.readinessLevel}
              color={readinessTone(pqc.readinessLevel)}
              variant={readinessTone(pqc.readinessLevel) === 'default' ? 'outlined' : 'filled'}
            />
          </Stack>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={1.5}>
            <Box sx={{ flex: 1 }}>
              <Box
                sx={(theme) => ({
                  borderLeft: '4px solid',
                  borderColor: 'info.main',
                  bgcolor: alpha(theme.palette.info.main, 0.08),
                  borderRadius: 1,
                  px: 1.25,
                  py: 1,
                })}
              >
                <Typography variant="body2" sx={{ color: 'text.primary' }}>
                  {cleanedApiNote}
                </Typography>
              </Box>
              <Typography variant="body2" sx={{ color: 'text.secondary', mt: 0.75 }}>
                {transitionNote}
              </Typography>
            </Box>
            <Link
              component={RouterLink}
              to={readMoreTo}
              underline="hover"
              sx={{
                fontWeight: 800,
                color: 'secondary.dark',
                whiteSpace: 'nowrap',
                alignSelf: { xs: 'flex-start', md: 'center' },
              }}
            >
              Read more →
            </Link>
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  )
}
