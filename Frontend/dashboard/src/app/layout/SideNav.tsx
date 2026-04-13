import Box from '@mui/material/Box'
import Drawer from '@mui/material/Drawer'
import Link from '@mui/material/Link'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { NavLink } from 'react-router-dom'
import { routes } from '../../shared/constants/routes'

const navItems = [
  { label: 'Phishing & spoofing', to: routes.threatPhishing },
  { label: 'Weak TLS / certs', to: routes.threatWeakTls },
  { label: 'Missing headers', to: routes.threatMissingHeaders },
]

type SideNavProps = {
  mobileOpen: boolean
  onCloseMobile: () => void
}

function NavContent({ onNavigate }: { onNavigate?: () => void }) {
  return (
    <Box role="navigation" aria-label="Threat landscape navigation" sx={{ py: 2 }}>
      <Typography
        variant="overline"
        component="h2"
        sx={{ px: 2, color: 'text.secondary', fontWeight: 700, letterSpacing: 0.6 }}
      >
        Threat landscape
      </Typography>

      <Stack component="ul" sx={{ listStyle: 'none', m: 0, p: 0, mt: 1 }}>
        {navItems.map((item) => (
          <Box component="li" key={item.to}>
            <Link
              component={NavLink}
              to={item.to}
              onClick={onNavigate}
              underline="none"
              sx={{
                display: 'block',
                px: 2,
                py: 1.1,
                mx: 1,
                my: 0.5,
                borderRadius: 1.2,
                color: 'text.primary',
                fontSize: 14,
                '&.active': {
                  bgcolor: 'primary.light',
                  fontWeight: 700,
                },
                '&:focus-visible': {
                  outline: '3px solid',
                  outlineColor: 'primary.main',
                },
              }}
            >
              {item.label}
            </Link>
          </Box>
        ))}
      </Stack>
    </Box>
  )
}

export function SideNav({ mobileOpen, onCloseMobile }: SideNavProps) {
  return (
    <>
      <Box
        sx={{
          display: { xs: 'none', lg: 'block' },
          width: 220,
          borderRight: '1px solid',
          borderColor: 'divider',
          bgcolor: 'background.paper',
        }}
      >
        <NavContent />
      </Box>

      <Drawer
        open={mobileOpen}
        onClose={onCloseMobile}
        ModalProps={{ keepMounted: true }}
        sx={{ display: { xs: 'block', lg: 'none' }, '& .MuiDrawer-paper': { width: 260 } }}
      >
        <NavContent onNavigate={onCloseMobile} />
      </Drawer>
    </>
  )
}
