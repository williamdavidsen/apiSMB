import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Typography from '@mui/material/Typography'
import { clampPercent } from '../../../shared/lib/score'

type ScoreRingProps = {
  value: number
  /** MUI palette key for the active arc */
  color: 'success.main' | 'warning.main' | 'error.main' | 'info.main' | 'primary.main'
  /** Accessible description, e.g. "Overall security score 59 out of 100" */
  ariaLabel: string
}

export function ScoreRing({ value, color, ariaLabel }: ScoreRingProps) {
  const percent = clampPercent(value)

  return (
    <Box
      sx={{
        position: 'relative',
        display: 'inline-flex',
        width: 160,
        height: 160,
        alignItems: 'center',
        justifyContent: 'center',
      }}
      role="img"
      aria-label={ariaLabel}
    >
      <CircularProgress
        variant="determinate"
        value={100}
        size={160}
        thickness={4}
        sx={{ color: 'action.hover', position: 'absolute' }}
        aria-hidden
      />
      <CircularProgress
        variant="determinate"
        value={percent}
        size={160}
        thickness={4}
        sx={{
          color,
          position: 'absolute',
        }}
        aria-hidden
      />
      <Typography
        variant="h4"
        component="span"
        sx={{ fontWeight: 800, color }}
      >
        {percent}%
      </Typography>
    </Box>
  )
}
