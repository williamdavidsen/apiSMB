import { createTheme } from '@mui/material/styles'

export const appTheme = createTheme({
  palette: {
    primary: {
      main: '#00bcd4',
      light: '#b2ebf2',
      dark: '#0097a7',
      contrastText: '#ffffff',
    },
    // Cyan–teal family aligned with primary (headings, links); avoids heavy forest green.
    secondary: {
      main: '#00acc1',
      light: '#b2ebf2',
      dark: '#00838f',
      contrastText: '#ffffff',
    },
    success: {
      main: '#4caf50',
      light: '#81c784',
      dark: '#2e7d32',
    },
    warning: {
      main: '#ff9800',
      light: '#ffcc80',
      dark: '#f57c00',
    },
    error: {
      main: '#f44336',
      light: '#ffcdd2',
      dark: '#d32f2f',
    },
    info: {
      main: '#0288d1',
      light: '#b3e5fc',
      dark: '#01579b',
      contrastText: '#ffffff',
    },
    background: {
      default: '#f3f6f8',
      paper: '#ffffff',
    },
    text: {
      primary: '#163238',
      secondary: '#35565d',
    },
  },
  shape: {
    borderRadius: 10,
  },
  typography: {
    fontFamily: ['Inter', 'Roboto', '"Segoe UI"', 'Arial', 'sans-serif'].join(','),
    h1: {
      fontSize: '1.8rem',
      fontWeight: 700,
    },
    h2: {
      fontSize: '1.4rem',
      fontWeight: 700,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  components: {
    MuiChip: {
      styleOverrides: {
        root: {
          boxShadow: 'none !important',
          filter: 'none',
          '&.MuiChip-filled': {
            boxShadow: 'none !important',
          },
          '&.MuiChip-filledSuccess': { boxShadow: 'none !important' },
          '&.MuiChip-filledWarning': { boxShadow: 'none !important' },
          '&.MuiChip-filledError': { boxShadow: 'none !important' },
          '&.MuiChip-filledDefault': { boxShadow: 'none !important' },
        },
      },
    },
  },
})
