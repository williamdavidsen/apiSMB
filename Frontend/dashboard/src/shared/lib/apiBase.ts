const API_BASE_URL = import.meta.env.VITE_API_URL?.trim() || ''

export function apiUrl(path: string): string {
  if (!path.startsWith('/')) {
    path = `/${path}`
  }
  return `${API_BASE_URL}${path}`
}
