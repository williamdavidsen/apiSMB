# Backend API Test Cases

## TC-API-HEADERS-001

- Requirement: REQ-003
- Technique: Use case testing
- Type: Integration
- Input: `POST /api/headers/check` with `{"domain":"example.com"}`
- Expected: HTTP 200 and a `HeadersCheckResult` body.

## TC-API-HEADERS-002

- Requirement: REQ-002
- Technique: Error guessing
- Type: Integration
- Input: empty or invalid request body
- Expected: HTTP 400 or validation error response.

## TC-ASSESS-001

- Requirement: REQ-006
- Technique: Decision table testing
- Type: Unit
- Condition: E-mail module has `ModuleApplicable=true` but `HasMailService=false`
- Expected: E-mail weight is zero and SSL/header/reputation weights are rebalanced.
