import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { AssessmentUiStatus } from '../../../shared/lib/status'
import { ScoreRing } from './ScoreRing'
import { StatusChip } from './StatusChip'

type DashboardSummaryCardProps = {
  title: string
  subtitle: string
  grade: string
  score: number
  maxScore: number
  uiStatus: AssessmentUiStatus
  onTestAnother: () => void
  extraActions?: React.ReactNode
}

function ringColor(status: AssessmentUiStatus): 'success.main' | 'warning.main' | 'error.main' | 'primary.main' {
  if (status.severity === 'error') return 'error.main'
  if (status.severity === 'warning') return 'warning.main'
  if (status.severity === 'success') return 'success.main'
  return 'primary.main'
}

export function DashboardSummaryCard({
  title,
  subtitle,
  grade,
  score,
  maxScore,
  uiStatus,
  onTestAnother,
  extraActions,
}: DashboardSummaryCardProps) {
  const ring = ringColor(uiStatus)
  const clampedScore = Math.min(Math.max(score, 0), maxScore > 0 ? maxScore : 100)
  const percentLabel = maxScore > 0 ? Math.round((clampedScore / maxScore) * 100) : 0

  return (
    <Paper
      variant="outlined"
      sx={{
        p: { xs: 2.5, md: 3.5 },
        borderRadius: 2,
        borderColor: 'divider',
        boxShadow: '0 1px 2px rgba(15, 23, 42, 0.06), 0 4px 18px rgba(15, 23, 42, 0.07)',
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={2}
          sx={{
            alignItems: { xs: 'flex-start', sm: 'center' },
            justifyContent: 'space-between',
          }}
        >
          <Box>
            <Typography component="h2" variant="h4" sx={{ fontWeight: 800, color: 'secondary.dark' }}>
              {title}
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', mt: 0.5 }}>
              {subtitle}
            </Typography>
          </Box>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1}
            sx={{ width: { xs: '100%', sm: 'auto' }, alignItems: { xs: 'stretch', sm: 'center' } }}
          >
            {extraActions}
            <Button variant="outlined" color="secondary" onClick={onTestAnother} sx={{ alignSelf: { xs: 'stretch', sm: 'center' } }}>
              Test another?
            </Button>
          </Stack>
        </Stack>

        <Stack
          direction={{ xs: 'column', md: 'row' }}
          spacing={3}
          sx={{ alignItems: 'center', justifyContent: 'space-between' }}
        >
          <Box sx={{ width: '100%', maxWidth: 420 }}>
            <Paper
              variant="outlined"
              sx={{
                p: 2,
                borderRadius: 2,
                borderColor: 'divider',
                bgcolor: 'background.default',
              }}
            >
              <Stack spacing={1.25}>
                <Typography variant="body1" sx={{ fontWeight: 700 }}>
                  Final security grade:{' '}
                  <Box component="span" sx={{ color: 'text.primary' }}>
                    {grade}
                  </Box>
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 700 }}>
                  Score:{' '}
                  <Box component="span" sx={{ color: 'text.primary' }}>
                    {score}/{maxScore}
                  </Box>
                </Typography>
                <Stack direction="row" spacing={1} useFlexGap sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
                  <Typography variant="body1" sx={{ fontWeight: 700 }}>
                    Status:
                  </Typography>
                  <StatusChip status={uiStatus} />
                </Stack>
              </Stack>
            </Paper>
          </Box>

          <ScoreRing
            value={percentLabel}
            color={ring}
            ariaLabel={`Overall security score ${clampedScore} out of ${maxScore}`}
          />
        </Stack>
      </Stack>
    </Paper>
  )
}
