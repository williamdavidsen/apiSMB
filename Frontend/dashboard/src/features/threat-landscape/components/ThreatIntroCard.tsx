import InfoOutlined from '@mui/icons-material/InfoOutlined'
import Box from '@mui/material/Box'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

type ThreatIntroCardProps = {
  title: string
  intro: string
  imageSrc: string
  imageAlt: string
}

export function ThreatIntroCard({ title, intro, imageSrc, imageAlt }: ThreatIntroCardProps) {
  return (
    <Paper variant="outlined" sx={{ p: { xs: 2, md: 2.5 }, borderRadius: 2 }}>
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2.2} sx={{ alignItems: { xs: 'stretch', md: 'center' } }}>
        <Box
          component="img"
          src={imageSrc}
          alt={imageAlt}
          sx={{
            width: { xs: '100%', md: 240 },
            maxWidth: 260,
            borderRadius: 1.5,
            objectFit: 'cover',
            border: '1px solid',
            borderColor: 'divider',
            alignSelf: { xs: 'center', md: 'stretch' },
            bgcolor: 'grey.50',
          }}
        />
        <Stack spacing={1.25} sx={{ flex: 1 }}>
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <InfoOutlined fontSize="small" color="secondary" />
            <Typography variant="overline" sx={{ fontWeight: 700, letterSpacing: 0.7, color: 'text.secondary' }}>
              Threat landscape
            </Typography>
          </Stack>
          <Typography component="h2" variant="h5" sx={{ fontWeight: 800, color: 'secondary.dark' }}>
            {title}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {intro}
          </Typography>
        </Stack>
      </Stack>
    </Paper>
  )
}
