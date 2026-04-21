import Box from '@mui/material/Box'
import Drawer from '@mui/material/Drawer'
import Link from '@mui/material/Link'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { NavLink } from 'react-router-dom'
import mailIcon from '../../assets/images/threat/icons/mail.svg'
import shieldIcon from '../../assets/images/threat/icons/shield.svg'
import webIcon from '../../assets/images/threat/icons/web.svg'
import { routes } from '../../shared/constants/routes'

const threatNavItems = [
  { label: 'Phishing & spoofing', to: routes.threatPhishing, icon: mailIcon },
  { label: 'Weak TLS / certs', to: routes.threatWeakTls, icon: shieldIcon },
  { label: 'Missing headers', to: routes.threatMissingHeaders, icon: webIcon },
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
        {threatNavItems.map((item) => (
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
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                <Box component="img" src={item.icon} alt="" aria-hidden sx={{ width: 17, height: 17 }} />
                <Box component="span">{item.label}</Box>
              </Stack>
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
          width: 240,
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
        sx={{ display: { xs: 'block', lg: 'none' }, '& .MuiDrawer-paper': { width: 280 } }}
      >
        <NavContent onNavigate={onCloseMobile} />
      </Drawer>
    </>
  )
}
