import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Grid from '@mui/material/Grid'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useParams } from 'react-router-dom'
import buildingIcon from '../assets/images/threat/icons/building.svg'
import mailIcon from '../assets/images/threat/icons/mail.svg'
import linkIcon from '../assets/images/threat/icons/link.svg'
import shieldIcon from '../assets/images/threat/icons/shield.svg'
import timeIcon from '../assets/images/threat/icons/time.svg'
import usersIcon from '../assets/images/threat/icons/users.svg'
import warningIcon from '../assets/images/threat/icons/warning.svg'
import webIcon from '../assets/images/threat/icons/web.svg'
import phishingIllustration from '../assets/images/threat/phishing-illustration.svg'
import tlsIllustration from '../assets/images/threat/tls-illustration.svg'
import headersIllustration from '../assets/images/threat/headers-illustration.svg'
import { ThreatFooterNote } from '../features/threat-landscape/components/ThreatFooterNote'
import { ThreatGuidanceColumns } from '../features/threat-landscape/components/ThreatGuidanceColumns'
import { ThreatIntroCard } from '../features/threat-landscape/components/ThreatIntroCard'
import { useThreatLandscapeContent } from '../features/threat-landscape/hooks/useThreatLandscapeContent'
import type { ThreatStat } from '../features/threat-landscape/model/threatLandscape.types'
import { threatTopics } from '../shared/constants/threatKeys'

function parsePercentValue(value: string): number | null {
  const clean = value.replace('~', '').trim()
  const match = clean.match(/^(\d+(\.\d+)?)%$/)
  if (!match) return null
  const numeric = Number(match[1])
  if (!Number.isFinite(numeric)) return null
  return Math.max(0, Math.min(100, numeric))
}

function PercentageDonut({ value }: { value: number }) {
  return (
    <Box
      sx={{
        position: 'relative',
        width: 84,
        height: 84,
        display: 'inline-flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <CircularProgress
        variant="determinate"
        value={100}
        size={84}
        thickness={4.2}
        sx={{ color: 'grey.300', position: 'absolute', inset: 0 }}
      />
      <CircularProgress
        variant="determinate"
        value={value}
        size={84}
        thickness={4.2}
        sx={{ color: 'secondary.main', position: 'absolute', inset: 0 }}
      />
      <Typography
        variant="h6"
        sx={{
          position: 'relative',
          zIndex: 1,
          fontWeight: 900,
          color: 'secondary.dark',
          lineHeight: 1,
          letterSpacing: -0.2,
          fontSize: 22,
        }}
      >
        {`${value}%`}
      </Typography>
    </Box>
  )
}

function StatCard({
  title,
  icon,
  value,
  caption,
  summary,
  source,
}: {
  title: string
  icon: ThreatStat['icon']
  value: string
  caption: string
  summary: string
  source: string
}) {
  const percentValue = parsePercentValue(value)
  const iconSrc =
    icon === 'mail'
      ? mailIcon
      : icon === 'users'
        ? usersIcon
        : icon === 'building'
          ? buildingIcon
          : icon === 'shield'
            ? shieldIcon
            : icon === 'warning'
              ? warningIcon
              : icon === 'time'
                ? timeIcon
                : icon === 'link'
                  ? linkIcon
                : webIcon

  return (
    <Paper variant="outlined" sx={{ p: 2, borderRadius: 2, height: '100%' }}>
      <Stack spacing={1}>
        <Stack direction="row" sx={{ alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
          <Typography variant="subtitle1" sx={{ fontWeight: 800, color: 'secondary.dark' }}>
            {title}
          </Typography>
          <Box
            component="img"
            src={iconSrc}
            alt={`${title} icon`}
            sx={{ width: 28, height: 28, flexShrink: 0 }}
          />
        </Stack>
        {percentValue != null ? (
          <PercentageDonut value={percentValue} />
        ) : (
          <Typography variant="h4" sx={{ fontWeight: 900, color: 'secondary.main', lineHeight: 1.05 }}>
            {value}
          </Typography>
        )}
        <Typography variant="body2" sx={{ fontWeight: 700, color: 'text.primary' }}>
          {caption}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {summary}
        </Typography>
        <Typography variant="caption" sx={{ color: 'text.disabled' }}>
          Source: {source}
        </Typography>
      </Stack>
    </Paper>
  )
}

export function ThreatLandscapePage() {
  const { topic } = useParams<{ topic: string }>()
  const content = useThreatLandscapeContent(topic)
  const introImage =
    content.topic === threatTopics.phishingSpoofing
      ? { src: phishingIllustration, alt: 'Phishing illustration with mail and device warning icons' }
      : content.topic === threatTopics.weakTlsCerts
        ? { src: tlsIllustration, alt: 'TLS security illustration with shield and lock concepts' }
        : { src: headersIllustration, alt: 'Web security headers illustration with protected browser interface' }

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', width: '100%' }}>
      <Stack spacing={2}>
        <ThreatIntroCard
          title={content.pageTitle}
          intro={content.intro}
          imageSrc={introImage.src}
          imageAlt={introImage.alt}
        />

        <Grid container spacing={2}>
          {content.stats.map((stat) => (
            <Grid key={stat.id} size={{ xs: 12, md: 4 }}>
              <StatCard
                title={stat.title}
                icon={stat.icon}
                value={stat.value}
                caption={stat.caption}
                summary={stat.summary}
                source={stat.source}
              />
            </Grid>
          ))}
        </Grid>

        <ThreatGuidanceColumns findings={content.findings} actions={content.actions} />

        <ThreatFooterNote note={content.footerNote} />
      </Stack>
    </Box>
  )
}
