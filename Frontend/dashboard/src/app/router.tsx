import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { HomePage } from '../pages/HomePage'
import { ModuleDetailPage } from '../pages/ModuleDetailPage'
import { ScanProgressPage } from '../pages/ScanProgressPage'
import { SecurityDashboardPage } from '../pages/SecurityDashboardPage'
import { routes } from '../shared/constants/routes'
import { MainLayout } from './layout/MainLayout'

function PlaceholderPage({ title }: { title: string }) {
  return (
    <Paper variant="outlined" sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        {title}
      </Typography>
      <Typography color="text.secondary">
        This page is prepared in the route structure and will be implemented next.
      </Typography>
    </Paper>
  )
}

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path={routes.home} element={<HomePage />} />
          <Route path={routes.scan} element={<ScanProgressPage />} />
          <Route path={routes.dashboard} element={<SecurityDashboardPage />} />
          <Route path={`${routes.dashboard}/:domain/:module`} element={<ModuleDetailPage />} />
          <Route
            path={`${routes.threatLandscape}/:topic`}
            element={<PlaceholderPage title="Threat landscape" />}
          />
          <Route path="*" element={<Navigate to={routes.home} replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
