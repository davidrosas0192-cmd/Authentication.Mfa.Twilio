namespace Authentication.Mfa.Twilio.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class MfaEnrollStartRequest
{
    public string Method { get; set; } = "sms";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    public string GetTarget()
    {
        return Method.Equals("sms", StringComparison.OrdinalIgnoreCase)
            ? PhoneNumber ?? string.Empty
            : Email ?? string.Empty;
    }
}

public class MfaVerifyRequest
{
    public string Code { get; set; } = string.Empty;
}