# Security Assessment API for SMB Customers

This project is an ASP.NET Core API for assessing the security posture of a domain. It is built as a bachelor project and is designed to give small and medium-sized business customers a simple, structured security overview based on several technical checks.

## What the project does

The API evaluates a target domain and produces a combined assessment score with grades, statuses, module-level scoring, and alerts.

The current assessment includes:

- SSL/TLS analysis
- HTTP security header analysis
- Email security analysis
- Reputation analysis
- PQC readiness analysis

## Main API modules

The API exposes dedicated endpoints for each module and one combined assessment endpoint:

- `/api/assessment/check/{domain}`
- `/api/ssl/check/{domain}`
- `/api/headers/check/{domain}`
- `/api/email/check/{domain}`
- `/api/reputation/check/{domain}`
- `/api/pqc/check/{domain}`

## Project structure

- `API/Controllers/Api` contains the REST API controllers
- `API/Services` contains the core assessment logic and external service clients
- `API/DTOs` contains request and response models
- `API/DAL` contains data access and repository code
- `Test/AssessmentBatchRunner` contains a small batch runner for testing many domains
- `Frontend/dashboard` contains the optional dashboard UI (React, TypeScript, Vite, Material UI)

## Running the project

Quick summary:

- Start the API from `API`.
- Run `npm run setup` once in `Frontend`, then start it with `npm run dev`.
- Run all automated tests from the repository root with `npm run test:all`.

From the `API` folder:

```powershell
dotnet run --project .\SecurityAssessmentAPI.csproj --launch-profile http
```

Swagger UI:

```text
http://localhost:1071/swagger
```

OpenAPI JSON:

```text
http://localhost:1071/swagger/v1/swagger.json
```

From the `Frontend` folder, install the dashboard dependencies once and then start the frontend:

```powershell
cd .\Frontend
npm run setup
npm run dev
```

This starts the frontend dev server, usually on `http://localhost:5173`.

With the API running, the frontend proxies `/api` requests to `http://localhost:1071` in dev. You can override the target with `VITE_DEV_API_PROXY` in `Frontend/dashboard/.env.development`.

If you want to run the dashboard directly from its own folder instead:

```powershell
cd .\Frontend\dashboard
npm install
npm run dev
```

Run the main automated test suite from the repository root:

```powershell
.\run-tests.ps1
```

Or use the npm shortcut from the repository root:

```powershell
npm run test:all
```

## Notes

- The project depends on external services and network-based checks.
- Some modules use third-party APIs and HTTP/DNS lookups.
- Output quality depends on network availability and the quality of upstream data sources.

## Purpose

The goal of the project is to provide a practical API-based security assessment workflow that can be used as a basis for customer-facing evaluation, experimentation, and further development.
