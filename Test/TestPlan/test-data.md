# Test Data

| Dataset | Purpose |
|---|---|
| `example.com` | Generic valid domain |
| `firma.no` | Norwegian-style valid domain |
| `sub.example.com` | Valid subdomain |
| Empty string | Invalid input |
| `http://example.com` | Invalid frontend input, valid backend normalization case |
| `example` | Invalid domain |
| `weak-domains.txt` | Optional batch validation of weak real-world examples |
| `domains.txt` | Optional batch validation of normal domains |

## Fake Security Profiles

| Profile | Description |
|---|---|
| StrongHeaders | HSTS, CSP, and clickjacking protection present |
| MissingHeaders | HSTS and CSP missing |
| HttpOnly | Final URI remains HTTP |
| ProviderUnavailable | Third-party benchmark returns no data |
