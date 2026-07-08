using Authentication.Mfa.Twilio.Common;

namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface ITwilioVerifyService
{
    Task<Result> SendOtpAsync(string target, string code, CancellationToken cancellationToken);
}