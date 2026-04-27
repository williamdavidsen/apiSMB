# Assessment API Test Cases

## TC-ASSESS-001 Weighted Score With E-mail

- Technique: Decision table testing
- Expected: SSL, headers, e-mail, and reputation weights sum to 100.

## TC-ASSESS-002 Weighted Score Without E-mail

- Technique: Decision table testing
- Expected: e-mail weight is zero and remaining modules sum to 100.

## TC-ASSESS-003 Critical SSL Failure

- Technique: Decision table testing
- Expected: final assessment status is `FAIL` when SSL has a zero-score failure.

## TC-ASSESS-004 Reputation Module Excluded On Provider Error

- Technique: Decision table testing
- Expected: reputation weight is redistributed across the remaining included modules and the result becomes `PARTIAL`.

## TC-ASSESS-005 Score And Grade Threshold Boundaries

- Technique: Boundary value analysis
- Expected: scores at 90, 80, 79, 60, 50, and 49 map to the correct grade and overall status.

## TC-ASSESS-006 Domain Normalization

- Technique: Equivalence partitioning
- Expected: URL and e-mail style inputs are normalized to the registrable host/domain in the response payload.

## TC-ASSESS-007 Executive Alert Summary For Critical TLS Findings

- Technique: Decision table testing
- Expected: SSL results that contain a `CRITICAL_ALARM` produce a high-priority executive alert in the final assessment summary.
