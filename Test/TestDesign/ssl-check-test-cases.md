# SSL/TLS Test Cases

## TC-SSL-001 Certificate Valid

- Technique: Decision table testing
- Expected: valid certificate contributes to score and no critical alarm is produced.

## TC-SSL-002 Certificate Expired

- Technique: Boundary value analysis
- Boundary: expiry date before current date
- Expected: score is reduced and status becomes `FAIL` or critical warning.

## TC-SSL-003 Certificate Near Expiry

- Technique: Boundary value analysis
- Boundary: short remaining lifetime
- Expected: warning alert is produced.
