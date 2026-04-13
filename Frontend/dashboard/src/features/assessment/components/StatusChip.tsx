import Chip from '@mui/material/Chip'
import type { AssessmentUiStatus } from '../../../shared/lib/status'

type StatusChipProps = {
  status: AssessmentUiStatus
}

export function StatusChip({ status }: StatusChipProps) {
  const color =
    status.severity === 'success'
      ? 'success'
      : status.severity === 'warning'
        ? 'warning'
        : status.severity === 'error'
          ? 'error'
          : 'default'

  return (
    <Chip
      size="small"
      label={status.label}
      color={color === 'default' ? 'default' : color}
      variant={color === 'default' ? 'outlined' : 'filled'}
      sx={{ fontWeight: 700 }}
    />
  )
}
