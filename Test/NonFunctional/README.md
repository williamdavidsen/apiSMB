# Non-Functional Smoke Tests

This folder contains lightweight scripts for performance, resilience, and operational smoke testing against a running API.

## Scripts

- `load-smoke.ps1`: fires concurrent assessment requests and reports latency/error-rate metrics.
- `resilience-smoke.ps1`: runs a mixed list of real and fake domains and checks that the API returns handled responses without crashing.

## Quality Gate Intent

- `load-smoke.ps1` is designed as a threshold-based smoke gate, not just a manual probe.
- Default pass criteria: failure rate `<= 5%` and p95 latency `<= 5000 ms`.
- `resilience-smoke.ps1` verifies that invalid, weak, and mixed live-domain inputs still return handled HTTP responses instead of transport-level failures.

## Usage

Start the API first, then run:

```powershell
pwsh .\Test\NonFunctional\load-smoke.ps1
pwsh .\Test\NonFunctional\resilience-smoke.ps1
```
