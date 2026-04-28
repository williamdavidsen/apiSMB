# SSL/TLS Test Cases

## Decision Table

| ID | SSL Labs status | Endpoint data | Certificate state | Expected outcome |
|---|---|---|---|---|
| TC-SSL-001 | `READY` | Present | Valid | Score is calculated from SSL Labs evidence |
| TC-SSL-002 | `READY` | Missing | Valid | `FAIL` with "No HTTPS endpoint" alert |
| TC-SSL-003 | `READY` | Present | Expired | `FAIL` with critical alarm |
| TC-SSL-004 | `IN_PROGRESS` then `READY` | Present | Valid | Polling continues until usable result |
| TC-SSL-005 | Error / canceled | No SSL Labs result | Fallback fails | `ERROR` summary remains user-safe |
| TC-SSL-006 | SSL Labs unavailable | Direct TLS works | Valid but lower confidence | `DIRECT_TLS` result with bounded score |

## Boundary Matrix

| ID | Technique | Boundary | Expected |
|---|---|---|---|
| TC-SSL-B01 | Boundary value analysis | Remaining lifetime `< 30` days | Expiry warning branch |
| TC-SSL-B02 | Boundary value analysis | Remaining lifetime `< 7` days | Critical expiry alarm branch |
| TC-SSL-B03 | Boundary value analysis | Long-lived cert `>= 30` days left | Full lifetime score branch |
| TC-SSL-B04 | Boundary value analysis | Short-lived cert `> 50%` life left | Short-lived high-score branch |
| TC-SSL-B05 | Boundary value analysis | TLS 1.3 / 1.2 / 1.1 / older | Distinct protocol score branches |

## Automated Coverage Mapping

- `SslCheckingServiceTests.CheckSslAsync_WithStrongSslLabsData_ReturnsPassAndFullScore`
- `SslCheckingServiceTests.GetSslDetailsAsync_WhenCertificateIsExpired_ReturnsFail`
- `SslCheckingServiceTests.GetSslDetailsAsync_WhenNoEndpointsAreReturned_ReturnsFail`
- `SslCheckingServiceBoundaryTests.GetSslDetailsAsync_WhenSslLabsNeedsRetry_PollsUntilReady`
- `SslCheckingServiceBoundaryTests.GetSslDetailsAsync_WhenCertificateExpiresWithinSevenDays_RaisesCriticalAlarm`
- `SslCheckingServiceBoundaryTests.GetSslDetailsAsync_WhenShortLivedCertificateHasHealthyRemainingPercentage_UsesShortLivedNarrative`
- `SslCheckingServiceBoundaryTests.GetSslDetailsAsync_WhenCancellationOccursBeforeSslLabsCompletes_ReturnsErrorSummary`
