# Frontend Flow Test Cases

## TC-FE-001

- Requirement: REQ-001
- Technique: Equivalence partitioning
- Input class: valid domain
- Examples: `example.com`, `firma.no`, `sub.example.com`
- Expected: Domain is accepted and scan navigation starts.

## TC-FE-002

- Requirement: REQ-002
- Technique: Equivalence partitioning
- Input class: invalid domain
- Examples: empty input, `http://example.com`, `example`
- Expected: Validation message is shown and scan is not started.

## TC-E2E-001

- Requirement: REQ-008
- Technique: Use case testing
- Steps:
  1. Open home page.
  2. Enter a valid domain.
  3. Start scan.
  4. Wait for dashboard result.
  5. Open one module detail page.
- Expected: The user can see assessment summary and module detail information.
