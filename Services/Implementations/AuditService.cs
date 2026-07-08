using Authentication.Mfa.Twilio.Data;
using Authentication.Mfa.Twilio.Data.Entities;
using Authentication.Mfa.Twilio.Services.Interfaces;

namespace Authentication.Mfa.Twilio.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(string eventType, string result, string? userId, string? ipAddress, string? userAgent, string? requestPath, string? reasonCode, CancellationToken cancellationToken)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            EventType = eventType,
            Result = result,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RequestPath = requestPath,
            ReasonCode = reasonCode,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
