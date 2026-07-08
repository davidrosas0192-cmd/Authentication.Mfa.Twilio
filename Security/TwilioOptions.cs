namespace Authentication.Mfa.Twilio.Security;

public class TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string VerifyServiceSid { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}
