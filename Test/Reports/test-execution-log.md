# Test Execution Log

| Date | Tester | Command / Activity | Result | Notes |
|---|---|---|---|---|
| 2026-04-19 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore` | Passed | 7 passed |
| 2026-04-19 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore` | Passed | 2 passed |
| 2026-04-19 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 8 passed |
| 2026-04-19 | Codex | `npm test -- --reporter=line --workers=1` in `Test/E2E` | Passed | 2 passed |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore` | Passed | 46 passed |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore` | Passed | 23 passed |
| 2026-04-27 | Codex | `npx vitest run src/assessment/assessment.mappers.test.ts` in `Test/Frontend.UnitTests` | Passed | 6 passed after updating the HTTP Headers card expectation |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 17 passed with `vitest run --pool=threads` |
| 2026-04-27 | Codex | `npm test -- --reporter=line --workers=1` in `Test/E2E` | Passed | 5 passed |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` | Passed | 46 passed after disabling shared compilation for test stability |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1` | Passed | 23 passed after disabling shared compilation for test stability |
| 2026-04-27 | Codex | `npm test -- src/shared/domain.test.ts src/pages/HomePage.test.tsx` in `Test/Frontend.UnitTests` | Passed | 6 passed for domain normalization and invalid input validation |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 23 passed with `vitest.config.mjs`, `maxWorkers: 1`, and heap cap |
| 2026-04-27 | Codex | `npm test -- --reporter=line --workers=1` in `Test/E2E` | Passed | 5 passed with `playwright.config.mjs` and heap-capped Vite startup |
| 2026-04-27 | Codex | `npm test -- src/pages/ScanProgressPage.test.tsx src/assessment/assessment.api.test.ts` in `Test/Frontend.UnitTests` | Passed | 6 passed for scan progress navigation and assessment API request behavior |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 29 passed after adding scan progress and API service coverage |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` | Passed | 48 passed after adding edge-case module combination coverage |
| 2026-04-27 | Codex | `npm run test:update-snapshots -- --reporter=line --workers=1` in `Test/E2E` | Passed | 9 passed and 1 skipped after adding accessibility smoke, visual regression baseline, and opt-in live full-stack smoke coverage |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage"` | Passed | Cobertura XML generated for backend unit coverage |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage"` | Passed | Cobertura XML generated for backend integration coverage |
| 2026-04-27 | Codex | `npm run test:coverage` in `Test/Frontend.UnitTests` | Passed | Frontend Istanbul coverage generated: statements 51.98%, branches 46.58%, functions 45.13%, lines 54.22% |
| 2026-04-27 | Codex | `npm run test:update-snapshots -- --reporter=line --workers=1` in `Test/E2E` | Passed | 12 passed and 1 skipped after resolving remaining accessibility color-contrast issues and refreshing snapshots |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` | Passed | 48 passed in the final verification run |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1` | Passed | 31 passed in the final verification run |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 29 passed in the final verification run |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 41 passed after adding router, top bar, threat landscape, and module detail coverage |
| 2026-04-27 | Codex | `npm run test:coverage` in `Test/Frontend.UnitTests` | Passed | Frontend Istanbul coverage improved to statements 64.46%, branches 60.23%, functions 62.38%, lines 67.6% |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 45 passed after adding home container, app provider, and PQC modal coverage |
| 2026-04-27 | Codex | `npm run test:coverage` in `Test/Frontend.UnitTests` | Passed | Frontend Istanbul coverage improved to statements 65.78%, branches 60.87%, functions 65.48%, lines 69.15% |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` | Passed | 65 passed after adding e-mail decision-table coverage, SSL boundary/cancellation checks, DNS cancellation coverage, and real `HttpClient` orchestration tests |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1` | Passed | 36 passed after adding repository persistence and API contract-shape integration checks |
| 2026-04-27 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 62 passed after adding scan timer/state tests and module-detail PDF/fallback coverage |
| 2026-04-27 | Codex | `npm run test:coverage` in `Test/Frontend.UnitTests` | Passed | Frontend Istanbul coverage improved to statements 74.34%, branches 66.20%, functions 73.68%, lines 77.45% |
| 2026-04-27 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage"` | Passed | Backend unit Cobertura updated to line-rate 54.12%, branch-rate 45.25% |
| 2026-04-27 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage"` | Passed | Backend integration Cobertura updated to line-rate 19.05%, branch-rate 3.40% |
| 2026-04-27 | Codex | `npm run test:update-snapshots -- --reporter=line --workers=1` in `Test/E2E` | Passed | 12 passed and 1 skipped after refreshing the module-detail visual regression baseline for the updated ASCII-safe copy |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1` | Passed | 71 passed after adding assessment invariant coverage |
| 2026-04-28 | Codex | `dotnet test Test/API.IntegrationTests/API.IntegrationTests.csproj --no-restore -m:1` | Passed | 49 passed after adding request-validation and route-abuse handling checks |
| 2026-04-28 | Codex | `npm test` in `Test/Frontend.UnitTests` | Passed | 63 passed after adding security-oriented domain normalization assertions |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --filter "FullyQualifiedName~DtoMapperTests|FullyQualifiedName~PqcCheckingServiceTests"` | Passed | 17 passed after expanding mapper null/default branches and PQC classification/polling coverage |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage" --results-directory Test/artifacts/coverage-unit-branch` | Passed | Backend unit Cobertura updated to line-rate 57.30%, branch-rate 49.45% |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --filter "FullyQualifiedName~ControllerBranchCoverageTests"` | Passed | 36 passed after adding controller invalid/success/error branch tests |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage" --results-directory Test/artifacts/coverage-unit-branch-2` | Passed | Backend unit Cobertura updated to line-rate 63.71%, branch-rate 51.63% |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage" --results-directory Test/artifacts/coverage-unit-branch-3` | Passed | Backend unit Cobertura updated to line-rate 64.53%, branch-rate 53.45% after expanding Mozilla Observatory and VirusTotal client branches |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage" --results-directory Test/artifacts/coverage-unit-branch-4` | Passed | Backend unit Cobertura updated to line-rate 68.04%, branch-rate 54.45% after adding repository delete branch coverage |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --filter "FullyQualifiedName~SslCheckingServiceTests"` | Passed | 6 SSL detail branch-focused tests passed |
| 2026-04-28 | Codex | `dotnet test Test/API.UnitTests/API.UnitTests.csproj --no-restore -m:1 --collect:"XPlat Code Coverage" --results-directory Test/artifacts/coverage-unit-branch-5` | Passed | Backend unit Cobertura updated to line-rate 69.62%, branch-rate 56.00% |
