export function clampPercent(value: number): number {
  if (Number.isNaN(value)) return 0
  return Math.min(100, Math.max(0, value))
}

export function gradeFromPercent(percent: number): string {
  const p = clampPercent(percent)
  if (p >= 90) return 'A'
  if (p >= 80) return 'B'
  if (p >= 70) return 'C'
  if (p >= 60) return 'D'
  if (p >= 50) return 'E'
  return 'F'
}

export function modulePercent(rawScore: number, rawMax: number): number {
  if (rawMax <= 0) return 0
  return clampPercent((rawScore / rawMax) * 100)
}
