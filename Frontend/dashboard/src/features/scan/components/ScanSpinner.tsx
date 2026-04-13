import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Typography from '@mui/material/Typography'

type ScanSpinnerProps = {
  progress: number
}

export function ScanSpinner({ progress }: ScanSpinnerProps) {
  return (
    <Box sx={{ position: 'relative', display: 'inline-flex', width: 132, height: 132 }}>
      <CircularProgress
        variant="determinate"
        value={100}
        size={132}
        thickness={4.5}
        sx={{ color: 'rgba(0, 166, 186, 0.2)' }}
      />
      <CircularProgress
        variant="determinate"
        value={progress}
        size={132}
        thickness={4.5}
        sx={{
          color: '#00a6ba',
          position: 'absolute',
          left: 0,
          top: 0,
          animation: 'pulse 1.8s ease-in-out infinite',
          '@keyframes pulse': {
            '0%, 100%': { filter: 'drop-shadow(0 0 0 rgba(0, 166, 186, 0.1))' },
            '50%': { filter: 'drop-shadow(0 0 8px rgba(0, 166, 186, 0.42))' },
          },
        }}
      />
      <CircularProgress
        variant="indeterminate"
        disableShrink
        size={82}
        thickness={4}
        sx={{
          color: 'rgba(0, 166, 186, 0.75)',
          position: 'absolute',
          left: 25,
          top: 25,
          animationDuration: '1200ms',
        }}
      />
      <Box
        sx={{
          inset: 0,
          position: 'absolute',
          display: 'grid',
          placeItems: 'center',
        }}
      >
        <Typography component="span" variant="h6" sx={{ fontWeight: 700, color: 'text.primary' }}>
          {progress}%
        </Typography>
      </Box>
    </Box>
  )
}
