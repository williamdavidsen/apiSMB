# Security Checklist

- API does not expose secrets or API keys in responses.
- Invalid domains do not trigger unsafe URL construction in frontend code.
- Backend gracefully handles external HTTP failures.
- Error responses avoid leaking stack traces in production.
- Security recommendations are conservative when evidence is missing.
