import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Collapse from '@mui/material/Collapse'
import Divider from '@mui/material/Divider'
import Drawer from '@mui/material/Drawer'
import List from '@mui/material/List'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemText from '@mui/material/ListItemText'
import MenuOutlined from '@mui/icons-material/MenuOutlined'
import ExpandLess from '@mui/icons-material/ExpandLess'
import ExpandMore from '@mui/icons-material/ExpandMore'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { Link as RouterLink, NavLink, useLocation } from 'react-router-dom'
import siteLogo from '../../assets/images/home/site-logo.svg'
import { routes } from '../../shared/constants/routes'
import { brandGradients } from '../../styles/designTokens'
import { threatNavItems } from './SideNav'

type TopBarProps = {
  title: string
}

export function TopBar({ title }: TopBarProps) {
  const location = useLocation()
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false)
  const [threatSubmenuOpen, setThreatSubmenuOpen] = useState(location.pathname.startsWith(routes.threatLandscape))
  const appTitleTop = 'SecureScan'
  const appTitleBottom = 'for SMBs'
  const navItems = [
    { label: 'Home', mobileLabel: 'Home', to: routes.home },
    { label: 'Dashboard', mobileLabel: 'Dashboard', to: routes.dashboard },
    { label: 'Threat Landscape', mobileLabel: 'Threats', to: routes.threatPhishing },
  ]

  return (
    <AppBar
      position="sticky"
      elevation={0}
      aria-label={`Top navigation - ${title}`}
      sx={{
        color: 'primary.contrastText',
        background: brandGradients.appBar,
        borderBottom: '1px solid',
        borderColor: 'rgba(0, 98, 115, 0.75)',
        width: '100%',
        boxSizing: 'border-box',
        overflowX: 'clip',
      }}
    >
      <Toolbar sx={{ minHeight: 56, position: 'relative', gap: 0.5 }}>
        <Button
          onClick={() => setMobileMenuOpen(true)}
          sx={{
            mr: 1,
            color: 'primary.contrastText',
            minWidth: 40,
            display: { xs: 'inline-flex', lg: 'none' },
          }}
          aria-label="Open mobile navigation menu"
        >
          <MenuOutlined sx={{ fontSize: 24 }} />
        </Button>

        <Box
          component={RouterLink}
          to={routes.home}
          sx={{
            textDecoration: 'none',
            color: 'primary.contrastText',
            mr: { xs: 1, md: 2.5 },
            display: 'inline-flex',
            alignItems: 'center',
            gap: 1,
            minWidth: 0,
          }}
          aria-label="Go to home page"
        >
          <Box component="img" src={siteLogo} alt="" aria-hidden sx={{ width: 24, height: 24 }} />
          <Box
            sx={{
              display: 'inline-flex',
              flexDirection: 'column',
              minWidth: 0,
              maxWidth: { xs: 150, sm: 'none' },
              width: 'fit-content',
            }}
          >
            <Typography
              sx={{
                fontWeight: 800,
                fontSize: { xs: 13.6, md: 17.1 },
                letterSpacing: { xs: 0.05, md: 0.1 },
                lineHeight: 1.05,
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }}
            >
              {appTitleTop}
            </Typography>
            <Box
              sx={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: { xs: 0.4, md: 0.7 },
                mt: 0.1,
                width: '100%',
                minWidth: 0,
              }}
            >
              <Box
                component="span"
                sx={{
                  flex: 1,
                  height: 1.5,
                  minWidth: { xs: 10, md: 18 },
                  bgcolor: 'rgba(255,255,255,0.7)',
                }}
              />
              <Typography
                sx={{
                  fontWeight: 700,
                  fontSize: { xs: 9.4, md: 10.8 },
                  letterSpacing: { xs: 0.2, md: 0.35 },
                  lineHeight: 1,
                  whiteSpace: 'nowrap',
                }}
              >
                {appTitleBottom}
              </Typography>
              <Box
                component="span"
                sx={{
                  flex: 1,
                  height: 1.5,
                  minWidth: { xs: 10, md: 18 },
                  bgcolor: 'rgba(255,255,255,0.7)',
                }}
              />
            </Box>
          </Box>
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

        <Box sx={{ flex: 1, display: { xs: 'block', lg: 'none' } }} />
      </Toolbar>
      <Drawer
        open={mobileMenuOpen}
        onClose={() => setMobileMenuOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{ display: { xs: 'block', lg: 'none' }, '& .MuiDrawer-paper': { width: 292 } }}
      >
        <Box sx={{ py: 1.5 }} role="navigation" aria-label="Mobile navigation">
          <Typography sx={{ px: 2, pb: 0.8, fontWeight: 800, color: 'secondary.dark' }}>
            Menu
          </Typography>
          <List sx={{ pt: 0 }}>
            {navItems.slice(0, 2).map((item) => (
              <ListItemButton
                key={`mobile-drawer-${item.to}`}
                component={NavLink}
                to={item.to}
                onClick={() => setMobileMenuOpen(false)}
                sx={{
                  '&.active': {
                    bgcolor: 'primary.light',
                    fontWeight: 700,
                  },
                }}
              >
                <ListItemText primary={item.label} />
              </ListItemButton>
            ))}
            <ListItemButton
              onClick={() => setThreatSubmenuOpen((prev) => !prev)}
              sx={{
                ...(location.pathname.startsWith(routes.threatLandscape)
                  ? { bgcolor: 'primary.light', fontWeight: 700 }
                  : null),
              }}
            >
              <ListItemText primary="Threat Landscape" />
              {threatSubmenuOpen ? <ExpandLess /> : <ExpandMore />}
            </ListItemButton>
            <Collapse in={threatSubmenuOpen} timeout="auto" unmountOnExit>
              <List component="div" disablePadding>
                {threatNavItems.map((item) => (
                  <ListItemButton
                    key={`threat-sub-${item.to}`}
                    component={NavLink}
                    to={item.to}
                    onClick={() => setMobileMenuOpen(false)}
                    sx={{
                      pl: 4,
                      '&.active': {
                        bgcolor: 'rgba(0, 172, 193, 0.14)',
                        fontWeight: 700,
                      },
                    }}
                  >
                    <ListItemText primary={item.label} />
                  </ListItemButton>
                ))}
              </List>
            </Collapse>
          </List>
          <Divider sx={{ my: 1.2 }} />
          <Box sx={{ px: 2 }}>
            <Button fullWidth variant="outlined" color="secondary" onClick={() => setMobileMenuOpen(false)}>
              Close menu
            </Button>
          </Box>
        </Box>
      </Drawer>
    </AppBar>
  )
}
