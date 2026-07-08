using System.Net.Http.Headers;
using System.Text;
using Authentication.Mfa.Twilio.Common;
using Authentication.Mfa.Twilio.Security;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Authentication.Mfa.Twilio.Services.Implementations;

public class TwilioVerifyService : ITwilioVerifyService
{
    private readonly TwilioOptions _twilioOptions;
    private readonly HttpClient _httpClient;

    public TwilioVerifyService(IOptions<TwilioOptions> twilioOptions, HttpClient httpClient)
    {
        _twilioOptions = twilioOptions.Value;
        _httpClient = httpClient;
    }

    public async Task<Result> SendOtpAsync(string target, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return Result.Failure("A delivery target is required.", "missing_target");
        }

        if (!_twilioOptions.Enabled || string.IsNullOrWhiteSpace(_twilioOptions.AccountSid) || string.IsNullOrWhiteSpace(_twilioOptions.AuthToken) || string.IsNullOrWhiteSpace(_twilioOptions.VerifyServiceSid))
        {
            Console.WriteLine($"Twilio integration disabled. OTP '{code}' would be sent to {target}");
            return Result.Success("OTP delivery simulated locally.");
        }

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_twilioOptions.AccountSid}:{_twilioOptions.AuthToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://verify.twilio.com/v2/Services/{_twilioOptions.VerifyServiceSid}/Verifications")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"] = target,
                ["Channel"] = "sms"
            })
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure("Failed to send OTP via Twilio.", "twilio_send_failed");
        }

        return Result.Success("OTP sent successfully.");
    }
}