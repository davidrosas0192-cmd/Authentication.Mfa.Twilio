namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string eventType, string result, string? userId, string? ipAddress, string? userAgent, string? requestPath, string? reasonCode, CancellationToken cancellationToken);
}
