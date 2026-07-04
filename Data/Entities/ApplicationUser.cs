namespace Authentication.Mfa.Twilio.Data.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }
    
    public string UserName { get; set; } = default!;



}