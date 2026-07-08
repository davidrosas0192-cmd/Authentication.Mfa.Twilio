using Authentication.Mfa.Twilio.Data.Entities;

namespace Authentication.Mfa.Twilio.Services.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);
    Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken);
}