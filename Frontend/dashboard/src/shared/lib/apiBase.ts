/**
 * Base URL for API calls. In local dev, leave unset so requests go to the Vite dev server
 * and are proxied to the ASP.NET API (see `vite.config.ts`).
 */
export function getApiBaseUrl(): string {
  const explicit = import.meta.env.VITE_API_BASE_URL
  if (typeof explicit === 'string' && explicit.trim().length > 0) {
    return explicit.replace(/\/+$/, '')
  }
  return ''
}

export function apiUrl(path: string): string {
  const base = getApiBaseUrl()
  const normalized = path.startsWith('/') ? path : `/${path}`
  return `${base}${normalized}`
}
