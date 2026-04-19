# Test Strategy

## Objective

Verify that the security assessment application gives reliable, understandable, and repeatable results for SMB domain security checks.

## Scope

In scope:

- Backend API controllers and service orchestration.
- Scoring logic for SSL/TLS, HTTP headers, e-mail security, reputation, and PQC readiness.
- Frontend domain validation, dashboard mapping, navigation, and user flows.
- Test documentation, execution logs, and defect reporting.

Out of scope for automated tests:

- Real third-party availability guarantees for SSL Labs, Mozilla Observatory, VirusTotal, and public DNS providers.
- Full penetration testing of external domains.

## Test Levels

| Level | Purpose | Tooling |
|---|---|---|
| Unit | Verify isolated logic and boundary decisions | xUnit, Vitest |
| Integration | Verify API endpoints and dependency wiring | xUnit, WebApplicationFactory |
| E2E | Verify browser-level user flows | Playwright |
| Manual | Explore usability, security explanations, and edge cases | Checklists and charters |

## Test Techniques

- Equivalence partitioning for valid and invalid domain input.
- Boundary value analysis for score thresholds and grades.
- Decision table testing for status and alert rules.
- State transition testing for scan progress and result navigation.
- Use case testing for the complete assessment flow.
- Error guessing for third-party API failures, missing DNS records, and empty responses.

## Test Data Policy

Tests should prefer deterministic fake data. Live third-party calls are reserved for manual or batch validation and must not be required for CI.

## Exit Criteria

- All automated backend tests pass.
- All automated frontend tests pass.
- E2E smoke flow passes locally.
- Defect log has no open critical defects.
- Test summary report is updated with executed tests and known limitations.
