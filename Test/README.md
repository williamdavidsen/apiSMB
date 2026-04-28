# Test

This folder is the testing workspace for the security assessment system. It is organized to show the full testing process expected in a software testing course: strategy, risk analysis, test design, automated execution, manual/exploratory testing, and reporting.

## Structure

- `TestPlan`: scope, risk analysis, environment, test data, and traceability.
- `TestDesign`: test cases grouped by module and linked to testing techniques.
- `API.UnitTests`: fast backend tests for services, mapping, and business rules.
- `API.IntegrationTests`: API endpoint tests using an in-memory test server.
- `Frontend.UnitTests`: Vitest tests for frontend utility and mapping logic.
- `E2E`: Playwright tests for user flows through the dashboard, accessibility smoke, visual regression, and an opt-in live full-stack smoke flow.
- `ManualTests`: exploratory and checklist-based manual testing.
- `Reports`: test execution summaries, coverage notes, and defect log.
- `AssessmentBatchRunner`: optional batch evaluation tool for running many domains.
- `NonFunctional`: lightweight load and resilience smoke helpers.

## Recommended Commands

Quick summary:

- `npm run dev` is for running the frontend application, not for running tests.
- Frontend app setup is `cd .\Frontend`, then `npm run setup`, then `npm run dev`.
- Use `npm run test:all` from the repository root to run the main automated test suite.

From the repository root:

```powershell
dotnet test .\Test\API.UnitTests\API.UnitTests.csproj -m:1
dotnet test .\Test\API.IntegrationTests\API.IntegrationTests.csproj -m:1
```

Frontend unit tests:

```powershell
cd .\Test\Frontend.UnitTests
npm install
npm test
```

End-to-end tests:

```powershell
cd .\Test\E2E
npm install
npm test
npm run test:update-snapshots
```

Frontend coverage:

```powershell
cd .\Test\Frontend.UnitTests
npm run test:coverage
```

Run the main automated suite in one step from the repository root:

```powershell
.\run-tests.ps1
```

Or:

```powershell
npm run test:all
```

Live validation helpers:

```powershell
pwsh .\Test\AssessmentBatchRunner\run-live-validation.ps1
cd .\Test\E2E
$env:LIVE_E2E_DOMAIN="example.com"
npm run test:live
```

Non-functional smoke helpers:

```powershell
pwsh .\Test\NonFunctional\load-smoke.ps1
pwsh .\Test\NonFunctional\resilience-smoke.ps1
```

## Test Levels

- Unit tests verify isolated business rules and scoring decisions.
- Integration tests verify API routing, HTTP status codes, and controller/service contracts.
- E2E tests verify user-visible flows from domain input to assessment result.
- Accessibility smoke verifies labelled forms, progress announcements, heading structure, and the absence of serious/critical axe violations on key pages.
- Visual regression tests verify stable baselines for the home page, dashboard, and module detail page.
- Manual tests cover exploratory usability and security review areas that are expensive or brittle to automate.

## Course Alignment

The documentation is written to support test technique explanation, evaluation, and test reporting. Each major test case should be traceable to a requirement, risk, test type, and test design technique.
