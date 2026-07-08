namespace Authentication.Mfa.Twilio.Data.Entities;

public class MfaTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Purpose { get; set; } = "login";
    public MfaMethod Method { get; set; }
    public string Target { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAttemptAt { get; set; }

    public ApplicationUser? User { get; set; }
}