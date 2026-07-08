namespace Authentication.Mfa.Twilio.Security;

public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}