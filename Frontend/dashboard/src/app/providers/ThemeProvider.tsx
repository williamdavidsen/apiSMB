import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles'
import type { PropsWithChildren } from 'react'
import { appTheme } from '../../styles/mui-theme'

export function ThemeProvider({ children }: PropsWithChildren) {
  return <MuiThemeProvider theme={appTheme}>{children}</MuiThemeProvider>
}
