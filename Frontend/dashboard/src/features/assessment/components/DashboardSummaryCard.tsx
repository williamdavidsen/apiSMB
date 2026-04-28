import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { AssessmentUiStatus } from '../../../shared/lib/status'
import type { AssessmentModuleScores } from '../model/assessment.types'
import { shadows } from '../../../styles/designTokens'
import { StatusChip } from './StatusChip'

type DashboardSummaryCardProps = {
  title: string
  subtitle: string
  grade: string
  score: number
  maxScore: number
  uiStatus: AssessmentUiStatus
  modules: AssessmentModuleScores
  onTestAnother: () => void
  extraActions?: React.ReactNode
}

type DonutSegment = {
  id: string
  label: string
  value: number
  rawScore: number
  rawMax: number
  included: boolean
  color: string
}

function centerTone(status: AssessmentUiStatus): { primary: string; secondary: string } {
  if (status.severity === 'error') return { primary: '#D32F2F', secondary: '#8A1F1F' }
  if (status.severity === 'warning') return { primary: '#F59E0B', secondary: '#8A5A00' }
  if (status.severity === 'success') return { primary: '#2E7D32', secondary: '#1F5A23' }
  return { primary: '#0B7285', secondary: '#35565d' }
}

export function DashboardSummaryCard({
  title,
  subtitle,
  grade,
  score,
  maxScore,
  uiStatus,
  modules,
  onTestAnother,
  extraActions,
}: DashboardSummaryCardProps) {
  const clampedScore = Math.min(Math.max(score, 0), maxScore > 0 ? maxScore : 100)
  const percentLabel = maxScore > 0 ? Math.round((clampedScore / maxScore) * 100) : 0
  const tone = centerTone(uiStatus)
  const segments: DonutSegment[] = [
    {
      id: 'ssl',
      label: 'TLS / SSL',
      value: modules.sslTls.included ? Math.max(0, modules.sslTls.weightedContribution) : 0,
      rawScore: modules.sslTls.rawScore,
      rawMax: modules.sslTls.rawMaxScore,
      included: modules.sslTls.included,
      color: '#00ACC1',
    },
    {
      id: 'headers',
      label: 'HTTP Headers',
      value: modules.httpHeaders.included ? Math.max(0, modules.httpHeaders.weightedContribution) : 0,
      rawScore: modules.httpHeaders.rawScore,
      rawMax: modules.httpHeaders.rawMaxScore,
      included: modules.httpHeaders.included,
      color: '#2F9E90',
    },
    {
      id: 'email',
      label: 'E-mail',
      value: modules.emailSecurity.included ? Math.max(0, modules.emailSecurity.weightedContribution) : 0,
      rawScore: modules.emailSecurity.rawScore,
      rawMax: modules.emailSecurity.rawMaxScore,
      included: modules.emailSecurity.included,
      color: '#5ED3E6',
    },
    {
      id: 'reputation',
      label: 'Domain / IP reputation',
      value: modules.reputation.included ? Math.max(0, modules.reputation.weightedContribution) : 0,
      rawScore: modules.reputation.rawScore,
      rawMax: modules.reputation.rawMaxScore,
      included: modules.reputation.included,
      color: '#1B7F79',
    },
  ]

  return (
    <Paper
      variant="outlined"
      sx={{
        p: { xs: 2.5, md: 3.5 },
        borderRadius: 2,
        borderColor: 'divider',
        boxShadow: shadows.cardSoftRaised,
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={2}
          sx={{
            alignItems: { xs: 'flex-start', sm: 'center' },
            justifyContent: 'space-between',
          }}
        >
          <Box>
            <Typography component="h2" variant="h4" sx={{ fontWeight: 800, color: 'secondary.dark' }}>
              {title}
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', mt: 0.5 }}>
              {subtitle}
            </Typography>
          </Box>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1}
            sx={{ width: { xs: '100%', sm: 'auto' }, alignItems: { xs: 'stretch', sm: 'center' } }}
          >
            {extraActions}
            <Button
              variant="outlined"
              color="secondary"
              onClick={onTestAnother}
              sx={{
                alignSelf: { xs: 'stretch', sm: 'center' },
                color: 'secondary.dark',
                borderColor: 'secondary.main',
              }}
            >
              Test another?
            </Button>
          </Stack>
        </Stack>

        <Stack
          direction={{ xs: 'column', md: 'row' }}
          spacing={3}
          sx={{
            alignItems: { xs: 'stretch', md: 'center' },
            justifyContent: { xs: 'flex-start', md: 'space-between' },
            flexWrap: 'wrap',
            rowGap: 2.5,
            columnGap: { md: 6 },
          }}
        >
          <Box sx={{ width: '100%', maxWidth: 420, flexShrink: 0 }}>
            <Paper
              variant="outlined"
              sx={{
                p: 2,
                borderRadius: 2,
                borderColor: 'divider',
                bgcolor: 'background.default',
              }}
            >
              <Stack spacing={1.25}>
                <Typography variant="body1" sx={{ fontWeight: 700 }}>
                  Final security grade:{' '}
                  <Box component="span" sx={{ color: 'text.primary' }}>
                    {grade}
                  </Box>
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 700 }}>
                  Score:{' '}
                  <Box component="span" sx={{ color: 'text.primary' }}>
                    {score}/{maxScore}
                  </Box>
                </Typography>
                <Stack direction="row" spacing={1} useFlexGap sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
                  <Typography variant="body1" sx={{ fontWeight: 700 }}>
                    Status:
                  </Typography>
                  <StatusChip status={uiStatus} />
                </Stack>
              </Stack>
            </Paper>
          </Box>

          <Box
            sx={{
              flex: { md: '1 1 420px' },
              minWidth: { md: 0 },
              width: '100%',
              maxWidth: 760,
              ml: { md: 'auto' },
              display: 'flex',
              justifyContent: 'flex-end',
            }}
          >
            <Stack spacing={1.25} sx={{ alignItems: 'flex-end', width: '100%', maxWidth: 640 }}>
              <Stack spacing={1.25} sx={{ width: '100%', maxWidth: 560, alignItems: 'flex-end' }}>
                <Stack spacing={1.25} sx={{ width: '100%', alignItems: 'flex-end' }}>
                  <Box sx={{ display: 'flex', justifyContent: 'flex-end', width: '100%' }}>
                    <Box
                      sx={{
                        display: 'grid',
                        width: 'max-content',
                        maxWidth: '100%',
                        gridTemplateColumns: { xs: '1fr', sm: 'minmax(0, 220px) minmax(0, auto)', md: '220px auto' },
                        gridTemplateRows: { xs: 'auto auto auto', sm: 'auto auto' },
                        columnGap: { xs: 2, md: 3 },
                        rowGap: 1,
                        alignItems: 'center',
                      }}
                    >
                      <Box sx={{ gridColumn: 1, gridRow: 1, width: 220, maxWidth: '100%', justifySelf: 'center' }}>
                        <PieBreakdownChart
                          segments={segments}
                          totalScore={clampedScore}
                          maxScore={maxScore}
                          totalPercent={percentLabel}
                          centerPrimaryColor={tone.primary}
                          centerSecondaryColor={tone.secondary}
                        />
                      </Box>

                      <Stack
                        spacing={0.6}
                        sx={{
                          gridColumn: { xs: 1, sm: 2 },
                          gridRow: { xs: 2, sm: 1 },
                          alignItems: { xs: 'center', sm: 'flex-start' },
                          alignSelf: 'center',
                        }}
                      >
                        {segments.map((segment) => (
                          <Stack key={segment.id} direction="row" spacing={0.75} sx={{ alignItems: 'center' }}>
                            <Box
                              sx={{
                                width: 9,
                                height: 9,
                                borderRadius: '50%',
                                bgcolor: segment.included ? segment.color : 'divider',
                                flexShrink: 0,
                              }}
                            />
                            <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
                              {segment.label}
                            </Typography>
                          </Stack>
                        ))}
                      </Stack>

                      <Typography
                        variant="caption"
                        sx={{
                          gridColumn: 1,
                          gridRow: { xs: 3, sm: 2 },
                          width: '100%',
                          maxWidth: 220,
                          justifySelf: 'center',
                          textAlign: 'center',
                          color: 'text.secondary',
                          fontWeight: 700,
                        }}
                      >
                        Total score {clampedScore}/{maxScore}
                      </Typography>
                    </Box>
                  </Box>

                  <Typography
                    variant="caption"
                    sx={{
                      color: 'text.secondary',
                      fontWeight: 400,
                      fontStyle: 'italic',
                      fontSize: 11,
                      lineHeight: 1.35,
                      textAlign: 'center',
                      alignSelf: 'stretch',
                      maxWidth: 560,
                      width: '100%',
                      px: { xs: 0.5, md: 0 },
                    }}
                  >
                    The donut shows how much each module contributes to the total score.
                  </Typography>
                </Stack>
              </Stack>
            </Stack>
          </Box>
        </Stack>
      </Stack>
    </Paper>
  )
}

function PieBreakdownChart({
  segments,
  totalScore,
  maxScore,
  totalPercent,
  centerPrimaryColor,
  centerSecondaryColor,
}: {
  segments: DonutSegment[]
  totalScore: number
  maxScore: number
  totalPercent: number
  centerPrimaryColor: string
  centerSecondaryColor: string
}) {
  const size = 220
  const outerRadius = 92
  const innerRadius = outerRadius * 0.54
  const cx = size / 2
  const cy = size / 2
  const total = segments.reduce((sum, segment) => sum + segment.value, 0) || 1
  let startAngle = -90

  function polarPoint(angleDeg: number, r: number): { x: number; y: number } {
    const rad = (angleDeg * Math.PI) / 180
    return { x: cx + r * Math.cos(rad), y: cy + r * Math.sin(rad) }
  }

  function annulusSlicePath(start: number, end: number, rOuter: number, rInner: number): string {
    const largeArcFlag = end - start > 180 ? 1 : 0
    const p1 = polarPoint(start, rOuter)
    const p2 = polarPoint(end, rOuter)
    const p3 = polarPoint(end, rInner)
    const p4 = polarPoint(start, rInner)
    return [
      `M ${p1.x} ${p1.y}`,
      `A ${rOuter} ${rOuter} 0 ${largeArcFlag} 1 ${p2.x} ${p2.y}`,
      `L ${p3.x} ${p3.y}`,
      `A ${rInner} ${rInner} 0 ${largeArcFlag} 0 ${p4.x} ${p4.y}`,
      'Z',
    ].join(' ')
  }

  return (
    <Box
      sx={{
        position: 'relative',
        width: { xs: 190, sm: size },
        height: { xs: 190, sm: size },
        flexShrink: 0,
        alignSelf: { xs: 'center', lg: 'flex-start' },
      }}
    >
      <svg width="100%" height="100%" viewBox={`0 0 ${size} ${size}`} role="img" aria-label={`Overall security score ${totalScore} out of ${maxScore}`}>
        {segments.map((segment) => {
          const share = segment.included ? Math.max(0, segment.value) / total : 0
          const sweep = share * 360
          const segmentStart = startAngle
          const segmentEnd = startAngle + sweep
          startAngle += sweep
          const midAngle = segmentStart + sweep / 2
          const labelRadius = (outerRadius + innerRadius) / 2
          const labelPoint = polarPoint(midAngle, labelRadius)
          const sharePercent = Math.round(share * 100)

          if (!segment.included || sweep <= 0) return null

          return (
            <g key={segment.id}>
              <path
                d={annulusSlicePath(segmentStart, segmentEnd, outerRadius, innerRadius)}
                fill={segment.color}
                stroke="#ffffff"
                strokeWidth={1.1}
              />
              {sharePercent >= 10 ? (
                <text
                  x={labelPoint.x}
                  y={labelPoint.y}
                  textAnchor="middle"
                  dominantBaseline="middle"
                  fill="#ffffff"
                  fontWeight="400"
                  fontSize="11.5"
                >
                  {sharePercent}%
                </text>
              ) : null}
            </g>
          )
        })}
        <circle
          cx={cx}
          cy={cy}
          r={innerRadius}
          fill="#ffffff"
          stroke="#e9f1f4"
          strokeWidth={1}
        />
        <text
          x={cx}
          y={cy - 3}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={centerPrimaryColor}
          fontWeight="900"
          fontSize="26"
        >
          {totalPercent}%
        </text>
        <text
          x={cx}
          y={cy + 17}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={centerSecondaryColor}
          fontWeight="700"
          fontSize="11"
        >
          total
        </text>
      </svg>
    </Box>
  )
}
