export const threatTopics = {
  phishingSpoofing: 'phishing-spoofing',
  weakTlsCerts: 'weak-tls-certs',
  missingHeaders: 'missing-headers',
} as const

export type ThreatTopic = (typeof threatTopics)[keyof typeof threatTopics]
