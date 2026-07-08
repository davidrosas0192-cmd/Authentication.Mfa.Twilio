# MFA with Twilio - Initial API Design

This document defines the initial MFA flow for this project using Twilio as the OTP provider. For now, only two delivery methods are supported:

- SMS
- Email

The MFA method should be represented as an enum in code, for example:

```csharp
public enum MfaMethod
{
    Sms = 1,
    Email = 2
}
```

The design focuses on the minimum endpoints needed for:

1. Enrolling a MFA method
2. Logging in with MFA
3. Using a short-lived temporary JWT to protect the MFA challenge flow

---

## 1. Goals

- Allow users to enroll an MFA method during account setup or later.
- Require MFA during login when enabled.
- Support only SMS and email delivery.
- Use a temporary JWT token for the MFA challenge steps.
- Apply basic security controls such as expiration, one-time codes, and rate limiting.

---

## 2. Security Model

### Temporary JWT Bearer Token

The temporary token used for MFA should be a JWT bearer token issued for MFA-related steps only. It should:

- Be short-lived (for example, 5 minutes)
- Contain only the minimum claims needed:
  - userId
  - purpose (`enrollment` or `login`)
  - challengeId
  - jti (unique token ID)
  - exp
  - iss and aud if supported
- Be signed with a separate secret or key from the normal access token
- Be validated on every protected MFA endpoint using the standard bearer authentication flow
- Be invalidated after use or after expiration
- Be bound to the intended flow and not accepted for unrelated endpoints
- Not be stored in browser local storage if the API is consumed from a browser; prefer secure cookies or in-memory handling when applicable

### Recommended protections

- HTTPS only
- Rate limiting on send/verify endpoints
- Max attempts per challenge (for example, 5)
- One-time verification codes
- Reuse prevention for the same challenge
- Short code lifetime (for example, 2 to 5 minutes)
- One challenge per user/session at a time
- Audit logging for enrollment and verification attempts
- No OTP leakage in responses, logs, or error messages
- Secure storage of secrets and Twilio credentials using environment variables or a secret manager

### Security concerns to address

- OTP brute force: enforce attempt limits and lockout policies.
- Token replay: prevent a temporary JWT or verification code from being reused after success.
- Token theft: keep the MFA JWT short-lived and avoid storing sensitive claims.
- SMS/email interception: treat these as secondary factors and do not rely on them alone for high-risk operations.
- User enumeration: avoid revealing whether a phone number or email exists during enrollment.
- Account takeover: require the primary authentication step before allowing MFA enrollment or login challenge initiation.
- Log safety: never log full OTPs, raw JWTs, or PII unnecessarily.
- Abuse and spam: throttle resend requests and block repeated abuse from the same IP or user.
- Twilio configuration security: validate webhook signatures and protect Twilio API credentials.
- Session fixation: ensure the login session is rotated after successful MFA verification.
- CSRF and CORS: apply strict CORS policies and CSRF protections for browser-based flows where appropriate.
- Transport security: force TLS and reject insecure HTTP traffic in production.

### Audit implementation for security

The API should implement audit logging following OWASP guidance for authentication and session management. Recommended audit events include:

- user login started
- login challenge issued
- login challenge verified successfully
- login challenge failed due to invalid code
- login challenge failed due to expired token
- MFA enrollment started
- MFA enrollment completed
- MFA enrollment failed
- OTP resend requested
- rate limit triggered
- suspicious repeated failures from the same user or IP
- session invalidated after MFA success or failure

Each audit event should capture:

- timestamp
- userId or anonymous identifier
- event type
- result (success/failure)
- IP address
- user agent
- request path
- reason code when applicable

Audit records should be:

- written asynchronously where possible
- stored separately from application logs
- protected from tampering
- reviewed regularly for suspicious patterns

### OWASP-aligned implementation guidance

- Implement secure authentication and session management controls as described in OWASP ASVS.
- Ensure MFA challenge endpoints require authentication and authorization before they can be used.
- Apply least privilege for all services and secrets.
- Validate all input and reject malformed or unexpected payloads.
- Use secure defaults and fail safely when verification services are unavailable.
- Enforce strict output encoding and avoid returning sensitive details in error responses.

---

## 3. Enrollment Flow

### Endpoint 1: Start MFA Enrollment

Method: POST

Path: /api/mfa/enroll/start

#### Authentication

Requires a valid primary authentication token for the signed-in user.

#### Request

```json
{
  "method": "sms",
  "phoneNumber": "+15551234567"
}
```

or

```json
{
  "method": "email",
  "email": "user@example.com"
}
```

#### Validation rules

- `method` must be `sms` or `email`
- `phoneNumber` is required when `method` is `sms`
- `email` is required when `method` is `email`
- The user must not already have the same method enrolled unless re-enrollment is explicitly allowed

#### Response

```json
{
  "success": true,
  "message": "Verification code sent",
  "tempToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresInSeconds": 300,
  "challengeId": "mfa-challenge-123",
  "method": "sms"
}
```

#### Notes

- Sends a one-time code through Twilio.
- Returns a temporary JWT that will be used in the verification step.

---

### Endpoint 2: Verify MFA Enrollment

Method: POST

Path: /api/mfa/enroll/verify

#### Authentication

Requires the temporary MFA JWT bearer token in the Authorization header:

```http
Authorization: Bearer <temporary-mfa-jwt>
```

#### Request

```json
{
  "code": "482913"
}
```

#### Response

```json
{
  "success": true,
  "message": "MFA enrollment completed",
  "enabledMethods": ["sms"],
  "mfaEnabled": true
}
```

#### Notes

- Validates the code against the pending enrollment challenge.
- Marks the selected method as enrolled for the user.
- Invalidates the temporary token once the verification succeeds.

---

## 4. Login Flow

### Endpoint 3: Login with Primary Credentials

Method: POST

Path: /api/auth/login

#### Request

```json
{
  "username": "jane.doe",
  "password": "StrongPassword123!"
}
```

#### Response when MFA is not enabled

```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "requiresMfa": false
}
```

#### Response when MFA is enabled

```json
{
  "success": true,
  "requiresMfa": true,
  "tempToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresInSeconds": 300,
  "challengeId": "login-challenge-456",
  "method": "email"
}
```

#### Notes

- The server checks the user's password first.
- If MFA is enabled for the user, the login request stops at the challenge step and returns a temporary JWT.
- The `method` indicates whether the next verification step will use SMS or email.

---

### Endpoint 4: Verify MFA Login

Method: POST

Path: /api/mfa/login/verify

#### Authentication

Requires the temporary MFA JWT bearer token in the Authorization header:

```http
Authorization: Bearer <temporary-mfa-jwt>
```

#### Request

```json
{
  "code": "482913"
}
```

#### Response

```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresInSeconds": 3600
}
```

#### Notes

- Validates the temporary JWT and the one-time code.
- If successful, the user is fully authenticated and receives the final access token.

---

## 5. Suggested DTOs

### Enrollment Start Request

```csharp
public class MfaEnrollStartRequest
{
    public string Method { get; set; } = "sms";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
```

### Enrollment Verify Request

```csharp
public class MfaVerifyRequest
{
    public string Code { get; set; } = string.Empty;
}
```

### Login Request

```csharp
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

---

## 6. Implementation Plan

1. Add an MFA enum and DTOs for enrollment and verification requests.
2. Extend the user model to persist:
   - whether MFA is enabled
   - the selected MFA method
   - the delivery target such as phone number or email
3. Create an MFA service that handles:
   - generating OTP codes
   - sending codes through Twilio
   - storing challenge state temporarily
   - validating verification attempts
4. Add endpoints for:
   - start enrollment
   - verify enrollment
   - start login challenge
   - verify login challenge
5. Introduce temporary JWT issuance for MFA challenge flows.
6. Add throttling, attempt limits, and audit logging.
7. Test the full flow for success, failure, expiration, and resend scenarios.

---

## 7. Best Practices for MFA in This API

- Use MFA as a second layer after strong primary authentication.
- Prefer short-lived, single-use OTPs generated server-side.
- Keep the temporary MFA JWT separate from the main access token.
- Use an enum for MFA methods to avoid stringly-typed logic.
- Validate the delivery target before sending a code.
- Enforce rate limiting on all OTP send/verify endpoints.
- Return generic errors for invalid codes so you do not expose whether an account exists or whether a method is configured.
- Invalidate old challenges when a new one is issued.
- Log security-relevant events without logging OTPs or tokens.
- Consider fallback or recovery options, such as backup codes, for users who lose access to their phone or email.
- Keep Twilio secrets in configuration and rotate them regularly.
- Prefer server-side challenge storage over trusting client-provided state.
- Ensure MFA enforcement is consistent across all sensitive endpoints, not only login.

---

## 8. Example Error Responses

```json
{
  "success": false,
  "message": "Invalid or expired verification code"
}
```

```json
{
  "success": false,
  "message": "Too many attempts. Please request a new code"
}
```
