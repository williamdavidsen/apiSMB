# Risk Analysis

| ID | Risk | Impact | Likelihood | Test Response |
|---|---|---:|---:|---|
| R-001 | SSL/TLS result is incorrect and gives a false sense of security | High | Medium | Unit tests for scoring, integration tests for controller responses |
| R-002 | Missing security headers are not detected | High | Medium | Decision table tests for HSTS, CSP, and clickjacking protection |
| R-003 | Invalid domains are accepted by the frontend | Medium | High | Equivalence partitioning tests for domain input |
| R-004 | External provider timeout breaks the full assessment | High | Medium | Error handling tests with fake failing clients |
| R-005 | E-mail module unfairly lowers score for domains without MX records | Medium | Medium | Unit tests for weighted score recalculation |
| R-006 | Dashboard presents misleading grade or status | High | Medium | Boundary tests for grade and status mapping |
| R-007 | E2E flow breaks during navigation between scan and dashboard | Medium | Medium | Playwright smoke tests |
| R-008 | PQC readiness is overstated from weak evidence | Medium | Low | Manual review and service-level tests for confidence fields |

## Priority

The highest priority is correctness of security-relevant scoring and user-facing risk communication. Tests should isolate third-party instability from application logic.
