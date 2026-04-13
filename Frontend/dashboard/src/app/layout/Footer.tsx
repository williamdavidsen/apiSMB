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
        Norway · {new Date().getFullYear()}
      </Typography>
    </Box>
  )
}
