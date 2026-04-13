import Box from '@mui/material/Box'
import { useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { routes } from '../../shared/constants/routes'
import { Footer } from './Footer'
import { SideNav } from './SideNav'
import { TopBar } from './TopBar'

function resolveTitle(pathname: string) {
  if (pathname === routes.home) return 'Home page'
  if (pathname === routes.scan) return 'Security scan'
  if (pathname === routes.dashboard) return 'Security dashboard'
  if (pathname.startsWith(`${routes.dashboard}/`)) return 'Module details'
  if (pathname.startsWith(routes.threatLandscape)) return 'Threat landscape'
  return 'Security dashboard'
}

export function MainLayout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Box
        component="a"
        href="#main-content"
        sx={{
          position: 'absolute',
          left: -9999,
          top: 0,
          zIndex: 1400,
          bgcolor: 'background.paper',
          color: 'text.primary',
          p: 1,
          '&:focus': { left: 8, top: 8, outline: '3px solid', outlineColor: 'primary.main' },
        }}
      >
        Skip to main content
      </Box>

      <TopBar title={resolveTitle(location.pathname)} onOpenNav={() => setMobileOpen(true)} />

      <Box sx={{ display: 'flex', flex: 1, minHeight: 0 }}>
        <SideNav mobileOpen={mobileOpen} onCloseMobile={() => setMobileOpen(false)} />

        <Box component="main" id="main-content" sx={{ flex: 1, p: { xs: 2, sm: 3, md: 4 } }}>
          <Outlet />
        </Box>
      </Box>

      <Footer />
    </Box>
  )
}
