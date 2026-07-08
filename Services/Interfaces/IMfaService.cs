using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.DTOs;

namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface IMfaService
{
    Task<MfaTransaction> StartEnrollmentChallengeAsync(Guid userId, MfaMethod method, MfaEnrollStartRequest request, CancellationToken cancellationToken);
    Task<(bool Success, string Message, string? ErrorCode)> VerifyEnrollmentAsync(Guid userId, string code, string token, CancellationToken cancellationToken);
    Task<MfaTransaction> StartLoginChallengeAsync(Guid userId, CancellationToken cancellationToken);
    Task<(bool Success, string Message, string? ErrorCode)> VerifyLoginAsync(Guid userId, string code, string token, CancellationToken cancellationToken);
}
