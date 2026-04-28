export type PqcReadMoreSource = {
  label: string
  url: string
  publisher: 'NIST' | 'NSM'
}

export type PqcReadMoreContent = {
  overviewTitle: string
  overview: string[]
  sectionTwoTitle: string
  sectionTwo: string[]
  sectionThreeTitle: string
  sectionThree: Array<{ title: string; summary: string }>
  sectionFourTitle: string
  sectionFour: string[]
  sources: PqcReadMoreSource[]
  lastReviewed: string
}

export const pqcReadMoreNotDetectedMock: PqcReadMoreContent = {
  overviewTitle: 'Overview',
  overview: [
    'Post-quantum cryptography (PQC) is designed to protect data against future attacks from cryptographically relevant quantum computers.',
    'Modern systems still rely heavily on public-key algorithms that are expected to become vulnerable once quantum capabilities mature.',
  ],
  sectionTwoTitle: 'Why transition now?',
  sectionTwo: [
    'NIST emphasizes that migration should start now, because full cryptographic transitions across products and infrastructure take many years.',
    'Data with long confidentiality lifetimes is exposed to harvest-now-decrypt-later risk, where encrypted traffic is collected today and decrypted later.',
    'NSM advises organizations to begin quantum migration planning early, including risk assessment, crypto inventory, and phased replacement plans.',
  ],
  sectionThreeTitle: 'Modern standards snapshot',
  sectionThree: [
    {
      title: 'FIPS 203 (ML-KEM)',
      summary: 'Primary post-quantum key-establishment standard for general encryption use cases.',
    },
    {
      title: 'FIPS 204 (ML-DSA)',
      summary: 'Primary post-quantum digital signature standard for identity and authenticity scenarios.',
    },
    {
      title: 'FIPS 205 (SLH-DSA)',
      summary: 'Alternative hash-based signature standard that provides diversity as a backup approach.',
    },
  ],
  sectionFourTitle: 'Recommended migration steps',
  sectionFour: [
    'Build and maintain a cryptographic inventory of algorithms, protocols, and dependencies.',
    'Prioritize systems that protect long-lived or sensitive data.',
    'Plan phased rollout with vendors and internal teams to maintain operational continuity.',
    'Adopt standardized, well-reviewed implementations and keep cryptographic agility as a requirement.',
  ],
  sources: [
    {
      publisher: 'NIST',
      label: 'What Is Post-Quantum Cryptography?',
      url: 'https://www.nist.gov/cybersecurity-and-privacy/what-post-quantum-cryptography',
    },
    {
      publisher: 'NIST',
      label: 'Post-quantum cryptography program page',
      url: 'https://www.nist.gov/pqc',
    },
    {
      publisher: 'NIST',
      label: 'NIST releases first 3 finalized PQC standards',
      url: 'https://www.nist.gov/news-events/news/2024/08/nist-releases-first-3-finalized-post-quantum-encryption-standards',
    },
    {
      publisher: 'NSM',
      label: 'Kvantemigrasjon',
      url: 'https://nsm.no/fagomrader/digital-sikkerhet/kryptosikkerhet/kvantemigrasjon',
    },
    {
      publisher: 'NSM',
      label: 'Hva er kvantemigrasjon?',
      url: 'https://nsm.no/fagomrader/digital-sikkerhet/kryptosikkerhet/kvantemigrasjon/hva-er-kvantemigrasjon',
    },
    {
      publisher: 'NSM',
      label: 'Cryptographic Recommendations',
      url: 'https://nsm.no/advice-and-guidance/publications/cryptographic-recommendations',
    },
  ],
  lastReviewed: '2026-04-28',
}

export const pqcReadMoreDetectedMock: PqcReadMoreContent = {
  overviewTitle: 'PQC detected in this environment',
  overview: [
    'Hybrid post-quantum indicators suggest this domain already uses a modern transition-friendly TLS posture.',
    'This is a strong starting point for long-term resilience, but ongoing governance is still required.',
  ],
  sectionTwoTitle: 'Why this matters',
  sectionTwo: [
    'Early adoption reduces long-term exposure to harvest-now-decrypt-later risk for sensitive data.',
    'Hybrid deployment supports interoperability while preparing for broader post-quantum adoption.',
    'NIST and NSM guidance both stress planning and crypto agility even after initial adoption.',
  ],
  sectionThreeTitle: 'What to sustain',
  sectionThree: [
    {
      title: 'Algorithm governance',
      summary: 'Track algorithm policy updates and maintain approved parameter sets over time.',
    },
    {
      title: 'Operational monitoring',
      summary: 'Continuously verify TLS posture, certificate behavior, and deployment drift across environments.',
    },
    {
      title: 'Migration maturity',
      summary: 'Keep a phased roadmap for broader rollout, vendor alignment, and fallback planning.',
    },
  ],
  sectionFourTitle: 'Recommended next steps',
  sectionFour: [
    'Keep a current crypto inventory and map where hybrid/PQC modes are actually enforced.',
    'Validate critical integrations under real traffic patterns and legacy compatibility constraints.',
    'Treat PQC as a lifecycle capability with periodic reviews, not a one-time change.',
    'Retain cryptographic agility so algorithms can be replaced quickly if guidance changes.',
  ],
  sources: pqcReadMoreNotDetectedMock.sources,
  lastReviewed: '2026-04-28',
}
