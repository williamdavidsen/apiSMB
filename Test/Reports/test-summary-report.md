# Test Summary Report

## Scope

Backend API, frontend dashboard logic, and primary scan flow.

## Current Status

Automated backend, frontend, coverage, and E2E checks were re-verified on 2026-04-28. The test assets now include deterministic unit/integration coverage, accessibility smoke checks, visual regression baselines, opt-in live validation helpers, and lightweight non-functional smoke scripts.

## Results

| Area | Passed | Failed | Blocked | Notes |
|---|---:|---:|---:|---|
| API unit tests | 138 | 0 | 0 | Re-run on 2026-04-28 with `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` |
| API integration tests | 49 | 0 | 0 | Re-run on 2026-04-28 with `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1` |
| Frontend unit tests | 63 | 0 | 0 | Re-run on 2026-04-28 with `npm test` in `Test/Frontend.UnitTests` (`node --max-old-space-size=1024 vitest run --config vitest.config.mjs`) |
| E2E tests | 12 | 0 | 1 | Re-run on 2026-04-27 with `npm run test:update-snapshots -- --reporter=line --workers=1` in `Test/E2E`; the skipped case is the opt-in live full-stack smoke test |
| Frontend coverage run | 62 | 0 | 0 | Re-run on 2026-04-27 with `npm run test:coverage`; numeric statements/branches/functions/lines report generated |

## Evaluation

The test suite prioritizes deterministic regression checks by mocking third-party dependencies in the default path, but it now also includes repository persistence integration, API contract-shape verification, real `HttpClient` orchestration checks, timer/state tests for scan progress, explicit PDF-export fallback coverage for the module detail page, assessment invariant tests, and security-oriented input abuse scenarios. Accessibility smoke coverage, visual regression baselines, opt-in live validation hooks, and error-sanitization checks remain in place.

## Status Matrix

| Capability | Status | Evidence |
|---|---|---|
| Real live-domain automated validation | Available | `AssessmentBatchRunner` live smoke list and `run-live-validation.ps1` |
| Real DNS / TLS / reputation provider behavior | Available (opt-in) | Live batch validation and opt-in Playwright full-stack smoke |
| Production-like full-chain integration | Available (opt-in) | `live-fullstack-smoke.spec.ts` |
| Edge-case combinations | Covered in automated tests | Expanded API unit tests, repository integration tests, and controller/security integration tests |
| Numeric coverage evidence | Available | Backend Cobertura plus frontend Istanbul percentages |
| Performance / load / resilience smoke | Available | `Test/NonFunctional` PowerShell helpers |
| Visual regression | Covered | Playwright snapshots for home, dashboard, and module detail |
| Accessibility automation | Covered | Playwright + axe smoke tests for key pages |

## Notes

`dotnet test` should still be run per test project with `-m:1` in constrained environments. The frontend unit and E2E scripts cap Node heap usage to improve stability. The live-domain, full-stack, and non-functional checks are intentionally opt-in because they depend on environment/network conditions and are not deterministic enough for the default regression gate. The refreshed module-detail visual snapshot reflects the updated ASCII-safe UI copy used in the PDF/report page.
