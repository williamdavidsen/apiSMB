import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import { useNavigate } from 'react-router-dom'
import { DomainScanForm } from '../features/home/components/DomainScanForm'
import { HomeCarousel } from '../features/home/components/HomeCarousel'
import { routes } from '../shared/constants/routes'

export function HomePage() {
  const navigate = useNavigate()

  const handleSubmitDomain = (domain: string) => {
    const params = new URLSearchParams({ domain })
    navigate(`${routes.scan}?${params.toString()}`)
  }

  return (
    <Box sx={{ maxWidth: 1180, mx: 'auto', width: '100%' }}>
      <Stack spacing={{ xs: 2.5, md: 3.5 }}>
        <HomeCarousel />
        <DomainScanForm onSubmitDomain={handleSubmitDomain} />
      </Stack>
    </Box>
  )
}
