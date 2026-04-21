import Box from '@mui/material/Box'
import { Outlet, useLocation } from 'react-router-dom'
import homeBackground from '../../assets/images/home/background.png'
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
  const location = useLocation()
  const isHomePage = location.pathname === routes.home
  const isThreatPage = location.pathname.startsWith(routes.threatLandscape)

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', width: '100%', overflowX: 'clip' }}>
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

      <TopBar title={resolveTitle(location.pathname)} />

      <Box sx={{ display: 'flex', flex: 1, minHeight: 0 }}>
        {isThreatPage ? <SideNav /> : null}

        <Box
          component="main"
          id="main-content"
          sx={{
            flex: 1,
            p: { xs: 1.5, sm: 2.5, md: 4 },
            minWidth: 0,
            ...(isHomePage
              ? {
                  backgroundImage: `linear-gradient(rgba(255, 255, 255, 0.95), rgba(255, 255, 255, 0.93)), url(${homeBackground})`,
                  backgroundSize: 'cover',
                  backgroundPosition: 'center',
                  backgroundRepeat: 'no-repeat',
                }
              : null),
          }}
        >
          <Outlet />
        </Box>
      </Box>

      <Footer />
    </Box>
  )
}
