# Test Environment

## Backend

- Runtime: .NET 10
- API project: `API/SecurityAssessmentAPI.csproj`
- Test framework: xUnit
- Integration host: `Microsoft.AspNetCore.Mvc.Testing`
- Database: Entity Framework InMemory

## Frontend

- Framework: React with Vite
- Unit test framework: Vitest
- Browser tests: Playwright

## External Services

Automated tests use fake clients for:

- Mozilla Observatory
- HTTP header probing
- SSL Labs
- VirusTotal
- DNS analysis

Live calls can be used in manual tests or `AssessmentBatchRunner`, but should not be required for repeatable automated test results.
