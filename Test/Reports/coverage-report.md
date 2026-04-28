# Coverage Report

## Backend

Coverage target:

- Service scoring and weighting logic.
- Controller happy paths and validation paths.
- DTO/entity mapping.

## Frontend

Coverage target:

- Score and grade utilities.
- Status mapping.
- Domain validation behavior.
- Dashboard mapper behavior.

## Notes

Backend Cobertura coverage was generated on 2026-04-28:

- API unit tests project aggregate coverage: line-rate `69.62%`, branch-rate `56.00%`
- API integration tests project aggregate coverage: line-rate `19.05%`, branch-rate `3.40%`
- Combined backend evidence now also includes repository/DB persistence flows, API contract-shape checks, HTTP-client orchestration coverage, and cancellation-oriented service scenarios.

Frontend Istanbul coverage was generated on 2026-04-27 with `npm run test:coverage`:

- Statements: `74.34%` (`623/838`)
- Branches: `66.20%` (`521/787`)
- Functions: `73.68%` (`168/228`)
- Lines: `77.45%` (`553/714`)

The frontend percentages now confirm coverage for validation, dashboard mapping, top-level routing, shared layout behavior, threat-landscape rendering, PQC modal behavior, module-detail logic, scan progress timing/state behavior, PDF-export failure handling, and API request construction. The largest remaining deterministic gaps are still concentrated in carousel-only presentation components, some low-level visual helpers, and the extracted PDF helper module itself.

Current automated execution evidence on 2026-04-28:

- API unit tests: 138 assertions passed across scoring, weighting, provider error handling, DNS/HTTP orchestration, cancellation/error handling, mapping, edge-case module combinations, assessment invariants, controller branch coverage, repository delete branches, HTTP client parsing branches, SSL detail fallback branches, and mapper/PQC branch coverage.
- API integration tests: 49 assertions passed across happy paths, controller error paths, root-route availability, repository persistence flows, JSON contract shape checks, overlong-input validation, route abuse handling, and error-message sanitization.
- Frontend unit tests: 63 assertions passed across score/status utilities, dashboard mapping, routing/layout behavior, threat-landscape rendering, PQC modal behavior, module details, domain validation behavior, scan progress flow, timer/state logic, PDF fallback handling, and security-oriented domain normalization assertions.
- E2E tests: 12 Playwright flows passed, plus 1 opt-in skipped live smoke test, across scan start, dashboard rendering, partial-result messaging, module navigation, retry behavior, accessibility smoke coverage, and visual regression baselines.
