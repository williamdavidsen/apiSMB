import type { ThreatTopic } from '../../../shared/constants/threatKeys'

export type ThreatStat = {
  id: string
  title: string
  icon: 'mail' | 'users' | 'building' | 'shield' | 'warning' | 'web' | 'time' | 'link'
  value: string
  caption: string
  summary: string
  source: string
}

export type ThreatSection = {
  title: string
  points: string[]
}

export type ThreatLandscapeContent = {
  topic: ThreatTopic
  pageTitle: string
  intro: string
  stats: ThreatStat[]
  findings: ThreatSection
  actions: ThreatSection
  footerNote: string
}
