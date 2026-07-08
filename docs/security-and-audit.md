# Security and Audit Implementation

## Authentication

- Use JWT bearer tokens for the main access token and for temporary MFA tokens.
- Keep the MFA token separate from the main access token.
- Validate MFA tokens on every verification endpoint.

## Audit Logging

Record events such as:
- login.started
- login.failed
- mfa.challenge.issued
- mfa.challenge.failed
- mfa.enrollment.completed
- mfa.enrollment.failed

Include user ID, IP address, user agent, request path, and reason code.

## OWASP Considerations

- Enforce TLS.
- Rate limit OTP sends and verification attempts.
- Rotate session identifiers after successful MFA.
- Avoid logging OTPs or bearer tokens.
- Store secrets outside source control.
