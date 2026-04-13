import type { DashboardModuleKey } from './assessment.types'

export const dashboardModuleOrder: DashboardModuleKey[] = ['ssl-tls', 'http-headers', 'email', 'reputation']

export const dashboardModulePathSegment: Record<DashboardModuleKey, string> = {
  'ssl-tls': 'ssl-tls',
  'http-headers': 'http-headers',
  email: 'email',
  reputation: 'reputation',
}

export function isDashboardModuleKey(value: string): value is DashboardModuleKey {
  return (
    value === 'ssl-tls' ||
    value === 'http-headers' ||
    value === 'email' ||
    value === 'reputation'
  )
}
