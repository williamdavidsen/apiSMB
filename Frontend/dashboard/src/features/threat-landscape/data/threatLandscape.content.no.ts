import { threatTopics } from '../../../shared/constants/threatKeys'
import type { ThreatLandscapeContent } from '../model/threatLandscape.types'

export const threatLandscapeContent: Record<string, ThreatLandscapeContent> = {
  [threatTopics.phishingSpoofing]: {
    topic: threatTopics.phishingSpoofing,
    pageTitle: 'Phishing & Spoofing',
    intro:
      'Social engineering remains a leading entry point for attackers. Campaigns now scale faster through phishing-as-a-service kits, QR lures, and impersonation of trusted brands.',
    stats: [
      {
        id: 'phishing-dominant-vector',
        title: 'Phishing as initial vector',
        icon: 'mail',
        value: '60%',
        caption: 'Observed cases in ENISA ETL 2025',
        summary: 'Phishing is still the most common initial intrusion method in the observed dataset.',
        source: 'ENISA Threat Landscape 2025',
      },
      {
        id: 'human-element-breaches',
        title: 'Breaches involving people',
        icon: 'users',
        value: '~60%',
        caption: 'Human element in DBIR 2025 breaches',
        summary: 'User interaction remains a key factor in successful compromise scenarios.',
        source: 'Verizon 2025 DBIR',
      },
      {
        id: 'sme-ransomware-impact',
        title: 'SMB ransomware exposure',
        icon: 'building',
        value: '88%',
        caption: 'SMB breaches with ransomware component',
        summary: 'Smaller organizations continue to face disproportionate ransomware pressure.',
        source: 'Verizon 2025 DBIR',
      },
    ],
    findings: {
      title: 'What this means',
      points: [
        'Attackers combine social engineering with low-cost automation to increase campaign reach.',
        'Credential theft, session hijacking, and malware delivery are frequently tied to phishing.',
        'SMBs remain attractive targets due to weaker controls and supply-chain access opportunities.',
      ],
    },
    actions: {
      title: 'Recommended baseline actions',
      points: [
        'Enforce MFA on all business-critical and remote-access accounts.',
        'Run recurring phishing simulations and awareness drills with measurable follow-up.',
        'Harden email controls (SPF, DKIM, DMARC) and strengthen link/attachment filtering.',
      ],
    },
    footerNote:
      'Global perspective based on ENISA and DBIR reports. Figures are included for awareness and prioritization, not direct regulatory reporting.',
  },
  [threatTopics.weakTlsCerts]: {
    topic: threatTopics.weakTlsCerts,
    pageTitle: 'Weak TLS / Certificates',
    intro:
      'Weak encryption posture and delayed remediation increase the chance that known weaknesses are used as practical intrusion paths, especially on internet-facing systems.',
    stats: [
      {
        id: 'vulnerability-initial-access',
        title: 'Exploitation as intrusion vector',
        icon: 'warning',
        value: '21.3%',
        caption: 'Share of intrusion vectors in ENISA ETL 2025',
        summary: 'Exploitation remains a significant route to gain initial access.',
        source: 'ENISA Threat Landscape 2025',
      },
      {
        id: 'edge-vpn-targeting',
        title: 'Edge and VPN targeting',
        icon: 'shield',
        value: '22%',
        caption: 'Target share within exploitation events',
        summary: 'External-facing appliances remain heavily targeted in practical attacks.',
        source: 'Verizon 2025 DBIR',
      },
      {
        id: 'remediation-time',
        title: 'Median remediation time',
        icon: 'time',
        value: '32 days',
        caption: 'Median to fully remediate tracked edge vulnerabilities',
        summary: 'Delays in patch closure extend attacker opportunity windows.',
        source: 'Verizon 2025 DBIR',
      },
    ],
    findings: {
      title: 'What this means',
      points: [
        'Internet-exposed components are consistently scanned and exploited soon after disclosure.',
        'Patch latency and weak cryptographic settings combine into avoidable risk.',
        'Certificate and protocol hygiene is a core control, not just a compliance checkbox.',
      ],
    },
    actions: {
      title: 'Recommended baseline actions',
      points: [
        'Standardize TLS policy (TLS 1.2+ minimum, modern ciphers, no legacy protocol fallback).',
        'Track certificate lifecycle and automate renewals to avoid weak or expired states.',
        'Set strict patch SLAs for internet-facing systems and verify closure with rescans.',
      ],
    },
    footerNote:
      'Figures summarize high-level trends from ENISA and DBIR. Use them to prioritize technical hardening in customer-facing services.',
  },
  [threatTopics.missingHeaders]: {
    topic: threatTopics.missingHeaders,
    pageTitle: 'Missing Security Headers',
    intro:
      'Missing HTTP security headers weaken browser-side protections and increase exposure to common web attack chains, especially when credentials and third-party dependencies are involved.',
    stats: [
      {
        id: 'basic-web-attacks',
        title: 'Basic web application attacks',
        icon: 'web',
        value: '12%',
        caption: 'DBIR 2025 breach pattern share',
        summary: 'Web-layer weaknesses and credential abuse remain frequent in breach patterns.',
        source: 'Verizon 2025 DBIR',
      },
      {
        id: 'third-party-involvement',
        title: 'Third-party involvement',
        icon: 'link',
        value: '30%',
        caption: 'Breaches involving a third party',
        summary: 'Dependency risk can amplify impact when web controls are weak.',
        source: 'Verizon 2025 DBIR',
      },
      {
        id: 'sme-economy-share',
        title: 'SMEs in EU economy',
        icon: 'building',
        value: '99%',
        caption: 'Share of businesses represented by SMEs',
        summary: 'Web hardening guidance needs to remain practical and accessible to SMEs.',
        source: 'ENISA Cybersecurity for SMEs',
      },
    ],
    findings: {
      title: 'What this means',
      points: [
        'Headers are a low-cost control that reduce browser exploitation opportunities.',
        'Credential theft and web abuse are more damaging when response maturity is low.',
        'SME-focused hardening should emphasize simple defaults and repeatable checks.',
      ],
    },
    actions: {
      title: 'Recommended baseline actions',
      points: [
        'Apply and validate core headers: CSP, HSTS, X-Content-Type-Options, Referrer-Policy.',
        'Use secure cookie settings (Secure, HttpOnly, SameSite) consistently across apps.',
        'Integrate header validation into release checks so regressions are caught early.',
      ],
    },
    footerNote:
      'Statistics and guidance are compiled from the provided ENISA and DBIR documents for global awareness-oriented communication.',
  },
}
