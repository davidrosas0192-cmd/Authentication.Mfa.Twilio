namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface IOtpSender
{
    Task SendOtpAsync(string target, string code, CancellationToken cancellationToken);
}
