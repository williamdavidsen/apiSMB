import AppBar from '@mui/material/AppBar'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import { Link as RouterLink } from 'react-router-dom'
import { routes } from '../../shared/constants/routes'

type TopBarProps = {
  title: string
  onOpenNav: () => void
}

export function TopBar({ title, onOpenNav }: TopBarProps) {
  return (
    <AppBar
      position="sticky"
      elevation={0}
      sx={{
        color: 'primary.contrastText',
        background:
          'linear-gradient(105deg, #008fa1 0%, #00a6ba 45%, #007b88 100%)',
        borderBottom: '1px solid',
        borderColor: 'rgba(0, 98, 115, 0.75)',
      }}
    >
      <Toolbar sx={{ minHeight: 56 }}>
        <Button
          onClick={onOpenNav}
          sx={{ mr: 1, color: 'primary.contrastText', minWidth: 40, display: { md: 'inline-flex', lg: 'none' } }}
          aria-label="Open navigation menu"
        >
          ☰
        </Button>

        <Button
          component={RouterLink}
          to={routes.home}
          sx={{ color: 'primary.contrastText', minWidth: 0, px: 1.2 }}
          aria-label="Go to home page"
        >
          ⌂ Home
        </Button>

        <Box sx={{ flex: 1, textAlign: 'center', pr: { xs: 0, md: 7 } }}>
          <Typography component="h1" variant="subtitle1" sx={{ fontWeight: 700, color: 'primary.contrastText' }}>
            {title}
          </Typography>
        </Box>
      </Toolbar>
    </AppBar>
  )
}
