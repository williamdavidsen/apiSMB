# HTTP Headers Test Cases

## TC-HEADERS-001 Missing Critical Headers

- Technique: Decision table testing
- Given: no HSTS, no CSP, no X-Frame-Options, no `frame-ancestors`
- Expected: score is low, status is `FAIL`, alerts include critical header warnings.

## TC-HEADERS-002 Strong Header Configuration

- Technique: Decision table testing
- Given: HSTS with one-year max age, CSP without unsafe directives, clickjacking protection
- Expected: score is high and status is `PASS`.

## TC-HEADERS-003 HTTP Only Endpoint

- Technique: Error guessing
- Given: final probe URI uses HTTP
- Expected: result is `FAIL` and warns that target is not served over HTTPS.
