# Twilio Setup

## Prerequisites

- A Twilio account
- A Twilio Verify Service SID
- An Account SID and Auth Token

## Configuration

Add the following settings to your environment or configuration:

```json
{
  "Twilio": {
    "Enabled": true,
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "VerifyServiceSid": "your-verify-service-sid"
  }
}
```

## Notes

- The implementation uses Twilio Verify for OTP delivery.
- If Twilio is not configured, the service logs the OTP target instead of sending it.
- For production, store secrets in a secure secret manager rather than source control.
