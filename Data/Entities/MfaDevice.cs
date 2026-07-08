namespace Authentication.Mfa.Twilio.Data.Entities;

public class MfaDevice
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string? DeviceIdentifier { get; set; }
    public bool IsTrusted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ApplicationUser? User { get; set; }
}