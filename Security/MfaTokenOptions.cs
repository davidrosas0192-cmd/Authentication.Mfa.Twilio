namespace Authentication.Mfa.Twilio.Security;

public class MfaTokenOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 5;
}