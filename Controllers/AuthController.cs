using System.Security.Claims;
using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.DTOs;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Mfa.Twilio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly ITwilioVerifyService _twilioVerifyService;
    private readonly IAuditService _auditService;
    private readonly ApplicationDbContext _dbContext;

    public AuthController(
        IUserService userService,
        IPasswordHasherService passwordHasherService,
        ITokenService tokenService,
        IMfaService mfaService,
        ITwilioVerifyService twilioVerifyService,
        IAuditService auditService,
        ApplicationDbContext dbContext)
    {
        _userService = userService;
        _passwordHasherService = passwordHasherService;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _twilioVerifyService = twilioVerifyService;
        _auditService = auditService;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { success = false, message = "Username and password are required." });
        }

        var user = await _userService.GetByUserNameAsync(request.Username, cancellationToken);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        if (user is null || !_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditService.LogAsync(
                "login.attempt",
                "failure",
                user?.Id.ToString(),
                ipAddress,
                userAgent,
                Request.Path,
                "invalid_credentials",
                cancellationToken);

            return Unauthorized(new { success = false, message = "Invalid username or password." });
        }

        if (user.IsMfaEnabled)
        {
            var challenge = await _mfaService.StartLoginChallengeAsync(user.Id, cancellationToken);
            await _twilioVerifyService.SendOtpAsync(user.MfaTarget, challenge.Code, cancellationToken);
            var tempToken = _tokenService.GenerateMfaToken(user, "login", challenge.Id.ToString());

            await _auditService.LogAsync(
                "mfa.challenge.issued",
                "success",
                user.Id.ToString(),
                ipAddress,
                userAgent,
                Request.Path,
                "login",
                cancellationToken);

            return Ok(new
            {
                success = true,
                requiresMfa = true,
                tempToken,
                expiresInSeconds = 300,
                challengeId = challenge.Id.ToString(),
                method = user.MfaMethod.ToString().ToLowerInvariant()
            });
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "login.success",
            "success",
            user.Id.ToString(),
            ipAddress,
            userAgent,
            Request.Path,
            null,
            cancellationToken);

        return Ok(new
        {
            success = true,
            requiresMfa = false,
            accessToken,
            refreshToken,
            expiresInSeconds = 3600
        });
    }
}
