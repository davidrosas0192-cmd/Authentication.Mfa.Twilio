using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.Security;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Mfa.Twilio.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly MfaTokenOptions _mfaTokenOptions;

    public TokenService(IOptions<JwtOptions> jwtOptions, IOptions<MfaTokenOptions> mfaTokenOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _mfaTokenOptions = mfaTokenOptions.Value;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        return CreateToken(claims, _jwtOptions.SecretKey, _jwtOptions.ExpirationMinutes);
    }

    public string GenerateMfaToken(ApplicationUser user, string purpose, string challengeId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("purpose", purpose),
            new("challengeId", challengeId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return CreateToken(claims, _mfaTokenOptions.SecretKey, _mfaTokenOptions.ExpirationMinutes);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    public ClaimsPrincipal? ValidateMfaToken(string token)
    {
        return ValidateToken(token, _mfaTokenOptions.SecretKey);
    }

    private string CreateToken(IEnumerable<Claim> claims, string secretKey, int expiryMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}