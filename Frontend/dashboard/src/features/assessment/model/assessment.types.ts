export type AssessmentAlert = {
  type: string
  message: string
}

export type AssessmentWeights = {
  sslTls: number
  httpHeaders: number
  emailSecurity: number
  reputation: number
}

export type AssessmentModuleScore = {
  included: boolean
  weightPercent: number
  rawScore: number
  rawMaxScore: number
  normalizedScore: number
  weightedContribution: number
  status: string
}

export type AssessmentModuleScores = {
  sslTls: AssessmentModuleScore
  httpHeaders: AssessmentModuleScore
  emailSecurity: AssessmentModuleScore
  reputation: AssessmentModuleScore
}

export type PqcCheckResult = {
  domain: string
  pqcDetected: boolean
  status: string
  mode: string
  readinessLevel: string
  algorithmFamily: string
  handshakeSupported: boolean
  confidence: string
  notes: string
  evidence: string[]
}

export type AssessmentCheckResult = {
  domain: string
  overallScore: number
  maxScore: number
  status: string
  grade: string
  emailModuleIncluded: boolean
  pqcReadiness: PqcCheckResult
  weights: AssessmentWeights
  modules: AssessmentModuleScores
  alerts: AssessmentAlert[]
}

export type SslScoreDetail = {
  score: number
  details: string
}

export type SslCriteria = {
  tlsVersion: SslScoreDetail
  certificateValidity: SslScoreDetail
  remainingLifetime: SslScoreDetail
  cipherStrength: SslScoreDetail
}

export type SslAlert = {
  type: string
  message: string
  expiryDate?: string | null
}

export type SslCheckResult = {
  domain: string
  overallScore: number
  maxScore: number
  status: string
  criteria: SslCriteria
  alerts: SslAlert[]
}

export type HeaderScoreDetail = {
  score: number
  details: string
}

export type HeadersCriteria = {
  strictTransportSecurity: HeaderScoreDetail
  contentSecurityPolicy: HeaderScoreDetail
  clickjackingProtection: HeaderScoreDetail
  mimeSniffingProtection: HeaderScoreDetail
  referrerPolicy: HeaderScoreDetail
}

export type HeadersObservatorySummary = {
  grade: string
  score: number
  testsPassed: number
  testsFailed: number
  testsQuantity: number
  detailsUrl: string
}

export type HeadersAlert = {
  type: string
  message: string
}

export type HeadersCheckResult = {
  domain: string
  overallScore: number
  maxScore: number
  status: string
  criteria: HeadersCriteria
  observatory: HeadersObservatorySummary
  alerts: HeadersAlert[]
}

export type EmailScoreDetail = {
  score: number
  confidence: string
  details: string
}

export type EmailCriteria = {
  spfVerification: EmailScoreDetail
  dkimActivated: EmailScoreDetail
  dmarcEnforcement: EmailScoreDetail
}

export type EmailDnsSummary = {
  mxRecords: string[]
  spfRecord: string
  dmarcRecord: string
  dkimSelectorsFound: string[]
}

export type EmailAlert = {
  type: string
  message: string
}

export type EmailCheckResult = {
  domain: string
  hasMailService: boolean
  moduleApplicable: boolean
  overallScore: number
  maxScore: number
  status: string
  criteria: EmailCriteria
  dnsSummary: EmailDnsSummary
  alerts: EmailAlert[]
}

export type ReputationScoreDetail = {
  score: number
  confidence: string
  details: string
}

export type ReputationCriteria = {
  blacklistStatus: ReputationScoreDetail
  malwareAssociation: ReputationScoreDetail
}

export type ReputationSummary = {
  maliciousDetections: number
  suspiciousDetections: number
  harmlessDetections: number
  undetectedDetections: number
  reputation: number
  communityMaliciousVotes: number
  communityHarmlessVotes: number
  lastAnalysisDate: string
  permalink: string
}

export type ReputationAlert = {
  type: string
  message: string
}

export type ReputationCheckResult = {
  domain: string
  overallScore: number
  maxScore: number
  status: string
  criteria: ReputationCriteria
  summary: ReputationSummary
  alerts: ReputationAlert[]
}

export type AssessmentDashboardBundle = {
  assessment: AssessmentCheckResult
  ssl: SslCheckResult
  headers: HeadersCheckResult
  email: EmailCheckResult
  reputation: ReputationCheckResult
}

export type DashboardModuleKey = 'ssl-tls' | 'http-headers' | 'email' | 'reputation'
