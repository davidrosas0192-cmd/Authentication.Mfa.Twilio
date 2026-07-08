using Authentication.Mfa.Twilio.Common;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.DTOs;

namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface IMfaService
{
    Task<Result<MfaTransaction>> StartEnrollmentChallengeAsync(Guid userId, MfaMethod method, MfaEnrollStartRequest request, CancellationToken cancellationToken);
    Task<Result> VerifyEnrollmentAsync(Guid userId, string code, CancellationToken cancellationToken);
    Task<Result<MfaTransaction>> StartLoginChallengeAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result> VerifyLoginAsync(Guid userId, string code, CancellationToken cancellationToken);
}
