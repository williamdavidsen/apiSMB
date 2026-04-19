# Traceability Matrix

| Requirement | Risk | Test Case | Test Type | Technique | Automated |
|---|---|---|---|---|---|
| REQ-001 User can submit a valid domain | R-003 | TC-FE-001 | Frontend unit | Equivalence partitioning | Yes |
| REQ-002 Invalid domains are rejected | R-003 | TC-FE-002 | Frontend unit | Equivalence partitioning | Yes |
| REQ-003 API returns header assessment | R-002 | TC-API-HEADERS-001 | Integration | Use case testing | Yes |
| REQ-004 Missing headers lower score | R-002 | TC-HEADERS-001 | Unit | Decision table | Yes |
| REQ-005 Secure headers receive pass result | R-002 | TC-HEADERS-002 | Unit | Decision table | Yes |
| REQ-006 Final score reweights when e-mail is not applicable | R-005 | TC-ASSESS-001 | Unit | Decision table | Yes |
| REQ-007 Grades follow score thresholds | R-006 | TC-FE-SCORE-001 | Frontend unit | Boundary value analysis | Yes |
| REQ-008 User can complete scan flow | R-007 | TC-E2E-001 | E2E | Use case testing | Yes |
