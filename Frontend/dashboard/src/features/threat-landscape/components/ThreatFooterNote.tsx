import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'

type ThreatFooterNoteProps = {
  note: string
}

export function ThreatFooterNote({ note }: ThreatFooterNoteProps) {
  return (
    <Paper variant="outlined" sx={{ p: 1.6, borderRadius: 2, bgcolor: 'grey.50' }}>
      <Typography variant="caption" sx={{ color: 'text.secondary' }}>
        {note}
      </Typography>
    </Paper>
  )
}
