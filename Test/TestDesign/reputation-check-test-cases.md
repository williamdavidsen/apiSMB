# Reputation Test Cases

## TC-REP-001 Clean Domain

- Technique: Equivalence partitioning
- Expected: no malicious or suspicious detections and status is `PASS`.

## TC-REP-002 Malicious Signals

- Technique: Decision table testing
- Expected: malicious detections create high-risk status and critical alert.

## TC-REP-003 Provider Unavailable

- Technique: Error guessing
- Expected: result is marked `ERROR` or partial without crashing the full assessment.
