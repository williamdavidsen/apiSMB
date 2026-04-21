const LAST_SCANNED_DOMAIN_KEY = 'smb.lastScannedDomain'

export function saveLastScannedDomain(domain: string): void {
  const value = domain.trim().toLowerCase()
  if (!value) return

  try {
    window.localStorage.setItem(LAST_SCANNED_DOMAIN_KEY, value)
  } catch {
    // Ignore storage errors (private mode, policy restrictions, etc.)
  }
}

export function getLastScannedDomain(): string | null {
  try {
    const value = window.localStorage.getItem(LAST_SCANNED_DOMAIN_KEY)?.trim()
    return value ? value : null
  } catch {
    return null
  }
}
