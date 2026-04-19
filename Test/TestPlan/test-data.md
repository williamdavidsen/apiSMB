# Test Data

This file defines the test data used for automated, manual, and batch testing. The data is split into two main groups:

- Real public domains used for normal positive testing.
- Fake/reserved domains used for negative testing, validation, and error handling.

The fake domains deliberately use reserved top-level domains such as `.invalid`, `.test`, and `.example`. This avoids accidentally testing real typo-squatting or phishing-like domains.

## Real Public Domains

These domains are suitable for normal validation, batch testing, and manual comparison of security assessment results. They represent public organizations, public services, technology companies, media, education, and commerce.

| ID | Domain | Category | Purpose |
|---|---|---|---|
| REAL-001 | `oslomet.no` | University | Norwegian higher education domain |
| REAL-002 | `uio.no` | University | Norwegian higher education domain |
| REAL-003 | `ntnu.no` | University | Norwegian higher education domain |
| REAL-004 | `uib.no` | University | Norwegian higher education domain |
| REAL-005 | `nmbu.no` | University | Norwegian higher education domain |
| REAL-006 | `usn.no` | University | Norwegian higher education domain |
| REAL-007 | `uit.no` | University | Norwegian higher education domain |
| REAL-008 | `hvl.no` | University | Norwegian higher education domain |
| REAL-009 | `kristiania.no` | University college | Norwegian education domain |
| REAL-010 | `student.oslomet.no` | University service | Subdomain test case |
| REAL-011 | `vy.no` | Transport | Norwegian public transport/company domain |
| REAL-012 | `ruter.no` | Transport | Norwegian public transport domain |
| REAL-013 | `entur.no` | Transport | Norwegian travel planning service |
| REAL-014 | `avinor.no` | Transport | Norwegian airport operator |
| REAL-015 | `norwegian.no` | Transport | Airline domain |
| REAL-016 | `sas.no` | Transport | Airline domain |
| REAL-017 | `regjeringen.no` | Public sector | Norwegian government domain |
| REAL-018 | `nav.no` | Public sector | Norwegian public service domain |
| REAL-019 | `skatteetaten.no` | Public sector | Norwegian tax authority domain |
| REAL-020 | `politiet.no` | Public sector | Norwegian police domain |
| REAL-021 | `helsenorge.no` | Public sector | Norwegian health service domain |
| REAL-022 | `digdir.no` | Public sector | Norwegian digitalization agency |
| REAL-023 | `altinn.no` | Public sector | Norwegian public reporting service |
| REAL-024 | `brreg.no` | Public sector | Norwegian register service |
| REAL-025 | `nrk.no` | Media | Norwegian public broadcaster |
| REAL-026 | `vg.no` | Media | Norwegian news domain |
| REAL-027 | `aftenposten.no` | Media | Norwegian news domain |
| REAL-028 | `finn.no` | Marketplace | Norwegian marketplace domain |
| REAL-029 | `dnb.no` | Finance | Norwegian banking domain |
| REAL-030 | `sparebank1.no` | Finance | Norwegian banking domain |
| REAL-031 | `telenor.no` | Telecom | Norwegian telecom domain |
| REAL-032 | `telia.no` | Telecom | Norwegian telecom domain |
| REAL-033 | `google.com` | Technology | Global technology domain |
| REAL-034 | `microsoft.com` | Technology | Global technology domain |
| REAL-035 | `apple.com` | Technology | Global technology domain |
| REAL-036 | `amazon.com` | Commerce/cloud | Global commerce and cloud domain |
| REAL-037 | `github.com` | Developer platform | Developer service domain |
| REAL-038 | `gitlab.com` | Developer platform | Developer service domain |
| REAL-039 | `stackoverflow.com` | Developer platform | Developer Q&A domain |
| REAL-040 | `cloudflare.com` | Internet infrastructure | CDN/security provider domain |
| REAL-041 | `mozilla.org` | Technology/non-profit | Browser and web technology domain |
| REAL-042 | `wikipedia.org` | Knowledge base | Public encyclopedia domain |
| REAL-043 | `wikimedia.org` | Knowledge base | Public non-profit domain |
| REAL-044 | `openai.com` | Technology | AI provider domain |
| REAL-045 | `linkedin.com` | Social/business | Professional network domain |
| REAL-046 | `youtube.com` | Media/platform | Video platform domain |
| REAL-047 | `bbc.com` | Media | International news domain |
| REAL-048 | `cnn.com` | Media | International news domain |
| REAL-049 | `reuters.com` | Media | International news domain |
| REAL-050 | `nasa.gov` | Public sector/science | US public science domain |

## Fake / Reserved Domains

These domains are intended for negative testing. They should not resolve to real production services. Use them for invalid input, unreachable domain handling, validation behavior, and error handling tests.

| ID | Domain | Category | Purpose |
|---|---|---|---|
| FAKE-001 | `empty.invalid` | Reserved invalid | Non-existing domain test |
| FAKE-002 | `missing.invalid` | Reserved invalid | Non-existing domain test |
| FAKE-003 | `no-such-domain.invalid` | Reserved invalid | DNS failure test |
| FAKE-004 | `security-check.invalid` | Reserved invalid | Generic fake domain |
| FAKE-005 | `headers-missing.invalid` | Reserved invalid | Fake missing headers profile |
| FAKE-006 | `ssl-expired.invalid` | Reserved invalid | Fake SSL error profile |
| FAKE-007 | `ssl-self-signed.invalid` | Reserved invalid | Fake SSL error profile |
| FAKE-008 | `mail-missing.invalid` | Reserved invalid | Fake e-mail module negative case |
| FAKE-009 | `mx-missing.invalid` | Reserved invalid | Fake MX lookup negative case |
| FAKE-010 | `reputation-unknown.invalid` | Reserved invalid | Fake reputation lookup case |
| FAKE-011 | `provider-timeout.invalid` | Reserved invalid | Fake external provider timeout |
| FAKE-012 | `provider-error.invalid` | Reserved invalid | Fake external provider error |
| FAKE-013 | `http-only.invalid` | Reserved invalid | Fake HTTP-only case |
| FAKE-014 | `redirect-loop.invalid` | Reserved invalid | Fake redirect loop case |
| FAKE-015 | `bad-certificate.invalid` | Reserved invalid | Fake certificate failure |
| FAKE-016 | `weak-cipher.invalid` | Reserved invalid | Fake weak TLS case |
| FAKE-017 | `old-tls.invalid` | Reserved invalid | Fake TLS version case |
| FAKE-018 | `no-csp.invalid` | Reserved invalid | Fake CSP missing case |
| FAKE-019 | `no-hsts.invalid` | Reserved invalid | Fake HSTS missing case |
| FAKE-020 | `no-dmarc.invalid` | Reserved invalid | Fake DMARC missing case |
| FAKE-021 | `no-spf.invalid` | Reserved invalid | Fake SPF missing case |
| FAKE-022 | `no-dkim.invalid` | Reserved invalid | Fake DKIM missing case |
| FAKE-023 | `malware-signal.invalid` | Reserved invalid | Fake malicious reputation case |
| FAKE-024 | `suspicious-signal.invalid` | Reserved invalid | Fake suspicious reputation case |
| FAKE-025 | `pqc-unknown.invalid` | Reserved invalid | Fake PQC unknown case |
| FAKE-026 | `pqc-detected.invalid` | Reserved invalid | Fake PQC detected case |
| FAKE-027 | `example-company.test` | Reserved test | Generic fake company domain |
| FAKE-028 | `demo-customer.test` | Reserved test | Demo customer domain |
| FAKE-029 | `smb-customer.test` | Reserved test | SMB-style fake domain |
| FAKE-030 | `internal-portal.test` | Reserved test | Fake internal portal |
| FAKE-031 | `customer-login.test` | Reserved test | Fake login domain |
| FAKE-032 | `mail-gateway.test` | Reserved test | Fake mail gateway |
| FAKE-033 | `legacy-server.test` | Reserved test | Fake legacy server |
| FAKE-034 | `staging-system.test` | Reserved test | Fake staging system |
| FAKE-035 | `dev-environment.test` | Reserved test | Fake development system |
| FAKE-036 | `api-service.test` | Reserved test | Fake API domain |
| FAKE-037 | `broken-api.test` | Reserved test | Fake API failure |
| FAKE-038 | `slow-response.test` | Reserved test | Fake slow response |
| FAKE-039 | `dns-timeout.test` | Reserved test | Fake DNS timeout |
| FAKE-040 | `unknown-host.test` | Reserved test | Fake unknown host |
| FAKE-041 | `domain-without-email.example` | Reserved example | Fake no-mail domain |
| FAKE-042 | `domain-with-email.example` | Reserved example | Fake mail-enabled profile |
| FAKE-043 | `strong-security.example` | Reserved example | Fake strong profile |
| FAKE-044 | `weak-security.example` | Reserved example | Fake weak profile |
| FAKE-045 | `partial-result.example` | Reserved example | Fake partial result |
| FAKE-046 | `all-modules-pass.example` | Reserved example | Fake all-pass profile |
| FAKE-047 | `all-modules-fail.example` | Reserved example | Fake all-fail profile |
| FAKE-048 | `headers-warning.example` | Reserved example | Fake warning profile |
| FAKE-049 | `email-warning.example` | Reserved example | Fake e-mail warning profile |
| FAKE-050 | `reputation-warning.example` | Reserved example | Fake reputation warning profile |

## Invalid Input Examples

These values are not valid frontend domain inputs and should be used to verify validation behavior.

| Value | Purpose |
|---|---|
| Empty string | Required field validation |
| `example` | Missing top-level domain |
| `http://example.com` | Frontend should reject protocol input |
| `https://example.com` | Frontend should reject protocol input |
| `example..com` | Invalid dot placement |
| `.example.com` | Invalid leading dot |
| `example.com/scan` | Path should not be accepted as domain input |
| `user@example.com` | E-mail address should not be accepted as domain input |
| `exa mple.com` | Space inside domain |
| `example!.com` | Invalid character |

## Batch Files

| File | Purpose |
|---|---|
| `Test/AssessmentBatchRunner/domains.txt` | Normal public-domain batch validation |
| `Test/AssessmentBatchRunner/weak-domains.txt` | Optional weak/fake domain validation |

## Fake Security Profiles

| Profile | Description |
|---|---|
| StrongHeaders | HSTS, CSP, and clickjacking protection present |
| MissingHeaders | HSTS and CSP missing |
| HttpOnly | Final URI remains HTTP |
| ProviderUnavailable | Third-party benchmark returns no data |
| ExpiredCertificate | Certificate validity has expired |
| NoMailService | Domain has no MX records and e-mail module should be excluded |
| ReputationWarning | Reputation provider returns suspicious but not malicious signals |
| ReputationFail | Reputation provider returns malicious signals |
| PqcUnknown | No PQC signal is detected and readiness is unknown |
| PqcDetected | PQC-related evidence exists in the fake profile |
