# E-mail Security Test Cases

## Decision Table

| ID | MX | SPF | DKIM | DMARC | Expected module status | Expected focus |
|---|---|---|---|---|---|---|
| TC-EMAIL-001 | Lookup fails | N/A | N/A | N/A | `ERROR` | DNS failure must not be misreported as "no mail" |
| TC-EMAIL-002 | No records | N/A | N/A | N/A | `NOT_APPLICABLE` | Module is excluded from weighted score |
| TC-EMAIL-003 | Present | Missing | Missing | Missing | `WARNING` / low score | Conservative scoring and critical missing-policy alerts |
| TC-EMAIL-004 | Present | `-all` | Found | `p=reject` | `PASS` | Strict SPF, verified DKIM, enforced DMARC |
| TC-EMAIL-005 | Present | `~all` | Found | `p=quarantine` | `PASS` or high `WARNING` | Soft-fail SPF and moderate DMARC enforcement |
| TC-EMAIL-006 | Present | Redirected SPF | Missing | `p=reject` | `PASS` | Redirect target must drive effective SPF score |
| TC-EMAIL-007 | Present | Present | Missing | `p=none` | `WARNING` | Weak DMARC enforcement must remain visible |

## Boundary Matrix

| ID | Technique | Boundary | Expected |
|---|---|---|---|
| TC-EMAIL-B01 | Boundary value analysis | DMARC `pct=100` with `p=reject` | Full reject-score branch |
| TC-EMAIL-B02 | Boundary value analysis | DMARC `pct<100` with `p=reject` | Partial reject-score branch |
| TC-EMAIL-B03 | Boundary value analysis | DMARC `pct=100` with `p=quarantine` | Full quarantine-score branch |
| TC-EMAIL-B04 | Boundary value analysis | DMARC `pct<100` with `p=quarantine` | Partial quarantine-score branch |
| TC-EMAIL-B05 | Boundary value analysis | SPF `-all`, `~all`, weaker catch-all | Distinct strict / soft / weak score mapping |

## Automated Coverage Mapping

- `EmailCheckingServiceTests.CheckEmailAsync_WhenMxLookupFails_ReturnsErrorInsteadOfNoMail`
- `EmailCheckingServiceTests.CheckEmailAsync_WhenNoMxRecordsExist_MarksModuleNotApplicable`
- `EmailCheckingServiceTests.CheckEmailAsync_DmarcDecisionTable_AssignsExpectedScoreAndNarrative`
- `EmailCheckingServiceTests.CheckEmailAsync_SpfBoundaryCases_AssignExpectedScore`
- `EmailCheckingServiceTests.CheckEmailAsync_WhenSpfUsesRedirect_FollowsRedirectAndScoresEffectivePolicy`
- `EmailCheckingServiceTests.CheckEmailAsync_WithRealDnsClientParsing_ProducesPassForStrictPolicyBundle`
