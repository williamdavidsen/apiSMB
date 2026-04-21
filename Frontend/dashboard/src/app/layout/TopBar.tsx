import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import MenuOutlined from '@mui/icons-material/MenuOutlined'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { Link as RouterLink, NavLink, useLocation } from 'react-router-dom'
import siteLogo from '../../assets/images/brand/site-logo.svg'
import { routes } from '../../shared/constants/routes'

type TopBarProps = {
  title: string
  onOpenNav: () => void
  showNavToggle?: boolean
}

export function TopBar({ title, onOpenNav, showNavToggle = true }: TopBarProps) {
  const location = useLocation()
  const appTitle = 'SMB Security Insight'
  const navItems = [
    { label: 'Home', to: routes.home },
    { label: 'Dashboard', to: routes.dashboard },
    { label: 'Threat Landscape', to: routes.threatPhishing },
  ]

  return (
    <AppBar
      position="sticky"
      elevation={0}
      aria-label={`Top navigation - ${title}`}
      sx={{
        color: 'primary.contrastText',
        background:
          'linear-gradient(105deg, #008fa1 0%, #00a6ba 45%, #007b88 100%)',
        borderBottom: '1px solid',
        borderColor: 'rgba(0, 98, 115, 0.75)',
      }}
    >
      <Toolbar sx={{ minHeight: 56, position: 'relative' }}>
        <Button
          onClick={onOpenNav}
          sx={{
            mr: 1,
            color: 'primary.contrastText',
            minWidth: 40,
            display: showNavToggle ? { xs: 'inline-flex', lg: 'none' } : 'none',
          }}
          aria-label="Open navigation menu"
        >
          <MenuOutlined sx={{ fontSize: 24 }} />
        </Button>

        <Box
          component={RouterLink}
          to={routes.home}
          sx={{
            textDecoration: 'none',
            color: 'primary.contrastText',
            mr: { xs: 1.5, md: 2.5 },
            display: 'inline-flex',
            alignItems: 'center',
            gap: 1,
          }}
          aria-label="Go to home page"
        >
          <Box component="img" src={siteLogo} alt="" aria-hidden sx={{ width: 24, height: 24 }} />
          <Typography sx={{ fontWeight: 800, fontSize: { xs: 14, md: 16 }, letterSpacing: 0.2 }}>
            {appTitle}
          </Typography>
        </Box>

        <Box
          sx={{
            display: { xs: 'none', lg: 'flex' },
            gap: 0.35,
            alignItems: 'center',
            ml: 'auto',
            mr: { lg: 1, xl: 2 },
            p: 0.32,
            borderRadius: 999,
            bgcolor: 'rgba(255,255,255,0.14)',
            border: '1px solid rgba(255,255,255,0.22)',
            backdropFilter: 'blur(2px)',
          }}
        >
          {navItems.map((item) => {
            const isActive =
              item.to === routes.dashboard
                ? location.pathname === routes.dashboard || location.pathname.startsWith(`${routes.dashboard}/`)
                : item.to === routes.threatPhishing
                  ? location.pathname.startsWith(routes.threatLandscape)
                  : location.pathname === item.to

            return (
              <Button
                key={item.to}
                component={NavLink}
                to={item.to}
                sx={{
                  color: 'primary.contrastText',
                  px: 1.35,
                  py: 0.42,
                  fontWeight: isActive ? 800 : 600,
                  borderRadius: 999,
                  minWidth: 'auto',
                  bgcolor: isActive ? 'rgba(255,255,255,0.34)' : 'transparent',
                  boxShadow: isActive ? 'inset 0 0 0 1px rgba(255,255,255,0.44)' : 'none',
                  '&:hover': { bgcolor: isActive ? 'rgba(255,255,255,0.39)' : 'rgba(255,255,255,0.16)' },
                }}
              >
                {item.label}
              </Button>
            )
          })}
        </Box>

        <Box
          sx={{
            display: { xs: 'flex', lg: 'none' },
            ml: 'auto',
            mr: 0.5,
            gap: 0.25,
            overflowX: 'auto',
            maxWidth: { xs: '54%', sm: '62%' },
            p: 0.26,
            borderRadius: 999,
            bgcolor: 'rgba(255,255,255,0.12)',
            border: '1px solid rgba(255,255,255,0.2)',
            '&::-webkit-scrollbar': { display: 'none' },
            scrollbarWidth: 'none',
          }}
        >
          {navItems.map((item) => {
            const isActive =
              item.to === routes.dashboard
                ? location.pathname === routes.dashboard || location.pathname.startsWith(`${routes.dashboard}/`)
                : item.to === routes.threatPhishing
                  ? location.pathname.startsWith(routes.threatLandscape)
                  : location.pathname === item.to

            return (
              <Button
                key={`mobile-${item.to}`}
                component={NavLink}
                to={item.to}
                size="small"
                sx={{
                  color: 'primary.contrastText',
                  px: 1,
                  py: 0.28,
                  minWidth: 'auto',
                  whiteSpace: 'nowrap',
                  fontSize: 12,
                  fontWeight: isActive ? 800 : 600,
                  borderRadius: 999,
                  bgcolor: isActive ? 'rgba(255,255,255,0.34)' : 'transparent',
                  boxShadow: isActive ? 'inset 0 0 0 1px rgba(255,255,255,0.44)' : 'none',
                }}
              >
                {item.label}
              </Button>
            )
          })}
        </Box>
      </Toolbar>
    </AppBar>
  )
}
