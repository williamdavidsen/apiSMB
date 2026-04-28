import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { HomePage } from '../pages/HomePage'
import { ModuleDetailPage } from '../pages/ModuleDetailPage'
import { PqcReadMorePage } from '../pages/PqcReadMorePage'
import { ScanProgressPage } from '../pages/ScanProgressPage'
import { SecurityDashboardPage } from '../pages/SecurityDashboardPage'
import { ThreatLandscapePage } from '../pages/ThreatLandscapePage'
import { routes } from '../shared/constants/routes'
import { MainLayout } from './layout/MainLayout'

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path={routes.home} element={<HomePage />} />
          <Route path={routes.scan} element={<ScanProgressPage />} />
          <Route path={routes.dashboard} element={<SecurityDashboardPage />} />
          <Route path={`${routes.dashboard}/:domain/pqc`} element={<PqcReadMorePage />} />
          <Route path={`${routes.dashboard}/:domain/:module`} element={<ModuleDetailPage />} />
          <Route path={`${routes.threatLandscape}/:topic`} element={<ThreatLandscapePage />} />
          <Route path="*" element={<Navigate to={routes.home} replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
