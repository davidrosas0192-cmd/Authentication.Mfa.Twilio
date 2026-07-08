# Authentication MFA Twilio API

A sample ASP.NET Core Web API that demonstrates username/password authentication with optional multi-factor authentication (MFA) using Twilio Verify OTP delivery. The implementation supports SMS and email as the allowed second factors and uses short-lived JWT bearer tokens for MFA verification flows.

## Features

- Username/password login
- Optional MFA enrollment for users
- MFA challenge delivery through SMS or email
- Temporary MFA JWTs for enrollment and login verification
- Audit logging for authentication and MFA events
- EF Core persistence with SQLite for local development
- Result-based service responses for cleaner error handling

## Technology Stack

- ASP.NET Core
- Entity Framework Core with SQLite
- JWT bearer authentication
- Twilio Verify OTP delivery
- xUnit for unit tests

## Project Structure

- Controllers: API endpoints for authentication and MFA
- Services: authentication, MFA, token, audit, and Twilio integration
- Data: EF Core context and entities
- DTOs: request and response models
- Repository pattern: persistence abstractions can be introduced around EF Core to keep services decoupled from data access details
- docs: implementation and security documentation
- tests: unit tests for core service behavior

## Getting Started

### Prerequisites

- .NET 10 SDK
- A local terminal or IDE such as Visual Studio Code

### Run the API

1. Restore dependencies:

   ```bash
   dotnet restore
   ```

2. Run the application:

   ```bash
   dotnet run
   ```

3. The API will start on the default ASP.NET Core port and create/update the SQLite database automatically.

### Demo Account

A development user is seeded automatically when the database is first created:

- Username: `demo`
- Password: `Password123!`

MFA is disabled for this demo account by default.

## Configuration

The app reads configuration from appsettings.json and environment variables. Key settings include:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth-mfa.db"
  },
  "JwtOptions": {
    "SecretKey": "...",
    "ExpirationMinutes": 60
  },
  "MfaTokenOptions": {
    "SecretKey": "...",
    "ExpirationMinutes": 5
  },
  "Twilio": {
    "Enabled": false,
    "AccountSid": "",
    "AuthToken": "",
    "VerifyServiceSid": ""
  }
}
```

For production, store secrets in a secure secret manager and enable HTTPS.

## API Endpoints

### Authentication

- POST `/api/auth/login`
  - Authenticates a user and returns an MFA challenge when MFA is enabled.

### MFA Enrollment

- POST `/api/mfa/enroll/start`
  - Starts MFA enrollment and sends an OTP challenge.
- POST `/api/mfa/enroll/verify`
  - Verifies the enrollment OTP and enables MFA for the user.

### MFA Login Verification

- POST `/api/mfa/login/verify`
  - Verifies the login OTP and returns the final access token pair.

## Security Notes

This implementation follows a number of OWASP-aligned practices:

- Short-lived JWTs for temporary MFA workflows
- Separate signing keys for access tokens and MFA tokens
- OTPs that are short-lived and single-use
- Audit logging for security-related events
- Restricted MFA methods to SMS and email only
- No OTP values logged in application output

## Testing

Run the unit tests with:

```bash
dotnet test tests/Authentication.Mfa.Twilio.Tests/Authentication.Mfa.Twilio.Tests.csproj
```

## Documentation

Additional implementation notes and design details are available in the docs folder:

- [docs/mfa-with-twilio.md](docs/mfa-with-twilio.md)
- [docs/implementation-plan.md](docs/implementation-plan.md)
- [docs/security-and-audit.md](docs/security-and-audit.md)
- [docs/twilio-setup.md](docs/twilio-setup.md)
- [docs/api-endpoints.md](docs/api-endpoints.md)
- [docs/code-review-notes.md](docs/code-review-notes.md)
