import CssBaseline from '@mui/material/CssBaseline'
import type { PropsWithChildren } from 'react'
import { ThemeProvider } from './ThemeProvider'

export function AppProviders({ children }: PropsWithChildren) {
  return (
    <ThemeProvider>
      <CssBaseline />
      {children}
    </ThemeProvider>
  )
}
