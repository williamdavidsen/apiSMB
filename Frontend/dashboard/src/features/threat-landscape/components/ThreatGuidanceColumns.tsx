import Grid from '@mui/material/Grid'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { ThreatSection } from '../model/threatLandscape.types'

type ThreatGuidanceColumnsProps = {
  findings: ThreatSection
  actions: ThreatSection
}

type GuidanceCardProps = {
  section: ThreatSection
}

function GuidanceCard({ section }: GuidanceCardProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, height: '100%' }}>
      <Stack spacing={1.25}>
        <Typography component="h3" variant="subtitle1" sx={{ fontWeight: 800, color: 'secondary.dark' }}>
          {section.title}
        </Typography>
        <Stack component="ul" spacing={0.8} sx={{ m: 0, pl: 2.25 }}>
          {section.points.map((point) => (
            <Typography key={point} component="li" variant="body2" color="text.secondary">
              {point}
            </Typography>
          ))}
        </Stack>
      </Stack>
    </Paper>
  )
}

export function ThreatGuidanceColumns({ findings, actions }: ThreatGuidanceColumnsProps) {
  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, md: 6 }}>
        <GuidanceCard section={findings} />
      </Grid>
      <Grid size={{ xs: 12, md: 6 }}>
        <GuidanceCard section={actions} />
      </Grid>
    </Grid>
  )
}
