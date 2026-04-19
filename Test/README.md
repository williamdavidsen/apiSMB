# Test

This folder is the testing workspace for the security assessment system. It is organized to show the full testing process expected in a software testing course: strategy, risk analysis, test design, automated execution, manual/exploratory testing, and reporting.

## Structure

- `TestPlan`: scope, risk analysis, environment, test data, and traceability.
- `TestDesign`: test cases grouped by module and linked to testing techniques.
- `API.UnitTests`: fast backend tests for services, mapping, and business rules.
- `API.IntegrationTests`: API endpoint tests using an in-memory test server.
- `Frontend.UnitTests`: Vitest tests for frontend utility and mapping logic.
- `E2E`: Playwright tests for user flows through the dashboard.
- `ManualTests`: exploratory and checklist-based manual testing.
- `Reports`: test execution summaries, coverage notes, and defect log.
- `AssessmentBatchRunner`: optional batch evaluation tool for running many domains.

## Recommended Commands

From the repository root:

```powershell
dotnet test .\Test\API.UnitTests\API.UnitTests.csproj
dotnet test .\Test\API.IntegrationTests\API.IntegrationTests.csproj
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
```

## Test Levels

- Unit tests verify isolated business rules and scoring decisions.
- Integration tests verify API routing, HTTP status codes, and controller/service contracts.
- E2E tests verify user-visible flows from domain input to assessment result.
- Manual tests cover exploratory usability and security review areas that are expensive or brittle to automate.

## Course Alignment

The documentation is written to support test technique explanation, evaluation, and test reporting. Each major test case should be traceable to a requirement, risk, test type, and test design technique.
