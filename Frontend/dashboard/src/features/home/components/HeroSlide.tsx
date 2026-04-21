import Box from '@mui/material/Box'
import type { HomeSlide } from '../model/home.types'

type HeroSlideProps = {
  slide: HomeSlide
  isActive: boolean
  reducedMotion: boolean
}

export function HeroSlide({ slide, isActive, reducedMotion }: HeroSlideProps) {
  return (
    <Box
      component="img"
      src={slide.imageUrl}
      alt={slide.imageAlt}
      sx={{
        width: '100%',
        height: { xs: 250, md: 320 },
        objectFit: 'cover',
        objectPosition: { xs: 'left center', sm: 'center' },
        transform: reducedMotion ? 'none' : isActive ? 'scale(1.02)' : 'scale(1)',
        transition: reducedMotion ? 'none' : 'transform 420ms ease-out',
        display: 'block',
      }}
    />
  )
}
