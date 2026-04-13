import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Fade from '@mui/material/Fade'
import MobileStepper from '@mui/material/MobileStepper'
import Paper from '@mui/material/Paper'
import useMediaQuery from '@mui/material/useMediaQuery'
import { useEffect, useState } from 'react'
import { homeSlides } from '../data/homeCarousel.mock'
import { HeroSlide } from './HeroSlide'

export function HomeCarousel() {
  const reducedMotion = useMediaQuery('(prefers-reduced-motion: reduce)')
  const [activeStep, setActiveStep] = useState(0)
  const [isPaused, setIsPaused] = useState(false)
  const maxSteps = homeSlides.length

  const handleNext = () => setActiveStep((prev) => (prev + 1) % maxSteps)
  const handleBack = () => setActiveStep((prev) => (prev - 1 + maxSteps) % maxSteps)

  useEffect(() => {
    if (reducedMotion || isPaused) return

    const timer = window.setInterval(() => {
      setActiveStep((prev) => (prev + 1) % maxSteps)
    }, 4000)

    return () => window.clearInterval(timer)
  }, [isPaused, maxSteps, reducedMotion])

  return (
    <Paper
      variant="outlined"
      sx={{
        overflow: 'hidden',
        borderColor: 'divider',
        boxShadow: '0 8px 18px rgba(0, 90, 110, 0.08)',
        bgcolor: 'background.paper',
        borderRadius: 2,
      }}
      aria-roledescription="carousel"
      aria-label="Security recommendations carousel"
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
      onFocusCapture={() => setIsPaused(true)}
      onBlurCapture={() => setIsPaused(false)}
    >
      <Box sx={{ display: 'flex', alignItems: 'stretch', minHeight: { xs: 420, md: 320 } }}>
        <Button
          onClick={handleBack}
          aria-label="Show previous slide"
          sx={{
            display: { xs: 'none', md: 'inline-flex' },
            minWidth: 52,
            borderRadius: 0,
            bgcolor: 'rgba(178, 235, 242, 0.65)',
            color: 'primary.dark',
            '&:hover': { bgcolor: 'rgba(128, 222, 234, 0.85)' },
          }}
        >
          ‹
        </Button>
        <Box sx={{ flex: 1 }}>
          <Fade in key={homeSlides[activeStep].id} timeout={reducedMotion ? 0 : 420}>
            <Box>
              <HeroSlide slide={homeSlides[activeStep]} isActive reducedMotion={reducedMotion} />
            </Box>
          </Fade>
        </Box>
        <Button
          onClick={handleNext}
          aria-label="Show next slide"
          sx={{
            display: { xs: 'none', md: 'inline-flex' },
            minWidth: 52,
            borderRadius: 0,
            bgcolor: 'rgba(178, 235, 242, 0.65)',
            color: 'primary.dark',
            '&:hover': { bgcolor: 'rgba(128, 222, 234, 0.85)' },
          }}
        >
          ›
        </Button>
      </Box>

      <MobileStepper
        variant="dots"
        steps={maxSteps}
        position="static"
        activeStep={activeStep}
        sx={{
          justifyContent: 'center',
          bgcolor: 'background.paper',
          py: 0.75,
          '& .MuiMobileStepper-dot': {
            width: 9,
            height: 9,
            bgcolor: 'rgba(0, 166, 186, 0.35)',
          },
          '& .MuiMobileStepper-dotActive': {
            width: 18,
            borderRadius: 999,
            bgcolor: '#00a6ba',
            boxShadow: '0 0 0 2px rgba(0, 166, 186, 0.25)',
          },
        }}
        nextButton={<Box sx={{ width: 48 }} />}
        backButton={<Box sx={{ width: 48 }} />}
      />
    </Paper>
  )
}
