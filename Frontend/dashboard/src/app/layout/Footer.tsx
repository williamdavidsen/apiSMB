import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'

export function Footer() {
  return (
    <Box
      component="footer"
      sx={{
        borderTop: '1px solid',
        borderColor: 'divider',
        bgcolor: 'background.paper',
        py: 1.5,
        width: '100%',
        boxSizing: 'border-box',
        overflowX: 'clip',
      }}
    >
      <Typography variant="caption" align="center" sx={{ display: 'block', color: 'text.secondary' }}>
        Developed for Storm IT Sikkerhet
      </Typography>
      <Typography
        variant="caption"
        align="center"
        sx={{ display: 'block', color: 'text.secondary', mt: 0.35 }}
      >
        OSLO-2026
      </Typography>
    </Box>
  )
}
