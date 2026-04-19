# E-mail Security Test Cases

## TC-EMAIL-001 No MX Records

- Technique: Decision table testing
- Expected: module is marked not included in final weighted score.

## TC-EMAIL-002 SPF Missing

- Technique: Equivalence partitioning
- Expected: SPF row is missing and score is reduced.

## TC-EMAIL-003 DMARC Enforcement Present

- Technique: Decision table testing
- Expected: DMARC receives positive score when policy is enforced.
