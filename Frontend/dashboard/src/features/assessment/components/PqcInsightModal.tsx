import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import IconButton from '@mui/material/IconButton'
import Typography from '@mui/material/Typography'
import { alpha } from '@mui/material/styles'
import Close from '@mui/icons-material/Close'
import type { PqcCheckResult } from '../model/assessment.types'

type PqcInsightModalProps = {
  open: boolean
  onClose: () => void
  pqc: PqcCheckResult
  /** Shown in UI and for screen readers (auto-close is controlled by the parent). */
  autoCloseSeconds?: number
}

export function PqcInsightModal({ open, onClose, pqc, autoCloseSeconds = 5 }: PqcInsightModalProps) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="pqc-dialog-title"
      aria-describedby="pqc-dialog-description"
    >
      <DialogTitle
        id="pqc-dialog-title"
        sx={{
          pr: 6,
          position: 'relative',
        }}
      >
        <Typography variant="overline" sx={{ color: 'text.secondary', fontWeight: 800, letterSpacing: 1.2 }}>
          Emerging technology
        </Typography>
        <Typography variant="h6" component="span" sx={{ display: 'block', mt: 0.5, fontWeight: 900, color: 'secondary.dark' }}>
          Post-quantum readiness
        </Typography>
        <IconButton
          onClick={onClose}
          aria-label="Close post-quantum dialog"
          size="small"
          sx={{ position: 'absolute', right: 8, top: 12 }}
        >
          <Close fontSize="small" />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers>
        <Typography
          id="pqc-dialog-description"
          variant="body2"
          sx={{ color: 'text.secondary', mb: 2 }}
          aria-live="polite"
        >
          This window closes automatically after about {autoCloseSeconds} seconds, or you can close it now using the
          button or the X control.
        </Typography>

        <Typography variant="body2" sx={{ color: 'text.secondary', mb: 1.5 }}>
          Status:{' '}
          <Box component="span" sx={{ fontWeight: 900, color: 'warning.dark' }}>
            {pqc.readinessLevel}
          </Box>
        </Typography>

        <Typography variant="body1" sx={{ color: 'text.primary' }}>
          {pqc.notes || 'No additional notes were returned for this domain.'}
        </Typography>

        {pqc.evidence?.length ? (
          <Box component="ul" sx={{ mt: 2, pl: 2, color: 'text.secondary' }}>
            {pqc.evidence.slice(0, 6).map((item) => (
              <Typography key={item} component="li" variant="body2" sx={{ mb: 0.75 }}>
                {item}
              </Typography>
            ))}
          </Box>
        ) : null}

        <Box
          sx={(theme) => ({
            mt: 2,
            p: 1.25,
            borderRadius: 1,
            borderLeft: '4px solid',
            borderColor: 'secondary.dark',
            bgcolor: alpha(theme.palette.secondary.main, 0.12),
          })}
        >
          <Typography variant="caption" sx={{ color: 'text.secondary' }}>
            This insight is not included in the current overall score yet.
          </Typography>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} variant="contained" color="secondary">
          Close
        </Button>
      </DialogActions>
    </Dialog>
  )
}
