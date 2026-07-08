using System.Security.Claims;
using Authentication.Mfa.Twilio.Data.Entities;

namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateMfaToken(ApplicationUser user, string purpose, string challengeId);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateMfaToken(string token);
}