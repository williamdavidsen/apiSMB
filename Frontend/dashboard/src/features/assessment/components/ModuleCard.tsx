import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Link from '@mui/material/Link'
import LinearProgress from '@mui/material/LinearProgress'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { alpha } from '@mui/material/styles'
import ErrorOutlineOutlined from '@mui/icons-material/ErrorOutlineOutlined'
import InfoOutlined from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlined from '@mui/icons-material/WarningAmberOutlined'
import { Link as RouterLink } from 'react-router-dom'
import type { ModuleCardFact, ModuleFactTone } from '../model/assessment.mappers'
import { shadows } from '../../../styles/designTokens'

type ModuleCardProps = {
  title: string
  icon: React.ReactNode
  grade: string
  moduleApiStatus: string
  scoreFill?: { current: number; max: number }
  /** Shown above the score bar when it adds info beyond the bar (e.g. Observatory). */
  statusLine?: string
  facts: ModuleCardFact[]
  bullet?: string
  callout?: { message: string; tone: 'critical' | 'warning' | 'info' }
  readMoreTo: string
  readMoreLabel?: string
}

function softToneColor(tone: ModuleFactTone): 'success.dark' | 'warning.dark' | 'error.dark' | 'text.secondary' {
  if (tone === 'success') return 'success.dark'
  if (tone === 'warning') return 'warning.dark'
  if (tone === 'error') return 'error.dark'
  return 'text.secondary'
}

function moduleStatusProps(status: string): { label: string; color: 'success.dark' | 'warning.dark' | 'error.dark' | 'text.secondary' } {
  const u = status.trim().toUpperCase()
  if (u === 'PASS') return { label: 'PASS', color: 'success.dark' }
  if (u === 'WARNING') return { label: 'WARNING', color: 'warning.dark' }
  if (u === 'FAIL') return { label: 'FAIL', color: 'error.dark' }
  if (u === 'ERROR') return { label: 'ERROR', color: 'error.dark' }
  return { label: status || '—', color: 'text.secondary' }
}

export function ModuleCard({
  title,
  icon,
  grade,
  moduleApiStatus,
  scoreFill,
  statusLine,
  facts,
  bullet,
  callout,
  readMoreTo,
  readMoreLabel = 'Read more',
}: ModuleCardProps) {
  const calloutBorder =
    callout?.tone === 'critical' ? 'error.main' : callout?.tone === 'warning' ? 'warning.main' : 'info.main'

  const calloutIcon =
    callout?.tone === 'critical' ? (
      <ErrorOutlineOutlined color="error" fontSize="small" sx={{ mt: 0.15 }} aria-hidden />
    ) : callout?.tone === 'warning' ? (
      <WarningAmberOutlined color="warning" fontSize="small" sx={{ mt: 0.15 }} aria-hidden />
    ) : (
      <InfoOutlined color="info" fontSize="small" sx={{ mt: 0.15 }} aria-hidden />
    )

  const scorePercent =
    scoreFill && scoreFill.max > 0
      ? Math.min(100, Math.max(0, Math.round((scoreFill.current / scoreFill.max) * 100)))
      : 0

  const statusTag = moduleStatusProps(moduleApiStatus)

  return (
    <Card
      variant="outlined"
      sx={(theme) => ({
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        borderRadius: 2,
        overflow: 'hidden',
        borderColor: 'divider',
        boxShadow: shadows.cardSoft,
        transition: theme.transitions.create(['box-shadow'], { duration: 180 }),
        '&:hover': {
          boxShadow: '0 2px 4px rgba(15, 23, 42, 0.07), 0 8px 22px rgba(15, 23, 42, 0.09)',
        },
      })}
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
            '& svg': { fontSize: 20 },
          }}
          aria-hidden
        >
          {icon}
        </Box>
        <Typography component="h3" variant="subtitle1" sx={{ fontWeight: 800 }}>
          {title}
        </Typography>
      </Box>

      <CardContent sx={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 1.5, p: 2.25 }}>
        <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
          <PaperGrade grade={grade} accent={gradeAccentColor(grade)} />
          <Typography variant="body2" sx={{ fontWeight: 800, color: statusTag.color }}>
            {statusTag.label}
          </Typography>
        </Stack>

        {statusLine?.trim() ? (
          <Typography variant="body2" sx={{ color: 'text.secondary' }}>
            {statusLine}
          </Typography>
        ) : null}

        {scoreFill ? (
          <Box>
            <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'baseline', mb: 0.5 }}>
              <Typography variant="body2" sx={{ color: 'text.secondary', fontWeight: 700 }}>
                Module score
              </Typography>
              <Typography variant="body2" sx={{ fontWeight: 800, color: 'text.primary' }}>
                {scoreFill.current}/{scoreFill.max}
              </Typography>
            </Stack>
            <LinearProgress
              variant="determinate"
              value={scorePercent}
              sx={(theme) => ({
                height: 8,
                borderRadius: 99,
                bgcolor: alpha(theme.palette.text.primary, 0.08),
                '& .MuiLinearProgress-bar': {
                  borderRadius: 99,
                  bgcolor: theme.palette.primary.dark,
                },
              })}
              aria-label={`Module score ${scoreFill.current} out of ${scoreFill.max}`}
            />
          </Box>
        ) : null}

        <Stack spacing={1.25}>
          {facts.map((row) => (
            <Stack
              key={row.label}
              direction={{ xs: 'column', sm: 'row' }}
              spacing={0.75}
              sx={{
                justifyContent: 'space-between',
                alignItems: { xs: 'flex-start', sm: 'center' },
                gap: { xs: 0.5, sm: 1 },
              }}
            >
              <Typography variant="body2" sx={{ color: 'text.secondary', minWidth: 0 }}>
                {row.label}
              </Typography>
              <Typography
                variant="body2"
                sx={{
                  fontWeight: 700,
                  color: softToneColor(row.tone),
                  alignSelf: { xs: 'stretch', sm: 'auto' },
                  maxWidth: { xs: '100%', sm: '60%' },
                  textAlign: { xs: 'left', sm: 'right' },
                  lineHeight: 1.3,
                }}
              >
                {row.value}
              </Typography>
            </Stack>
          ))}
        </Stack>

        <Stack spacing={1.25} sx={{ mt: 'auto', pt: 1 }}>
          {bullet ? (
            <Box
              sx={(theme) => ({
                pl: 1.5,
                py: 1,
                borderLeft: '3px solid',
                borderColor: 'divider',
                bgcolor: alpha(theme.palette.text.primary, 0.04),
                borderRadius: 1,
              })}
            >
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                {bullet}
              </Typography>
            </Box>
          ) : null}

          {callout ? (
            <Box
              sx={(theme) => ({
                borderLeft: '4px solid',
                borderColor: calloutBorder,
                bgcolor: alpha(
                  callout.tone === 'critical'
                    ? theme.palette.error.main
                    : callout.tone === 'warning'
                      ? theme.palette.warning.main
                      : theme.palette.info.main,
                  0.08,
                ),
                p: 1.25,
                borderRadius: 1,
              })}
            >
              <Stack direction="row" spacing={1} sx={{ alignItems: 'flex-start' }}>
                {calloutIcon}
                <Typography variant="body2" sx={{ color: 'text.primary', flex: 1 }}>
                  {callout.message}
                </Typography>
              </Stack>
            </Box>
          ) : null}
        </Stack>

        <Box sx={{ pt: 1 }}>
          <Link
            component={RouterLink}
            to={readMoreTo}
            underline="hover"
            sx={{ fontWeight: 800, color: 'secondary.dark' }}
          >
            {readMoreLabel} →
          </Link>
        </Box>
      </CardContent>
    </Card>
  )
}

function gradeAccentColor(grade: string): 'error.main' | 'warning.main' | 'success.main' | 'secondary.main' {
  const g = grade.trim().toUpperCase()
  if (g === 'F' || g === 'E' || g === 'D') return 'error.main'
  if (g === 'C') return 'warning.main'
  if (g === 'B' || g === 'A') return 'success.main'
  if (g === '—' || g === '-' || g.length === 0) return 'secondary.main'
  return 'secondary.main'
}

function PaperGrade({ grade, accent }: { grade: string; accent: 'error.main' | 'warning.main' | 'success.main' | 'secondary.main' }) {
  return (
    <Box
      sx={{
        borderRadius: 2,
        border: '1px solid',
        borderColor: 'divider',
        px: 1.5,
        py: 1.25,
        position: 'relative',
        overflow: 'hidden',
        bgcolor: 'background.paper',
        '&::before': {
          content: '""',
          position: 'absolute',
          left: 0,
          top: 0,
          bottom: 0,
          width: 6,
          bgcolor: accent,
        },
      }}
    >
      <Typography variant="subtitle2" sx={{ pl: 1, fontWeight: 900, letterSpacing: 0.4 }}>
        Grade {grade}
      </Typography>
    </Box>
  )
}
