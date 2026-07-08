using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.DTOs;
using Authentication.Mfa.Twilio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Mfa.Twilio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfaService;
    private readonly ITwilioVerifyService _twilioVerifyService;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly ApplicationDbContext _dbContext;

    public MfaController(
        IMfaService mfaService,
        ITwilioVerifyService twilioVerifyService,
        ITokenService tokenService,
        IUserService userService,
        IAuditService auditService,
        ApplicationDbContext dbContext)
    {
        _mfaService = mfaService;
        _twilioVerifyService = twilioVerifyService;
        _tokenService = tokenService;
        _userService = userService;
        _auditService = auditService;
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost("enroll/start")]
    public async Task<IActionResult> StartEnrollment([FromBody] MfaEnrollStartRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        if (user is null) return NotFound();

        if (!Enum.TryParse<MfaMethod>(request.Method, true, out var method))
        {
            return BadRequest(new { success = false, message = "Unsupported MFA method." });
        }

        var challenge = await _mfaService.StartEnrollmentChallengeAsync(user.Id, method, request, cancellationToken);
        await _twilioVerifyService.SendOtpAsync(request.GetTarget(), challenge.Code, cancellationToken);
        var tempToken = _tokenService.GenerateMfaToken(user, "enrollment", challenge.Id.ToString());

        return Ok(new
        {
            success = true,
            message = "Verification code sent",
            tempToken,
            expiresInSeconds = 300,
            challengeId = challenge.Id.ToString(),
            method = method.ToString().ToLowerInvariant()
        });
    }

    [Authorize]
    [HttpPost("enroll/verify")]
    public async Task<IActionResult> VerifyEnrollment([FromBody] MfaVerifyRequest request, CancellationToken cancellationToken)
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

        var principal = _tokenService.ValidateMfaToken(token);
        if (principal is null) return Unauthorized(new { success = false, message = "Invalid or expired MFA token." });

        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        if (user is null) return NotFound();

        var result = await _mfaService.VerifyEnrollmentAsync(user.Id, request.Code, token, cancellationToken);
        if (!result.Success)
        {
            await _auditService.LogAsync("mfa.enrollment.failed", "failure", user.Id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), Request.Path, result.ErrorCode, cancellationToken);
            return BadRequest(new { success = false, message = result.Message });
        }

        await _auditService.LogAsync("mfa.enrollment.completed", "success", user.Id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), Request.Path, null, cancellationToken);
        return Ok(new { success = true, message = "MFA enrollment completed", enabledMethods = new[] { user.MfaMethod.ToString().ToLowerInvariant() }, mfaEnabled = true });
    }

    [HttpPost("login/verify")]
    public async Task<IActionResult> VerifyLogin([FromBody] MfaVerifyRequest request, CancellationToken cancellationToken)
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized();

        var principal = _tokenService.ValidateMfaToken(token);
        if (principal is null) return Unauthorized(new { success = false, message = "Invalid or expired MFA token." });

        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        if (user is null) return NotFound();

        var result = await _mfaService.VerifyLoginAsync(user.Id, request.Code, token, cancellationToken);
        if (!result.Success)
        {
            await _auditService.LogAsync("mfa.login.failed", "failure", user.Id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), Request.Path, result.ErrorCode, cancellationToken);
            return BadRequest(new { success = false, message = result.Message });
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

        await _auditService.LogAsync("mfa.login.success", "success", user.Id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers.UserAgent.ToString(), Request.Path, null, cancellationToken);
        return Ok(new { success = true, message = "Login successful", accessToken, refreshToken, expiresInSeconds = 3600 });
    }
}
