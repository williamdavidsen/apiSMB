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
