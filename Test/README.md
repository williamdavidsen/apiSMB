# Test

This folder contains a reduced test set for the security assessment application. The project is not test-focused, so the tests are kept as a minimum quality gate around the most important backend decisions, API contract shape, frontend mapping logic, and the main user flow.

## Structure

- `API.UnitTests`: core backend service tests for assessment, SSL/TLS, headers, e-mail security, reputation, and PQC.
- `API.IntegrationTests`: small API smoke tests for assessment endpoints and dashboard contract shape.
- `Frontend.UnitTests`: Vitest tests for domain validation, score/status mapping, and assessment dashboard mapping.
- `E2E`: one Playwright smoke test for the main scan flow.
- `Reports`: short test summary report.

## Commands

From the repository root:

```powershell
dotnet test .\Test\API.UnitTests\API.UnitTests.csproj -m:1
dotnet test .\Test\API.IntegrationTests\API.IntegrationTests.csproj -m:1
```

Frontend unit tests:

```powershell
cd .\Test\Frontend.UnitTests
npm test
```

E2E smoke test:

```powershell
cd .\Test\E2E
npm test
```

Full reduced test set:

```powershell
.\run-tests.ps1
```

or:

```powershell
cd .\Test
npm run test:all
```

## Strategy

The reduced strategy keeps only tests that protect the main product behavior:

- Unit tests for scoring and security module decisions.
- Integration smoke tests for the API endpoints used by the dashboard.
- Frontend unit tests for user input and score/status presentation logic.
- One E2E smoke test for starting a scan and reaching the dashboard.

Coverage reports, visual regression, automated accessibility checks, load tests, resilience scripts, traceability matrices, live domain batch validation, and detailed test design documents were removed to keep the project focused on the application itself.
