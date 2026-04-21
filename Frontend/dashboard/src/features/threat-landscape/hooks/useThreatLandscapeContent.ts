import { useMemo } from 'react'
import { threatTopics, type ThreatTopic } from '../../../shared/constants/threatKeys'
import { threatLandscapeContent } from '../data/threatLandscape.content.no'
import type { ThreatLandscapeContent } from '../model/threatLandscape.types'

const fallbackTopic: ThreatTopic = threatTopics.phishingSpoofing

export function useThreatLandscapeContent(topic: string | undefined): ThreatLandscapeContent {
  return useMemo(() => {
    if (!topic) return threatLandscapeContent[fallbackTopic]
    return threatLandscapeContent[topic] ?? threatLandscapeContent[fallbackTopic]
  }, [topic])
}
