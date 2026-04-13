import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { alpha } from '@mui/material/styles'
import type { Theme } from '@mui/material/styles'
import ErrorOutlineOutlined from '@mui/icons-material/ErrorOutlineOutlined'
import InfoOutlined from '@mui/icons-material/InfoOutlined'
import WarningAmberOutlined from '@mui/icons-material/WarningAmberOutlined'
import type { ExecutiveBanner } from '../model/assessment.mappers'

type AlertBoxProps = {
  banner: ExecutiveBanner
}

export function AlertBox({ banner }: AlertBoxProps) {
  const palette =
    banner.severity === 'critical'
      ? {
          border: 'error.dark',
          bg: (theme: Theme) => alpha(theme.palette.error.main, 0.08),
          icon: <ErrorOutlineOutlined color="error" fontSize="small" aria-hidden />,
        }
      : banner.severity === 'warning'
        ? {
            border: 'warning.dark',
            bg: (theme: Theme) => alpha(theme.palette.warning.main, 0.12),
            icon: <WarningAmberOutlined color="warning" fontSize="small" aria-hidden />,
          }
        : {
            border: 'info.main',
            bg: (theme: Theme) => alpha(theme.palette.info.main, 0.08),
            icon: <InfoOutlined color="info" fontSize="small" aria-hidden />,
          }

  return (
    <Box
      role={banner.severity === 'critical' ? 'alert' : 'status'}
      aria-live="polite"
      sx={{
        border: '1px solid',
        borderColor: palette.border,
        bgcolor: palette.bg,
        borderRadius: 2,
        p: 2,
      }}
    >
      <Stack direction="row" spacing={1.5} sx={{ alignItems: 'flex-start' }}>
        <Box sx={{ mt: 0.2 }}>{palette.icon}</Box>
        <Box>
          <Typography variant="subtitle1" sx={{ fontWeight: 800, color: 'text.primary' }}>
            {banner.title}
          </Typography>
          <Typography variant="body2" sx={{ color: 'text.secondary', mt: 0.5 }}>
            {banner.body}
          </Typography>
        </Box>
      </Stack>
    </Box>
  )
}
