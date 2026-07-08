# MFA Implementation Plan

## Overview

This document outlines the implementation plan for adding MFA with Twilio OTP to the API using EF Core, JWT bearer tokens, and a clean service-oriented architecture.

## Goals

- Add login with username and password.
- Support MFA enrollment with SMS and email.
- Use Twilio as the OTP provider.
- Protect MFA verification endpoints with a temporary JWT bearer token.
- Implement audit logging and OWASP-aligned security controls.

## Architecture

- Controllers: API endpoints.
- DTOs: request/response contracts.
- Services: auth, MFA, token, audit, and Twilio integration.
- EF Core entities: users, MFA transactions, refresh tokens, devices, audit logs.
- Repository pattern: persistence should be abstracted behind repository interfaces where appropriate so services depend on contracts rather than EF Core directly.
- SQLite for local development.

## Implementation Steps

1. Define EF Core entities and DB context.
2. Implement authentication and token services.
3. Add MFA service for challenge creation and verification.
4. Add controller endpoints for login and MFA verification.
5. Add audit logging and throttling hooks.
6. Add documentation and test scenarios.

## Notes

- Use environment variables or a secret store for secrets.
- Keep OTPs short-lived and single-use.
- Ensure temporary JWTs are separate from the main access token.
