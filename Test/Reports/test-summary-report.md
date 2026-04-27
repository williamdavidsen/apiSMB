# Test Summary Report

## Scope

Backend API, frontend dashboard logic, and primary scan flow.

## Current Status

Automated backend, frontend, and E2E coverage were re-verified on 2026-04-27, and the test documents were synchronized with the current dashboard behavior.

## Results

| Area | Passed | Failed | Blocked | Notes |
|---|---:|---:|---:|---|
| API unit tests | 46 | 0 | 0 | Re-run on 2026-04-27 with `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore` |
| API integration tests | 23 | 0 | 0 | Re-run on 2026-04-27 with `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore` |
| Frontend unit tests | 17 | 0 | 0 | Re-run on 2026-04-27 with `npm test` in `Test/Frontend.UnitTests` (`vitest run --pool=threads`) |
| E2E tests | 5 | 0 | 0 | Re-run on 2026-04-27 with `npm test -- --reporter=line --workers=1` in `Test/E2E` |

## Evaluation

The test suite prioritizes deterministic tests by using fakes for third-party services. Live provider checks should be documented separately as exploratory or batch validation. The frontend dashboard tests were also updated to reflect the current HTTP Headers card behavior after the Observatory text was removed from the summary card.

## Notes

`dotnet test` should be run per test project. In this environment, solution-level `dotnet test` returned exit code 1 without actionable compiler/test errors, while the individual API test projects passed. The frontend unit test script now uses a thread-based Vitest pool to avoid worker crashes in constrained environments.
