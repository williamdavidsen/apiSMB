import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useEffect, useState } from 'react'
import type { HomeSlide } from '../model/home.types'

type HeroSlideProps = {
  slide: HomeSlide
  isActive: boolean
  reducedMotion: boolean
}

export function HeroSlide({ slide, isActive, reducedMotion }: HeroSlideProps) {
  const [hasImageError, setHasImageError] = useState(false)

  useEffect(() => {
    setHasImageError(false)
  }, [slide.id])

  if (hasImageError) {
    return (
      <Box
        sx={{
          width: '100%',
          height: { xs: 250, md: 320 },
          display: 'flex',
          alignItems: 'center',
          background:
            'linear-gradient(135deg, rgba(0, 166, 186, 0.12) 0%, rgba(255, 255, 255, 0.98) 45%, rgba(0, 166, 186, 0.18) 100%)',
          px: { xs: 2.5, md: 4 },
        }}
      >
        <Stack spacing={1.5} sx={{ maxWidth: 420 }}>
          <Typography component="h2" variant="h4" sx={{ color: 'secondary.dark', fontWeight: 900, lineHeight: 1.1 }}>
            {slide.titleParts.map((part) => part.text).join(' ')}
          </Typography>
          <Typography variant="body1" sx={{ color: 'text.secondary', maxWidth: 380 }}>
            {slide.description}
          </Typography>
        </Stack>
      </Box>
    )
  }

  return (
    <Box
      sx={{
        position: 'relative',
        width: '100%',
        height: { xs: 250, sm: 280, md: 320 },
        overflow: 'hidden',
        bgcolor: '#e9f7fa',
      }}
    >
      <Box
        component="img"
        src={slide.imageUrl}
        alt={slide.imageAlt}
        onError={() => setHasImageError(true)}
        sx={{
          position: 'absolute',
          inset: 0,
          width: '100%',
          height: '100%',
          objectFit: 'cover',
          objectPosition: { xs: 'center', md: 'left center' },
          transform: reducedMotion ? 'none' : isActive ? 'scale(1.02)' : 'scale(1)',
          transition: reducedMotion ? 'none' : 'transform 420ms ease-out',
          display: 'block',
        }}
      />
      <Box
        sx={{
          position: 'relative',
          zIndex: 1,
          height: '100%',
          display: 'flex',
          alignItems: 'center',
          maxWidth: { xs: '56%', sm: '46%', md: 410 },
          px: { xs: 2.2, sm: 3.2, md: 4.8 },
        }}
      >
        <Stack
          spacing={1.35}
          sx={{
            alignItems: 'flex-start',
            width: '100%',
            mt: { xs: -1.1, sm: -1, md: -1.2 },
          }}
        >
          <Typography
            component="h2"
            sx={{
              color: '#18425d',
              fontWeight: 800,
              fontSize: { xs: '1.4rem', sm: '1.78rem', md: '2.44rem' },
              lineHeight: 1.08,
              letterSpacing: '-0.03em',
            }}
          >
            {slide.titleParts.map((part) => (
              <Box
                key={`${slide.id}-${part.text}`}
                component="span"
                sx={{
                  display: 'block',
                  color: part.tone === 'accent' ? slide.accentColor : '#18425d',
                }}
              >
                {part.text}
              </Box>
            ))}
          </Typography>
          <Typography
            sx={{
              color: 'rgba(54, 79, 95, 0.9)',
              fontSize: { xs: '0.9rem', sm: '1rem', md: '1.21rem' },
              lineHeight: { xs: 1.42, md: 1.48 },
              maxWidth: { xs: 188, sm: 236, md: 292 },
            }}
          >
            {slide.description}
          </Typography>
        </Stack>
      </Box>
    </Box>
  )
}
