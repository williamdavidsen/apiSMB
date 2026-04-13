import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import type { HomeSlide } from '../model/home.types'

type HeroSlideProps = {
  slide: HomeSlide
  isActive: boolean
  reducedMotion: boolean
}

export function HeroSlide({ slide, isActive, reducedMotion }: HeroSlideProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', md: '50% 1fr' },
        minHeight: { xs: 420, md: 320 },
      }}
    >
      <Box
        component="img"
        src={slide.imageUrl}
        alt={slide.imageAlt}
        sx={{
          width: '100%',
          height: { xs: 250, md: 320 },
          objectFit: 'cover',
          objectPosition: 'center',
          transform: reducedMotion ? 'none' : isActive ? 'scale(1.02)' : 'scale(1)',
          transition: reducedMotion ? 'none' : 'transform 420ms ease-out',
        }}
      />

      <Box
        sx={{
          p: { xs: 2.5, md: 4.5 },
          display: 'grid',
          alignContent: 'center',
          gap: 1.8,
          transform: reducedMotion ? 'none' : isActive ? 'translateY(0)' : 'translateY(6px)',
          opacity: isActive ? 1 : 0.92,
          transition: reducedMotion ? 'none' : 'opacity 420ms ease-out, transform 420ms ease-out',
          bgcolor: 'background.paper',
        }}
      >
        <Typography
          component="h2"
          variant="h5"
          sx={{
            color: 'secondary.dark',
            textWrap: 'balance',
            fontWeight: 800,
            lineHeight: 1.2,
          }}
        >
          {slide.title}
        </Typography>
        <Typography
          variant="body1"
          sx={{ color: 'text.secondary', maxWidth: 56 + 'ch', fontSize: { xs: '0.98rem', md: '1.04rem' } }}
        >
          {slide.description}
        </Typography>
      </Box>
    </Box>
  )
}
