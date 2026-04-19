import { describe, expect, it } from 'vitest'
import {
  clampPercent,
  gradeFromPercent,
  modulePercent,
} from '../../../../Frontend/dashboard/src/shared/lib/score'

describe('score utilities', () => {
  it('clamps percentage values to the 0-100 interval', () => {
    expect(clampPercent(-1)).toBe(0)
    expect(clampPercent(101)).toBe(100)
    expect(clampPercent(Number.NaN)).toBe(0)
  })

  it('maps grade boundaries using boundary value analysis', () => {
    expect(gradeFromPercent(90)).toBe('A')
    expect(gradeFromPercent(89.99)).toBe('B')
    expect(gradeFromPercent(80)).toBe('B')
    expect(gradeFromPercent(79.99)).toBe('C')
    expect(gradeFromPercent(50)).toBe('E')
    expect(gradeFromPercent(49.99)).toBe('F')
  })

  it('normalizes module score and guards against invalid max score', () => {
    expect(modulePercent(5, 10)).toBe(50)
    expect(modulePercent(5, 0)).toBe(0)
    expect(modulePercent(15, 10)).toBe(100)
  })
})
