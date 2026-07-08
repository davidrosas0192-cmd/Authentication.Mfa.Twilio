# MFA API Endpoints

## Authentication

### POST /api/auth/login

Request body:

```json
{
  "username": "demo",
  "password": "Password123!"
}
```

Response when MFA is disabled:

```json
{
  "success": true,
  "requiresMfa": false,
  "accessToken": "...",
  "refreshToken": "..."
}
```

Response when MFA is enabled:

```json
{
  "success": true,
  "requiresMfa": true,
  "tempToken": "...",
  "expiresInSeconds": 300,
  "challengeId": "...",
  "method": "sms"
}
```

## MFA Enrollment

### POST /api/mfa/enroll/start

Headers:

```http
Authorization: Bearer <access-token>
```

Body:

```json
{
  "method": "sms",
  "phoneNumber": "+15551234567"
}
```

### POST /api/mfa/enroll/verify

Headers:

```http
Authorization: Bearer <temporary-mfa-jwt>
```

Body:

```json
{
  "code": "482913"
}
```

## MFA Login Verification

### POST /api/mfa/login/verify

Headers:

```http
Authorization: Bearer <temporary-mfa-jwt>
```

Body:

```json
{
  "code": "482913"
}
```
