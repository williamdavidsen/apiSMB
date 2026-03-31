# API-basert-vurdering-av-sikkerhetstilstand-for-SMB-kunder

# Security Assessment Platform

This project is an ASP.NET Core-based prototype designed to evaluate domain security through multiple security checks. It analyzes SSL/TLS, HTTP security headers, email security, domain reputation, and post-quantum readiness, then combines the results into a clear and understandable assessment.

## Features
- SSL/TLS and certificate analysis
- HTTP security header checks
- SPF, DKIM, and DMARC evaluation
- Domain reputation analysis
- PQC (Post-Quantum Readiness) information
- Combined security scoring (0–100)
- Letter grade classification (A–F)
- Batch runner support for testing multiple domains

## Purpose
The main goal of this project is to bring together different security signals in one platform and make domain security easier to understand, measure, and compare.

## Technologies
- ASP.NET Core Web API
- MVC-based architecture
- Swagger / OpenAPI
- External security APIs
- HttpClient-based service integrations

## Modules
- SSL/TLS
- HTTP Headers
- Email Security
- Reputation
- Assessment
- PQC Readiness

## Note
This project is an academic prototype developed for evaluation, demonstration, and further improvement. It is not intended as a production-ready commercial security product.