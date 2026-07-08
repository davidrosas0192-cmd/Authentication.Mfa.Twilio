namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface ITwilioVerifyService
{
    Task SendOtpAsync(string target, string code, CancellationToken cancellationToken);
}